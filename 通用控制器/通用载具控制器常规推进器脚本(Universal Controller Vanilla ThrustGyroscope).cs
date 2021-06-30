public Program()
{
    Restart();
    if (!Me.CustomName.EndsWith("[UC_TGC]"))
        Me.CustomName += "[UC_TGC]";
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}
public void Main(string argument, UpdateType updateSource)
{
    try
    {
        if (updateSource.HasFlag(UpdateType.IGC) && argument == callback_Tag)
        {
            while (listener.HasPendingMessage)
            {
                MyIGCMessage msg = listener.AcceptMessage();
                try
                {
                    if (msg.Tag == ListenerTag)
                    {
                        var Data = (MyTuple<MyTuple<bool, MatrixD, bool>, MyTuple<bool, Vector3D, float, bool>, MyTuple<bool, Vector3D, Vector3>, ImmutableArray<string>, MyTuple<Vector3D, Vector3D>>)msg.Data;
                        ReceiveDatas = Data;
                    }
                }
                catch (Exception) { }
                try
                {
                    GyroController.Running(ReceiveDatas.Item1.Item1, ReceiveDatas.Item3);
                }
                catch (Exception) { }
                try
                {
                    ThrusterController.ENCT.Clear();
                    ThrusterController.ENCT.AddRange(ReceiveDatas.Item4);
                    ThrusterController.Running(ReceiveDatas.Item1.Item1, ReceiveDatas.Item1.Item2, ReceiveDatas.Item1.Item3, ReceiveDatas.Item2);
                }
                catch (Exception) { }
                try
                {
                    VectorEngineMotorManage.Running(ReceiveDatas.Item1.Item1, ReceiveDatas.Item5, SLDVM);
                }
                catch (Exception) { }
            }
        }
    }
    catch (Exception) { Restart(); }
}
const string callback_Tag = "<UC_Signal>";
const string ListenerTag = "UC_ThrustGyroscopeController";
IMyUnicastListener listener;
MyTuple<MyTuple<bool, MatrixD, bool>, MyTuple<bool, Vector3D, float, bool>, MyTuple<bool, Vector3D, Vector3>, ImmutableArray<string>, MyTuple<Vector3D, Vector3D>> ReceiveDatas = default(MyTuple<MyTuple<bool, MatrixD, bool>, MyTuple<bool, Vector3D, float, bool>, MyTuple<bool, Vector3D, Vector3>, ImmutableArray<string>, MyTuple<Vector3D, Vector3D>>);
void Restart()
{
    try
    {
        ThrusterController.ForceUpdate(Me, GridTerminalSystem, Me.IsSameConstructAs);
        GyroController.ForceUpdate(GridTerminalSystem, Me.IsSameConstructAs);
        listener = IGC.UnicastListener;
        listener.SetMessageCallback(callback_Tag);
        VectorEngineMotorManage.ForceUpdate(GridTerminalSystem, Me.IsSameConstructAs);
    }
    catch (Exception) { }
}
MyThrusterController ThrusterController { get; } = new MyThrusterController();
MyGyroController GyroController { get; } = new MyGyroController();
MyVectorEngineMotorManage VectorEngineMotorManage { get; } = new MyVectorEngineMotorManage();
float SLDVM => (Common.IsNull(Me?.CubeGrid) ? 5f : ((Me.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large) ? 20 : 4)) * 10;
sealed class MyThrusterController
{
    const double DGM = 0.875f;
    IMyTerminalBlock Me;
    MyTuple<IMyThrust, double>[] T;
    public List<string> ENCT { get; } = new List<string>();
    bool NullThrust => Common.IsNullC(T);
    public void ForceUpdate(IMyTerminalBlock Me, IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE)
    {
        if (Common.IsNull(Me) || ITE == null)
            return;
        this.Me = Me;
        T = Common.GetTs(GTS, (IMyThrust thrust) => Common.ExceptKeywords(thrust) && ITE(thrust)).ConvertAll(t => new MyTuple<IMyThrust, double>(t, 1)).ToArray();
        foreach (var t in T)
        {
            t.Item1.ShowInToolbarConfig = false;
            t.Item1.ShowInTerminal = false;
        }
    }
    public void Running(bool CanRun, MatrixD ControllerMatrix, bool HoverMode, MyTuple<bool, Vector3D, float, bool> ReceiveDatas_Thrusters)
    {
        if (NullThrust)
            return;
        if (!CanRun)
        {
            foreach (var thrust in T)
            {
                if (Common.NullEntity(thrust.Item1))
                    continue;
                if (thrust.Item1.ThrustOverridePercentage != 0)
                    thrust.Item1.ThrustOverridePercentage = 0;
                if (!thrust.Item1.Enabled)
                    thrust.Item1.Enabled = true;
            }
            return;
        }
        if (!ReceiveDatas_Thrusters.Item1)
        {
            foreach (var thrust in T)
            {
                if (thrust.Item1.Enabled)
                    thrust.Item1.Enabled = false;
                if (thrust.Item1.ThrustOverridePercentage != 0)
                    thrust.Item1.ThrustOverridePercentage = 0;
            }
            return;
        }
        Vector3D TotalThrust_Needed_Velocity = ReceiveDatas_Thrusters.Item2 * ReceiveDatas_Thrusters.Item3;
        Vector3D V_Velocity = CMF.NormalizeD(TotalThrust_Needed_Velocity);
        double length_Avaliable_Velocity = 0;
        for (int index = 0; index < T.Length; index++)
        {
            if (Common.NullEntity(T[index].Item1) || !T[index].Item1.IsFunctional)
                continue;
            T[index].Item2 = Math.Max(0, T[index].Item1.WorldMatrix.Backward.Dot(V_Velocity));
            length_Avaliable_Velocity += T[index].Item2 * T[index].Item1.MaxEffectiveThrust;
        }
        double p_v = 0;
        double length_Needed_Velocity = TotalThrust_Needed_Velocity.Length();
        if (length_Avaliable_Velocity != 0)
            p_v = length_Needed_Velocity / length_Avaliable_Velocity;
        if (p_v == 0)
        {
            foreach (var thrust in T)
            {
                thrust.Item1.ThrustOverridePercentage = 0;
                return;
            }
        }
        foreach (var thrust in T)
        {
            if (Common.NullEntity(thrust.Item1) || !thrust.Item1.IsFunctional)
                continue;
            thrust.Item1.ThrustOverridePercentage = (float)(MathHelper.Clamp(thrust.Item2 * p_v, Math.Max(1 / thrust.Item1.MaxEffectiveThrust, 1e-8f), 1));
        }
        foreach (var thrust in T)
        {
            if (Common.NullEntity(thrust.Item1))
                continue;
            bool thrustenabled = ReceiveDatas_Thrusters.Item1;
            if (thrust.Item1.WorldMatrix.Backward.Dot(ControllerMatrix.Up) > DGM)
                thrustenabled = thrustenabled || HoverMode || NIMG(thrust.Item1);
            else if (thrust.Item1.WorldMatrix.Backward.Dot(ControllerMatrix.Forward) > DGM)
                thrustenabled = thrustenabled || !HoverMode || NIMG(thrust.Item1);
            if (thrust.Item1.ThrustOverridePercentage == 0)
                thrustenabled = false;
            if (ENCT.Contains("Ion") && SCPF.IsIonEngine(thrust.Item1))
            {
                if (thrust.Item1.Enabled)
                    thrust.Item1.Enabled = false;
                continue;
            }
            if (ENCT.Any(thrust.Item1.BlockDefinition.SubtypeId.Contains))
            {
                if (thrust.Item1.Enabled)
                    thrust.Item1.Enabled = false;
                continue;
            }
            if (thrust.Item1.Enabled != thrustenabled)
                thrust.Item1.Enabled = thrustenabled;
            if (!thrust.Item1.IsWorking)
            {
                thrust.Item1.ThrustOverridePercentage = 0;
                continue;
            }
        }
    }
    bool NIMG(IMyThrust thrust)
    {
        if (Common.IsNull(Me))
            return true;
        return Me.CubeGrid != thrust.CubeGrid;
    }
}
sealed class MyGyroController
{
    public Vector3 PS3A = Vector3.One;
    List<IMyGyro> G;
    public void ForceUpdate(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE)
    {
        if (ITE == null)
            return;
        G = Common.GetTs(GTS, (IMyGyro gyro) => Common.ExceptKeywords(gyro) && NoControl(gyro) && ITE(gyro));
        foreach (var gyro in G)
        {
            gyro.ShowInTerminal = false;
            gyro.ShowInToolbarConfig = false;
        }
    }
    public void Running(bool CanRun, MyTuple<bool, Vector3D, Vector3> ReceiveDatas_Gyros)
    {
        PS3A = ReceiveDatas_Gyros.Item3;
        if (Common.IsNullC(G))
            return;
        if (CanRun)
        {
            foreach (var gyro in G)
                if (gyro.Enabled != ReceiveDatas_Gyros.Item1)
                    gyro.Enabled = ReceiveDatas_Gyros.Item1;
            if (!ReceiveDatas_Gyros.Item1)
                return;
            foreach (var gyro in G)
            {
                if (!gyro.GyroOverride)
                    gyro.GyroOverride = true;
                var result = CMF.VTransWorld2LocalD(gyro.WorldMatrix, ReceiveDatas_Gyros.Item2) * PS3A;
                gyro.Roll = (float)result.Z;
                gyro.Yaw = (float)result.Y;
                gyro.Pitch = (float)result.X;
            }
        }
        else
        {
            foreach (var gyro in G)
            {
                if (gyro == null)
                    continue;
                if (!gyro.Enabled)
                    gyro.Enabled = true;
                gyro.Roll = gyro.Yaw = gyro.Pitch = 0;
                gyro.GyroOverride = false;
            }
            return;
        }
    }
    static bool NoControl(IMyGyro Gyro)
    {
        if (Gyro.CustomName.Contains("Klang") || Gyro.CustomName.Contains("Clang"))
            return false;
        return true;
    }
}
sealed class MyVectorEngineMotorManage
{
    List<IMyMotorStator> Motors;
    public void ForceUpdate(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE)
    {
        if (GTS == null || ITE == null)
            return;
        Motors = Common.GetTs<IMyMotorStator>(GTS, b => ITE(b) && b.CustomName.Contains("Engine"));
    }
    public void Running(bool EnabledRunning, MyTuple<Vector3D, Vector3D> ReceiveDatas_VectorEngineRotor, float SLDVM)
    {
        if (!EnabledRunning) return;
        var direction = ReceiveDatas_VectorEngineRotor.Item1;
        var direction_onlynormal = ReceiveDatas_VectorEngineRotor.Item2;
        foreach (var Motor in Motors)
        {
            if (Motor == null || Motor.Top == null)
                continue;
            var vector = Motor.Top.WorldMatrix.Forward;
            if (Motor.CustomName.Contains("Backward"))
                vector = Motor.Top.WorldMatrix.Backward;
            else if (Motor.CustomName.Contains("Backward"))
                vector = Motor.Top.WorldMatrix.Backward;
            else if (Motor.CustomName.Contains("Up"))
                vector = Motor.Top.WorldMatrix.Up;
            else if (Motor.CustomName.Contains("Down"))
                vector = Motor.Top.WorldMatrix.Down;
            else if (Motor.CustomName.Contains("Left"))
                vector = Motor.Top.WorldMatrix.Left;
            else if (Motor.CustomName.Contains("Right"))
                vector = Motor.Top.WorldMatrix.Right;
            else if (Motor.CustomName.Contains("Forward"))
                vector = Motor.Top.WorldMatrix.Forward;
            float value;
            if ((Vector3.IsZero(direction) && !Motor.CustomName.Contains("Normal")) || (Vector3.IsZero(direction_onlynormal) && Motor.CustomName.Contains("Normal")))
                value = -Motor.Angle;
            else
                value = (float)SCPF.ControlWithVector(Motor.WorldMatrix.Up, Motor.CustomName.Contains("Normal") ? direction_onlynormal : direction, -vector) * SLDVM;
            if (float.IsNaN(value))
                value = 0;
            Motor.TargetVelocityRad = CMF.SmoothValue(value);
        }
    }
}
static class Common
{
    static readonly string[] BlackList_ShipController = new string[] { "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", "Clang" };
    public static bool ExceptKeywords(IMyTerminalBlock block)
    {
        foreach (var item in BlackList_ShipController)
        {
            if (block.BlockDefinition.SubtypeId.Contains(item) || block.CustomName.Contains("Clang"))
                return false;
        }
        return true;
    }
    public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class
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
    public static bool IsNull<T>(T Value) where T : class => Value == null;
    public static bool IsNullC<T>(ICollection<T> Value) => (Value?.Count ?? 0) < 1;
    public static bool NullEntity<T>(T Ent) where T : IMyEntity => Ent == null;
}
static class SCPF
{
    public static bool IsIonEngine(IMyThrust thrust)
    {
        if (thrust == null)
            return false;
        return IonThrustList.Contains(thrust.BlockDefinition.SubtypeId) || thrust.BlockDefinition.SubtypeId.Contains("Ion") || thrust.BlockDefinition.SubtypeId.Contains("ion");
    }
    public static float ControlWithVector(Vector3 Axe, Vector3 Target, Vector3 Current, float Epsilon = 0) => CMF.SignedAngle(CMF.ProjectOnPlane(Target, Axe, Epsilon), Current, Axe);
    static List<string> IonThrustList = new List<string>() { "SmallBlockSmallThrust", "SmallBlockLargeThrust", "LargeBlockSmallThrust", "LargeBlockLargeThrust", "SmallBlockSmallThrustSciFi", "SmallBlockLargeThrustSciFi", "LargeBlockSmallThrustSciFi", "LargeBlockLargeThrustSciFi" };
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
    public static Vector3D VTransWorld2LocalD(MatrixD Parent_World, Vector3D Vector_World) => Vector3D.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World));
}