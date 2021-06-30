public Program()
{
    Restart();
    if (GridTerminalSystem.GetBlockWithId(ThrustGyroControl_Address) == null)
        ThrustGyroControl_Address = Common.GetT<IMyProgrammableBlock>(GridTerminalSystem, b => Me.CubeGrid == b.CubeGrid && b.CustomName.EndsWith("[UC_TGC]"))?.EntityId ?? -1;
    if (GridTerminalSystem.GetBlockWithId(WheelControl_Address) == null)
        WheelControl_Address = Common.GetT<IMyProgrammableBlock>(GridTerminalSystem, b => Me.CubeGrid == b.CubeGrid && b.CustomName.EndsWith("[UC_WCC]"))?.EntityId ?? -1;
    Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10 | UpdateFrequency.Update100;
}
public void Main(string argument, UpdateType updateSource)
{
    try
    {
        switch (updateSource)
        {
            case UpdateType.Terminal:
            case UpdateType.Trigger:
            case UpdateType.Mod:
            case UpdateType.Script:
            case UpdateType.IGC:
                if (CommandLine.TryParse(argument))
                {
                    if (CommandLine.Switch("ControlLine"))
                    {
                        CommandLine.Clear();
                        SwitchCommonds(argument);
                        RestorgeCustomDataDirect();
                    }
                    else if (CommandLine.Switch("SaveData"))
                    {
                        CommandLine.Clear();
                        Storage = RestorgeCustomData();
                        if (IGC.IsEndpointReachable(WheelControl_Address, TransmissionDistance.ConnectedConstructs))
                            IGC.SendUnicastMessage(WheelControl_Address, W_SetupTag, PackageWheelConfig());
                    }
                    else if (CommandLine.Switch("ChangeRole"))
                    {
                        CommandLine.Clear();
                        EditRole(argument);
                        RestorgeCustomDataDirect();
                    }
                    ShowInCustomData();
                }
                break;
            case UpdateType.Update1:
                Update();
                if (IGC.IsEndpointReachable(ThrustGyroControl_Address, TransmissionDistance.ConnectedConstructs))
                    IGC.SendUnicastMessage(ThrustGyroControl_Address, ThrustGyroscopeTag, PackageThrustGyroscopeSignal());
                if (IGC.IsEndpointReachable(WheelControl_Address, TransmissionDistance.ConnectedConstructs))
                    IGC.SendUnicastMessage(WheelControl_Address, WheelCtrlTag, PackageWheelControlLines());
                //IGC.SendBroadcastMessage(ThrustGyroscopeTag, PackageThrustGyroscopeSignal(), TransmissionDistance.CurrentConstruct);
                //IGC.SendBroadcastMessage(WheelCtrlTag, PackageWheelControlLines(), TransmissionDistance.CurrentConstruct);
                break;
            case UpdateType.Update10:
                break;
            case UpdateType.Update100:
                if (GridTerminalSystem.GetBlockWithId(ThrustGyroControl_Address) == null)
                    ThrustGyroControl_Address = Common.GetT<IMyProgrammableBlock>(GridTerminalSystem, b => Me.CubeGrid == b.CubeGrid && b.CustomName.EndsWith("[UC_TGC]"))?.EntityId ?? -1;
                if (GridTerminalSystem.GetBlockWithId(WheelControl_Address) == null)
                    WheelControl_Address = Common.GetT<IMyProgrammableBlock>(GridTerminalSystem, b => Me.CubeGrid == b.CubeGrid && b.CustomName.EndsWith("[UC_WCC]"))?.EntityId ?? -1;
                break;
            case UpdateType.Once:
                break;
            default:
                break;
        }
        //Echo($"{IGC.Me.Equals(Me.EntityId)}");
        Echo($"{ShipControllerDSP.ToString()}");
        Echo($"{ShipControllerDSP.SLM.ToString()}");
        Echo($"Klang:{EnabledKLang}");
        Echo($"Gravity:{EnabledGravity}");
    }
    catch (Exception) { Restart(); }
}
void Restart()
{
    Restart(Me, GridTerminalSystem);
    LoadOrAddConfig(Storage);
    listener = IGC.RegisterBroadcastListener(ListenerTag);
    listener.SetMessageCallback(callback_Tag);
    ShowInCustomData();
}
IMyBroadcastListener listener;

long ThrustGyroControl_Address = -1;
long WheelControl_Address = -1;

#region 配置文件和命令行
readonly MyCommandLine CommandLine = new MyCommandLine();
readonly MyIni Configs = new MyIni();
#endregion
#region 飞控信号处理单元
readonly MyShipControllerDSP ShipControllerDSP = new MyShipControllerDSP();
bool DockMode = false;
Vector3 GyroMultipy = Vector3.One;
MyTuple<MyTuple<bool, MatrixD, bool>, MyTuple<bool, Vector3D, float, bool>, MyTuple<bool, Vector3D, Vector3>, ImmutableArray<string>, MyTuple<Vector3D, Vector3D>> PackageThrustGyroscopeSignal()
{
    return new MyTuple<MyTuple<bool, MatrixD, bool>, MyTuple<bool, Vector3D, float, bool>, MyTuple<bool, Vector3D, Vector3>, ImmutableArray<string>, MyTuple<Vector3D, Vector3D>>
    (new MyTuple<bool, MatrixD, bool>(!ShipControllerDSP.UnabledControl, ShipControllerDSP.WorldMatrix, ShipControllerDSP.HoverMode),
    new MyTuple<bool, Vector3D, float, bool>(ShipControllerDSP.EnabledThrusts, ShipControllerDSP.TCS, ShipControllerDSP.ShipMass, ShipControllerDSP.EnabledAllDirectionThruster),
    new MyTuple<bool, Vector3D, Vector3>(ShipControllerDSP.EnabledGyros, ShipControllerDSP.GCS, GyroMultipy),
    ImmutableArray.Create(EngineNot2Control.ToArray()), new MyTuple<Vector3D, Vector3D>(ShipControllerDSP.GPN4VectorEngine(), ShipControllerDSP.GPN4VectorEngine_OnlyNormal()));
}
MyTuple<ImmutableArray<bool>, int, MatrixD, float, ImmutableArray<Vector3>> PackageWheelControlLines()
{
    return new MyTuple<ImmutableArray<bool>, int, MatrixD, float, ImmutableArray<Vector3>>(
        ImmutableArray.Create(!ShipControllerDSP.UnabledControl, ShipControllerDSP.HandBrake, ShipControllerDSP.HasWings, ShipControllerDSP.EnabledWheels),
        (int)ShipControllerDSP.Role, ShipControllerDSP.WorldMatrix, ShipControllerDSP.MSL,
        ImmutableArray.Create(new Vector3[] { ShipControllerDSP.LinearVelocity, ShipControllerDSP.MoveIndicator, ShipControllerDSP.RotationIndicator, ShipControllerDSP.RI_SC, ShipControllerDSP.Center })
        );
}
#endregion
#region 速度查找表（规定不同模式下不同的移速）
float MaxiumSpeed_Dock = 5f;
float MaxiumSpeed_Flight = 120;
float MaxiumSpeed_Hover = 30;
float MaxiumSpeed_Land = 50f;
float MaxiumSpeed_Sea = 30f;
float MaxiumSpeed_Space = 1000f;
#endregion
#region 引擎的控制（不同种类引擎开关）
bool AtomThrust_OnOff = true;
bool HydgenThrust_OnOff = true;
bool IonThrust_OnOff = true;
bool EnabledKLang = false;
bool EnabledGravity = false;
readonly List<string> EngineNot2Control = new List<string>();
#endregion
#region 轮子辅助控制
bool DisabledAssist;
bool DisabledSpinTurn;
int CurrentGearMode;
float FrictionBalancePercentage;
float PowerBalancePercentage;
float WRotorMaxiumRpm;
float MaxTurnAngle;
float MinTurnAngle;
float PowerMult;
float TurnSlippingMult;
float Friction;
float SuspensionHeight;
float DiffRpmPercentage;
float FrontAngleRate;
float RearWheelTurnRate;
MyTuple<bool, bool, int, ImmutableArray<float>> PackageWheelConfig()
{
    return new MyTuple<bool, bool, int, ImmutableArray<float>>(DisabledAssist, DisabledSpinTurn, CurrentGearMode,
        ImmutableArray.Create(new float[] { FrictionBalancePercentage, PowerBalancePercentage,
            WRotorMaxiumRpm, MaxTurnAngle, MinTurnAngle , PowerMult,
            TurnSlippingMult,Friction,SuspensionHeight,DiffRpmPercentage,FrontAngleRate,RearWheelTurnRate }));
}
#endregion
#region 主程序功能区段
void Restart(IMyProgrammableBlock Me, IMyGridTerminalSystem GridTerminalSystem)
{
    try
    {
        this.Me = Me;
        this.GridTerminalSystem = GridTerminalSystem;
        ForceUpdate();
    }
    catch (Exception) { }
}
void Update()
{
    try
    {
        if (!ShipControllerDSP.UnabledControl)
            ShipControllerDSP.RefindCockpit(GridTerminalSystem, Me.IsSameConstructAs, StablizeBlockTag);
        ShipControllerDSP.Update();
    }
    catch (Exception) { }
}
void EditRole(string CommondLine)
{
    if (!CommandLine.TryParse(CommondLine))
        return;
    if (CommandLine.Switch("None"))
        ShipControllerDSP.Role = MyRole.None;
    else if (CommandLine.Switch("Aeroplane"))
        ShipControllerDSP.Role = MyRole.Aeroplane;
    else if (CommandLine.Switch("Helicopter"))
        ShipControllerDSP.Role = MyRole.Helicopter;
    else if (CommandLine.Switch("VTOL"))
        ShipControllerDSP.Role = MyRole.VTOL;
    else if (CommandLine.Switch("SpaceShip"))
        ShipControllerDSP.Role = MyRole.SpaceShip;
    else if (CommandLine.Switch("SeaShip"))
        ShipControllerDSP.Role = MyRole.SeaShip;
    else if (CommandLine.Switch("Submarine"))
        ShipControllerDSP.Role = MyRole.Submarine;
    else if (CommandLine.Switch("TrackVehicle"))
        ShipControllerDSP.Role = MyRole.TrackVehicle;
    else if (CommandLine.Switch("WheelVehicle"))
        ShipControllerDSP.Role = MyRole.WheelVehicle;
    else if (CommandLine.Switch("HoverVehicle"))
        ShipControllerDSP.Role = MyRole.HoverVehicle;
    CommandLine.Clear();
}
void LoadOrAddConfig(string Storge)
{
    if (!Configs.TryParse(Storge))
    {
        SaveCommonSetup();
        SaveSpeedSetup();
        SaveThrustSetup();
        SaveWheelsSetup();
    }
    LoadCommonSetup();
    LoadSpeedSetup();
    switch (ShipControllerDSP.Role)
    {
        case MyRole.Aeroplane:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Flight;
            break;
        case MyRole.Helicopter:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Hover;
            break;
        case MyRole.VTOL:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.HoverMode ? MaxiumSpeed_Hover : ShipControllerDSP.NoGravity ? MaxiumSpeed_Space : MaxiumSpeed_Flight;
            break;
        case MyRole.SpaceShip:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.HoverMode ? MaxiumSpeed_Hover : ShipControllerDSP.NoGravity ? MaxiumSpeed_Space : MaxiumSpeed_Flight;
            break;
        case MyRole.SeaShip:
        case MyRole.Submarine:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Sea;
            break;
        case MyRole.TrackVehicle:
        case MyRole.WheelVehicle:
        case MyRole.HoverVehicle:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Land;
            break;
        default:
            ShipControllerDSP.MSL = 100;
            break;
    }
    LoadThrustSetup();
    LoadWheelsSetup();
    if (Common.IsNull(Me))
        return;
}
string RestorgeCustomData()
{
    if (Common.IsNull(Me))
        return "";
    Configs.TryParse(Me.CustomData);
    LoadCommonSetup();
    LoadSpeedSetup();
    LoadThrustSetup();
    LoadWheelsSetup();
    switch (ShipControllerDSP.Role)
    {
        case MyRole.Aeroplane:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Flight;
            break;
        case MyRole.Helicopter:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Hover;
            break;
        case MyRole.VTOL:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.HoverMode ? MaxiumSpeed_Hover : ShipControllerDSP.NoGravity ? MaxiumSpeed_Space : MaxiumSpeed_Flight;
            break;
        case MyRole.SpaceShip:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.HoverMode ? MaxiumSpeed_Hover : ShipControllerDSP.NoGravity ? MaxiumSpeed_Space : MaxiumSpeed_Flight;
            break;
        case MyRole.SeaShip:
        case MyRole.Submarine:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Sea;
            break;
        case MyRole.TrackVehicle:
        case MyRole.WheelVehicle:
        case MyRole.HoverVehicle:
            ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Land;
            break;
        default:
            ShipControllerDSP.MSL = 100;
            break;
    }
    return Configs.ToString();
}
void RestorgeCustomDataDirect()
{
    if (Common.IsNull(Me)) return;
    SaveCommonSetup();
    SaveSpeedSetup();
    SaveThrustSetup();
    SaveWheelsSetup();
    var context = Configs.ToString();
    Me.CustomData = context;
    Storage = context;
}
void ShowInCustomData()
{
    if (Common.IsNull(Me))
        return;
    Me.CustomData = Configs.ToString();
}
void SwitchCommonds(string CommondLine)
{
    if (!CommandLine.TryParse(CommondLine))
        return;
    ShipControllerDSP.HasWings = CommandLine.Switch("HasWings") ^ ShipControllerDSP.HasWings;
    ShipControllerDSP.CHLSM = CommandLine.Switch("SeaLevel") ^ ShipControllerDSP.CHLSM;
    ShipControllerDSP.CalculateAllGravity = CommandLine.Switch("AllGravity") ^ ShipControllerDSP.CalculateAllGravity;
    DisabledAssist = CommandLine.Switch("DisabledAssist") ^ DisabledAssist;
    if (CommandLine.Switch("Hover") || CommandLine.Switch("DockMode") || CommandLine.Switch("Cruise"))
    {
        DockMode = CommandLine.Switch("DockMode") ^ ShipControllerDSP.CalculateAllGravity;
        ShipControllerDSP.HoverMode = CommandLine.Switch("Hover") ^ ShipControllerDSP.HoverMode;
        ShipControllerDSP.CruiseMode = CommandLine.Switch("Cruise") ^ ShipControllerDSP.CruiseMode;

        switch (ShipControllerDSP.Role)
        {
            case MyRole.Aeroplane:
                ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Flight;
                break;
            case MyRole.Helicopter:
                ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Hover;
                break;
            case MyRole.VTOL:
                ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.HoverMode ? MaxiumSpeed_Hover : ShipControllerDSP.NoGravity ? MaxiumSpeed_Space : MaxiumSpeed_Flight;
                break;
            case MyRole.SpaceShip:
                ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.HoverMode ? MaxiumSpeed_Hover : ShipControllerDSP.NoGravity ? MaxiumSpeed_Space : MaxiumSpeed_Flight;
                break;
            case MyRole.SeaShip:
            case MyRole.Submarine:
                ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Sea;
                break;
            case MyRole.TrackVehicle:
            case MyRole.WheelVehicle:
            case MyRole.HoverVehicle:
                ShipControllerDSP.MSL = DockMode ? MaxiumSpeed_Dock : ShipControllerDSP.NoGravity ? 0 : MaxiumSpeed_Land;
                break;
            default:
                ShipControllerDSP.MSL = 100;
                break;
        }
    }
    EnabledKLang = CommandLine.Switch("EnabledKLang") ^ EnabledKLang;
    EnabledGravity = CommandLine.Switch("EnabledGravity") ^ EnabledGravity;
    if (CommandLine.Switch("IonThrust_OnOff") || CommandLine.Switch("AtomThrust_OnOff") || CommandLine.Switch("HydgenThrust_OnOff"))
    {
        IonThrust_OnOff = CommandLine.Switch("IonThrust_OnOff") ^ IonThrust_OnOff;
        AtomThrust_OnOff = CommandLine.Switch("AtomThrust_OnOff") ^ AtomThrust_OnOff;
        HydgenThrust_OnOff = CommandLine.Switch("HydgenThrust_OnOff") ^ HydgenThrust_OnOff;
    }
}
void ForceUpdate()
{
    try
    {
        ShipControllerDSP.ForceUpdate(GridTerminalSystem, Me.IsSameConstructAs, StablizeBlockTag);
        ShipControllerDSP.Update();
    }
    catch (Exception) { }
}
void LoadCommonSetup()
{
    if (!Configs.ContainsSection(CSTag))
        SaveCommonSetup();
    ShipControllerDSP.Enabled = Configs.Get(CSTag, "Enabled").ToBoolean();
    DockMode = Configs.Get(CSTag, "DockMode").ToBoolean();
    ShipControllerDSP.Role = ParseControllerRole(Configs.Get(CSTag, "Role").ToString());
    ShipControllerDSP.DetectorHeight = Configs.Get(CSTag, "DetectorHeight").ToSingle();
    ShipControllerDSP.HoverMode = Configs.Get(CSTag, "HoverMode").ToBoolean();
    ShipControllerDSP.CruiseMode = Configs.Get(CSTag, "CruiseMode").ToBoolean();
    ShipControllerDSP.HasWings = Configs.Get(CSTag, "HasWings").ToBoolean();
    ShipControllerDSP.CalculateAllGravity = Configs.Get(CSTag, "CalculateAllGravity").ToBoolean();
    ShipControllerDSP.CHLSM = Configs.Get(CSTag, "CurrentHeightSeaLevelMode").ToBoolean();
    ShipControllerDSP.MaxAngularVelocity = Configs.Get(CSTag, "MaxAngularVelocity").ToSingle();
    ShipControllerDSP.SafetyStage = Configs.Get(CSTag, "SafetyStage").ToSingle();
    ShipControllerDSP.AssignedVelocityRate = Configs.Get(CSTag, "AssignedVelocityRate").ToSingle();
    ShipControllerDSP.AntiRollK = Configs.Get(CSTag, "AntiRollK").ToSingle();
    ShipControllerDSP.HeightSpringK = Configs.Get(CSTag, "HeightSpringK").ToSingle();
    ShipControllerDSP.HeightDampener = Configs.Get(CSTag, "HeightDampener").ToSingle();
    ShipControllerDSP.MaxiumDownSpeed = Configs.Get(CSTag, "MaxiumDownSpeed").ToSingle();
    ShipControllerDSP.DownForce = Configs.Get(CSTag, "DownForce").ToSingle();
    ShipControllerDSP.AngularDampener_Pitch = Configs.Get(CSTag, "AngularDampener_Pitch").ToSingle();
    ShipControllerDSP.AngularDampener_Yaw = Configs.Get(CSTag, "AngularDampener_Yaw").ToSingle();
    ShipControllerDSP.AngularDampener_Roll = Configs.Get(CSTag, "AngularDampener_Roll").ToSingle();
    ShipControllerDSP.MaxTargetSpeed = Configs.Get(CSTag, "MaxTargetSpeed").ToSingle();
    ShipControllerDSP.VelocitySensitive = Configs.Get(CSTag, "VelocitySensitive").ToSingle();
    GyroMultipy.X = Configs.Get(CSTag, "GyroMultipy_Pitch").ToSingle();
    GyroMultipy.Y = Configs.Get(CSTag, "GyroMultipy_Yaw").ToSingle();
    GyroMultipy.Z = Configs.Get(CSTag, "GyroMultipy_Roll").ToSingle();
}
void LoadSpeedSetup()
{
    if (!Configs.ContainsSection(STTag))
        SaveSpeedSetup();
    MaxiumSpeed_Hover = Configs.Get(STTag, "MaxiumSpeed_Hover").ToSingle();
    MaxiumSpeed_Flight = Configs.Get(STTag, "MaxiumSpeed_Flight").ToSingle();
    MaxiumSpeed_Dock = Configs.Get(STTag, "MaxiumSpeed_Dock").ToSingle();
    MaxiumSpeed_Land = Configs.Get(STTag, "MaxiumSpeed_Land").ToSingle();
    MaxiumSpeed_Sea = Configs.Get(STTag, "MaxiumSpeed_Sea").ToSingle();
    MaxiumSpeed_Space = Configs.Get(STTag, "MaxiumSpeed_Space").ToSingle();
}
void LoadThrustSetup()
{
    if (!Configs.ContainsSection(T_OnOffTag))
        SaveThrustSetup();
    IonThrust_OnOff = Configs.Get(T_OnOffTag, "IonThrust_OnOff").ToBoolean();
    AtomThrust_OnOff = Configs.Get(T_OnOffTag, "AtomThrust_OnOff").ToBoolean();
    HydgenThrust_OnOff = Configs.Get(T_OnOffTag, "HydgenThrust_OnOff").ToBoolean();
    EnabledKLang = Configs.Get(T_OnOffTag, "EnabledKLang").ToBoolean();
    EnabledGravity = Configs.Get(T_OnOffTag, "EnabledGravity").ToBoolean();
    EngineNot2Control.Clear();
    if (!IonThrust_OnOff)
        EngineNot2Control.Add("Ion");
    if (!AtomThrust_OnOff)
        EngineNot2Control.Add("Atmospheric");
    if (!HydgenThrust_OnOff)
        EngineNot2Control.Add("Hydrogen");
}
void LoadWheelsSetup()
{
    if (!Configs.ContainsSection(W_SetupTag))
        SaveWheelsSetup();
    CurrentGearMode = Configs.Get(W_SetupTag, "CurrentGearMode").ToInt32();
    WRotorMaxiumRpm = Configs.Get(W_SetupTag, "WRotorMaxiumRpm").ToSingle();
    MaxTurnAngle = MathHelper.ToRadians(Configs.Get(W_SetupTag, "MaxTurnAngle").ToSingle());
    MinTurnAngle = MathHelper.ToRadians(Configs.Get(W_SetupTag, "MinTurnAngle").ToSingle());
    PowerMult = Configs.Get(W_SetupTag, "PowerMult").ToSingle();
    TurnSlippingMult = Configs.Get(W_SetupTag, "TurnSlippingMult").ToSingle();
    Friction = Configs.Get(W_SetupTag, "Friction").ToSingle();
    SuspensionHeight = Configs.Get(W_SetupTag, "SuspensionHeight").ToSingle();
    DiffRpmPercentage = Configs.Get(W_SetupTag, "DiffRpmPercentage").ToSingle();
    FrontAngleRate = Configs.Get(W_SetupTag, "FrontAngleRate").ToSingle();
    RearWheelTurnRate = Configs.Get(W_SetupTag, "RearWheelTurnRate").ToSingle();
    DisabledSpinTurn = Configs.Get(W_SetupTag, "DisabledSpinTurn").ToBoolean();
    DisabledAssist = Configs.Get(W_SetupTag, "DisabledAssist").ToBoolean();
    FrictionBalancePercentage = Configs.Get(W_SetupTag, "FrictionBalancePercentage").ToSingle();
    PowerBalancePercentage = Configs.Get(W_SetupTag, "PowerBalancePercentage").ToSingle();
}
void SaveCommonSetup()
{
    if (!Configs.ContainsSection(CSTag))
        Configs.AddSection(CSTag);
    Configs.Set(CSTag, "Enabled", ShipControllerDSP.Enabled.ToString());
    Configs.Set(CSTag, "Role", ShipControllerDSP.Role.ToString());
    Configs.Set(CSTag, "DockMode", DockMode.ToString());
    Configs.Set(CSTag, "HoverMode", ShipControllerDSP.HoverMode.ToString());
    Configs.Set(CSTag, "CruiseMode", ShipControllerDSP.CruiseMode.ToString());
    Configs.Set(CSTag, "HasWings", ShipControllerDSP.HasWings.ToString());
    Configs.Set(CSTag, "CalculateAllGravity", ShipControllerDSP.CalculateAllGravity.ToString());
    Configs.Set(CSTag, "MaxAngularVelocity", ShipControllerDSP.MaxAngularVelocity.ToString());
    Configs.Set(CSTag, "SafetyStage", ShipControllerDSP.SafetyStage.ToString());
    Configs.Set(CSTag, "AssignedVelocityRate", ShipControllerDSP.AssignedVelocityRate.ToString());
    Configs.Set(CSTag, "AntiRollK", ShipControllerDSP.AntiRollK.ToString());
    Configs.Set(CSTag, "HeightSpringK", ShipControllerDSP.HeightSpringK.ToString());
    Configs.Set(CSTag, "HeightDampener", ShipControllerDSP.HeightDampener.ToString());
    Configs.Set(CSTag, "MaxiumDownSpeed", ShipControllerDSP.MaxiumDownSpeed.ToString());
    Configs.Set(CSTag, "AngularDampener_Pitch", ShipControllerDSP.AngularDampener_Pitch.ToString());
    Configs.Set(CSTag, "AngularDampener_Yaw", ShipControllerDSP.AngularDampener_Yaw.ToString());
    Configs.Set(CSTag, "AngularDampener_Roll", ShipControllerDSP.AngularDampener_Roll.ToString());
    Configs.Set(CSTag, "DetectorHeight", ShipControllerDSP.DetectorHeight.ToString());
    Configs.Set(CSTag, "DownForce", ShipControllerDSP.DownForce.ToString());
    Configs.Set(CSTag, "GyroMultipy_Pitch", GyroMultipy.X.ToString());
    Configs.Set(CSTag, "GyroMultipy_Yaw", GyroMultipy.Y.ToString());
    Configs.Set(CSTag, "GyroMultipy_Roll", GyroMultipy.Z.ToString());
    Configs.Set(CSTag, "VelocitySensitive", ShipControllerDSP.VelocitySensitive.ToString());
}
void SaveSpeedSetup()
{
    if (!Configs.ContainsSection(STTag))
        Configs.AddSection(STTag);
    Configs.Set(STTag, "MaxiumSpeed_Hover", MaxiumSpeed_Hover.ToString());
    Configs.Set(STTag, "MaxiumSpeed_Flight", MaxiumSpeed_Flight.ToString());
    Configs.Set(STTag, "MaxiumSpeed_Dock", MaxiumSpeed_Dock.ToString());
    Configs.Set(STTag, "MaxiumSpeed_Land", MaxiumSpeed_Land.ToString());
    Configs.Set(STTag, "MaxiumSpeed_Sea", MaxiumSpeed_Sea.ToString());
    Configs.Set(STTag, "MaxiumSpeed_Space", MaxiumSpeed_Space.ToString());
}
void SaveThrustSetup()
{
    if (!Configs.ContainsSection(T_OnOffTag))
        Configs.AddSection(T_OnOffTag);
    Configs.Set(T_OnOffTag, "IonThrust_OnOff", IonThrust_OnOff.ToString());
    Configs.Set(T_OnOffTag, "AtomThrust_OnOff", AtomThrust_OnOff.ToString());
    Configs.Set(T_OnOffTag, "HydgenThrust_OnOff", HydgenThrust_OnOff.ToString());
    Configs.Set(T_OnOffTag, "EnabledKLang", EnabledKLang.ToString());
    Configs.Set(T_OnOffTag, "EnabledGravity", EnabledGravity.ToString());
}
void SaveWheelsSetup()
{
    if (!Configs.ContainsSection(W_SetupTag))
        Configs.AddSection(W_SetupTag);
    Configs.Set(W_SetupTag, "CurrentGearMode", CurrentGearMode.ToString());
    Configs.Set(W_SetupTag, "WRotorMaxiumRpm", WRotorMaxiumRpm.ToString());
    Configs.Set(W_SetupTag, "MaxTurnAngle", MathHelper.ToDegrees(MaxTurnAngle).ToString());
    Configs.Set(W_SetupTag, "MinTurnAngle", MathHelper.ToDegrees(MinTurnAngle).ToString());
    Configs.Set(W_SetupTag, "PowerMult", PowerMult.ToString());
    Configs.Set(W_SetupTag, "TurnSlippingMult", TurnSlippingMult.ToString());
    Configs.Set(W_SetupTag, "Friction", Friction.ToString());
    Configs.Set(W_SetupTag, "SuspensionHeight", (-SuspensionHeight).ToString());
    Configs.Set(W_SetupTag, "DiffRpmPercentage", DiffRpmPercentage.ToString());
    Configs.Set(W_SetupTag, "FrontAngleRate", FrontAngleRate.ToString());
    Configs.Set(W_SetupTag, "RearWheelTurnRate", RearWheelTurnRate.ToString());
    Configs.Set(W_SetupTag, "DisabledSpinTurn", DisabledSpinTurn.ToString());
    Configs.Set(W_SetupTag, "DisabledAssist", DisabledAssist.ToString());
    Configs.Set(W_SetupTag, "FrictionBalancePercentage", FrictionBalancePercentage.ToString());
    Configs.Set(W_SetupTag, "PowerBalancePercentage", PowerBalancePercentage.ToString());
}
static MyRole ParseControllerRole(string str)
{
    MyRole value;
    if (!Enum.TryParse(str, out value))
        value = default(MyRole);
    return value;
}
#endregion
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
    internal MyShipControllerDSP SCIFM
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
        switch (SCIFM.Role)
        {
            case MyRole.Aeroplane:
                break;
            case MyRole.Helicopter:
                break;
            case MyRole.VTOL:
                if (SCIFM.HoverMode)
                {
                }
                else
                {
                }
                break;
            case MyRole.SpaceShip:
                break;
            case MyRole.SeaShip:
                break;
            case MyRole.Submarine:
                break;
            case MyRole.TrackVehicle:
                break;
            case MyRole.WheelVehicle:
                break;
            case MyRole.HoverVehicle:
                break;
            default:
                break;
        }
    }
}
sealed class MyShipControllerDSP
{
    public bool Enabled { get; set; } = true;
    public bool HoverMode { get { return _HoverMode; } set { if (value == _HoverMode) return; _HoverMode = value; SLM.ForceUpdateHeight(); } }
    private bool _HoverMode = true;
    public bool CruiseMode { get; set; } = false;
    public bool HasWings { get; set; } = false;
    public float AngularDampener_Pitch { get; set; } = 10;
    public float AngularDampener_Roll { get; set; } = 10;
    public float AngularDampener_Yaw { get; set; } = 10;
    public float AntiRollK { get; set; } = 1;
    public float DetectorHeight { get; set; } = 50f;
    public float DownForce { get; set; } = 1.5f;
    public bool EnabledAllDirectionThruster { get; set; } = true;
    public bool ERE { get; set; } = false;
    public bool CHLSM { get; set; } = false;
    public float AssignedVelocityRate { get; set; } = 1f;
    public bool CalculateAllGravity { get; set; } = false;
    public float HeightDampener { get; set; } = 1;
    public float HeightSpringK { get; set; } = 2f;
    public float MaxAngularVelocity { get; set; } = 1f;
    public float MaxiumDownSpeed { get; set; } = 10f;
    List<MyCameraBlockTestHeight> TestHeights;
    public MyShipControllerDSP()
    {
        SLM.SCIFM = this;
    }
    public float VelocitySensitive { get; set; } = 1;
    Vector3 AngularDampener => new Vector3(Math.Abs(AngularDampener_Pitch), Math.Abs(AngularDampener_Yaw), Math.Abs(AngularDampener_Roll));
    public Vector3 AngularVelocity => Me?.GetShipVelocities().AngularVelocity ?? Vector3.Zero;
    public Vector3D GCS { get; private set; } = Vector3D.Zero;
    public Vector3D TCS { get; private set; } = Vector3D.Zero;
    public Vector3 Gravity => (CalculateAllGravity ? Me?.GetTotalGravity() : Me?.GetNaturalGravity()) ?? (GravityDirection * 10);
    public Vector3D GravityDirection => GGD();
    public Vector3D Center => Me?.CubeGrid?.WorldAABB.Center ?? Vector3D.Zero;
    public bool HandBrake => Me?.HandBrake ?? true;
    public float ShipMass => Me?.CalculateShipMass().TotalMass ?? 0;
    public Func<IMyTerminalBlock, bool> InThisEntity => block => Me?.IsSameConstructAs(block) ?? false;
    public Vector3 LinearVelocity => Me?.GetShipVelocities().LinearVelocity ?? Vector3.Zero;
    public bool NoGravity => Vector3.IsZero(Gravity);
    public bool UnabledControl => Common.IsNull(Me?.CubeGrid);
    public bool EnabledGyros => Me?.GetValueBool("ControlGyros") ?? true;
    public bool EnabledThrusts => Me?.ControlThrusters ?? true;
    public bool EnabledWheels => Me?.ControlWheels ?? true;
    internal Vector3 MI_SC => (MoveIndicator_Override ?? Me?.MoveIndicator) ?? Vector3.Zero;
    internal Vector3 RI_SC => RotationIndicator_Override ?? new Vector3(Me?.RotationIndicator ?? Vector2.Zero, Me?.RollIndicator ?? 0);
    internal float SLDVM => (Common.IsNull(Me?.CubeGrid) ? 5f : ((Me.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large) ? 20 : 4)) * VelocitySensitive;
    internal double MaxTargetSpeed { get; set; } = 0;
    internal IMyShipController Me { get; set; } = null;
    public Vector3 MoveIndicator { get; set; } = Vector3.Zero;
    public Vector3D? ReferNormal { get; set; } = null;
    public MyRole Role { get; set; } = MyRole.SpaceShip;
    public Vector3 RotationIndicator { get; set; } = Vector3.Zero;
    public float SafetyStage { get; set; } = 0.5f;
    public Vector3D TorqueAddon { get; set; } = Vector3D.Zero;
    internal float MSL { get; set; } = 50f;
    internal Vector3? MoveIndicator_Override { get; set; } = null;
    internal Vector3? RotationIndicator_Override { get; set; } = null;
    internal MySLManager SLM { get; } = new MySLManager();
    internal MatrixD WorldMatrix => Me?.WorldMatrix ?? MatrixD.Identity;
    public void ForceUpdate(IMyGridTerminalSystem GridTerminalSystem, Func<IMyTerminalBlock, bool> InThisEntity, string StablizeBlockTag = "")
    {
        RefindCockpit(GridTerminalSystem, InThisEntity, StablizeBlockTag);
        TestHeights = MyCameraBlockTestHeight.CreateCameraBlockParametersList(Me, GridTerminalSystem, StablizeBlockTag);
        if (SLM.FirstRun)
        {
            SLM.InitParameters(100);
        }
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
        Vector3D ResultLinnerVelocity = LinearVelocity - CommonMathFunction.VTransLocal2WorldD(WorldMatrix, new Vector3(MI_SC.X, 0, MI_SC.Z)) * MSL;
        if (!HoverMode)
            ResultLinnerVelocity = ResultLinnerVelocity - CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward) + WorldMatrix.Backward * MSL * 8f;
        return ResultLinnerVelocity + Gravity;
    }
    public Vector3 GPN4VectorEngine_OnlyNormal()
    {
        double MaxLength = Gravity.Length();
        Vector3D ResultLinnerVelocity = LinearVelocity - CommonMathFunction.VTransLocal2WorldD(WorldMatrix, new Vector3(MI_SC.X, 0, MI_SC.Z)) * MSL;
        if (!HoverMode)
            ResultLinnerVelocity = ResultLinnerVelocity - CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
        return Vector3D.ClampToSphere(ResultLinnerVelocity, MaxLength) + Gravity;
    }
    public override string ToString()
    {
        CI.Clear();
        CI.AppendLine("Ship Controller State");
        CI.AppendLine($"Running:{Enabled}");
        if (HoverMode)
        {
            if (CruiseMode)
                CI.AppendLine("Hover Mode:Pitch Assinged Gravity");
            else
                CI.AppendLine("Hover Mode:Pitch Normal");
        }
        else
        {
            if (CruiseMode)
                CI.AppendLine("Flight Mode:Pitch Assinged Gravity");
            else
                CI.AppendLine("Flight Mode:Pitch Normal");
        }
        CI.AppendLine($"Wing Mode:{HasWings}");
        CI.AppendLine($"Sea Level:{SLM.Current_SeaLevel}m");
        CI.AppendLine($"Height:{SLM.Current_Height}m");
        return CI.ToString();
    }
    public void Update()
    {
        if (UnabledControl)
            Enabled = false;
        MSL = Math.Max(MSL, 0);
        if (!UnabledControl && Enabled)
            SLM.UpdateParameters();
        if (HandBrake || UnabledControl || !Enabled)
        {
            MoveIndicator = Vector3.Zero;
            RotationIndicator = Vector3.Zero;
        }
        else
        {
            UpdateControlSignal();
        }
        switch (Role)
        {
            case MyRole.Helicopter:
                EnabledAllDirectionThruster = HandBrake || GravityDirection == Vector3.Zero;
                break;
            case MyRole.Aeroplane:
            case MyRole.VTOL:
                EnabledAllDirectionThruster = HandBrake || GravityDirection == Vector3.Zero || (!HasWings);
                break;
            default:
                EnabledAllDirectionThruster = true;
                break;
        }
        ProcessGyrosCS();
        ProcessThrustsCS();
    }
    double Rate
    {
        get
        {
            switch (Role)
            {
                case MyRole.Aeroplane:
                    return MathHelper.Clamp(SafetyStage, 0.05, 0.95);
                case MyRole.Helicopter:
                    return MathHelper.Clamp(SafetyStage, 0.5, 1);
                case MyRole.VTOL:
                case MyRole.SpaceShip:
                    if ((HoverMode || CruiseMode) && (!NoGravity))
                        return MathHelper.Clamp(SafetyStage, 0.5, 1);
                    else
                        return MathHelper.Clamp(SafetyStage, 0.05, 0.95);
                default:
                    return 0;
            }
        }
    }
    Vector3D GGD()
    {
        if (UnabledControl)
            return Vector3D.Zero;
        Vector3D PositionP;
        if (!Me.TryGetPlanetPosition(out PositionP))
            return Vector3D.Zero;
        return CommonMathFunction.NormalizeD(PositionP - Me.GetPosition());
    }
    Vector3D GPN()
    {
        Vector3D _RLV = Vector3D.Zero;
        Vector3D ControlLine = CommonMathFunction.VTransLocal2WorldD(WorldMatrix, new Vector3(RotationIndicator.Z, 0, -RotationIndicator.X)) * MSL;
        switch (Role)
        {
            case MyRole.Aeroplane:
                _RLV = LinearVelocity - CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
                if (_RLV.Length() < (MSL * 0.1f))
                    _RLV = Vector3.Zero;
                break;
            case MyRole.Helicopter:
                _RLV = LinearVelocity - ControlLine;
                break;
            case MyRole.VTOL:
                if (NoGravity)
                {
                    if (HoverMode)
                        break;
                    _RLV = LinearVelocity - CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
                }
                else
                {
                    if (HoverMode)
                    {
                        if (HasWings)
                        {
                            _RLV = LinearVelocity - ControlLine;
                        }
                        else
                        {
                            _RLV = LinearVelocity - CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
                            if (_RLV.Length() < (MSL * 0.1f))
                                _RLV = Vector3.Zero;
                        }
                    }
                    else
                    {
                        _RLV = LinearVelocity - CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
                        if (_RLV.Length() < (MSL * 0.1f))
                            _RLV = Vector3.Zero;
                        break;
                    }
                }
                break;
            case MyRole.SpaceShip:
                if (NoGravity)
                {
                    if (HoverMode)
                        break;
                    _RLV = LinearVelocity - CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
                }
                else
                {
                    _RLV = LinearVelocity - (HoverMode ? Vector3D.Zero : CommonMathFunction.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward));
                }
                if (_RLV.Length() < (MSL * 0.1f))
                    _RLV = Vector3.Zero;
                break;
            default:
                break;
        }
        switch (Role)
        {
            case MyRole.Helicopter:
                return PMG_Sub_Helicopter(_RLV, Rate);
            case MyRole.VTOL:
                if (NoGravity)
                    return PMG_Sub(_RLV, (HoverMode && LinearVelocity.LengthSquared() < 900 && !HasWings) ? 1 : Rate, HoverMode || CruiseMode);
                else
                {
                    if (HoverMode && HasWings)
                        return PMG_Sub_Helicopter(_RLV, Rate);
                    else
                        return PMG_Sub(_RLV, (HoverMode && LinearVelocity.LengthSquared() < 900 && !HasWings) ? 1 : Rate, HoverMode || CruiseMode || HasWings);
                }
            case MyRole.Aeroplane:
                return PMG_Sub(_RLV, (LinearVelocity.LengthSquared() < 1) ? 1 : Rate, CruiseMode);
            case MyRole.SpaceShip:
                return PMG_Sub(_RLV, (HoverMode && LinearVelocity.LengthSquared() < 900) ? 1 : Rate, HoverMode || CruiseMode);
            default:
                return Gravity;
        }
    }
    Vector3D PMG_Sub_Helicopter(Vector3D Velocity, double r)
    {
        if (NoGravity)
            return Velocity;
        return Vector3D.ClampToSphere(CommonMathFunction.ProjectOnPlaneD(Velocity, CommonMathFunction.NormalizeD(Gravity)), 10 * (1 - r)) + Gravity;
    }
    Vector3D PMG_Sub(Vector3D Velocity, double r, bool SafeMode)
    {
        var pv = CommonMathFunction.ProjectOnVectorD(Velocity, WorldMatrix.Right);
        if (NoGravity)
            return pv;
        return Vector3D.ClampToSphere(CommonMathFunction.ProjectOnPlaneD(pv, CommonMathFunction.NormalizeD(Gravity)), 10 * (1 - r)) + Gravity * (SafeMode ? 1 : (float)r);
    }
    void ProcessGyrosCS()
    {
        if (UnabledControl || !Enabled)
        {
            GCS = Vector3D.Zero;
            return;
        }
        double ARSKM;
        switch (Role)
        {
            case MyRole.None:
                ARSKM = 0;
                break;
            case MyRole.TrackVehicle:
            case MyRole.WheelVehicle:
                ARSKM = 500;
                break;
            default:
                ARSKM = 20;
                break;
        }
        if (Role == MyRole.None)
        {
            GCS = Vector3D.Zero;
            return;
        }
        Vector3D N = ReferNormal ?? GPN();
        var non_N = Vector3D.IsZero(N);
        Vector3D GCSMultipy = ARSKM * Vector3D.One * SLDVM * MaxAngularVelocity;
        if (non_N)
        {
            GCS = CommonMathFunction.VTransLocal2WorldD(WorldMatrix, PDFC(RotationIndicator * 100) * GCSMultipy);
            return;
        }
        Vector3D CSL = RotationIndicator * 100;
        var CSN_Roll = SCPF.ControlWithVectorD(WorldMatrix.Backward, N, WorldMatrix.Down);
        var CSN_Pitch = SCPF.ControlWithVectorD(WorldMatrix.Right, N, WorldMatrix.Down);
        double LDR;
        switch (Role)
        {
            case MyRole.Aeroplane:
                LDR = (CSL.Z == 0) ? CSN_Roll : CSL.Z;
                break;
            case MyRole.Helicopter:
                LDR = CSN_Roll;
                break;
            case MyRole.SpaceShip:
                LDR = CSN_Roll + CSL.Z;
                break;
            case MyRole.VTOL:
                if (NoGravity)
                    LDR = CSN_Roll + CSL.Z;
                else if (HoverMode && HasWings)
                    LDR = CSN_Roll;
                else
                    LDR = (CSL.Z == 0) ? CSN_Roll : CSL.Z;
                break;
            case MyRole.TrackVehicle:
            case MyRole.WheelVehicle:
            case MyRole.HoverVehicle:
                LDR = SCPF.ControlWithVectorD(WorldMatrix.Backward, N, WorldMatrix.Down, RollLimitedValue);
                break;
            default:
                LDR = CSN_Roll;
                break;
        }
        double LDP;
        switch (Role)
        {
            case MyRole.Aeroplane:
                LDP = (CruiseMode ? CSN_Pitch : 0) + CSL.X;
                break;
            case MyRole.Helicopter:
                LDP = HasWings ? CSL.X : CSN_Pitch;
                break;
            case MyRole.SpaceShip:
                LDP = (CruiseMode ? CSN_Pitch : 0) + CSL.X;
                break;
            case MyRole.VTOL:
                if (HoverMode && HasWings && !NoGravity)
                    LDP = CSN_Pitch;
                else
                    LDP = (CruiseMode ? CSN_Pitch : 0) + CSL.X;
                break;
            case MyRole.TrackVehicle:
            case MyRole.WheelVehicle:
            case MyRole.HoverVehicle:
                LDP = SCPF.ControlWithVectorD(WorldMatrix.Right, N, WorldMatrix.Down, PitchLimitedValue);
                break;
            default:
                LDP = CSN_Pitch;
                break;
        }
        CSL.X = LDP;
        CSL.Z = LDR;
        var LT = PDFC(CSL) * GCSMultipy;
        switch (Role)
        {
            case MyRole.TrackVehicle:
            case MyRole.WheelVehicle:
            case MyRole.HoverVehicle:
                Vector3D ControllerMult = Vector3D.One * AntiRollK;
                if (TestHeights != null)
                    foreach (var TestHeight in TestHeights)
                        TestHeight.UpdateInfos(DetectorHeight);
                var ct = MyCameraBlockTestHeight.GetTorqueByCameras(TestHeights, ARSKM * AntiRollK);
                if (!Vector3D.IsZero(ct))
                    ControllerMult = new Vector3D(0.01f, 1000f, 0.01f);
                GCS = CommonMathFunction.VTransLocal2WorldD(WorldMatrix, LT * ControllerMult) + ct;
                return;
            default:
                GCS = CommonMathFunction.VTransLocal2WorldD(WorldMatrix, LT);
                return;
        }
    }
    void ProcessThrustsCS()
    {
        if (UnabledControl || !Enabled || Role == MyRole.None)
        {
            TCS = Vector3D.Zero;
            return;
        }
        var MIW = Vector3D.Zero;
        var Gravity = CalculateAllGravity ? Me.GetTotalGravity() : Me.GetNaturalGravity();
        if (!HandBrake)
        {
            if (HoverMode)
            {
                MIW = CommonMathFunction.VTransLocal2WorldD(WorldMatrix, MoveIndicator) * MSL;
            }
            else
            {
                float AccOrDec = MoveIndicator.Dot(Vector3D.Forward);
                if (AccOrDec > 0)
                    MaxTargetSpeed = MathHelper.Clamp(Math.Max(LinearVelocity.Dot(WorldMatrix.Forward), 0), 0, MSL);
                if (AccOrDec < 0)
                    MaxTargetSpeed = 0;
                MIW = WorldMatrix.Forward * (float)((AccOrDec > 0) ? MSL : MaxTargetSpeed);
            }
        }
        float DFAC = 0;
        if (Role == MyRole.WheelVehicle || Role == MyRole.TrackVehicle)
        {
            var speed_cav = MathHelper.Clamp(Math.Max(MathHelper.Clamp(LinearVelocity.Length(), 0, 100), 0) / Math.Max((MathHelper.Clamp(MSL, 30, 100)), 1), 0, 1);
            DFAC = -(float)speed_cav * DownForce;
        }
        float HSKX = MathHelper.Clamp((float)SCPF.GetSpring(SLM.Diff_Level, HeightSpringK) * (1 - MathHelper.Clamp(HeightDampener, 0.01f, 0.99f)), -10, 10);
        float HRKS = Math.Max((float)Gravity.Dot(LinearVelocity), 0);
        float SpeedLimit_Up = MathHelper.Clamp(MaxiumDownSpeed, 0, MSL);
        float SpeedLimit_Down = MathHelper.Clamp(MaxiumDownSpeed, 0, Math.Min(Math.Max(MSL, 0), SLM.Current_Height * AssignedVelocityRate));
        if (HRKS != 0)
        {
            HRKS /= (float)Gravity.Length();
            HRKS = (HRKS > MathHelper.Clamp(MaxiumDownSpeed, -SpeedLimit_Up, SpeedLimit_Down)) ? (CommonMathFunction.SmoothValue(HRKS) * HeightDampener) : 0;
        }
        HSKX += HRKS;
        float G_Mult;
        switch (Role)
        {
            case MyRole.Aeroplane:
                G_Mult = 0.9f;
                HSKX = CruiseMode ? MathHelper.Clamp(HSKX, -0.5f, 0.5f) : 0;
                break;
            case MyRole.SpaceShip:
            case MyRole.VTOL:
                if (HoverMode)
                {
                    G_Mult = SLM.HeightChangedEnable ? -0.9f : 0.9f;
                }
                else
                {
                    G_Mult = 0.9f;
                    HSKX = MathHelper.Clamp(HSKX, -0.5f, 0.5f);
                }
                break;
            case MyRole.SeaShip:
                G_Mult = 0.9f;
                HSKX = MathHelper.Clamp(HSKX, -0.5f, 0.5f);
                break;
            case MyRole.Helicopter:
            case MyRole.Submarine:
            case MyRole.HoverVehicle:
                G_Mult = SLM.HeightChangedEnable ? -0.9f : 0.9f;
                break;
            default:
                G_Mult = 0f;
                break;
        }
        TCS = CommonMathFunction.VTransLocal2WorldD(WorldMatrix, CommonMathFunction.SmoothValueD(CommonMathFunction.VTransWorld2LocalD(WorldMatrix, MIW - LinearVelocity - (Gravity * (HSKX + G_Mult + DFAC)))) * SLDVM);
    }
    void UpdateControlSignal()
    {
        switch (Role)
        {
            case MyRole.Aeroplane:
                MoveIndicator = new Vector3(0, 0, MI_SC.Z);
                break;
            case MyRole.Helicopter:
                MoveIndicator = new Vector3(0, MI_SC.Y, 0);
                break;
            case MyRole.VTOL:
                if (NoGravity)
                    MoveIndicator = MI_SC;
                else if (HoverMode && HasWings)
                    MoveIndicator = new Vector3(0, MI_SC.Y, 0);
                else if (HoverMode)
                    MoveIndicator = MI_SC;
                else if (CruiseMode)
                    MoveIndicator = new Vector3(0, 0, MI_SC.Z);
                else
                    MoveIndicator = MI_SC;
                break;
            case MyRole.SpaceShip:
                if (NoGravity || HoverMode)
                    MoveIndicator = MI_SC;
                else
                    MoveIndicator = new Vector3(0, 0, MI_SC.Z);
                break;
            case MyRole.TrackVehicle:
            case MyRole.WheelVehicle:
            case MyRole.SeaShip:
                MoveIndicator = new Vector3(0, 0, MI_SC.Z);
                break;
            case MyRole.Submarine:
            case MyRole.HoverVehicle:
                MoveIndicator = new Vector3(0, MI_SC.Y, MI_SC.Z);
                break;
            default:
                MoveIndicator = Vector3.Zero;
                break;
        }
        switch (Role)
        {
            case MyRole.Aeroplane:
            case MyRole.SpaceShip:
                RotationIndicator = RI_SC;
                break;
            case MyRole.Helicopter:
                RotationIndicator = HasWings ? RI_SC : new Vector3(-MI_SC.Z, RI_SC.Z, MI_SC.X);
                break;
            case MyRole.VTOL:
                if (NoGravity)
                    RotationIndicator = Vector3.Clamp(RI_SC + (ERE ? new Vector3(0, 0, MI_SC.X) : Vector3.Zero), Vector3.MinValue, Vector3.One);
                else if (HoverMode && HasWings)
                    RotationIndicator = new Vector3(-MI_SC.Z, RI_SC.Z, MI_SC.X);
                else
                {
                    RotationIndicator = RI_SC + (ERE ? new Vector3(0, 0, MI_SC.X) : Vector3.Zero);
                }
                break;
            case MyRole.Submarine:
            case MyRole.HoverVehicle:
            case MyRole.SeaShip:
            case MyRole.TrackVehicle:
            case MyRole.WheelVehicle:
                RotationIndicator = new Vector3(0, MI_SC.X + (HasWings ? RI_SC.Y : 0), 0);
                break;
            default:
                RotationIndicator = Vector3.Zero;
                break;
        }
    }
    Vector3D PDFC(Vector3 RI)
    {
        var av = CommonMathFunction.VTransWorld2Local(WorldMatrix, AngularVelocity);
        av = av * 0.2f * AngularDampener + RI;
        return CommonMathFunction.SmoothValueD(av);
    }
    internal bool TryGetPlanetElevation(MyPlanetElevation Mode, out double elevation)
    {
        elevation = double.MaxValue;
        return Me?.TryGetPlanetElevation(Mode, out elevation) ?? false;
    }
    StringBuilder CI { get; } = new StringBuilder();
}
sealed class MySLManager
{
    double h, _Th; float HTh; bool nply; double sl, _Ts;
    public MySLManager()
    {
        nply = false;
        h = sl = 0;
        _Ts = 0;
        _Th = 0;
        HTh = 100;
    }
    public float Current_Height => nply ? (float)h : 0;
    public float Current_SeaLevel => nply ? (float)sl : 0;
    public float DH => SCIFM.DetectorHeight;
    public float Diff_Height => nply ? (float)((HO ?? _Th) - h) : 0;
    public float Diff_Level => (float)(nply ? (h < HTh) ? Diff_Height : Diff_SeaLevel : 0);
    public float Diff_SeaLevel => nply ? (float)((SLO ?? _Ts) - sl) : 0;
    public bool Force_PullUp => h < DH;
    public bool HeightChangedEnable
    {
        get
        {
            if (Common.IsNull(SCIFM.Me) || SCIFM.NoGravity)
                return true;
            switch (SCIFM.Role)
            {
                case MyRole.Aeroplane:
                    return !SCIFM.CruiseMode;
                case MyRole.SpaceShip:
                case MyRole.VTOL:
                    if (SCIFM.HoverMode)
                        return SCIFM.MoveIndicator.Y != 0;
                    else if (SCIFM.CruiseMode)
                        return SCIFM.MoveIndicator.Y != 0;
                    else
                        return false;
                case MyRole.SeaShip:
                case MyRole.TrackVehicle:
                case MyRole.WheelVehicle:
                    return true;
                default:
                    return SCIFM.MoveIndicator.Y != 0;
            }
        }
    }
    public void ForceUpdateHeight()
    {
        if (Common.IsNull(SCIFM.Me))
            return;
        if (SCIFM.NoGravity)
        {
            nply = false;
            h = sl = _Ts = _Th = 0;
            return;
        }
        var _nearplanetary = SCIFM.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out sl) && SCIFM.TryGetPlanetElevation(MyPlanetElevation.Surface, out h);
        if (!_nearplanetary)
        {
            nply = false;
            h = sl = _Ts = _Th = 0;
            return;
        }
        if (nply != _nearplanetary)
        {
            nply = _nearplanetary;
            _Th = h;
            _Ts = sl;
            return;
        }
        _Ts = sl;
        _Th = h;
    }
    public double? HO { get; set; } = null; public double? SLO { get; set; } = null;
    internal bool FirstRun { get; set; } = true;
    internal MyShipControllerDSP SCIFM
    {
        get; set;
    }
    public void InitParameters(float HeightThreshold = 100)
    {
        if (Common.IsNull(SCIFM.Me))
            return;
        this.HTh = Math.Max(HeightThreshold, 20);
        if (!FirstRun)
            return;
        nply = SCIFM.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out sl) && SCIFM.TryGetPlanetElevation(MyPlanetElevation.Surface, out h);
        if (nply)
        {
            _Ts = sl;
            _Th = h;
        }
        else
        {
            h = sl = _Ts = _Th = 0;
        }
        FirstRun = false;
    }
    public override string ToString()
    {
        string str = $"[SealevelManager]\n\r" + $"Has Main Controller:{SCIFM?.Me != null}\n\r" + $"Target SeaLevel:{_Ts}m" + $"\n\rCurrent SeaLevel:{sl}m\n\r" + $"Target Height:{_Th}m" + $"\n\rCurrent Height:{h}m\n\r";
        return str;
    }
    public void UpdateParameters()
    {
        if (Common.IsNull(SCIFM.Me))
            return;
        if (SCIFM.NoGravity)
        {
            nply = false;
            h = sl = _Ts = _Th = 0;
            return;
        }
        var _nearplanetary = SCIFM.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out sl) && SCIFM.TryGetPlanetElevation(MyPlanetElevation.Surface, out h);
        if (!_nearplanetary)
        {
            nply = false;
            h = sl = _Ts = _Th = 0;
            return;
        }
        if (nply != _nearplanetary)
        {
            nply = _nearplanetary;
            _Th = h;
            _Ts = sl;
            return;
        }
        if (HeightChangedEnable)
        {
            _Ts = sl;
            _Th = h;
        }
    }
}
#region 通用功能
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
    public static IMyCameraBlock ID2Camera(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyCameraBlock; public static IMyMotorStator ID2Motor(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyMotorStator; public static IMyTerminalBlock ID2Weapon(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyTerminalBlock; public static bool IsNull(Vector3? Value) => Value == null || Value.Value == Vector3.Zero; public static bool IsNull(Vector3D? Value) => Value == null || Value.Value == Vector3D.Zero; public static bool IsNull<T>(T Value) where T : class => Value == null; public static bool IsNullC<T>(ICollection<T> Value) => (Value?.Count ?? 0) < 1; public static bool IsNullC<T>(IEnumerable<T> Value) => (Value?.Count() ?? 0) < 1; public static bool NullEntity<T>(T Ent) where T : IMyEntity => Ent == null; public static float SetInRange_AngularDampeners(float data) => MathHelper.Clamp(data, 0.01f, 20f); public static List<string> SpliteByQ(string context) => context?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? (new List<string>());
}
static class CommonMathFunction
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
    public static Vector3D VTransWorld2LocalD(MatrixD Parent_World, Vector3D Vector_World) => Vector3D.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World)); public static Vector3D VTransLocal2WorldD(MatrixD Parent_World, Vector3D Vector_Local) => Vector3D.TransformNormal(Vector_Local, Parent_World); public static Vector3 VTransWorld2Local(MatrixD Parent_World, Vector3 Vector_World) => Vector3.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World)); public static Vector3 VTransLocal2World(MatrixD Parent_World, Vector3 Vector_Local) => Vector3.TransformNormal(Vector_Local, Parent_World);
}
sealed class MyCameraBlockTestHeight
{
    readonly IMyCameraBlock CameraBlock;
    public MyCameraBlockTestHeight(IMyCameraBlock Block)
    {
        CameraBlock = Block;
        UpdateInfos(100);
    }
    public bool CCT { get; set; }
    public double CH { get; set; }
    public Vector3D D { get; set; }
    public Vector3D P { get; set; }
    public static List<MyCameraBlockTestHeight> CreateCameraBlockParametersList(IMyTerminalBlock Me, IMyGridTerminalSystem GTS, string Tag = "TestDistance") =>
        Common.GetTs<IMyCameraBlock>(GTS, b => Me.IsSameConstructAs(b) && b.CustomName.EndsWith(Tag))?.ConvertAll(b => new MyCameraBlockTestHeight(b));
    public static Vector3D GetTorqueByCameras(List<MyCameraBlockTestHeight> THS, double TSSK)
    {
        double H = float.MaxValue;
        if (Common.IsNullC(THS))
            return Vector3D.Zero;
        Vector3D Center = CommonMathFunction.CalculateAvg(THS?.ConvertAll(T => T.P));
        H = THS?.Average(b => b.CH) ?? 0;
        var h = H;
        return CommonMathFunction.CalculateSum(THS?.ConvertAll(TH =>
        {
            var diff = SCPF.GetSpring(TH.CH - h, TSSK);
            var force = diff * CommonMathFunction.NormalizeD(TH.D);
            var arm = TH.P - Center;
            return force.Cross(arm);
        }));
    }
    public void UpdateInfos(float FloatHeight)
    {
        P = CameraBlock.GetPosition();
        D = CameraBlock.WorldMatrix.Forward;
        if (!CameraBlock.Enabled)
        {
            CH = 0;
            CCT = false;
            return;
        }
        if (!CameraBlock.EnableRaycast)
            CameraBlock.EnableRaycast = true;
        var Target = CameraBlock.Raycast(FloatHeight);
        if (Target.IsEmpty() || !Target.HitPosition.HasValue)
        {
            CH = 0;
            CCT = false;
            return;
        }
        CH = Vector3D.Distance(P, Target.HitPosition.Value);
        CCT = true;
    }
}
static class SCPF
{
    public static bool IsIonEngine(IMyThrust thrust)
    {
        if (thrust == null)
            return false;
        return IonThrustList.Contains(thrust.BlockDefinition.SubtypeId) || thrust.BlockDefinition.SubtypeId.Contains("Ion") || thrust.BlockDefinition.SubtypeId.Contains("ion");
    }
    public static float ControlWithVector(Vector3 Axe, Vector3 Target, Vector3 Current, float Epsilon = 0) => CommonMathFunction.SignedAngle(CommonMathFunction.ProjectOnPlane(Target, Axe, Epsilon), Current, Axe); public static double ControlWithVectorD(Vector3D Axe, Vector3D Target, Vector3D Current, double Epsilon = 0) => CommonMathFunction.SetNaNZero(CommonMathFunction.SignedAngleD(CommonMathFunction.ProjectOnPlaneD(Target, Axe, Epsilon), Current, Axe)); public static float ControlWithVectorLinear(Vector3 Axe, Vector3 Target, Vector3 Current, float Epsilon = 0) => CommonMathFunction.SignedAngle(CommonMathFunction.ProjectOnPlane(Target, Axe, Epsilon), Current, Axe); public static double ControlWithVectorLinearD(Vector3D Axe, Vector3D Target, Vector3D Current, double Epsilon = 0) => CommonMathFunction.SetNaNZero(CommonMathFunction.SignedAngleD(CommonMathFunction.ProjectOnPlaneD(Target, Axe, Epsilon), Current, Axe)); public static Vector3 Dampener_Mutlipy_Vector(Vector3 Local_ReferNormal, Vector3 Normal)
    {
        Vector3 Local_Diff = Normal - Local_ReferNormal;
        Vector3 Dampener_Temp = new Vector3(Local_Diff.Y, Local_Diff.Z, Local_Diff.X);
        return Dampener_Temp * Dampener_Temp;
    }
    public static double GetSpring(double x, double k = 1) => k * x * Math.Abs(x); public static Vector3D GetSuspensionAntiRoll(ICollection<IMyMotorSuspension> Wheels)
    {
        if (Common.IsNullC(Wheels))
            return Vector3D.Zero;
        Vector3D Center = CommonMathFunction.CalculateAvg(Wheels.Where(b => b.Top != null)?.ToList()?.ConvertAll(b => b.GetPosition()));
        return CommonMathFunction.CalculateSum(Wheels.Where(b => b.Top != null)?.ToList()?.ConvertAll(b => { var FL = CommonMathFunction.ProjectOnVectorD(b.Top.GetPosition() - b.GetPosition(), b.WorldMatrix.Backward); var Arm = b.GetPosition() - Center; return FL.Cross(Arm); }));
    }
    static List<string> IonThrustList = new List<string>() { "SmallBlockSmallThrust", "SmallBlockLargeThrust", "LargeBlockSmallThrust", "LargeBlockLargeThrust", "SmallBlockSmallThrustSciFi", "SmallBlockLargeThrustSciFi", "LargeBlockSmallThrustSciFi", "LargeBlockLargeThrustSciFi" };
}
enum MyRole : int { None, Aeroplane, Helicopter, VTOL, SpaceShip, SeaShip, Submarine, TrackVehicle, WheelVehicle, HoverVehicle }
#endregion
#region 常量定义
const string T_OnOffTag = @"ThrusterOnOff";
const string W_SetupTag = @"WheelsSetup";
const string STTag = @"SpeedTable";
const string CSTag = @"CommonSetup";
const string StablizeBlockTag = "TestDistance";
static float PitchLimitedValue = (float)Math.Sin(MathHelper.ToRadians(50));
static float RollLimitedValue = (float)Math.Sin(MathHelper.ToRadians(50));
const string callback_Tag = "<UC_Signal>";
const string ListenerTag = "UC_ThrustGyroscopeController";
const string ThrustGyroscopeTag = "UC_ThrustGyroscopeController";
const string WheelCtrlTag = "UC_Wheel";
#endregion