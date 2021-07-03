public Program() { Restart(); Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10 | UpdateFrequency.Update100; }
public void Main(string argument, UpdateType updateSource)
{
    try
    {
        if (updateSource.HasFlag(UpdateType.Update1))
            Update();
        if (updateSource.HasFlag(UpdateType.Terminal) || updateSource.HasFlag(UpdateType.Trigger))
        {
            if (CL.TryParse(argument))
            {
                if (CL.Switch("CruisingOnOff"))
                    SCIFM.BooleanValues["Cruising Mode"] = !SCIFM.BooleanValues["Cruising Mode"];
                if (CL.Switch("CalculateAllGravity"))
                    SCIFM.BooleanValues["Calculate All Gravity"] = !SCIFM.BooleanValues["Calculate All Gravity"];
                if (CL.Switch("CricleLockHeight"))
                    SCIFM.IntValues["Lock Height Mode"] = MathHelper.Clamp((SCIFM.IntValues["Lock Height Mode"] + 1) % 3, 0, 2);
                else if (CL.Switch("LockHeightSurface"))
                    SCIFM.IntValues["Lock Height Mode"] = 2;
                else if (CL.Switch("LockHeightSealevel"))
                    SCIFM.IntValues["Lock Height Mode"] = 1;
                else if (CL.Switch("LockHeightDisable"))
                    SCIFM.IntValues["Lock Height Mode"] = 0;
                if (CL.Switch("PitchAssignedGravity"))
                    SCIFM.BooleanValues["Pitch Assigned Gravity OnOff"] = !SCIFM.BooleanValues["Pitch Assigned Gravity OnOff"];
                if (CL.Switch("RollAssignedGravity"))
                    SCIFM.BooleanValues["Roll Assigned Gravity OnOff"] = !SCIFM.BooleanValues["Roll Assigned Gravity OnOff"];
                if (CL.Switch("PitchAssignedVelocity"))
                    SCIFM.BooleanValues["Pitch Assigned Velocity OnOff"] = !SCIFM.BooleanValues["Pitch Assigned Velocity OnOff"];
                if (CL.Switch("RollAssignedVelocity"))
                    SCIFM.BooleanValues["Roll Assigned Velocity OnOff"] = !SCIFM.BooleanValues["Roll Assigned Velocity OnOff"];
                if (CL.Switch("PitchSlopeThreshold"))
                    SCIFM.BooleanValues["Pitch Slope Threshold OnOff"] = !SCIFM.BooleanValues["Pitch Slope Threshold OnOff"];
                if (CL.Switch("RollSlopeThreshold"))
                    SCIFM.BooleanValues["Roll Slope Threshold OnOff"] = !SCIFM.BooleanValues["Roll Slope Threshold OnOff"];
                if (CL.Switch("IonThrust"))
                    SCIFM.BooleanValues["Ion Thrust OnOff"] = !SCIFM.BooleanValues["Ion Thrust OnOff"];
                if (CL.Switch("AtomThrust"))
                    SCIFM.BooleanValues["Atom Thrust OnOff"] = !SCIFM.BooleanValues["Atom Thrust OnOff"];
                if (CL.Switch("HydrogenThrust"))
                    SCIFM.BooleanValues["Hydrogen Thrust OnOff"] = !SCIFM.BooleanValues["Hydrogen Thrust OnOff"];
                if (CL.Switch("KhrustThrust"))
                    SCIFM.BooleanValues["Khrust Thrust OnOff"] = !SCIFM.BooleanValues["Khrust Thrust OnOff"];
                if (CL.Switch("GravityThrust"))
                    SCIFM.BooleanValues["Gravity Thrust OnOff"] = !SCIFM.BooleanValues["Gravity Thrust OnOff"];
                if (CL.Switch("FrontThrust"))
                    SCIFM.BooleanValues["Front Thrust OnOff"] = !SCIFM.BooleanValues["Front Thrust OnOff"];
                if (CL.Switch("BackThrust"))
                    SCIFM.BooleanValues["Back Thrust OnOff"] = !SCIFM.BooleanValues["Back Thrust OnOff"];
                if (CL.Switch("UpThrust"))
                    SCIFM.BooleanValues["Up Thrust OnOff"] = !SCIFM.BooleanValues["Up Thrust OnOff"];
                if (CL.Switch("DownThrust"))
                    SCIFM.BooleanValues["Down Thrust OnOff"] = !SCIFM.BooleanValues["Down Thrust OnOff"];
                if (CL.Switch("LeftThrust"))
                    SCIFM.BooleanValues["Left Thrust OnOff"] = !SCIFM.BooleanValues["Left Thrust OnOff"];
                if (CL.Switch("RightThrust"))
                    SCIFM.BooleanValues["Right Thrust OnOff"] = !SCIFM.BooleanValues["Right Thrust OnOff"];
                if (CL.Switch("DefaultMode"))
                {
                    SCIFM.BooleanValues["Pitch Assigned Gravity OnOff"] = false;
                    SCIFM.BooleanValues["Roll Assigned Gravity OnOff"] = false;
                    SCIFM.BooleanValues["Pitch Assigned Velocity OnOff"] = false;
                    SCIFM.BooleanValues["Roll Assigned Velocity OnOff"] = false;
                    SCIFM.BooleanValues["Pitch Slope Threshold OnOff"] = false;
                    SCIFM.BooleanValues["Roll Slope Threshold OnOff"] = false;
                    SCIFM.BooleanValues["Ion Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Atom Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Hydrogen Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Khrust Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Gravity Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Front Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Back Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Up Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Down Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Left Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Right Thrust OnOff"] = true;
                }
                else if (CL.Switch("AeroplaneMode"))
                {
                    SCIFM.BooleanValues["Pitch Assigned Gravity OnOff"] = false;
                    SCIFM.BooleanValues["Roll Assigned Gravity OnOff"] = true;
                    SCIFM.BooleanValues["Pitch Assigned Velocity OnOff"] = false;
                    SCIFM.BooleanValues["Roll Assigned Velocity OnOff"] = true;
                    SCIFM.BooleanValues["Pitch Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Roll Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Ion Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Atom Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Hydrogen Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Khrust Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Gravity Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Front Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Back Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Up Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Down Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Left Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Right Thrust OnOff"] = true;
                }
                else if (CL.Switch("HelicopterMode"))
                {
                    SCIFM.BooleanValues["Pitch Assigned Gravity OnOff"] = true;
                    SCIFM.BooleanValues["Roll Assigned Gravity OnOff"] = true;
                    SCIFM.BooleanValues["Pitch Assigned Velocity OnOff"] = true;
                    SCIFM.BooleanValues["Roll Assigned Velocity OnOff"] = true;
                    SCIFM.BooleanValues["Pitch Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Roll Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Ion Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Atom Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Hydrogen Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Khrust Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Gravity Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Front Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Back Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Up Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Down Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Left Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Right Thrust OnOff"] = false;
                }
                else if (CL.Switch("AtomWingsAeroplaneMode"))
                {
                    SCIFM.BooleanValues["Pitch Assigned Gravity OnOff"] = false;
                    SCIFM.BooleanValues["Roll Assigned Gravity OnOff"] = true;
                    SCIFM.BooleanValues["Pitch Assigned Velocity OnOff"] = false;
                    SCIFM.BooleanValues["Roll Assigned Velocity OnOff"] = true;
                    SCIFM.BooleanValues["Pitch Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Roll Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Ion Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Atom Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Hydrogen Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Khrust Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Gravity Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Front Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Back Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Up Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Down Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Left Thrust OnOff"] = false;
                    SCIFM.BooleanValues["Right Thrust OnOff"] = false;
                }
                else if (CL.Switch("SpaceShipMode"))
                {
                    SCIFM.BooleanValues["Pitch Assigned Gravity OnOff"] = true;
                    SCIFM.BooleanValues["Roll Assigned Gravity OnOff"] = true;
                    SCIFM.BooleanValues["Pitch Assigned Velocity OnOff"] = false;
                    SCIFM.BooleanValues["Roll Assigned Velocity OnOff"] = false;
                    SCIFM.BooleanValues["Pitch Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Roll Slope Threshold OnOff"] = true;
                    SCIFM.BooleanValues["Ion Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Atom Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Hydrogen Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Khrust Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Gravity Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Front Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Back Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Up Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Down Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Left Thrust OnOff"] = true;
                    SCIFM.BooleanValues["Right Thrust OnOff"] = true;
                }
                RestorgeCustomData(CL.Switch("SaveData"));
            }
            CL.Clear();
        }
        Echo($"{SCIFM}");
        Echo($"Klang:{SCIFM.BooleanValues["Khrust Thrust OnOff"]}");
        Echo($"Klang C:{khrust_Ms.Count}");
        Echo($"Gravity:{SCIFM.BooleanValues["Gravity Thrust OnOff"]}");
    }
    catch (Exception) { Restart(); }
}
readonly MyIni Configs = new MyIni();
MyCommandLine CL { get; } = new MyCommandLine();
MySCDSP SCIFM { get; } = new MySCDSP();
MyACDCR ACDC { get; } = new MyACDCR();
MyGCtrl GSC { get; } = new MyGCtrl();
MyThCtrl TSC { get; } = new MyThCtrl();
MyVEM VEM { get; } = new MyVEM();
MyGDC GDC { get; } = new MyGDC();
List<Khrust_M> khrust_Ms { get; } = new List<Khrust_M>();
void Restart()
{
    try
    {
        TSC.SCIFM = SCIFM;
        GSC.SCIFM = SCIFM;
        VEM.SCIFM = SCIFM;
        GDC.SCIFM = SCIFM;
        ForceUpdate();
        Khrust_M.CreateKhrust_M(GridTerminalSystem, Me.IsSameConstructAs, khrust_Ms);
        LoadOrAddConfig(Storage);
    }
    catch (Exception) { }
}
void Update() { try { if (!SCIFM.UnabledControl) SCIFM.RefindCockpit(GridTerminalSystem, Me.IsSameConstructAs, StablizeBlockTag); SCIFM.Update(); if (!SCIFM.UnabledControl) GSC.Running(); if (!SCIFM.UnabledControl) TSC.Running(); if (!SCIFM.UnabledControl) VEM.Running(); UpdateKlang(); GDC.Running(SCIFM.BooleanValues["Gravity Thrust OnOff"]); ACDC.Running(); } catch (Exception) { } }
void UpdateKlang()
{
    Khrust_M.Running(khrust_Ms, SCIFM.BooleanValues["Khrust Thrust OnOff"] ? (SCIFM.TCS / 40) : Vector3D.Zero);
}
void LoadOrAddConfig(string Storge)
{
    if (!Configs.TryParse(Storge))
    {
        SCS();
    }
    LCS();
    if (Common.IsNull(Me))
        return;
    Me.CustomData = Configs.ToString();
}
void RestorgeCustomData(bool SaveData = false)
{
    if (Common.IsNull(Me)) return;
    if (SaveData)
    {
        Configs.TryParse(Me.CustomData);
        LCS();
        Storage = Configs.ToString();
    }
    else
    {
        SCS();
        Me.CustomData = Storage = Configs.ToString();
    }
}
void ForceUpdate() { try { SCIFM.ForceUpdate(GridTerminalSystem, Me.IsSameConstructAs, StablizeBlockTag); SCIFM.Update(); if (!SCIFM.UnabledControl) GSC.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); if (!SCIFM.UnabledControl) TSC.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); if (!SCIFM.UnabledControl) VEM.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); ACDC.UpdateBlocks(GridTerminalSystem); GDC.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); } catch (Exception) { } }
void LCS()
{
    if (!Configs.ContainsSection(MySCDSP.section))
        SCS();
    SCIFM.LoadFromIni(Configs);
}
void SCS()
{
    if (!Configs.ContainsSection(MySCDSP.section))
        Configs.AddSection(MySCDSP.section);
    SCIFM.SaveToIni(Configs);
}
sealed class MyAutoPilot
{
    public float AssignedVelocityRate { get; set; } = 0.1f;
    public Base6Directions.Direction Direction { get; set; } = Base6Directions.Direction.Forward;
    public bool EnabledAutoPilot { get; set; } = false;
    public float MaxiumSpeedDistanceRate { get; set; } = 1;
    public float MiniAutoPilotCircleRadius { get; set; } = 100f;
    public float MiniAutoPilotHeight { get; set; } = 200f;
    public float MiniAutoPilotTurnRate { get; set; } = 0.2f;
    public Vector3D? TargetPosition { get; set; } = null;
    internal MySCDSP SCIFM
    {
        get; set;
    }
    public void Update()
    {
        if (SCIFM == null || SCIFM.Me == null)
            return;
    }
    private void Calculate()
    {

    }
}
sealed class MySCDSP
{
    public IMyShipController Me { get; set; } = null;
    public Vector3D? ReferNormal { get; set; } = null;
    public Vector3? MoveIndicator_Override { get; set; } = null;
    public Vector3? RotationIndicator_Override { get; set; } = null;
    public MySCDSP()
    {
        BooleanValues["Enabled"] = true;
        BooleanValues["Cruising Mode"] = false;
        BooleanValues["Calculate All Gravity"] = false;
        BooleanValues["Pitch Assigned Gravity OnOff"] = false;
        BooleanValues["Roll Assigned Gravity OnOff"] = false;
        BooleanValues["Pitch Assigned Velocity OnOff"] = false;
        BooleanValues["Roll Assigned Velocity OnOff"] = false;
        BooleanValues["Pitch Slope Threshold OnOff"] = false;
        BooleanValues["Roll Slope Threshold OnOff"] = false;
        BooleanValues["Ion Thrust OnOff"] = true;
        BooleanValues["Atom Thrust OnOff"] = true;
        BooleanValues["Hydrogen Thrust OnOff"] = true;
        BooleanValues["Khrust Thrust OnOff"] = true;
        BooleanValues["Gravity Thrust OnOff"] = true;
        BooleanValues["Front Thrust OnOff"] = true;
        BooleanValues["Back Thrust OnOff"] = true;
        BooleanValues["Up Thrust OnOff"] = true;
        BooleanValues["Down Thrust OnOff"] = true;
        BooleanValues["Left Thrust OnOff"] = true;
        BooleanValues["Right Thrust OnOff"] = true;
        IntValues["Lock Height Mode"] = 0;
        FloatValues["Pitch Assigned Gravity Rate"] = 1;
        FloatValues["Roll Assigned Gravity Rate"] = 1;
        FloatValues["Pitch Assigned Velocity Rate"] = 1;
        FloatValues["Roll Assigned Velocity Rate"] = 1;
        FloatValues["Angular Dampener Pitch"] = 1;
        FloatValues["Angular Dampener Roll"] = 1;
        FloatValues["Angular Dampener Yaw"] = 1;
        FloatValues["Height Spring K"] = 1;
        FloatValues["Maxium Speed"] = 50;
        FloatValues["Max Angular Velocity"] = 1;
        FloatValues["Velocity Sensitive"] = 1;
        FloatValues["GyroMultipy Pitch"] = 1;
        FloatValues["GyroMultipy Yaw"] = 1;
        FloatValues["GyroMultipy Roll"] = 1;
        FloatValues["Slope Threshold Pitch"] = 10;
        FloatValues["Slope Threshold Roll"] = 30;
        MaxTargetSpeed = 0;
    }
    public void LoadFromIni(MyIni Config)
    {
        try
        {
            BooleanValues.ReadDatas(section, Config);
        }
        catch (Exception) { }
        try
        {
            IntValues.ReadDatas(section, Config);
        }
        catch (Exception) { }
        try
        {
            FloatValues.ReadDatas(section, Config);
        }
        catch (Exception) { }
    }
    public void SaveToIni(MyIni Config)
    {
        try
        {
            BooleanValues.WriteDatas(section, Config);
        }
        catch (Exception) { }
        try
        {
            IntValues.WriteDatas(section, Config);
        }
        catch (Exception) { }
        try
        {
            FloatValues.WriteDatas(section, Config);
        }
        catch (Exception) { }
    }
    public void ForceUpdate(IMyGridTerminalSystem GridTerminalSystem, Func<IMyTerminalBlock, bool> InThisEntity, string StablizeBlockTag = "")
    {
        RefindCockpit(GridTerminalSystem, InThisEntity, StablizeBlockTag);
        Update();
    }
    public void RefindCockpit(IMyGridTerminalSystem GridTerminalSystem, Func<IMyTerminalBlock, bool> InThisEntity, string StablizeBlockTag = "")
    {
        Me = Common.GetT<IMyShipController>(GridTerminalSystem, b => InThisEntity(b) && (b.IsUnderControl));
        if (Me == null)
            Me = Common.GetT<IMyShipController>(GridTerminalSystem, b => InThisEntity(b) && b.IsMainCockpit);
        if (Me == null)
            Me = Common.GetT<IMyShipController>(GridTerminalSystem, b => InThisEntity(b) && b.CustomName.EndsWith(StablizeBlockTag));
        if (Me == null)
            Me = Common.GetT<IMyShipController>(GridTerminalSystem, b => InThisEntity(b));
    }
    public Vector3 GPN4VectorEngine()
    {
        Vector3D ResultLinnerVelocity = LinearVelocity - CMF.VTransLocal2WorldD(WorldMatrix, new Vector3(MI_SC.X, 0, MI_SC.Z)) * MaxiumSpeed;
        return ResultLinnerVelocity + Gravity;
    }
    public Vector3 GPN4VectorEngine_OnlyNormal()
    {
        double MaxLength = Gravity.Length();
        Vector3D ResultLinnerVelocity = LinearVelocity - CMF.VTransLocal2WorldD(WorldMatrix, new Vector3(MI_SC.X, 0, MI_SC.Z)) * MaxiumSpeed;
        return Vector3D.ClampToSphere(ResultLinnerVelocity, MaxLength) + Gravity;
    }
    public override string ToString()
    {
        CI.Clear();
        CI.AppendLine("Ship Controller State");
        CI.AppendLine($"Running:{Enabled}");
        CI.AppendLine($"Cruising Mode:{CruisingMode}");
        return CI.ToString();
    }
    public void Update() { ProcessGyrosCS(); ProcessThrustsCS(); }
    Vector3D GGD() { if (UnabledControl) return Vector3D.Zero; Vector3D PositionP; if (!Me.TryGetPlanetPosition(out PositionP)) return Vector3D.Zero; return CMF.NormalizeD(PositionP - Me.GetPosition()); }
    void ProcessGyrosCS()
    {
        if (UnabledControl || !Enabled)
        {
            GCS = Vector3D.Zero; return;
        }
        Vector3D N;
        if (ReferNormal != null) { N = ReferNormal.Value; }
        else
        {
            var length_g = Gravity.Length() * 0.9;
            var vector_v = Vector3D.ClampToSphere(LinearVelocity, length_g);
            var vector_v_roll = CMF.ProjectOnVectorD(vector_v, WorldMatrix.Right);
            var vector_v_pitch = CMF.ProjectOnVectorD(vector_v, WorldMatrix.Backward);
            var control_signal_roll = (vector_v_roll.Length() > (BooleanValues["Roll Slope Threshold OnOff"] ? FloatValues["Slope Threshold Roll"] : 0)) ? MathHelper.Clamp(1 - (BooleanValues["Roll Assigned Velocity OnOff"] ? FloatValues["Roll Assigned Velocity Rate"] : 0), 0, 1) : 1;
            var control_signal_pitch = (vector_v_pitch.Length() > (BooleanValues["Pitch Slope Threshold OnOff"] ? FloatValues["Slope Threshold Pitch"] : 0)) ? MathHelper.Clamp(1 - (BooleanValues["Pitch Assigned Velocity OnOff"] ? FloatValues["Pitch Assigned Velocity Rate"] : 0), 0, 1) : 1;
            N = vector_v - vector_v_roll * control_signal_roll - vector_v_pitch * control_signal_pitch + Gravity - CMF.ProjectOnVectorD(Gravity, WorldMatrix.Right) * MathHelper.Clamp(1 - (BooleanValues["Roll Assigned Gravity OnOff"] ? FloatValues["Roll Assigned Gravity Rate"] : 0), 0, 1) - CMF.ProjectOnVectorD(Gravity, WorldMatrix.Backward) * MathHelper.Clamp(1 - (BooleanValues["Pitch Assigned Gravity OnOff"] ? FloatValues["Pitch Assigned Gravity Rate"] : 0), 0, 1);
        }
        var non_N = Vector3D.IsZero(N); Vector3D GCSMultipy = GyroMultipy * SLDVM_Gyroscope * 10; if (non_N) { GCS = CMF.VTransLocal2WorldD(WorldMatrix, PDFC(RI_SC * 100) * GCSMultipy); return; }
        Vector3D CSL = RI_SC * 100 + new Vector3(SCPF.ControlWithVectorD(WorldMatrix.Right, N, WorldMatrix.Down), 0, SCPF.ControlWithVectorD(WorldMatrix.Backward, N, WorldMatrix.Down)); var LT = PDFC(CSL) * GCSMultipy; GCS = CMF.VTransLocal2WorldD(WorldMatrix, LT);
    }
    void ProcessThrustsCS() { if (UnabledControl || !Enabled) { TCS = Vector3D.Zero; return; } var MIW = Vector3D.Zero; if (!HandBrake) { MIW = CMF.VTransLocal2WorldD(WorldMatrix, MI_SC) * MaxiumSpeed; if (CruisingMode) { double AccOrDec = MIW.Dot(WorldMatrix.Forward); if (AccOrDec > 0) MaxTargetSpeed = MathHelper.Clamp(Math.Max(LinearVelocity.Dot(WorldMatrix.Forward), 0), 0, MaxiumSpeed); if (AccOrDec < 0) MaxTargetSpeed = 0; MIW = WorldMatrix.Forward * ((AccOrDec > 0) ? MaxiumSpeed : MaxTargetSpeed); } } TCS = CMF.VTransLocal2WorldD(WorldMatrix, CMF.VTransWorld2LocalD(WorldMatrix, MIW - LinearVelocity) * SLDVM_Thruster - CMF.VTransWorld2LocalD(WorldMatrix, CalculateAllGravity ? Me.GetTotalGravity() : Me.GetNaturalGravity()) * (1 + HeightDifferent_Rate * MathHelper.Clamp(FloatValues["Height Spring K"], 0, 20))); }
    Vector3D PDFC(Vector3 RI) { var av = CMF.VTransWorld2Local(WorldMatrix, AngularVelocity) * AngularDampener * 0.2f; av = av + RI; return CMF.SmoothValueD(av); }
    public bool ThrustOnOff(IMyThrust thrust) { if (!EnabledThrusts || Common.IsNull(thrust)) return false; var onoff = (SCPF.IsIonEngine(thrust) && BooleanValues["Ion Thrust OnOff"]) || (thrust.BlockDefinition.SubtypeId.Contains("Atmospheric") && BooleanValues["Atom Thrust OnOff"]) || (thrust.BlockDefinition.SubtypeId.Contains("Hydrogen") && BooleanValues["Hydrogen Thrust OnOff"]); if (onoff == false) return false; if (thrust.CubeGrid != Me.CubeGrid) return onoff; if (Base6Directions.GetOppositeDirection(Me.Orientation.Forward) == thrust.Orientation.Forward) return BooleanValues["Front Thrust OnOff"] && onoff; if (Base6Directions.GetOppositeDirection(Me.Orientation.Up) == thrust.Orientation.Forward) return BooleanValues["Up Thrust OnOff"] && onoff; if (Base6Directions.GetOppositeDirection(Me.Orientation.Left) == thrust.Orientation.Forward) return BooleanValues["Left Thrust OnOff"] && onoff; if (Me.Orientation.Forward == thrust.Orientation.Forward) return BooleanValues["Back Thrust OnOff"] && onoff; if (Me.Orientation.Up == thrust.Orientation.Forward) return BooleanValues["Down Thrust OnOff"] && onoff; if (Me.Orientation.Left == thrust.Orientation.Forward) return BooleanValues["Right Thrust OnOff"] && onoff; return onoff; }
    #region 属性
    public bool EnabledRotorEngine { get; set; } = false;
    public Vector3D GCS { get; private set; } = Vector3D.Zero;
    public Vector3D TCS { get; private set; } = Vector3D.Zero;
    public bool Enabled => BooleanValues["Enabled"];
    bool CruisingMode => BooleanValues["Cruising Mode"];
    bool CalculateAllGravity => BooleanValues["Calculate All Gravity"];
    float MaxTargetSpeed { get { return FloatValues["Max Target Speed"]; } set { FloatValues["Max Target Speed"] = value; } }
    float MaxiumSpeed => FloatValues["Maxium Speed"];
    Vector3 AngularDampener => new Vector3(MathHelper.Clamp(FloatValues["Angular Dampener Pitch"], 0, 100), MathHelper.Clamp(FloatValues["Angular Dampener Yaw"], 0, 100), MathHelper.Clamp(FloatValues["Angular Dampener Roll"], 0, 100));
    Vector3 GyroMultipy => new Vector3(FloatValues["GyroMultipy Pitch"], FloatValues["GyroMultipy Yaw"], FloatValues["GyroMultipy Roll"]);
    public MatrixD WorldMatrix => Me?.WorldMatrix ?? MatrixD.Identity;
    Vector3 MI_SC => (MoveIndicator_Override ?? Me?.MoveIndicator) ?? Vector3.Zero;
    Vector3 RI_SC => RotationIndicator_Override ?? new Vector3(Me?.RotationIndicator ?? Vector2.Zero, Me?.RollIndicator ?? 0);
    float SLDVM_Thruster => (Common.IsNull(Me?.CubeGrid) ? 5f : ((Me.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large) ? 20 : 4)) * FloatValues["Velocity Sensitive"];
    float SLDVM_Gyroscope => (Common.IsNull(Me?.CubeGrid) ? 5f : ((Me.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large) ? 20 : 4)) * FloatValues["Max Angular Velocity"];
    Vector3 LinearVelocity => Me?.GetShipVelocities().LinearVelocity ?? Vector3.Zero;
    public bool UnabledControl => Common.IsNull(Me?.CubeGrid);
    public bool EnabledGyros => Me?.GetValueBool("ControlGyros") ?? true;
    public bool EnabledThrusts => Me?.ControlThrusters ?? true;
    Vector3 Gravity => (CalculateAllGravity ? Me?.GetTotalGravity() : Me?.GetNaturalGravity()) ?? (GravityDirection * 10);
    Vector3D GravityDirection => GGD();
    bool HandBrake => Me?.HandBrake ?? true;
    public float ShipMass => Me?.CalculateShipMass().TotalMass ?? 0;
    Vector3 AngularVelocity => Me?.GetShipVelocities().AngularVelocity ?? Vector3.Zero;
    double HeightDifferent_Rate { get { if (Me == null) return 0; double Height_Current; switch (IntValues["Lock Height Mode"]) { case 1: if (Me.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out Height_Current)) return MathHelper.Clamp(Sealevel_Target - Height_Current, -10, 10); else { Me.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out Sealevel_Target); return 0; } case 2: if (Me.TryGetPlanetElevation(MyPlanetElevation.Surface, out Height_Current)) return MathHelper.Clamp(Surface_Target - Height_Current, -10, 10); else { Me.TryGetPlanetElevation(MyPlanetElevation.Surface, out Surface_Target); return 0; } default: Me.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out Sealevel_Target); Me.TryGetPlanetElevation(MyPlanetElevation.Surface, out Surface_Target); return 0; } } }
    StringBuilder CI { get; } = new StringBuilder();
    #region 变量存储空间
    public MyValueDictionary_Float FloatValues { get; } = new MyValueDictionary_Float();
    public MyValueDictionary_Boolean BooleanValues { get; } = new MyValueDictionary_Boolean();
    public MyValueDictionary_Int IntValues { get; } = new MyValueDictionary_Int();
    #endregion
    #endregion
    #region 成员变量
    public const string section = "Ship Controller DSP Parameter";
    double Sealevel_Target = float.MaxValue;
    double Surface_Target = float.MaxValue;
    #endregion
}
public class Khrust_M { public static void CreateKhrust_M(IMyGridTerminalSystem GridTerminalSystem, Func<IMyTerminalBlock, bool> ITE, List<Khrust_M> KMS) { List<IMyMotorStator> Motors = Common.GetTs<IMyMotorStator>(GridTerminalSystem, ITE); if (Motors == null || KMS == null) return; KMS.Clear(); foreach (var Motor in Motors) { CreateKhrust_M_P(Motor, KMS); } } private static void CreateKhrust_M_P(IMyMotorStator Motor, List<Khrust_M> KMS) { if (Motor == null || KMS == null) return; IMyMotorStator K_Base = Motor; var BG = K_Base.CubeGrid; if (BG == null) return; var size = (BG.GridSizeEnum == VRage.Game.MyCubeSize.Large ? 2.5 : 0.5); IMyDoor K_Door = null; for (int index = 1; index < 6; index++) { var position = BG.WorldToGridInteger(K_Base.WorldMatrix.Translation + K_Base.WorldMatrix.Up * size * index); if (!BG.CubeExists(position)) continue; K_Door = BG.GetCubeBlock(position).FatBlock as IMyDoor; if (K_Door != null) break; } if (K_Door == null) return; KMS.Add(new Khrust_M(K_Base, K_Door)); } public static void Running(List<Khrust_M> khrusts, Vector3D Force) { if (Common.IsNullC(khrusts)) return; foreach (var khrust in khrusts) khrust.RF(Force); } private Khrust_M(IMyMotorStator K_Base, IMyDoor K_Door) { this.K_Base = K_Base; this.K_Door = K_Door; if (this.K_Base != null) { if (!this.K_Base.CustomName.Contains(ClangEngineTag)) this.K_Base.CustomName += $" ({ClangEngineTag})"; this.K_Base.TargetVelocityRad = 0; this.K_Base.ShowInTerminal = false; this.K_Base.ShowOnHUD = false; this.K_Base.ShowInToolbarConfig = false; } if (this.K_Door != null) { if (!this.K_Door.CustomName.Contains(ClangEngineTag)) this.K_Door.CustomName += $" ({ClangEngineTag})"; this.K_Door.ShowInTerminal = false; this.K_Door.ShowOnHUD = false; this.K_Door.ShowInToolbarConfig = false; } } private void RF(Vector3D Force) { if (Common.IsNull(K_Base)) return; if (Vector3D.IsZero(Force)) K_Base.Displacement = P0P; else K_Base.Displacement = (float)MathHelper.Clamp(Force.Dot(K_Base.WorldMatrix.Down), P0P, P100P); if (Common.IsNull(K_Door)) return; if (K_Door.Enabled) K_Door.Enabled = false; } IMyMotorStator K_Base; IMyDoor K_Door; float P0P => K_Base.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? -0.4f : -0.02f; float P100P => K_Base.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? -0.02f : 0.11f; }
sealed class MyGDC { internal MySCDSP SCIFM { get; set; } IMyBlockGroup GDs; List<IMyGravityGenerator> DS; List<IMyArtificialMassBlock> MP; public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { GDs = GTS.GetBlockGroupWithName(GravityDriveNM); DS = Common.GetTs<IMyGravityGenerator>(GDs, ITE); MP = Common.GetTs<IMyArtificialMassBlock>(GDs, ITE); if (!Common.IsNullC(DS)) foreach (var d in DS) { d.ShowInTerminal = false; d.ShowInToolbarConfig = false; } if (!Common.IsNullC(MP)) foreach (var mp in MP) { mp.ShowInTerminal = false; mp.ShowInToolbarConfig = false; } } public void Running(bool EnabledKLang) { if (Common.IsNullC(DS) || Common.IsNullC(MP)) return; if (!EnabledKLang) { foreach (var Driver in DS) { if (Driver == null) continue; if (Driver.Enabled) Driver.Enabled = false; } foreach (var MassProvider in MP) { if (MassProvider == null) continue; if (MassProvider.Enabled) MassProvider.Enabled = false; } return; } foreach (var Driver in DS) { if (Driver == null) continue; if (!Driver.Enabled) Driver.Enabled = true; Driver.GravityAcceleration = (float)SCIFM.TCS.Dot(Driver.WorldMatrix.Down) * 20; } foreach (var MassProvider in MP) { if (MassProvider == null) continue; if (!MassProvider.Enabled) MassProvider.Enabled = true; } } }
sealed class MyVEM { List<IMyMotorStator> Motors; internal MySCDSP SCIFM { get; set; } public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { if (GTS == null || ITE == null) return; Motors = Common.GetTs<IMyMotorStator>(GTS, b => ITE(b) && b.CustomName.Contains("Engine")); } public void Running() { if (SCIFM == null) return; if (Common.IsNullC(Motors) || SCIFM.UnabledControl) { SCIFM.EnabledRotorEngine = false; return; } else SCIFM.EnabledRotorEngine = true; var direction = SCIFM.GPN4VectorEngine(); var direction_onlynormal = SCIFM.GPN4VectorEngine_OnlyNormal(); foreach (var Motor in Motors) { if (Motor == null || Motor.Top == null) continue; var vector = Motor.Top.WorldMatrix.Forward; if (Motor.CustomName.Contains("Backward")) vector = Motor.Top.WorldMatrix.Backward; else if (Motor.CustomName.Contains("Backward")) vector = Motor.Top.WorldMatrix.Backward; else if (Motor.CustomName.Contains("Up")) vector = Motor.Top.WorldMatrix.Up; else if (Motor.CustomName.Contains("Down")) vector = Motor.Top.WorldMatrix.Down; else if (Motor.CustomName.Contains("Left")) vector = Motor.Top.WorldMatrix.Left; else if (Motor.CustomName.Contains("Right")) vector = Motor.Top.WorldMatrix.Right; else if (Motor.CustomName.Contains("Forward")) vector = Motor.Top.WorldMatrix.Forward; float value; if ((Vector3.IsZero(direction) && !Motor.CustomName.Contains("Normal")) || (Vector3.IsZero(direction_onlynormal) && Motor.CustomName.Contains("Normal"))) value = -Motor.Angle; else value = (float)SCPF.ControlWithVector(Motor.WorldMatrix.Up, Motor.CustomName.Contains("Normal") ? direction_onlynormal : direction, -vector) * 20; if (float.IsNaN(value)) value = 0; Motor.TargetVelocityRad = CMF.SmoothValue(value); } } }
sealed class MyThCtrl { MyTuple<IMyThrust, double>[] T; public List<string> ENCT { get; } = new List<string>(); internal MySCDSP SCIFM { get; set; } bool NullThrust => Common.IsNullC(T); public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { if (Common.IsNull(SCIFM.Me) || ITE == null) return; T = Common.GetTs(GTS, (IMyThrust thrust) => Common.ExceptKeywords(thrust) && ITE(thrust)).ConvertAll(t => new MyTuple<IMyThrust, double>(t, 1)).ToArray(); foreach (var t in T) { t.Item1.ShowInToolbarConfig = false; t.Item1.ShowInTerminal = false; } } public void Running() { if (NullThrust) return; if (Common.IsNull(SCIFM.Me) || SCIFM.UnabledControl) { foreach (var thrust in T) { if (Common.NullEntity(thrust.Item1)) continue; if (thrust.Item1.ThrustOverridePercentage != 0) thrust.Item1.ThrustOverridePercentage = 0; if (!thrust.Item1.Enabled) thrust.Item1.Enabled = true; } return; } if (!SCIFM.EnabledThrusts) { foreach (var thrust in T) { if (thrust.Item1.Enabled) thrust.Item1.Enabled = false; if (thrust.Item1.ThrustOverridePercentage != 0) thrust.Item1.ThrustOverridePercentage = 0; } return; } Vector3D TotalThrust_Needed_Velocity = SCIFM.TCS * SCIFM.ShipMass; Vector3D V_Velocity = CMF.NormalizeD(TotalThrust_Needed_Velocity); double length_Avaliable_Velocity = 0; for (int index = 0; index < T.Length; index++) { if (Common.NullEntity(T[index].Item1) || !T[index].Item1.IsFunctional) continue; T[index].Item2 = Math.Max(0, T[index].Item1.WorldMatrix.Backward.Dot(V_Velocity)); length_Avaliable_Velocity += T[index].Item2 * T[index].Item1.MaxEffectiveThrust; } double p_v = 0; double length_Needed_Velocity = TotalThrust_Needed_Velocity.Length(); if (length_Avaliable_Velocity != 0) p_v = length_Needed_Velocity / length_Avaliable_Velocity; if (p_v == 0) { foreach (var thrust in T) { thrust.Item1.ThrustOverridePercentage = 0; return; } } foreach (var thrust in T) { if (Common.NullEntity(thrust.Item1) || !thrust.Item1.IsFunctional) continue; thrust.Item1.ThrustOverridePercentage = (float)(MathHelper.Clamp(thrust.Item2 * p_v, Math.Max(1 / thrust.Item1.MaxEffectiveThrust, 1e-8f), 1)); } foreach (var thrust in T) { if (Common.NullEntity(thrust.Item1)) continue; bool thrustenabled = SCIFM.ThrustOnOff(thrust.Item1); if (thrust.Item1.Enabled != thrustenabled) thrust.Item1.Enabled = thrustenabled; if (!thrust.Item1.IsWorking) { thrust.Item1.ThrustOverridePercentage = 0; continue; } } } }
sealed class MyGCtrl { List<IMyGyro> G; internal MySCDSP SCIFM { get; set; } public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { if (Common.IsNull(SCIFM.Me) || ITE == null) return; G = Common.GetTs(GTS, (IMyGyro gyro) => Common.ExceptKeywords(gyro) && NC(gyro) && ITE(gyro)); foreach (var gyro in G) { if (gyro.Enabled != SCIFM.EnabledGyros) gyro.Enabled = SCIFM.EnabledGyros; gyro.ShowInTerminal = false; gyro.ShowInToolbarConfig = false; } } public void Running() { if (Common.IsNullC(G)) return; if (SCIFM.Enabled) { foreach (var gyro in G) if (gyro.Enabled != SCIFM.EnabledGyros) gyro.Enabled = SCIFM.EnabledGyros; if (!SCIFM.EnabledGyros) return; foreach (var gyro in G) { if (!gyro.GyroOverride) gyro.GyroOverride = true; var result = CMF.VTransWorld2LocalD(gyro.WorldMatrix, SCIFM.GCS); gyro.Roll = (float)result.Z; gyro.Yaw = (float)result.Y; gyro.Pitch = (float)result.X; } } else { foreach (var gyro in G) { if (gyro == null) continue; if (!gyro.Enabled) gyro.Enabled = true; gyro.Roll = gyro.Yaw = gyro.Pitch = 0; gyro.GyroOverride = false; } return; } } static bool NC(IMyGyro Gyro) { if (Gyro.CustomName.Contains("Klang") || Gyro.CustomName.Contains("Clang")) return false; return true; } }
sealed class MyACDCR { List<MyACDT> TS { get; } = new List<MyACDT>(); public void Running() { foreach (var Timer in TS) { Timer.Running(); } } public void UpdateBlocks(IMyGridTerminalSystem GTS) { var dsg = GTS.GetBlockGroupWithName(ACDoorsGroupNM); if (dsg == null) return; var ds = Common.GetTs<IMyDoor>(dsg); foreach (var d in ds) { TS.Add(new MyACDT(d)); } } }
sealed class MyACDT { const int g = 25; readonly IMyDoor d; int c; public MyACDT(IMyDoor Door) { d = Door; } public void Running() { if (d == null) return; switch (d.Status) { case DoorStatus.Opening: c = g; return; case DoorStatus.Open: if (c > 0) c--; else d.CloseDoor(); return; default: break; } } }
static class Common { static readonly string[] BlackList = new string[] { "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", "Clang" }; public static bool BlockInTurretGroup(IMyBlockGroup group, IMyTerminalBlock Me) { List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); group?.GetBlocks(blocks); if (blocks.Count < 1 || !blocks.Contains(Me)) return false; return true; } public static bool ExceptKeywords(IMyTerminalBlock block) { foreach (var item in BlackList) { if (block.BlockDefinition.SubtypeId.Contains(item) || block.CustomName.Contains("Clang")) return false; } return true; } public static IMyTerminalBlock GetBlock(IMyGridTerminalSystem gridTerminalSystem, long EntIds = 0) => gridTerminalSystem?.GetBlockWithId(EntIds) as IMyTerminalBlock; public static List<IMyTerminalBlock> GetBlocks(IMyGridTerminalSystem gridTerminalSystem, List<long> EntIds = null) { if (gridTerminalSystem == null) return null; return EntIds?.ConvertAll(id => gridTerminalSystem.GetBlockWithId(id) as IMyTerminalBlock); } public static T GetT<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = GetTs(gridTerminalSystem, requst); if (IsNullC(Items)) return null; else return Items.First(); } public static T GetT<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class => GetTs(blockGroup, requst).FirstOrDefault(); public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (gridTerminalSystem == null) return Items; gridTerminalSystem.GetBlocksOfType(Items, requst); return Items; } public static List<T> GetTs<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (blockGroup == null) return Items; blockGroup.GetBlocksOfType(Items, requst); return Items; } public static Matrix GetWorldMatrix(IMyTerminalBlock ShipController) { Matrix me_matrix; ShipController.Orientation.GetMatrix(out me_matrix); return me_matrix; } public static IMyCameraBlock ID2Camera(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyCameraBlock; public static IMyMotorStator ID2Motor(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyMotorStator; public static IMyTerminalBlock ID2Weapon(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyTerminalBlock; public static bool IsNull(Vector3? Value) => Value == null || Value.Value == Vector3.Zero; public static bool IsNull(Vector3D? Value) => Value == null || Value.Value == Vector3D.Zero; public static bool IsNull<T>(T Value) where T : class => Value == null; public static bool IsNullC<T>(ICollection<T> Value) => (Value?.Count ?? 0) < 1; public static bool IsNullC<T>(IEnumerable<T> Value) => (Value?.Count() ?? 0) < 1; public static bool NullEntity<T>(T Ent) where T : IMyEntity => Ent == null; public static float SetInRange_AngularDampeners(float data) => MathHelper.Clamp(data, 0.01f, 20f); public static List<string> SpliteByQ(string context) => context?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? (new List<string>()); }
static class CMF { public static double SetNaNZero(double Value) => double.IsNaN(Value) ? 0 : Value; public static double AngleBetweenD(Vector3D a, Vector3D b) { var dotProd = Vector3D.Dot(a, b); var lenProd = a.Length() * b.Length(); return Math.Acos(dotProd / lenProd); } public static Vector3D CalculateAvg(ICollection<Vector3D> Vectors) { if (Common.IsNullC(Vectors)) return Vector3D.Zero; Vector3D Value = Vector3D.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value /= Vectors.Count; } public static Vector3 CalculateAvg(ICollection<Vector3> Vectors) { if (Common.IsNullC(Vectors)) return Vector3.Zero; Vector3 Value = Vector3.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value /= Vectors.Count; } public static Vector3D CalculateSum(ICollection<Vector3D> Vectors) { if (Common.IsNullC(Vectors)) return Vector3D.Zero; Vector3D Value = Vector3D.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value; } public static Vector3 CalculateSum(ICollection<Vector3> Vectors) { if (Common.IsNullC(Vectors)) return Vector3.Zero; Vector3 Value = Vector3.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value; } public static Vector3 Normalize(Vector3 vector) { if (Vector3.IsZero(vector)) return Vector3.Zero; return Vector3.Normalize(vector); } public static Vector3 Normalize(Vector3 vector, float Epsional, float Multipy) { return Normalize(vector * (Vector3.One - Vector3.IsZeroVector(vector, Epsional))) * Multipy; } public static Vector3D NormalizeD(Vector3D vector) { if (Vector3D.IsZero(vector)) return Vector3D.Zero; return Vector3D.Normalize(vector); } public static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal, float Epsilon = 0) { if (Epsilon == 0) return Vector3.ProjectOnPlane(ref direction, ref planeNormal); var result = Vector3.ProjectOnPlane(ref direction, ref planeNormal); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static Vector3D ProjectOnPlaneD(Vector3D direction, Vector3D planeNormal, double Epsilon = 0) { if (Epsilon == 0) return Vector3D.ProjectOnPlane(ref direction, ref planeNormal); var result = Vector3D.ProjectOnPlane(ref direction, ref planeNormal); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static Vector3 ProjectOnVector(Vector3 vector, Vector3 direction, double Epsilon = 0) { if (Epsilon == 0) return Vector3.ProjectOnVector(ref vector, ref direction); var result = Vector3.ProjectOnVector(ref vector, ref direction); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static Vector3D ProjectOnVectorD(Vector3D vector, Vector3D direction, double Epsilon = 0) { if (Epsilon == 0) return Vector3D.ProjectOnVector(ref vector, ref direction); var result = Vector3D.ProjectOnVector(ref vector, ref direction); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static float SignedAngle(Vector3 A, Vector3 B, Vector3 Axis) { return MyMath.AngleBetween(A, B) * SignNonZero(A.Cross(B).Dot(Axis)); } public static double SignedAngleD(Vector3D A, Vector3D B, Vector3D Axis) { if (Vector3.IsZero(A) || Vector3.IsZero(A)) return 1; return AngleBetweenD(A, B) * SignNonZero(A.Cross(B).Dot(Axis)); } public static int SignNonZero(double Value) => (Value >= 0) ? 1 : -1; public static float SmoothValue(float value) { return value * Math.Abs(value); } public static double SmoothValueD(double value) { return value * Math.Abs(value); } public static Vector3 SmoothValue(Vector3 value) { return value * Vector3.Abs(value); } public static Vector3D SmoothValueD(Vector3D value) { return value * Vector3D.Abs(value); } public static Vector3D VTransWorld2LocalD(MatrixD Parent_World, Vector3D Vector_World) => Vector3D.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World)); public static Vector3D VTransLocal2WorldD(MatrixD Parent_World, Vector3D Vector_Local) => Vector3D.TransformNormal(Vector_Local, Parent_World); public static Vector3 VTransWorld2Local(MatrixD Parent_World, Vector3 Vector_World) => Vector3.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World)); public static Vector3 VTransLocal2World(MatrixD Parent_World, Vector3 Vector_Local) => Vector3.TransformNormal(Vector_Local, Parent_World); }
static class SCPF { public static bool IsIonEngine(IMyThrust thrust) { if (thrust == null) return false; return IonThrustList.Contains(thrust.BlockDefinition.SubtypeId) || thrust.BlockDefinition.SubtypeId.Contains("Ion") || thrust.BlockDefinition.SubtypeId.Contains("ion"); } public static float ControlWithVector(Vector3 Axe, Vector3 Target, Vector3 Current, float Epsilon = 0) => CMF.SignedAngle(CMF.ProjectOnPlane(Target, Axe, Epsilon), Current, Axe); public static double ControlWithVectorD(Vector3D Axe, Vector3D Target, Vector3D Current, double Epsilon = 0) => CMF.SetNaNZero(CMF.SignedAngleD(CMF.ProjectOnPlaneD(Target, Axe, Epsilon), Current, Axe)); public static float ControlWithVectorLinear(Vector3 Axe, Vector3 Target, Vector3 Current, float Epsilon = 0) => CMF.SignedAngle(CMF.ProjectOnPlane(Target, Axe, Epsilon), Current, Axe); public static double ControlWithVectorLinearD(Vector3D Axe, Vector3D Target, Vector3D Current, double Epsilon = 0) => CMF.SetNaNZero(CMF.SignedAngleD(CMF.ProjectOnPlaneD(Target, Axe, Epsilon), Current, Axe)); public static Vector3 Dampener_Mutlipy_Vector(Vector3 Local_ReferNormal, Vector3 Normal) { Vector3 Local_Diff = Normal - Local_ReferNormal; Vector3 Dampener_Temp = new Vector3(Local_Diff.Y, Local_Diff.Z, Local_Diff.X); return Dampener_Temp * Dampener_Temp; } public static double GetSpring(double x, double k = 1) => k * x * Math.Abs(x); public static Vector3D GetSuspensionAntiRoll(ICollection<IMyMotorSuspension> Wheels) { if (Common.IsNullC(Wheels)) return Vector3D.Zero; Vector3D Center = CMF.CalculateAvg(Wheels.Where(b => b.Top != null)?.ToList()?.ConvertAll(b => b.GetPosition())); return CMF.CalculateSum(Wheels.Where(b => b.Top != null)?.ToList()?.ConvertAll(b => { var FL = CMF.ProjectOnVectorD(b.Top.GetPosition() - b.GetPosition(), b.WorldMatrix.Backward); var Arm = b.GetPosition() - Center; return FL.Cross(Arm); })); } static List<string> IonThrustList = new List<string>() { "SmallBlockSmallThrust", "SmallBlockLargeThrust", "LargeBlockSmallThrust", "LargeBlockLargeThrust", "SmallBlockSmallThrustSciFi", "SmallBlockLargeThrustSciFi", "LargeBlockSmallThrustSciFi", "LargeBlockLargeThrustSciFi" }; }
const string ACDoorsGroupNM = @"ACDoors"; const string GravityDriveNM = @"GravityDrive"; const string ClangEngineTag = "Clang"; const string T_OnOffTag = @"ThrusterOnOff"; public const string StablizeBlockTag = "TestDistance"; public static float PitchLimitedValue = (float)Math.Sin(MathHelper.ToRadians(87)); public static float RollLimitedValue = (float)Math.Sin(MathHelper.ToRadians(50));
abstract class MyValueDictionary<T> where T : struct, IComparable, IComparable<T>, IEquatable<T> { public T this[string key] { get { if (!Values.ContainsKey(key)) Values.Add(key, new ValueStorgy<T>()); return Values[key].GetValue(); } set { if (!Values.ContainsKey(key)) Values.Add(key, new ValueStorgy<T>()); Values[key].SetValue(value); } } protected Dictionary<string, ValueStorgy<T>> Values { get; } = new Dictionary<string, ValueStorgy<T>>(); public void InitTable(params string[] args) { if (args == null || args.Length < 1) return; foreach (var arg in args) { if (!Values.ContainsKey(arg)) Values.Add(arg, new ValueStorgy<T>()); } } public abstract void ReadDatas(string section, MyIni Config); public abstract void WriteDatas(string section, MyIni Config); }
sealed class ValueStorgy<T> where T : struct, IComparable, IComparable<T>, IEquatable<T> { T value = default(T); public T GetValue() => value; public void SetValue(T value) => this.value = value; public override int GetHashCode() => value.GetHashCode(); public override string ToString() => value.ToString(); }
sealed class MyValueDictionary_Int : MyValueDictionary<int> { public override void ReadDatas(string section, MyIni Config) { if (Config == null) return; if (!Config.ContainsSection(section)) return; foreach (var Value in Values) { var value = Config.Get(section, Value.Key); Value.Value.SetValue(value.ToInt32()); } } public override void WriteDatas(string section, MyIni Config) { if (Config == null) return; if (!Config.ContainsSection(section)) Config.AddSection(section); foreach (var Value in Values) Config.Set(section, Value.Key, Value.Value.GetValue()); } }
sealed class MyValueDictionary_Float : MyValueDictionary<float> { public override void ReadDatas(string section, MyIni Config) { if (Config == null) return; if (!Config.ContainsSection(section)) return; foreach (var Value in Values) { var value = Config.Get(section, Value.Key); Value.Value.SetValue(value.ToSingle()); } } public override void WriteDatas(string section, MyIni Config) { if (Config == null) return; if (!Config.ContainsSection(section)) Config.AddSection(section); foreach (var Value in Values) Config.Set(section, Value.Key, Value.Value.GetValue()); } }
sealed class MyValueDictionary_Boolean : MyValueDictionary<bool> { public override void ReadDatas(string section, MyIni Config) { if (Config == null) return; if (!Config.ContainsSection(section)) return; foreach (var Value in Values) { var value = Config.Get(section, Value.Key); Value.Value.SetValue(value.ToBoolean()); } } public override void WriteDatas(string section, MyIni Config) { if (Config == null) return; if (!Config.ContainsSection(section)) Config.AddSection(section); foreach (var Value in Values) Config.Set(section, Value.Key, Value.Value.GetValue()); } }
