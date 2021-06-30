public Program()
{
    Restart();
    if (!Me.CustomName.EndsWith("[UC_WCC]"))
        Me.CustomName += "[UC_WCC]";
    Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10 | UpdateFrequency.Update100;
}
public void Main(string argument, UpdateType updateSource)
{
    try
    {              
        if (updateSource.HasFlag(UpdateType.IGC) && argument == Callback_Tag)
        {
            while (listener.HasPendingMessage)
            {
                MyIGCMessage msg = listener.AcceptMessage();
                switch (msg.Tag)
                {
                    case Config_ListenerTag:
                        try
                        {
                            var Data = (MyTuple<bool, bool, int, ImmutableArray<float>>)msg.Data;
                            WheelsUtilsCtrl.Configs = Data;
                        }
                        catch (Exception) { }
                        WheelsUtilsCtrl.ForceUpdate(GridTerminalSystem, Me.IsSameConstructAs);
                        break;
                    case Ctrl_ListenerTag:
                        try
                        {
                            var Data = (MyTuple<ImmutableArray<bool>, int, MatrixD, float, ImmutableArray<Vector3>>)msg.Data;
                            WheelsUtilsCtrl.ControlLines = Data;
                        }
                        catch (Exception) { }
                        WheelsUtilsCtrl.Running();
                        break;
                    default:
                        break;
                }
            }
        }
    }
    catch (Exception) { Restart(); }
}
void Restart()
{
    try
    {
        listener = IGC.UnicastListener;
        listener.SetMessageCallback(Callback_Tag);
        WheelsUtilsCtrl.ForceUpdate(GridTerminalSystem, Me.IsSameConstructAs);
    }
    catch (Exception) { }
}
IMyUnicastListener listener;
MyWheelsUtilsCtrl WheelsUtilsCtrl { get; } = new MyWheelsUtilsCtrl();
sealed class MyWheelsUtilsCtrl
{
    internal const float MSAS = 30f;
    const float SGG = UGG / 2f;
    const float UGG = 0.9f;
    int _CG = 1;
    float _TM = 0.75f;
    List<IMyInteriorLight> BKL;
    List<IMyInteriorLight> BWL;
    int CT = 0;
    float FI = 0;
    List<IMyLandingGear> LGS;
    List<IMyTerminalBlock> MH;
    List<IMyMotorStator> MWS;
    List<IMyPistonBase> PT;
    List<IMyShipConnector> SCS;
    List<IMyMotorSuspension> SWS;
    IMyBlockGroup WS;
    #region 控制线
    public MyTuple<ImmutableArray<bool>, int, MatrixD, float, ImmutableArray<Vector3>> ControlLines = default(MyTuple<ImmutableArray<bool>, int, MatrixD, float, ImmutableArray<Vector3>>);
    bool NotEnabledLine => ControlLines.Item1.Length != 4;
    bool NotControlLine => ControlLines.Item5.Length != 5;
    bool CanRun => NotEnabledLine ? false : ControlLines.Item1[0];
    bool HandBrake => NotEnabledLine ? true : ControlLines.Item1[1];
    bool HasWings => NotEnabledLine ? false : ControlLines.Item1[2];
    bool EnabledWheels => NotEnabledLine ? false : ControlLines.Item1[3];
    MyRole Role => (MyRole)ControlLines.Item2;
    MatrixD WorldMatrix => ControlLines.Item3;
    float MaxiumSpeedLimited => ControlLines.Item4;
    Vector3 LinearVelocity => NotControlLine ? Vector3.Zero : ControlLines.Item5[0];
    Vector3 MoveIndicator => NotControlLine ? Vector3.Zero : ControlLines.Item5[1];
    Vector3 RotationIndicator => NotControlLine ? Vector3.Zero : ControlLines.Item5[2];
    Vector3 RI_SC => NotControlLine ? Vector3.Zero : ControlLines.Item5[3];
    Vector3 Center => NotControlLine ? Vector3.Zero : ControlLines.Item5[4];
    #endregion
    #region 车轮的配置信息
    public MyTuple<bool, bool, int, ImmutableArray<float>> Configs = default(MyTuple<bool, bool, int, ImmutableArray<float>>);
    bool DisabledAssist => Configs.Item1;
    bool DisabledSpinTurn => Configs.Item2;
    int CurrentGearMode => MathHelper.Clamp(Configs.Item3, 1, GMS.Count);
    bool NotConfig => Configs.Item4.Length != 12;
    float FrictionBalancePercentage => NotConfig ? 0 : MathHelper.Clamp(Configs.Item4[0], -1, 1);
    float PowerBalancePercentage => NotConfig ? 0 : MathHelper.Clamp(Configs.Item4[1], -1, 1);
    float WRotorMaxiumRpm => NotConfig ? 0 : MathHelper.Clamp(Configs.Item4[2], 0, 60);
    float MaxTurnAngle => NotConfig ? 0 : Math.Abs(Configs.Item4[3]);
    float MinTurnAngle => NotConfig ? 0 : Math.Min(Math.Abs(Configs.Item4[4]), MaxTurnAngle);
    float PowerMult => NotConfig ? 0 : Configs.Item4[5];
    float TurnSlippingMult { get { if (NotConfig) return 1; _TM = 1 - MathHelper.Clamp(Configs.Item4[6], 0, 1); return 1 - _TM; } }
    float Friction => NotConfig ? 100 : MathHelper.Clamp(Configs.Item4[7], 0, 100);
    float SuspensionHeight => NotConfig ? 0 : Configs.Item4[8];
    float DiffRpmPercentage => NotConfig ? 1 : MathHelper.Clamp(Configs.Item4[9], -1, 1);
    float FrontAngleRate => NotConfig ? 0 : MathHelper.Clamp(Configs.Item4[10], -1, 1);
    float RearWheelTurnRate => NotConfig ? 0 : MathHelper.Clamp(Configs.Item4[11], 0, 1);
    #endregion




    public int? CurrentGearOverride { get; set; } = null;
    public int CurrentGear => CurrentGearOverride ?? _CG;
    public float CurrentMaxiumSpeed { get; set; }

    public bool DC => (LGS?.Any(b => b.IsLocked) ?? false) || (SCS?.Any(b => b.Status == MyShipConnectorStatus.Connected) ?? false);
    public bool RetractWheels => HandBrake;
    bool ED => (LinearVelocity.LengthSquared() < 25f && FI == 0);
    bool HDS => !Common.IsNullC(MH); bool NMW => Common.IsNullC(MWS);
    bool NSW => Common.IsNullC(SWS);
    float TI => RotationIndicator.Y + (HasWings ? RI_SC.Y : 0) * 30;
    public void ForceUpdate(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE)
    {
        if (!CanRun || ITE == null) return;
        WS = GTS.GetBlockGroupWithName(WheelsGroupNM);
        MH = Common.GetTs(GTS, (IMyTerminalBlock thrust) => thrust.BlockDefinition.SubtypeId.Contains(HoverEngineNM));
        SWS = Common.GetTs<IMyMotorSuspension>(WS, ITE);
        MWS = Common.GetTs<IMyMotorStator>(WS, ITE);
        PT = Common.GetTs<IMyPistonBase>(GTS, p => ITE(p) && p.CustomName.Contains("UCR"));
        SCS = Common.GetTs<IMyShipConnector>(GTS, p => ITE(p) && p.CustomName.Contains("UCR"));
        LGS = Common.GetTs<IMyLandingGear>(GTS, p => ITE(p) && p.CustomName.Contains("UCR"));
        BKL = Common.GetTs(GTS, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BrakeNM) && ITE(lightblock));
        BWL = Common.GetTs(GTS, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BackwardNM) && ITE(lightblock));
    }
    public void Running()
    {
        if (!CanRun) return;
        LII();
        LLG();
        LCS();
        LPS();
        bool NeedRunningWheels = (Role == MyRole.WheelVehicle || Role == MyRole.TrackVehicle) && EnabledWheels;
        if (!Common.IsNullC(SWS))
        {
            foreach (var SWheel in SWS)
                if (SWheel.Enabled != NeedRunningWheels)
                    SWheel.Enabled = NeedRunningWheels;
        }
        if (!Common.IsNullC(MWS))
        {
            foreach (var MWheel in MWS)
                if (MWheel.Enabled != NeedRunningWheels)
                    MWheel.Enabled = NeedRunningWheels;
        }
        if (NeedRunningWheels)
        {
            FI = MoveIndicator.Z;
            ProcessForwardSignal();
            RunningGearSimulate();
            LSP();
            LWW();
        }
    }
    public void SAS(IMyMotorSuspension Wheel, float speed_cav, float delta, bool IRW)
    {
        switch (Role)
        {
            case MyRole.TrackVehicle:
                Wheel.MaxSteerAngle = 0;
                break;
            case MyRole.WheelVehicle:
                if (delta == 0)
                {
                    if (IRW)
                        Wheel.MaxSteerAngle = MaxTurnAngle * RearWheelTurnRate;
                    else
                        Wheel.MaxSteerAngle = MaxTurnAngle;
                    break;
                }
                if (IRW)
                    Wheel.MaxSteerAngle = (MinTurnAngle + speed_cav * delta) * RearWheelTurnRate;
                else
                    Wheel.MaxSteerAngle = MinTurnAngle + speed_cav * delta;
                break;
            default:
                break;
        }
        if (Wheel.InvertSteer)
            Wheel.InvertSteer = false;
    }
    void FSS(IMyMotorSuspension Wheel, bool IRW)
    {
        if (Role == MyRole.TrackVehicle)
        {
            if (TI == 0)
                Wheel.Friction = Friction;
            else
            {
                var Values = Math.Abs(WorldMatrix.Forward.Dot(Wheel.GetPosition() - Center));
                Values = Values * Values;
                Wheel.Friction = Friction * _TM / (float)(Values + 1);
            }
        }
        else if (Role == MyRole.WheelVehicle)
        {
            if (IRW)
            {
                var f = Friction * MathHelper.Clamp(1 + FrictionBalancePercentage, 0, 2);
                if (TurnSlippingMult <= 0)
                {
                    _TM = 1;
                }
                if (RotationIndicator.Y != 0)
                    f = f * _TM;
                Wheel.Friction = f;
            }
            else
            {
                Wheel.Friction = Friction * MathHelper.Clamp(1 - FrictionBalancePercentage, 0, 2);
            }
        }
        else
            Wheel.Friction = Friction;
    }
    void LCS()
    {
        if (!CanRun || Common.IsNullC(SCS))
            return;
        foreach (var ShipConnector in SCS)
        {
            if (ShipConnector == null)
                continue;
            if (RetractWheels)
                ShipConnector.Connect();
            else
                ShipConnector.Disconnect();
            ShipConnector.PullStrength = 1;
        }
    }
    void LII()
    {
        if (!CanRun)
            return;
        if (!Common.IsNullC(BKL))
            foreach (var item in BKL)
            {
                if (Common.IsNull(item))
                    continue;
                item.Color = Color.Red;
                item.Enabled = MoveIndicator.Z == 0 || (LinearVelocity.Dot(WorldMatrix.Forward) * MoveIndicator.Z) > 0;
            }
        if (!Common.IsNullC(BWL))
            foreach (var item in BWL)
            {
                if (Common.IsNull(item))
                    continue;
                item.Color = Color.White;
                item.Enabled = MoveIndicator.Z > 0 && LinearVelocity.Dot(WorldMatrix.Backward) > 0;
            }
    }
    void LLG()
    {
        if (!CanRun || Common.IsNullC(LGS))
            return;
        foreach (var LandingGear in LGS)
        {
            if (LandingGear == null)
                continue;
            if (RetractWheels)
            {
                if (!LandingGear.IsLocked)
                    LandingGear.Lock();
                LandingGear.AutoLock = true;
            }
            else
            {
                if (LandingGear.IsLocked)
                    LandingGear.Unlock();
                LandingGear.AutoLock = false;
            }
        }
    }
    void LPS()
    {
        if (!CanRun || Common.IsNullC(SCS))
            return;
        foreach (var Piston in PT)
        {
            if (Piston == null || Piston.TopGrid == null)
                continue;
            if (RetractWheels)
                Piston.Velocity = 1;
            else
                Piston.Velocity = -1;
        }
    }
    void LSP()
    {
        if (!CanRun || HDS || NSW)
            return;
        var SCV = MathHelper.Clamp(1 - Math.Max(MathHelper.Clamp(LinearVelocity.Length(), 0, 100) - MSAS, 0) / Math.Max((MathHelper.Clamp(MaxiumSpeedLimited, 30, 100) - MSAS), 1), 0, 1);
        var delta = Math.Max(MaxTurnAngle - MinTurnAngle, 0);
        foreach (var W in SWS)
        {
            if (W == null)
                continue;
            var _WDP = (float)CMF.ProjectOnVectorD(CMF.NormalizeD(W.GetPosition() - Center), WorldMatrix.Forward).Dot(WorldMatrix.Forward);
            var _IRW = W.CustomName.Contains("Rear") || _WDP < 0;
            SAS(W, (float)SCV, delta, _IRW);
            FSS(W, _IRW);
            if (DisabledAssist && Role != MyRole.TrackVehicle)
            {
                SuspensionSteerOverride(W, 0);
                SuspensionMotorOverride(W, 0);
            }
            else
            {
                var sign = Math.Sign(WorldMatrix.Right.Dot(W.WorldMatrix.Up));
                float PO;
                switch (Role)
                {
                    case MyRole.TrackVehicle:
                        PO = FI * sign + TI * DiffRpmPercentage;
                        break;
                    case MyRole.WheelVehicle:
                        PO = FI * sign + ((ED && FI == 0) ? (TI * DiffRpmPercentage) : (TI * (1 - (float)SCV) * DiffRpmPercentage));
                        break;
                    default:
                        PO = 0;
                        break;
                }
                W.Power = PowerMult * 100f * MathHelper.Clamp(1 + (_IRW ? PowerBalancePercentage : -PowerBalancePercentage), 0, 2);
                W.Height = RetractWheels ? 200 : (((_IRW ? 1 : -1) * FrontAngleRate + 1) * SuspensionHeight);
                W.Enabled = !RetractWheels;
                W.Brake = PO == 0 || RetractWheels;
                SuspensionSteer(W, _IRW);
                SuspensionMotorOverride(W, W.Power == 0 ? 0 : MathHelper.Clamp(PO, -PowerMult, PowerMult));
            }
        }
    }
    void LWW()
    {
        if (!CanRun || HDS || NMW) return;
        foreach (var Motor in MWS)
        {
            if (Motor == null || Motor.TopGrid == null)
                continue;
            var sign = Math.Sign(WorldMatrix.Right.Dot(Motor.WorldMatrix.Up));
            Motor.TargetVelocityRPM = RetractWheels ? 0 : (-(FI * sign + TI * DiffRpmPercentage) * WRotorMaxiumRpm * PowerMult);
            if (!Motor.RotorLock && HandBrake)
                Motor.RotorLock = true;
            else if (Motor.RotorLock)
                Motor.RotorLock = false;
        }
    }
    void ProcessForwardSignal()
    {
        var CurrentSpeed = Math.Max(LinearVelocity.Dot(WorldMatrix.Forward), 0);
        FI = CMF.SmoothValue((float)CurrentSpeed + CurrentMaxiumSpeed * FI);
    }
    void RunningGearSimulate()
    {
        if (HandBrake)
        {
            CurrentMaxiumSpeed = 0;
            _CG = 0;
            return;
        }
        if (Common.IsNullC(GMS) || Common.IsNullC(GMSD) || GMS.Count != GMSD.Count)
        {
            _CG = 1;
            CurrentMaxiumSpeed = MaxiumSpeedLimited;
        }
        var _CurrentGearMode = MathHelper.Clamp(CurrentGearMode, 1, GMS.Count);
        if (_CurrentGearMode != CurrentGearMode) CT = 0;
        _CG = MathHelper.Clamp(_CG, 1, GMS[CurrentGearMode].Count);
        var speed = Math.Abs(WorldMatrix.Forward.Dot(LinearVelocity));
        if (speed > UGG * GMS[CurrentGearMode][CurrentGear])
        {
            if (CT <= 0)
            {
                _CG = MathHelper.Clamp(CurrentGear + 1, 1, GMS[CurrentGearMode].Count);
                CT = GMSD[CurrentGearMode];
            }
            else
            {
                CT--;
            }
        }
        else if (speed < SGG * GMS[CurrentGearMode][CurrentGear])
        {
            _CG = MathHelper.Clamp(CurrentGear - 1, 1, GMS[CurrentGearMode].Count);
            CT = 0;
        }
        CurrentMaxiumSpeed = MathHelper.Clamp(GMS[CurrentGearMode][CurrentGear], 0, Math.Max(MaxiumSpeedLimited, 0));
    }
    void SuspensionSteer(IMyMotorSuspension Wheel, bool PRW)
    {
        if (Role == MyRole.TrackVehicle)
        {
            SuspensionSteerOverride(Wheel, 0);
            if (Wheel.Steering)
                Wheel.Steering = false;
            return;
        }
        else if (Role == MyRole.WheelVehicle)
        {
            if (!Wheel.Steering)
                Wheel.Steering = true;
            if (TI != 0)
            {
                if (ED && (!DisabledSpinTurn))
                    SuspensionSteerOverride(Wheel, Math.Sign(WorldMatrix.Left.Dot(Wheel.WorldMatrix.Up)) * (PRW ? -1 : 1));
                else
                    SuspensionSteerOverride(Wheel, TI * (PRW ? -1 : 1));
            }
            else
            {
                SuspensionSteerOverride(Wheel, 0);
            }
            return;
        }
        else
        {
            SuspensionSteerOverride(Wheel, 0);
            return;
        }
    }
    static void SuspensionMotorOverride(IMyMotorSuspension Wheel, float Value)
    {
        if (Wheel.GetProperty(MotorOverrideId).AsFloat().GetValue(Wheel) != Value)
            Wheel.GetProperty(MotorOverrideId).AsFloat().SetValue(Wheel, Value);
    }
    static void SuspensionSteerOverride(IMyMotorSuspension Wheel, float Value)
    {
        if (Wheel.GetProperty(SteerOverrideId).AsFloat().GetValue(Wheel) != Value)
            Wheel.GetProperty(SteerOverrideId).AsFloat().SetValue(Wheel, Value);
    }
    static readonly Dictionary<int, Dictionary<int, float>> GMS = new Dictionary<int, Dictionary<int, float>>() { { 1, new Dictionary<int, float>() { { 1, 30 }, { 2, 50 }, { 3, 70 }, { 4, 90 }, { 5, 120 }, { 6, float.PositiveInfinity } } }, { 2, new Dictionary<int, float>() { { 1, 30 }, { 2, 80 }, { 3, 120 }, { 4, float.PositiveInfinity } } }, { 3, new Dictionary<int, float>() { { 1, float.PositiveInfinity } } } };
    static readonly Dictionary<int, int> GMSD = new Dictionary<int, int>() { { 1, 10 }, { 2, 2 }, { 3, 0 } };
}
static class Common
{
    static readonly string[] BlackList_ShipController = new string[] { "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", "Clang" }; public static bool BlockInTurretGroup(IMyBlockGroup group, IMyTerminalBlock Me)
    {
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        group?.GetBlocks(blocks);
        if (blocks.Count < 1 || !blocks.Contains(Me))
            return false;
        return true;
    }
    public static bool ExceptKeywords(IMyTerminalBlock block)
    {
        foreach (var item in BlackList_ShipController)
        {
            if (block.BlockDefinition.SubtypeId.Contains(item) || block.CustomName.Contains("Clang"))
                return false;
        }
        return true;
    }
    public static IMyTerminalBlock GetBlock(IMyGridTerminalSystem gridTerminalSystem, long EntIds = 0) => gridTerminalSystem?.GetBlockWithId(EntIds) as IMyTerminalBlock; public static List<IMyTerminalBlock> GetBlocks(IMyGridTerminalSystem gridTerminalSystem, List<long> EntIds = null)
    {
        if (gridTerminalSystem == null)
            return null;
        return EntIds?.ConvertAll(id => gridTerminalSystem.GetBlockWithId(id) as IMyTerminalBlock);
    }
    public static T GetT<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class
    {
        List<T> Items = GetTs(gridTerminalSystem, requst);
        if (IsNullC(Items))
            return null;
        else
            return Items.First();
    }
    public static T GetT<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class => GetTs(blockGroup, requst).FirstOrDefault(); public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class
    {
        List<T> Items = new List<T>();
        if (gridTerminalSystem == null)
            return Items;
        gridTerminalSystem.GetBlocksOfType(Items, requst);
        return Items;
    }
    public static List<T> GetTs<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class
    {
        List<T> Items = new List<T>();
        if (blockGroup == null)
            return Items;
        blockGroup.GetBlocksOfType(Items, requst);
        return Items;
    }
    public static Matrix GetWorldMatrix(IMyTerminalBlock ShipController)
    {
        Matrix me_matrix;
        ShipController.Orientation.GetMatrix(out me_matrix);
        return me_matrix;
    }
    public static IMyCameraBlock ID2Camera(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyCameraBlock;
    public static IMyMotorStator ID2Motor(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyMotorStator;
    public static IMyTerminalBlock ID2Weapon(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyTerminalBlock;
    public static bool IsNull(Vector3? Value) => Value == null || Value.Value == Vector3.Zero;
    public static bool IsNull(Vector3D? Value) => Value == null || Value.Value == Vector3D.Zero;
    public static bool IsNull<T>(T Value) where T : class => Value == null;
    public static bool IsNullC<T>(ICollection<T> Value) => (Value?.Count ?? 0) < 1;
    public static bool IsNullC<T>(IEnumerable<T> Value) => (Value?.Count() ?? 0) < 1;
    public static bool NullEntity<T>(T Ent) where T : IMyEntity => Ent == null;
    public static float SetInRange_AngularDampeners(float data) => MathHelper.Clamp(data, 0.01f, 20f);
    public static List<string> SpliteByQ(string context) => context?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? (new List<string>());
}
static class CMF
{
    public static double SetNaNZero(double Value) => double.IsNaN(Value) ? 0 : Value; public static double AngleBetweenD(Vector3D a, Vector3D b)
    {
        var dotProd = Vector3D.Dot(a, b);
        var lenProd = a.Length() * b.Length();
        return Math.Acos(dotProd / lenProd);
    }
    public static Vector3D CalculateAvg(ICollection<Vector3D> Vectors)
    {
        if (Common.IsNullC(Vectors))
            return Vector3D.Zero;
        Vector3D Value = Vector3D.Zero;
        foreach (var Vector in Vectors)
        {
            Value += Vector;
        }
        return Value /= Vectors.Count;
    }
    public static Vector3 CalculateAvg(ICollection<Vector3> Vectors)
    {
        if (Common.IsNullC(Vectors))
            return Vector3.Zero;
        Vector3 Value = Vector3.Zero;
        foreach (var Vector in Vectors)
        {
            Value += Vector;
        }
        return Value /= Vectors.Count;
    }
    public static Vector3D CalculateSum(ICollection<Vector3D> Vectors)
    {
        if (Common.IsNullC(Vectors))
            return Vector3D.Zero;
        Vector3D Value = Vector3D.Zero;
        foreach (var Vector in Vectors)
        {
            Value += Vector;
        }
        return Value;
    }
    public static Vector3 CalculateSum(ICollection<Vector3> Vectors)
    {
        if (Common.IsNullC(Vectors))
            return Vector3.Zero;
        Vector3 Value = Vector3.Zero;
        foreach (var Vector in Vectors)
        {
            Value += Vector;
        }
        return Value;
    }
    public static Vector3 Normalize(Vector3 vector)
    {
        if (Vector3.IsZero(vector))
            return Vector3.Zero;
        return Vector3.Normalize(vector);
    }
    public static Vector3 Normalize(Vector3 vector, float Epsional, float Multipy)
    {
        return Normalize(vector * (Vector3.One - Vector3.IsZeroVector(vector, Epsional))) * Multipy;
    }
    public static Vector3D NormalizeD(Vector3D vector)
    {
        if (Vector3D.IsZero(vector))
            return Vector3D.Zero;
        return Vector3D.Normalize(vector);
    }
    public static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal, float Epsilon = 0)
    {
        if (Epsilon == 0)
            return Vector3.ProjectOnPlane(ref direction, ref planeNormal);
        var result = Vector3.ProjectOnPlane(ref direction, ref planeNormal);
        var length = result.Length();
        if (Math.Abs(Epsilon) > length)
            return Vector3D.Zero;
        return result;
    }
    public static Vector3D ProjectOnPlaneD(Vector3D direction, Vector3D planeNormal, double Epsilon = 0)
    {
        if (Epsilon == 0)
            return Vector3D.ProjectOnPlane(ref direction, ref planeNormal);
        var result = Vector3D.ProjectOnPlane(ref direction, ref planeNormal);
        var length = result.Length();
        if (Math.Abs(Epsilon) > length)
            return Vector3D.Zero;
        return result;
    }
    public static Vector3 ProjectOnVector(Vector3 vector, Vector3 direction, double Epsilon = 0)
    {
        if (Epsilon == 0)
            return Vector3.ProjectOnVector(ref vector, ref direction);
        var result = Vector3.ProjectOnVector(ref vector, ref direction);
        var length = result.Length();
        if (Math.Abs(Epsilon) > length)
            return Vector3D.Zero;
        return result;
    }
    public static Vector3D ProjectOnVectorD(Vector3D vector, Vector3D direction, double Epsilon = 0)
    {
        if (Epsilon == 0)
            return Vector3D.ProjectOnVector(ref vector, ref direction);
        var result = Vector3D.ProjectOnVector(ref vector, ref direction);
        var length = result.Length();
        if (Math.Abs(Epsilon) > length)
            return Vector3D.Zero;
        return result;
    }
    public static float SignedAngle(Vector3 A, Vector3 B, Vector3 Axis)
    {
        return MyMath.AngleBetween(A, B) * SignNonZero(A.Cross(B).Dot(Axis));
    }
    public static double SignedAngleD(Vector3D A, Vector3D B, Vector3D Axis)
    {
        if (Vector3.IsZero(A) || Vector3.IsZero(A))
            return 1;
        return AngleBetweenD(A, B) * SignNonZero(A.Cross(B).Dot(Axis));
    }
    public static int SignNonZero(double Value) => (Value >= 0) ? 1 : -1; public static float SmoothValue(float value)
    {
        return value * Math.Abs(value);
    }
    public static double SmoothValueD(double value)
    {
        return value * Math.Abs(value);
    }
    public static Vector3 SmoothValue(Vector3 value)
    {
        return value * Vector3.Abs(value);
    }
    public static Vector3D SmoothValueD(Vector3D value)
    {
        return value * Vector3D.Abs(value);
    }
    public static Vector3D VTransWorld2LocalD(MatrixD Parent_World, Vector3D Vector_World) => Vector3D.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World));
    public static Vector3D VTransLocal2WorldD(MatrixD Parent_World, Vector3D Vector_Local) => Vector3D.TransformNormal(Vector_Local, Parent_World);
    public static Vector3 VTransWorld2Local(MatrixD Parent_World, Vector3 Vector_World) => Vector3.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World));
    public static Vector3 VTransLocal2World(MatrixD Parent_World, Vector3 Vector_Local) => Vector3.TransformNormal(Vector_Local, Parent_World);
}
enum MyRole : int { None, Aeroplane, Helicopter, VTOL, SpaceShip, SeaShip, Submarine, TrackVehicle, WheelVehicle, HoverVehicle }
const string BackwardNM = @"Backward";
const string BrakeNM = @"Brake";
const string HoverEngineNM = "Hover";
const string MotorOverrideId = @"Propulsion override";
const string SteerOverrideId = @"Steer override";
const string WheelsGroupNM = @"Wheels";
const string Config_ListenerTag = @"WheelsSetup";
const string Callback_Tag = "<UC_Signal>";
const string Ctrl_ListenerTag = "UC_Wheel";