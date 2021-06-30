public Program() { Restart(); Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10 | UpdateFrequency.Update100; }
        MyUniversalController UniversalController { get; } = new MyUniversalController();
        public void Main(string argument, UpdateType updateSource) { try { switch (updateSource) { case UpdateType.Terminal: case UpdateType.Trigger: case UpdateType.Mod: case UpdateType.Script: case UpdateType.IGC: if (UniversalController.CL.TryParse(argument)) { if (UniversalController.CL.Switch("ControlLine")) { UniversalController.CL.Clear(); UniversalController.SwitchCommonds(argument); Storage = UniversalController.RestorgeCustomDataDirect(); } else if (UniversalController.CL.Switch("EditSetting")) { UniversalController.CL.Clear(); UniversalController.EditConfigs(argument); } else if (UniversalController.CL.Switch("SaveData")) { UniversalController.CL.Clear(); Storage = UniversalController.RestorgeCustomData(); } else if (UniversalController.CL.Switch("ChangeRole")) { UniversalController.CL.Clear(); UniversalController.EditRole(argument); Storage = UniversalController.RestorgeCustomDataDirect(); } } break; case UpdateType.Update1: UniversalController.Update(); break; case UpdateType.Update10: break; case UpdateType.Update100: break; case UpdateType.Once: break; default: break; } Echo($"{UniversalController.SCIFM.ToString()}"); Echo($"{UniversalController.SCIFM.SLM.ToString()}"); Echo($"Klang:{UniversalController.EKL}"); Echo($"Klang C:{UniversalController.khrust_Ms.Count}"); Echo($"Gravity:{UniversalController.EG}"); } catch (Exception) { Restart(); } }
        void Restart() { UniversalController.Restart(Me, GridTerminalSystem); UniversalController.LoadOrAddConfig(Storage); }
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
        sealed class MyUniversalController
        {
            readonly MyIni Configs = new MyIni();
            bool AT_OnOff = true;
            bool DockMode = false;
            IMyGridTerminalSystem GridTerminalSystem = null;
            bool HT_OnOff = true;
            bool IT_OnOff = true;
            IMyProgrammableBlock Me = null;
            float MS_Dock = 5f;
            float MS_Flight = 120;
            float MS_Hover = 30;
            float MS_Land = 50f;
            float MS_Sea = 30f;
            float MS_Space = 1000f;
            bool SICD_CS = false;
            bool SICD_SC = false;
            bool SICD_TC = false;
            bool SICD_WS = false;
            internal bool EKL = false;
            internal bool EG = false;
            internal MyCommandLine CL { get; } = new MyCommandLine();
            internal MySCDSP SCIFM { get; } = new MySCDSP();
            MyACDCR ACDC { get; } = new MyACDCR();
            MyGCtrl GSC { get; } = new MyGCtrl();
            MyThCtrl TSC { get; } = new MyThCtrl();
            MyVEM VEM { get; } = new MyVEM();
            MyWCtrl WSC { get; } = new MyWCtrl();
            MyGDC GDC { get; } = new MyGDC();
            internal List<Khrust_M> khrust_Ms { get; } = new List<Khrust_M>();
            public void Restart(IMyProgrammableBlock Me, IMyGridTerminalSystem GridTerminalSystem) { try { this.Me = Me; this.GridTerminalSystem = GridTerminalSystem; TSC.SCIFM = SCIFM; GSC.SCIFM = SCIFM; WSC.SCIFM = SCIFM; VEM.SCIFM = SCIFM; GDC.SCIFM = SCIFM; ForceUpdate(); Khrust_M.CreateKhrust_M(GridTerminalSystem, Me.IsSameConstructAs, khrust_Ms); } catch (Exception) { } }
            public void Update() { try { if (!SCIFM.UnabledControl) SCIFM.RefindCockpit(GridTerminalSystem, Me.IsSameConstructAs, StablizeBlockTag); SCIFM.Update(); if (!SCIFM.UnabledControl) GSC.Running(); if (!SCIFM.UnabledControl) TSC.Running(); if (!SCIFM.UnabledControl) WSC.Running(); if (!SCIFM.UnabledControl) VEM.Running(); UpdateKlang(); GDC.Running(EG); ACDC.Running(); } catch (Exception) { } }
            internal void UpdateKlang()
            {
                switch (SCIFM.Role)
                {

                    case MyRole.Aeroplane:
                    case MyRole.Helicopter:
                    case MyRole.VTOL:
                    case MyRole.SpaceShip:
                    case MyRole.SeaShip:
                    case MyRole.Submarine:
                    case MyRole.HoverVehicle:
                        Khrust_M.Running(khrust_Ms, EKL ? (SCIFM.TCS / 40) : Vector3D.Zero);
                        return;
                    case MyRole.TrackVehicle:
                    case MyRole.WheelVehicle:
                        Khrust_M.Running(khrust_Ms, CMF.VTransLocal2WorldD(SCIFM.WorldMatrix, SCIFM.MI_SC));
                        return;
                    default:
                        Khrust_M.Running(khrust_Ms, Vector3D.Zero);
                        return;
                }
            }
            internal void EditRole(string CommondLine)
            {
                if (!CL.TryParse(CommondLine))
                    return;
                if (CL.Switch("None"))
                    SCIFM.Role = MyRole.None;
                else if (CL.Switch("Aeroplane"))
                    SCIFM.Role = MyRole.Aeroplane;
                else if (CL.Switch("Helicopter"))
                    SCIFM.Role = MyRole.Helicopter;
                else if (CL.Switch("VTOL"))
                    SCIFM.Role = MyRole.VTOL;
                else if (CL.Switch("SpaceShip"))
                    SCIFM.Role = MyRole.SpaceShip;
                else if (CL.Switch("SeaShip"))
                    SCIFM.Role = MyRole.SeaShip;
                else if (CL.Switch("Submarine"))
                    SCIFM.Role = MyRole.Submarine;
                else if (CL.Switch("TrackVehicle"))
                    SCIFM.Role = MyRole.TrackVehicle;
                else if (CL.Switch("WheelVehicle"))
                    SCIFM.Role = MyRole.WheelVehicle;
                else if (CL.Switch("HoverVehicle"))
                    SCIFM.Role = MyRole.HoverVehicle;
                CL.Clear();
            }
            internal void EditConfigs(string CommondLine)
            {
                if (!CL.TryParse(CommondLine))
                    return;
                SICD_CS = CL.Switch("SICD_CommonSetup") ^ SICD_CS;
                SICD_SC = CL.Switch("SICD_SpeedConfig") ^ SICD_SC;
                SICD_TC = CL.Switch("SICD_ThrusterConfig") ^ SICD_TC;
                SICD_WS = CL.Switch("SICD_WheelsSetup") ^ SICD_WS;
                CL.Clear();
                ShowInCustomData();
            }
            internal void LoadOrAddConfig(string Storge)
            {
                //KSE_Configs[]
                if (!Configs.TryParse(Storge))
                {
                    SCS();
                    SSC();
                    STC();
                    SWS();
                }
                //MyConfigs.Read_INI(Storge, Configs);
                LCS();
                LSC();
                switch (SCIFM.Role)
                {
                    case MyRole.Aeroplane:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Flight;
                        break;
                    case MyRole.Helicopter:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Hover;
                        break;
                    case MyRole.VTOL:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.HoverMode ? MS_Hover : SCIFM.NoGravity ? MS_Space : MS_Flight;
                        break;
                    case MyRole.SpaceShip:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.HoverMode ? MS_Hover : SCIFM.NoGravity ? MS_Space : MS_Flight;
                        break;
                    case MyRole.SeaShip:
                    case MyRole.Submarine:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Sea;
                        break;
                    case MyRole.TrackVehicle:
                    case MyRole.WheelVehicle:
                    case MyRole.HoverVehicle:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Land;
                        break;
                    default:
                        SCIFM.MSL = 100;
                        break;
                }
                LTC();
                LWS();
                if (Common.IsNull(Me))
                    return;
            }
            internal string RestorgeCustomData()
            {
                if (Common.IsNull(Me))
                    return "";
                Configs.TryParse(Me.CustomData);
                SICD_CS = false;
                SICD_SC = false;
                SICD_TC = false;
                SICD_WS = false;
                LCS();
                LSC();
                LTC();
                LWS();
                switch (SCIFM.Role)
                {
                    case MyRole.Aeroplane:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Flight;
                        break;
                    case MyRole.Helicopter:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Hover;
                        break;
                    case MyRole.VTOL:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.HoverMode ? MS_Hover : SCIFM.NoGravity ? MS_Space : MS_Flight;
                        break;
                    case MyRole.SpaceShip:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.HoverMode ? MS_Hover : SCIFM.NoGravity ? MS_Space : MS_Flight;
                        break;
                    case MyRole.SeaShip:
                    case MyRole.Submarine:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Sea;
                        break;
                    case MyRole.TrackVehicle:
                    case MyRole.WheelVehicle:
                    case MyRole.HoverVehicle:
                        SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Land;
                        break;
                    default:
                        SCIFM.MSL = 100;
                        break;
                }
                return Configs.ToString();
            }
            internal string RestorgeCustomDataDirect()
            {
                if (Common.IsNull(Me))
                    return "";
                SCS();
                SSC();
                STC();
                SWS();
                var context = Configs.ToString();
                Me.CustomData = context;
                return context;
            }
            internal void ShowInCustomData()
            {
                if (Common.IsNull(Me)) return;
                if (SICD_CS)
                {
                    if (!Configs.ContainsSection(CSTag))
                        LCS();
                }
                if (SICD_SC)
                {
                    if (!Configs.ContainsSection(STTag))
                        LSC();
                }
                if (SICD_TC)
                {
                    if (!Configs.ContainsSection(T_OnOffTag))
                        LTC();
                }
                if (SICD_WS)
                {
                    if (!Configs.ContainsSection(W_SetupTag))
                        LWS();
                }
                Me.CustomData = Configs.ToString();
            }
            internal void SwitchCommonds(string CommondLine)
            {
                if (!CL.TryParse(CommondLine))
                    return;
                SCIFM.HasWings = CL.Switch("HasWings") ^ SCIFM.HasWings;
                SCIFM.CHLSM = CL.Switch("SeaLevel") ^ SCIFM.CHLSM;
                SCIFM.CAG = CL.Switch("AllGravity") ^ SCIFM.CAG;
                WSC.DA = CL.Switch("DisabledAssist") ^ WSC.DA;
                if (CL.Switch("Hover") || CL.Switch("DockMode") || CL.Switch("Cruise"))
                {
                    DockMode = CL.Switch("DockMode") ^ SCIFM.CAG;
                    SCIFM.HoverMode = CL.Switch("Hover") ^ SCIFM.HoverMode;
                    SCIFM.CruiseMode = CL.Switch("Cruise") ^ SCIFM.CruiseMode;

                    switch (SCIFM.Role)
                    {
                        case MyRole.Aeroplane:
                            SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Flight;
                            break;
                        case MyRole.Helicopter:
                            SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Hover;
                            break;
                        case MyRole.VTOL:
                            SCIFM.MSL = DockMode ? MS_Dock : SCIFM.HoverMode ? MS_Hover : SCIFM.NoGravity ? MS_Space : MS_Flight;
                            break;
                        case MyRole.SpaceShip:
                            SCIFM.MSL = DockMode ? MS_Dock : SCIFM.HoverMode ? MS_Hover : SCIFM.NoGravity ? MS_Space : MS_Flight;
                            break;
                        case MyRole.SeaShip:
                        case MyRole.Submarine:
                            SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Sea;
                            break;
                        case MyRole.TrackVehicle:
                        case MyRole.WheelVehicle:
                        case MyRole.HoverVehicle:
                            SCIFM.MSL = DockMode ? MS_Dock : SCIFM.NoGravity ? 0 : MS_Land;
                            break;
                        default:
                            SCIFM.MSL = 100;
                            break;
                    }
                }
                EKL = CL.Switch("EnabledKLang") ^ EKL;
                EG = CL.Switch("EnabledGravity") ^ EG;
                if (CL.Switch("IonThrust_OnOff") || CL.Switch("AtomThrust_OnOff") || CL.Switch("HydgenThrust_OnOff"))
                {
                    IT_OnOff = CL.Switch("IonThrust_OnOff") ^ IT_OnOff;
                    AT_OnOff = CL.Switch("AtomThrust_OnOff") ^ AT_OnOff;
                    HT_OnOff = CL.Switch("HydgenThrust_OnOff") ^ HT_OnOff;
                    TSC.ENCT.Clear();
                    if (!IT_OnOff) TSC.ENCT.Add("Ion");
                    if (!AT_OnOff) TSC.ENCT.Add("Atmospheric");
                    if (!HT_OnOff) TSC.ENCT.Add("Hydrogen");
                }
            }
            void ForceUpdate() { try { SCIFM.ForceUpdate(GridTerminalSystem, Me.IsSameConstructAs, StablizeBlockTag); SCIFM.Update(); if (!SCIFM.UnabledControl) GSC.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); if (!SCIFM.UnabledControl) TSC.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); if (!SCIFM.UnabledControl) WSC.FUD(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); if (!SCIFM.UnabledControl) VEM.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); ACDC.UpdateBlocks(GridTerminalSystem); GDC.FU(GridTerminalSystem, SCIFM.Me.IsSameConstructAs); } catch (Exception) { } }
            void LCS() { if (!Configs.ContainsSection(CSTag)) SCS(); SCIFM.Enabled = Configs.Get(CSTag, "Enabled").ToBoolean(); DockMode = Configs.Get(CSTag, "DockMode").ToBoolean(); SCIFM.Role = ParseControllerRole(Configs.Get(CSTag, "Role").ToString()); SCIFM.DH = Configs.Get(CSTag, "DetectorHeight").ToSingle(); SCIFM.HoverMode = Configs.Get(CSTag, "HoverMode").ToBoolean(); SCIFM.CruiseMode = Configs.Get(CSTag, "CruiseMode").ToBoolean(); SCIFM.HasWings = Configs.Get(CSTag, "HasWings").ToBoolean(); SCIFM.CAG = Configs.Get(CSTag, "CalculateAllGravity").ToBoolean(); SCIFM.CHLSM = Configs.Get(CSTag, "CurrentHeightSeaLevelMode").ToBoolean(); SCIFM.MAV = Configs.Get(CSTag, "MaxAngularVelocity").ToSingle(); SCIFM.SafetyStage = Configs.Get(CSTag, "SafetyStage").ToSingle(); SCIFM.AVRC = Configs.Get(CSTag, "AssignedVelocityRate").ToSingle(); SCIFM.ARS_K = Configs.Get(CSTag, "AntiRollK").ToSingle(); SCIFM.HSS_K = Configs.Get(CSTag, "HeightSpringK").ToSingle(); SCIFM.HDS = Configs.Get(CSTag, "HeightDampener").ToSingle(); SCIFM.MDS = Configs.Get(CSTag, "MaxiumDownSpeed").ToSingle(); SCIFM.DF_Acc = Configs.Get(CSTag, "DownForce").ToSingle(); SCIFM.AD_Pitch = Configs.Get(CSTag, "AngularDampener_Pitch").ToSingle(); SCIFM.AD_Yaw = Configs.Get(CSTag, "AngularDampener_Yaw").ToSingle(); SCIFM.AD_Roll = Configs.Get(CSTag, "AngularDampener_Roll").ToSingle(); SCIFM.MaxTargetSpeed = Configs.Get(CSTag, "MaxTargetSpeed").ToSingle(); SCIFM.VelocitySensitive = Configs.Get(CSTag, "VelocitySensitive").ToSingle(); GSC.PS3A.X = Configs.Get(CSTag, "GyroMultipy_Pitch").ToSingle(); GSC.PS3A.Y = Configs.Get(CSTag, "GyroMultipy_Yaw").ToSingle(); GSC.PS3A.Z = Configs.Get(CSTag, "GyroMultipy_Roll").ToSingle(); }
            void LSC() { if (!Configs.ContainsSection(STTag)) SSC(); MS_Hover = Configs.Get(STTag, "MaxiumSpeed_Hover").ToSingle(); MS_Flight = Configs.Get(STTag, "MaxiumSpeed_Flight").ToSingle(); MS_Dock = Configs.Get(STTag, "MS_Dock").ToSingle(); MS_Land = Configs.Get(STTag, "MaxiumSpeed_Land").ToSingle(); MS_Sea = Configs.Get(STTag, "MaxiumSpeed_Sea").ToSingle(); MS_Space = Configs.Get(STTag, "MaxiumSpeed_Space").ToSingle(); }
            void LTC() { if (!Configs.ContainsSection(T_OnOffTag)) STC(); IT_OnOff = Configs.Get(T_OnOffTag, "IonThrust_OnOff").ToBoolean(); AT_OnOff = Configs.Get(T_OnOffTag, "AtomThrust_OnOff").ToBoolean(); HT_OnOff = Configs.Get(T_OnOffTag, "HydgenThrust_OnOff").ToBoolean(); EKL = Configs.Get(T_OnOffTag, "EnabledKLang").ToBoolean(); EG = Configs.Get(T_OnOffTag, "EnabledGravity").ToBoolean(); TSC.ENCT.Clear(); if (!IT_OnOff) TSC.ENCT.Add("Ion"); if (!AT_OnOff) TSC.ENCT.Add("Atmospheric"); if (!HT_OnOff) TSC.ENCT.Add("Hydrogen"); }
            void LWS() { if (!Configs.ContainsSection(W_SetupTag)) SWS(); WSC.CGM = Configs.Get(W_SetupTag, "CurrentGearMode").ToInt32(); WSC.CGO = Configs.Get(W_SetupTag, "CurrentGearOverride").ToInt32(); if (WSC.CGO == 0) WSC.CGO = null; WSC.MR = Configs.Get(W_SetupTag, "WRotorMaxiumRpm").ToSingle(); WSC.MTA = MathHelper.ToRadians(Configs.Get(W_SetupTag, "MaxTurnAngle").ToSingle()); WSC.MTAN = MathHelper.ToRadians(Configs.Get(W_SetupTag, "MinTurnAngle").ToSingle()); WSC.PPM = Configs.Get(W_SetupTag, "PowerMult").ToSingle(); WSC.TSM = Configs.Get(W_SetupTag, "TurnSlippingMult").ToSingle(); WSC.Frc = Configs.Get(W_SetupTag, "Friction").ToSingle(); WSC.SH = Configs.Get(W_SetupTag, "SuspensionHeight").ToSingle(); WSC.DRP = Configs.Get(W_SetupTag, "DiffRpmPercentage").ToSingle(); WSC.FAR = Configs.Get(W_SetupTag, "FrontAngleRate").ToSingle(); WSC.RTR = Configs.Get(W_SetupTag, "RearWheelTurnRate").ToSingle(); WSC.DST = Configs.Get(W_SetupTag, "DisabledSpinTurn").ToBoolean(); WSC.DA = Configs.Get(W_SetupTag, "DisabledAssist").ToBoolean(); WSC.GRBP = Configs.Get(W_SetupTag, "FrictionBalancePercentage").ToSingle(); WSC.FBP = Configs.Get(W_SetupTag, "PowerBalancePercentage").ToSingle(); }
            void SCS() { if (!Configs.ContainsSection(CSTag)) Configs.AddSection(CSTag); Configs.Set(CSTag, "Enabled", SCIFM.Enabled.ToString()); Configs.Set(CSTag, "Role", SCIFM.Role.ToString()); Configs.Set(CSTag, "DockMode", DockMode.ToString()); Configs.Set(CSTag, "HoverMode", SCIFM.HoverMode.ToString()); Configs.Set(CSTag, "CruiseMode", SCIFM.CruiseMode.ToString()); Configs.Set(CSTag, "HasWings", SCIFM.HasWings.ToString()); Configs.Set(CSTag, "CalculateAllGravity", SCIFM.CAG.ToString()); Configs.Set(CSTag, "MaxAngularVelocity", SCIFM.MAV.ToString()); Configs.Set(CSTag, "SafetyStage", SCIFM.SafetyStage.ToString()); Configs.Set(CSTag, "AssignedVelocityRate", SCIFM.AVRC.ToString()); Configs.Set(CSTag, "AntiRollK", SCIFM.ARS_K.ToString()); Configs.Set(CSTag, "HeightSpringK", SCIFM.HSS_K.ToString()); Configs.Set(CSTag, "HeightDampener", SCIFM.HDS.ToString()); Configs.Set(CSTag, "MaxiumDownSpeed", SCIFM.MDS.ToString()); Configs.Set(CSTag, "AngularDampener_Pitch", SCIFM.AD_Pitch.ToString()); Configs.Set(CSTag, "AngularDampener_Yaw", SCIFM.AD_Yaw.ToString()); Configs.Set(CSTag, "AngularDampener_Roll", SCIFM.AD_Roll.ToString()); Configs.Set(CSTag, "DetectorHeight", SCIFM.DH.ToString()); Configs.Set(CSTag, "DownForce", SCIFM.DF_Acc.ToString()); Configs.Set(CSTag, "GyroMultipy_Pitch", GSC.PS3A.X.ToString()); Configs.Set(CSTag, "GyroMultipy_Yaw", GSC.PS3A.Y.ToString()); Configs.Set(CSTag, "GyroMultipy_Roll", GSC.PS3A.Z.ToString()); Configs.Set(CSTag, "VelocitySensitive", SCIFM.VelocitySensitive.ToString()); }
            void SSC() { if (!Configs.ContainsSection(STTag)) Configs.AddSection(STTag); Configs.Set(STTag, "MaxiumSpeed_Hover", MS_Hover.ToString()); Configs.Set(STTag, "MaxiumSpeed_Flight", MS_Flight.ToString()); Configs.Set(STTag, "MS_Dock", MS_Dock.ToString()); Configs.Set(STTag, "MaxiumSpeed_Land", MS_Land.ToString()); Configs.Set(STTag, "MaxiumSpeed_Sea", MS_Sea.ToString()); Configs.Set(STTag, "MaxiumSpeed_Space", MS_Space.ToString()); }
            void STC() { if (!Configs.ContainsSection(T_OnOffTag)) Configs.AddSection(T_OnOffTag); Configs.Set(T_OnOffTag, "IonThrust_OnOff", IT_OnOff.ToString()); Configs.Set(T_OnOffTag, "AtomThrust_OnOff", AT_OnOff.ToString()); Configs.Set(T_OnOffTag, "HydgenThrust_OnOff", HT_OnOff.ToString()); Configs.Set(T_OnOffTag, "EnabledKLang", EKL.ToString()); Configs.Set(T_OnOffTag, "EnabledGravity", EG.ToString()); }
            void SWS() { if (!Configs.ContainsSection(W_SetupTag)) Configs.AddSection(W_SetupTag); Configs.Set(W_SetupTag, "CurrentGearMode", WSC.CGM.ToString()); Configs.Set(W_SetupTag, "CurrentGearOverride", WSC.CGO.ToString()); Configs.Set(W_SetupTag, "WRotorMaxiumRpm", WSC.MR.ToString()); Configs.Set(W_SetupTag, "MaxTurnAngle", MathHelper.ToDegrees(WSC.MTA).ToString()); Configs.Set(W_SetupTag, "MinTurnAngle", MathHelper.ToDegrees(WSC.MTAN).ToString()); Configs.Set(W_SetupTag, "PowerMult", WSC.PPM.ToString()); Configs.Set(W_SetupTag, "TurnSlippingMult", WSC.TSM.ToString()); Configs.Set(W_SetupTag, "Friction", WSC.Frc.ToString()); Configs.Set(W_SetupTag, "SuspensionHeight", (-WSC.SH).ToString()); Configs.Set(W_SetupTag, "DiffRpmPercentage", WSC.DRP.ToString()); Configs.Set(W_SetupTag, "FrontAngleRate", WSC.FAR.ToString()); Configs.Set(W_SetupTag, "RearWheelTurnRate", WSC.RTR.ToString()); Configs.Set(W_SetupTag, "DisabledSpinTurn", WSC.DST.ToString()); Configs.Set(W_SetupTag, "DisabledAssist", WSC.DA.ToString()); Configs.Set(W_SetupTag, "FrictionBalancePercentage", WSC.GRBP.ToString()); Configs.Set(W_SetupTag, "PowerBalancePercentage", WSC.FBP.ToString()); }
            public static MyRole ParseControllerRole(string str) { MyRole value; if (!Enum.TryParse(str, out value)) value = default(MyRole); return value; }
        }
        sealed class MySCDSP
        {
            public bool Enabled { get; set; } = true;
            public bool HoverMode { get { return _HoverMode; } set { if (value == _HoverMode) return; _HoverMode = value; SLM.ForceUpdateHeight(); } }
            private bool _HoverMode = true;
            public bool CruiseMode { get; set; } = false;
            public bool HasWings { get; set; } = false;
            public float AD_Pitch { get; set; } = 10;
            public float AD_Roll { get; set; } = 10;
            public float AD_Yaw { get; set; } = 10;
            public float ARS_K { get; set; } = 1;
            public float DH { get; set; } = 50f;
            public float DF_Acc { get; set; } = 1.5f;
            public bool EAD { get; set; } = true;
            public bool ERE { get; set; } = false;
            public bool CHLSM { get; set; } = false;
            public float AVRC { get; set; } = 1f;
            public bool CAG { get; set; } = false;
            public float HDS { get; set; } = 1;
            public float HSS_K { get; set; } = 2f;
            public float MAV { get; set; } = 1f;
            public float MDS { get; set; } = 10f;
            List<MyCBPS> TestHeights; public MySCDSP()
            {
                SLM.SCIFM = this;
            }
            public float VelocitySensitive { get; set; } = 1;
            Vector3D AD => new Vector3D(Math.Abs(AD_Pitch), Math.Abs(AD_Yaw), Math.Abs(AD_Roll));
            public Vector3 AngularVelocity => Me?.GetShipVelocities().AngularVelocity ?? Vector3.Zero;
            public Vector3D GCS { get; private set; } = Vector3D.Zero;
            public Vector3D TCS { get; private set; } = Vector3D.Zero;
            public Vector3 Gravity => (CAG ? Me?.GetTotalGravity() : Me?.GetNaturalGravity()) ?? (GravityDirection * 10);
            public Vector3D GravityDirection => GGD();
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
                TestHeights = MyCBPS.CreateCameraBlockParametersList(Me, GridTerminalSystem, StablizeBlockTag);
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
                Vector3D ResultLinnerVelocity = LinearVelocity - CMF.VTransLocal2WorldD(WorldMatrix, new Vector3(MI_SC.X, 0, MI_SC.Z)) * MSL;
                if (!HoverMode)
                    ResultLinnerVelocity = ResultLinnerVelocity - CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward) + WorldMatrix.Backward * MSL * 8f;
                return ResultLinnerVelocity + Gravity;
            }
            public Vector3 GPN4VectorEngine_OnlyNormal()
            {
                double MaxLength = Gravity.Length();
                Vector3D ResultLinnerVelocity = LinearVelocity - CMF.VTransLocal2WorldD(WorldMatrix, new Vector3(MI_SC.X, 0, MI_SC.Z)) * MSL;
                if (!HoverMode)
                    ResultLinnerVelocity = ResultLinnerVelocity - CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
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
                        EAD = HandBrake || GravityDirection == Vector3.Zero;
                        break;
                    case MyRole.Aeroplane:
                    case MyRole.VTOL:
                        EAD = HandBrake || GravityDirection == Vector3.Zero || (!HasWings);
                        break;
                    default:
                        EAD = true;
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
                return CMF.NormalizeD(PositionP - Me.GetPosition());
            }
            Vector3D GPN()
            {
                Vector3D _RLV = Vector3D.Zero;
                Vector3D ControlLine = CMF.VTransLocal2WorldD(WorldMatrix, new Vector3(RotationIndicator.Z, 0, -RotationIndicator.X)) * MSL;
                switch (Role)
                {
                    case MyRole.Aeroplane:
                        _RLV = LinearVelocity - CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
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
                            _RLV = LinearVelocity - CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
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
                                    _RLV = LinearVelocity - CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
                                    if (_RLV.Length() < (MSL * 0.1f))
                                        _RLV = Vector3.Zero;
                                }
                            }
                            else
                            {
                                _RLV = LinearVelocity - CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
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
                            _RLV = LinearVelocity - CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward);
                        }
                        else
                        {
                            _RLV = LinearVelocity - (HoverMode ? Vector3D.Zero : CMF.ProjectOnVectorD(LinearVelocity, WorldMatrix.Forward));
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
                return Vector3D.ClampToSphere(CMF.ProjectOnPlaneD(Velocity, CMF.NormalizeD(Gravity)), 10 * (1 - r)) + Gravity;
            }
            Vector3D PMG_Sub(Vector3D Velocity, double r, bool SafeMode)
            {
                var pv = CMF.ProjectOnVectorD(Velocity, WorldMatrix.Right);
                if (NoGravity)
                    return pv;
                return Vector3D.ClampToSphere(CMF.ProjectOnPlaneD(pv, CMF.NormalizeD(Gravity)), 10 * (1 - r)) + Gravity * (SafeMode ? 1 : (float)r);
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
                        ARSKM = 10;
                        break;
                    default:
                        ARSKM = 2;
                        break;
                }
                if (Role == MyRole.None)
                {
                    GCS = Vector3D.Zero;
                    return;
                }
                Vector3D N = ReferNormal ?? GPN();
                var non_N = Vector3D.IsZero(N);
                Vector3D GCSMultipy = AD * SLDVM * ARSKM * MAV;
                if (non_N)
                {
                    GCS = CMF.VTransLocal2WorldD(WorldMatrix, PDFC(RotationIndicator * 100) * GCSMultipy);
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
                        LDP = SCPF.ControlWithVectorD(WorldMatrix.Right, N, WorldMatrix.Down);
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
                        Vector3D ControllerMult = Vector3D.One * ARS_K;
                        if (TestHeights != null) foreach (var TestHeight in TestHeights) TestHeight.UpdateInfos(DH);
                        var ct = MyCBPS.GetTorqueByCameras(TestHeights, ARSKM * ARS_K);
                        if (!Vector3D.IsZero(ct)) ControllerMult = new Vector3D(0.001f, 1000f, 0.001f);
                        GCS = CMF.VTransLocal2WorldD(WorldMatrix, LT * AD * ControllerMult) + ct;
                        return;
                    default:
                        GCS = CMF.VTransLocal2WorldD(WorldMatrix, LT * AD);
                        return;
                }
            }
            void ProcessThrustsCS()
            {
                if (UnabledControl || !Enabled || Role == MyRole.None) { TCS = Vector3D.Zero; return; }
                var MIW = Vector3D.Zero;
                var Gravity = CAG ? Me.GetTotalGravity() : Me.GetNaturalGravity();
                if (!HandBrake)
                {
                    if (HoverMode) { MIW = CMF.VTransLocal2WorldD(WorldMatrix, MoveIndicator) * MSL; }
                    else
                    {
                        float AccOrDec = MoveIndicator.Dot(Vector3D.Forward);
                        if (AccOrDec > 0) MaxTargetSpeed = MathHelper.Clamp(Math.Max(LinearVelocity.Dot(WorldMatrix.Forward), 0), 0, MSL);
                        if (AccOrDec < 0) MaxTargetSpeed = 0; MIW = WorldMatrix.Forward * (float)((AccOrDec > 0) ? MSL : MaxTargetSpeed);
                    }
                }
                float DFAC = 0;
                if (Role == MyRole.WheelVehicle || Role == MyRole.TrackVehicle)
                {
                    var speed_cav = MathHelper.Clamp(Math.Max(MathHelper.Clamp(LinearVelocity.Length(), 0, 100), 0) / Math.Max((MathHelper.Clamp(MSL, 30, 100)), 1), 0, 1);
                    DFAC = -(float)speed_cav * DF_Acc;
                }
                float HSKX = MathHelper.Clamp((float)SCPF.GetSpring(SLM.Diff_Level, HSS_K) * (1 - MathHelper.Clamp(HDS, 0.01f, 0.99f)), -10, 10);
                //float HRKS = Math.Max((float)Gravity.Dot(LinearVelocity), 0);
                //float SpeedLimit_Up = MathHelper.Clamp(MDS, 0, MSL);
                //float SpeedLimit_Down = MathHelper.Clamp(MDS, 0, Math.Min(Math.Max(MSL, 0), SLM.Current_Height * AVRC));
                //if (HRKS != 0)
                //{
                //    HRKS /= (float)Gravity.Length();
                //    HRKS = (HRKS > MathHelper.Clamp(MDS, -SpeedLimit_Up, SpeedLimit_Down)) ? (CMF.SmoothValue(HRKS) * HDS) : 0;
                //}
                //HSKX += HRKS;
                float G_Mult;
                switch (Role)
                {
                    case MyRole.Aeroplane: G_Mult = 1f; HSKX = CruiseMode ? HSKX : 0; break;
                    case MyRole.SpaceShip: case MyRole.VTOL: if (HoverMode) { G_Mult = SLM.HeightChangedEnable ? -0.9f : 0.9f; } else { G_Mult = 1f; HSKX = 0; } break;
                    case MyRole.SeaShip: G_Mult = 1f; break;
                    case MyRole.Helicopter: case MyRole.Submarine: case MyRole.HoverVehicle: G_Mult = SLM.HeightChangedEnable ? -0.9f : 0.9f; break;
                    default: G_Mult = 0.9f; break;
                }
                TCS = CMF.VTransLocal2WorldD(WorldMatrix, CMF.SmoothValueD(CMF.VTransWorld2LocalD(WorldMatrix, MIW - LinearVelocity - (Gravity * (HSKX + G_Mult + DFAC)))) * SLDVM);
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
                        { RotationIndicator = RI_SC + (ERE ? new Vector3(0, 0, MI_SC.X) : Vector3.Zero); }
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
            Vector3D PDFC(Vector3 RI) { var av = CMF.VTransWorld2Local(WorldMatrix, AngularVelocity); av = av * 0.2f + RI; return CMF.SmoothValueD(av); }
            internal bool TryGetPlanetElevation(MyPlanetElevation Mode, out double elevation) { elevation = double.MaxValue; return Me?.TryGetPlanetElevation(Mode, out elevation) ?? false; }
            StringBuilder CI { get; } = new StringBuilder();
        }
        public class Khrust_M { public static void CreateKhrust_M(IMyGridTerminalSystem GridTerminalSystem, Func<IMyTerminalBlock, bool> ITE, List<Khrust_M> KMS) { List<IMyMotorStator> Motors = Common.GetTs<IMyMotorStator>(GridTerminalSystem, ITE); if (Motors == null || KMS == null) return; KMS.Clear(); foreach (var Motor in Motors) { CreateKhrust_M_P(Motor, KMS); } } private static void CreateKhrust_M_P(IMyMotorStator Motor, List<Khrust_M> KMS) { if (Motor == null || KMS == null) return; IMyMotorStator K_Base = Motor; var BG = K_Base.CubeGrid; if (BG == null) return; var size = (BG.GridSizeEnum == VRage.Game.MyCubeSize.Large ? 2.5 : 0.5); IMyDoor K_Door = null; for (int index = 1; index < 6; index++) { var position = BG.WorldToGridInteger(K_Base.WorldMatrix.Translation + K_Base.WorldMatrix.Up * size * index); if (!BG.CubeExists(position)) continue; K_Door = BG.GetCubeBlock(position).FatBlock as IMyDoor; if (K_Door != null) break; } if (K_Door == null) return; KMS.Add(new Khrust_M(K_Base, K_Door)); } public static void Running(List<Khrust_M> khrusts, Vector3D Force) { if (Common.IsNullC(khrusts)) return; foreach (var khrust in khrusts) khrust.RF(Force); } private Khrust_M(IMyMotorStator K_Base, IMyDoor K_Door) { this.K_Base = K_Base; this.K_Door = K_Door; if (this.K_Base != null) { if (!this.K_Base.CustomName.Contains(ClangEngineTag)) this.K_Base.CustomName += $" ({ClangEngineTag})"; this.K_Base.TargetVelocityRad = 0; this.K_Base.ShowInTerminal = false; this.K_Base.ShowOnHUD = false; this.K_Base.ShowInToolbarConfig = false; } if (this.K_Door != null) { if (!this.K_Door.CustomName.Contains(ClangEngineTag)) this.K_Door.CustomName += $" ({ClangEngineTag})"; this.K_Door.ShowInTerminal = false; this.K_Door.ShowOnHUD = false; this.K_Door.ShowInToolbarConfig = false; } } private void RF(Vector3D Force) { if (Common.IsNull(K_Base)) return; if (Vector3D.IsZero(Force)) K_Base.Displacement = P0P; else K_Base.Displacement = (float)MathHelper.Clamp(Force.Dot(K_Base.WorldMatrix.Down), P0P, P100P); if (Common.IsNull(K_Door)) return; if (K_Door.Enabled) K_Door.Enabled = false; } IMyMotorStator K_Base; IMyDoor K_Door; float P0P => K_Base.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? -0.4f : -0.02f; float P100P => K_Base.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large ? -0.02f : 0.11f; }
        sealed class MyGDC { internal MySCDSP SCIFM { get; set; } IMyBlockGroup GDs; List<IMyGravityGenerator> DS; List<IMyArtificialMassBlock> MP; public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { GDs = GTS.GetBlockGroupWithName(GravityDriveNM); DS = Common.GetTs<IMyGravityGenerator>(GDs, ITE); MP = Common.GetTs<IMyArtificialMassBlock>(GDs, ITE); if (!Common.IsNullC(DS)) foreach (var d in DS) { d.ShowInTerminal = false; d.ShowInToolbarConfig = false; } if (!Common.IsNullC(MP)) foreach (var mp in MP) { mp.ShowInTerminal = false; mp.ShowInToolbarConfig = false; } } public void Running(bool EnabledKLang) { if (Common.IsNullC(DS) || Common.IsNullC(MP)) return; if (!EnabledKLang) { foreach (var Driver in DS) { if (Driver == null) continue; if (Driver.Enabled) Driver.Enabled = false; } foreach (var MassProvider in MP) { if (MassProvider == null) continue; if (MassProvider.Enabled) MassProvider.Enabled = false; } return; } foreach (var Driver in DS) { if (Driver == null) continue; if (!Driver.Enabled) Driver.Enabled = true; Driver.GravityAcceleration = (float)SCIFM.TCS.Dot(Driver.WorldMatrix.Down) * 20; } foreach (var MassProvider in MP) { if (MassProvider == null) continue; if (!MassProvider.Enabled) MassProvider.Enabled = true; } } }
        sealed class MyVEM { List<IMyMotorStator> Motors; internal MySCDSP SCIFM { get; set; } public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { if (GTS == null || ITE == null) return; Motors = Common.GetTs<IMyMotorStator>(GTS, b => ITE(b) && b.CustomName.Contains("Engine")); } public void Running() { if (SCIFM == null) return; if (Common.IsNullC(Motors) || SCIFM.UnabledControl) { SCIFM.ERE = false; return; } else SCIFM.ERE = true; var direction = SCIFM.GPN4VectorEngine(); var direction_onlynormal = SCIFM.GPN4VectorEngine_OnlyNormal(); foreach (var Motor in Motors) { if (Motor == null || Motor.Top == null) continue; var vector = Motor.Top.WorldMatrix.Forward; if (Motor.CustomName.Contains("Backward")) vector = Motor.Top.WorldMatrix.Backward; else if (Motor.CustomName.Contains("Backward")) vector = Motor.Top.WorldMatrix.Backward; else if (Motor.CustomName.Contains("Up")) vector = Motor.Top.WorldMatrix.Up; else if (Motor.CustomName.Contains("Down")) vector = Motor.Top.WorldMatrix.Down; else if (Motor.CustomName.Contains("Left")) vector = Motor.Top.WorldMatrix.Left; else if (Motor.CustomName.Contains("Right")) vector = Motor.Top.WorldMatrix.Right; else if (Motor.CustomName.Contains("Forward")) vector = Motor.Top.WorldMatrix.Forward; float value; if ((Vector3.IsZero(direction) && !Motor.CustomName.Contains("Normal")) || (Vector3.IsZero(direction_onlynormal) && Motor.CustomName.Contains("Normal"))) value = -Motor.Angle; else value = (float)SCPF.ControlWithVector(Motor.WorldMatrix.Up, Motor.CustomName.Contains("Normal") ? direction_onlynormal : direction, -vector) * SCIFM.SLDVM; if (float.IsNaN(value)) value = 0; Motor.TargetVelocityRad = CMF.SmoothValue(value); } } }
        sealed class MyWCtrl { internal const float MSAS = 30f; const float SGG = UGG / 2f; const float UGG = 0.9f; static readonly Dictionary<int, Dictionary<int, float>> GMS = new Dictionary<int, Dictionary<int, float>>() { { 1, new Dictionary<int, float>() { { 1, 30 }, { 2, 50 }, { 3, 70 }, { 4, 90 }, { 5, 120 }, { 6, float.PositiveInfinity } } }, { 2, new Dictionary<int, float>() { { 1, 30 }, { 2, 80 }, { 3, 120 }, { 4, float.PositiveInfinity } } }, { 3, new Dictionary<int, float>() { { 1, float.PositiveInfinity } } } }; static readonly Dictionary<int, int> GMSD = new Dictionary<int, int>() { { 1, 10 }, { 2, 2 }, { 3, 0 } }; int _CG = 1; float _TM = 0.75f; List<IMyInteriorLight> BKL; List<IMyInteriorLight> BWL; int CT = 0; float FI = 0; List<IMyLandingGear> LGS; List<IMyTerminalBlock> MH; List<IMyMotorStator> MWS; List<IMyPistonBase> PT; List<IMyShipConnector> SCS; List<IMyMotorSuspension> SWS; IMyBlockGroup WS; public int CGM { get; set; } = 1; public int? CGO { get; set; } = null; public int CurrentGear => CGO ?? _CG; public float CurrentMaxiumSpeed { get; set; } public bool DA { get; set; } = false; public bool DC => (LGS?.Any(b => b.IsLocked) ?? false) || (SCS?.Any(b => b.Status == MyShipConnectorStatus.Connected) ?? false); public float DRP { get; set; } = 1f; public bool DST { get; set; } = false; public float FAR { get; set; } = 0f; public float FBP { get; set; } = 0; public float Frc { get; set; } = 100f; public float GRBP { get; set; } = 0; public float MR { get; set; } = 60f; public float MTA { get; set; } = MathHelper.ToRadians(45); public float MTAN { get; set; } = MathHelper.ToRadians(45); public float PPM { get; set; } = 1; public bool RetractWheels => SCIFM.HandBrake; public float RTR { get; set; } = 1f; public float SH { get; set; } = 0.25f; public float TSM { get { return 1 - _TM; } set { _TM = 1 - MathHelper.Clamp(value, 0, 1); } } internal MySCDSP SCIFM { get; set; } bool ED => (SCIFM.LinearVelocity.LengthSquared() < 25f && FI == 0); bool HDS => !Common.IsNullC(MH); bool NMW => Common.IsNullC(MWS); bool NSW => Common.IsNullC(SWS); float TI => SCIFM.RotationIndicator.Y + (SCIFM.HasWings ? SCIFM.RI_SC.Y : 0) * 30; public void FUD(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { if (SCIFM.UnabledControl || ITE == null) return; WS = GTS.GetBlockGroupWithName(WheelsGroupNM); MH = Common.GetTs(GTS, (IMyTerminalBlock thrust) => thrust.BlockDefinition.SubtypeId.Contains(HoverEngineNM)); SWS = Common.GetTs<IMyMotorSuspension>(WS, ITE); MWS = Common.GetTs<IMyMotorStator>(WS, ITE); PT = Common.GetTs<IMyPistonBase>(GTS, p => ITE(p) && p.CustomName.Contains("UCR")); SCS = Common.GetTs<IMyShipConnector>(GTS, p => ITE(p) && p.CustomName.Contains("UCR")); LGS = Common.GetTs<IMyLandingGear>(GTS, p => ITE(p) && p.CustomName.Contains("UCR")); BKL = Common.GetTs(GTS, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BrakeNM) && ITE(lightblock)); BWL = Common.GetTs(GTS, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BackwardNM) && ITE(lightblock)); } public void Running() { SCIFM.TorqueAddon = SCPF.GetSuspensionAntiRoll(SWS); if (Common.IsNull(SCIFM.Me)) return; LII(); LLG(); LCS(); LPS(); bool NeedRunningWheels = (SCIFM.Role == MyRole.WheelVehicle || SCIFM.Role == MyRole.TrackVehicle) && SCIFM.EnabledWheels; if (!Common.IsNullC(SWS)) { foreach (var SWheel in SWS) if (SWheel.Enabled != NeedRunningWheels) SWheel.Enabled = NeedRunningWheels; } if (!Common.IsNullC(MWS)) { foreach (var MWheel in MWS) if (MWheel.Enabled != NeedRunningWheels) MWheel.Enabled = NeedRunningWheels; } if (NeedRunningWheels) { FI = SCIFM.MoveIndicator.Z; ProcessForwardSignal(); RunningGearSimulate(); LSP(); LWW(); } } public void SAS(IMyMotorSuspension Wheel, float speed_cav, float delta, bool IRW) { switch (SCIFM.Role) { case MyRole.TrackVehicle: Wheel.MaxSteerAngle = 0; break; case MyRole.WheelVehicle: if (delta == 0) { if (IRW) Wheel.MaxSteerAngle = MTA * RTR; else Wheel.MaxSteerAngle = MTA; break; } if (IRW) Wheel.MaxSteerAngle = (MTAN + speed_cav * delta) * RTR; else Wheel.MaxSteerAngle = MTAN + speed_cav * delta; break; default: break; } if (Wheel.InvertSteer) Wheel.InvertSteer = false; } static void SMO(IMyMotorSuspension Wheel, float Value) { if (Wheel.GetProperty(MotorOverrideId).AsFloat().GetValue(Wheel) != Value) Wheel.GetProperty(MotorOverrideId).AsFloat().SetValue(Wheel, Value); } static void SSO(IMyMotorSuspension Wheel, float Value) { if (Wheel.GetProperty(SteerOverrideId).AsFloat().GetValue(Wheel) != Value) Wheel.GetProperty(SteerOverrideId).AsFloat().SetValue(Wheel, Value); } void FSS(IMyMotorSuspension Wheel, bool IRW) { if (SCIFM.Role == MyRole.TrackVehicle) { if (TI == 0) Wheel.Friction = Frc; else { var Values = Math.Abs(SCIFM.WorldMatrix.Forward.Dot(Wheel.GetPosition() - (SCIFM.Me?.CubeGrid?.WorldAABB.Center ?? Vector3D.Zero))); Values = Values * Values; Wheel.Friction = Frc * _TM / (float)(Values + 1); } } else if (SCIFM.Role == MyRole.WheelVehicle) { GRBP = MathHelper.Clamp(GRBP, -1, 1); if (IRW) { var f = Frc * MathHelper.Clamp(1 + GRBP, 0, 2); if (TSM <= 0) { _TM = 1; } if (SCIFM.RotationIndicator.Y != 0) f = f * _TM; Wheel.Friction = f; } else { Wheel.Friction = Frc * MathHelper.Clamp(1 - GRBP, 0, 2); } } else Wheel.Friction = Frc; } void LCS() { if (Common.IsNull(SCIFM.Me) || Common.IsNullC(SCS)) return; foreach (var ShipConnector in SCS) { if (ShipConnector == null) continue; if (RetractWheels) ShipConnector.Connect(); else ShipConnector.Disconnect(); ShipConnector.PullStrength = 1; } } void LII() { if (Common.IsNull(SCIFM.Me)) return; if (!Common.IsNullC(BKL)) foreach (var item in BKL) { if (Common.IsNull(item)) continue; item.Color = Color.Red; item.Enabled = SCIFM.MoveIndicator.Z == 0 || (SCIFM.LinearVelocity.Dot(SCIFM.WorldMatrix.Forward) * SCIFM.MoveIndicator.Z) > 0; } if (!Common.IsNullC(BWL)) foreach (var item in BWL) { if (Common.IsNull(item)) continue; item.Color = Color.White; item.Enabled = SCIFM.MoveIndicator.Z > 0 && SCIFM.LinearVelocity.Dot(SCIFM.WorldMatrix.Backward) > 0; } } void LLG() { if (Common.IsNull(SCIFM.Me) || Common.IsNullC(LGS)) return; foreach (var LandingGear in LGS) { if (LandingGear == null) continue; if (RetractWheels) { if (!LandingGear.IsLocked) LandingGear.Lock(); LandingGear.AutoLock = true; } else { if (LandingGear.IsLocked) LandingGear.Unlock(); LandingGear.AutoLock = false; } } } void LPS() { if (Common.IsNull(SCIFM.Me) || Common.IsNullC(SCS)) return; foreach (var Piston in PT) { if (Piston == null || Piston.TopGrid == null) continue; if (RetractWheels) Piston.Velocity = 1; else Piston.Velocity = -1; } } void LSP() { if (Common.IsNull(SCIFM.Me) || HDS || NSW) return; MTA = Math.Abs(MTA); MTAN = Math.Min(Math.Abs(MTAN), MTA); FBP = MathHelper.Clamp(FBP, -1, 1); FAR = MathHelper.Clamp(FAR, -1, 1); RTR = MathHelper.Clamp(RTR, 0, 1); var SCV = MathHelper.Clamp(1 - Math.Max(MathHelper.Clamp(SCIFM.LinearVelocity.Length(), 0, 100) - MSAS, 0) / Math.Max((MathHelper.Clamp(SCIFM.MSL, 30, 100) - MSAS), 1), 0, 1); var delta = Math.Max(MTA - MTAN, 0); foreach (var W in SWS) { if (W == null) continue; var _WDP = (float)CMF.ProjectOnVectorD(CMF.NormalizeD(W.GetPosition() - (SCIFM.Me?.CubeGrid?.WorldAABB.Center ?? Vector3D.Zero)), SCIFM.WorldMatrix.Forward).Dot(SCIFM.WorldMatrix.Forward); var _IRW = W.CustomName.Contains("Rear") || _WDP < 0; SAS(W, (float)SCV, delta, _IRW); FSS(W, _IRW); if (DA && SCIFM.Role != MyRole.TrackVehicle) { SSO(W, 0); SMO(W, 0); } else { var sign = Math.Sign(SCIFM.WorldMatrix.Right.Dot(W.WorldMatrix.Up)); float PO; switch (SCIFM.Role) { case MyRole.TrackVehicle: PO = FI * sign + TI * DRP; break; case MyRole.WheelVehicle: PO = FI * sign + ((ED && FI == 0) ? (TI * DRP) : (TI * (1 - (float)SCV) * DRP)); break; default: PO = 0; break; } W.Power = PPM * 100f * MathHelper.Clamp(1 + (_IRW ? FBP : -FBP), 0, 2); W.Height = RetractWheels ? 200 : (((_IRW ? 1 : -1) * FAR + 1) * SH); W.Enabled = !RetractWheels; W.Brake = PO == 0 || RetractWheels; SS(W, _IRW); SMO(W, W.Power == 0 ? 0 : MathHelper.Clamp(PO, -PPM, PPM)); } } } void LWW() { if (Common.IsNull(SCIFM.Me) || HDS || NMW) return; foreach (var Motor in MWS) { if (Motor == null || Motor.TopGrid == null) continue; var sign = Math.Sign(SCIFM.WorldMatrix.Right.Dot(Motor.WorldMatrix.Up)); Motor.TargetVelocityRPM = RetractWheels ? 0 : (-(FI * sign + TI * DRP) * MR * PPM); if (!Motor.RotorLock && SCIFM.HandBrake) Motor.RotorLock = true; else if (Motor.RotorLock) Motor.RotorLock = false; } } void ProcessForwardSignal() { var CurrentSpeed = Math.Max(SCIFM.LinearVelocity.Dot(SCIFM.WorldMatrix.Forward), 0); FI = CMF.SmoothValue((float)CurrentSpeed + CurrentMaxiumSpeed * FI); } void RunningGearSimulate() { if (SCIFM.HandBrake) { CurrentMaxiumSpeed = 0; _CG = 0; return; } if (Common.IsNullC(GMS) || Common.IsNullC(GMSD) || GMS.Count != GMSD.Count) { _CG = 1; CurrentMaxiumSpeed = SCIFM.MSL; } var _CurrentGearMode = MathHelper.Clamp(CGM, 1, GMS.Count); if (_CurrentGearMode != CGM) { CGM = _CurrentGearMode; CT = 0; } _CG = MathHelper.Clamp(_CG, 1, GMS[CGM].Count); var speed = Math.Abs(SCIFM.WorldMatrix.Forward.Dot(SCIFM.LinearVelocity)); if (speed > UGG * GMS[CGM][CurrentGear]) { if (CT <= 0) { _CG = MathHelper.Clamp(CurrentGear + 1, 1, GMS[CGM].Count); CT = GMSD[CGM]; } else { CT--; } } else if (speed < SGG * GMS[CGM][CurrentGear]) { _CG = MathHelper.Clamp(CurrentGear - 1, 1, GMS[CGM].Count); CT = 0; } CurrentMaxiumSpeed = MathHelper.Clamp(GMS[CGM][CurrentGear], 0, Math.Max(SCIFM.MSL, 0)); } void SS(IMyMotorSuspension Wheel, bool PRW) { if (SCIFM.Role == MyRole.TrackVehicle) { SSO(Wheel, 0); if (Wheel.Steering) Wheel.Steering = false; return; } else if (SCIFM.Role == MyRole.WheelVehicle) { if (!Wheel.Steering) Wheel.Steering = true; if (TI != 0) { if (ED && (!DST)) SSO(Wheel, Math.Sign(SCIFM.WorldMatrix.Left.Dot(Wheel.WorldMatrix.Up)) * (PRW ? -1 : 1)); else SSO(Wheel, TI * (PRW ? -1 : 1)); } else { SSO(Wheel, 0); } return; } else { SSO(Wheel, 0); return; } } }
        sealed class MyThCtrl { const double DGM = 0.875f; MyTuple<IMyThrust, double>[] T; public List<string> ENCT { get; } = new List<string>(); internal MySCDSP SCIFM { get; set; } bool NullThrust => Common.IsNullC(T); public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { if (Common.IsNull(SCIFM.Me) || ITE == null) return; T = Common.GetTs(GTS, (IMyThrust thrust) => Common.ExceptKeywords(thrust) && ITE(thrust)).ConvertAll(t => new MyTuple<IMyThrust, double>(t, 1)).ToArray(); foreach (var t in T) { t.Item1.ShowInToolbarConfig = false; t.Item1.ShowInTerminal = false; } } public void Running() { if (NullThrust) return; if (Common.IsNull(SCIFM.Me) || SCIFM.UnabledControl) { foreach (var thrust in T) { if (Common.NullEntity(thrust.Item1)) continue; if (thrust.Item1.ThrustOverridePercentage != 0) thrust.Item1.ThrustOverridePercentage = 0; if (!thrust.Item1.Enabled) thrust.Item1.Enabled = true; } return; } if (!SCIFM.EnabledThrusts) { foreach (var thrust in T) { if (thrust.Item1.Enabled) thrust.Item1.Enabled = false; if (thrust.Item1.ThrustOverridePercentage != 0) thrust.Item1.ThrustOverridePercentage = 0; } return; } Vector3D TotalThrust_Needed_Velocity = SCIFM.TCS * SCIFM.ShipMass; Vector3D V_Velocity = CMF.NormalizeD(TotalThrust_Needed_Velocity); double length_Avaliable_Velocity = 0; for (int index = 0; index < T.Length; index++) { if (Common.NullEntity(T[index].Item1) || !T[index].Item1.IsFunctional) continue; T[index].Item2 = Math.Max(0, T[index].Item1.WorldMatrix.Backward.Dot(V_Velocity)); length_Avaliable_Velocity += T[index].Item2 * T[index].Item1.MaxEffectiveThrust; } double p_v = 0; double length_Needed_Velocity = TotalThrust_Needed_Velocity.Length(); if (length_Avaliable_Velocity != 0) p_v = length_Needed_Velocity / length_Avaliable_Velocity; if (p_v == 0) { foreach (var thrust in T) { thrust.Item1.ThrustOverridePercentage = 0; return; } } foreach (var thrust in T) { if (Common.NullEntity(thrust.Item1) || !thrust.Item1.IsFunctional) continue; thrust.Item1.ThrustOverridePercentage = (float)(MathHelper.Clamp(thrust.Item2 * p_v, Math.Max(1 / thrust.Item1.MaxEffectiveThrust, 1e-8f), 1)); ; } foreach (var thrust in T) { if (Common.NullEntity(thrust.Item1)) continue; bool thrustenabled = SCIFM.EAD; if (thrust.Item1.WorldMatrix.Backward.Dot(SCIFM.WorldMatrix.Up) > DGM) thrustenabled = thrustenabled || SCIFM.HoverMode || NIMG(thrust.Item1); else if (thrust.Item1.WorldMatrix.Backward.Dot(SCIFM.WorldMatrix.Forward) > DGM) thrustenabled = thrustenabled || !SCIFM.HoverMode || NIMG(thrust.Item1); if (thrust.Item1.ThrustOverridePercentage == 0) thrustenabled = false; if (ENCT.Contains("Ion") && SCPF.IsIonEngine(thrust.Item1)) { if (thrust.Item1.Enabled) thrust.Item1.Enabled = false; continue; } if (ENCT.Any(thrust.Item1.BlockDefinition.SubtypeId.Contains)) { if (thrust.Item1.Enabled) thrust.Item1.Enabled = false; continue; } if (thrust.Item1.Enabled != thrustenabled) thrust.Item1.Enabled = thrustenabled; if (!thrust.Item1.IsWorking) { thrust.Item1.ThrustOverridePercentage = 0; continue; } } } bool NIMG(IMyThrust thrust) { if (Common.IsNull(SCIFM.Me)) return true; return SCIFM.Me.CubeGrid != thrust.CubeGrid; } }
        sealed class MyGCtrl { public Vector3 PS3A = Vector3.One; List<IMyGyro> G; internal MySCDSP SCIFM { get; set; } public void FU(IMyGridTerminalSystem GTS, Func<IMyTerminalBlock, bool> ITE) { if (Common.IsNull(SCIFM.Me) || ITE == null) return; G = Common.GetTs(GTS, (IMyGyro gyro) => Common.ExceptKeywords(gyro) && NC(gyro) && ITE(gyro)); foreach (var gyro in G) { if (gyro.Enabled != SCIFM.EnabledGyros) gyro.Enabled = SCIFM.EnabledGyros; gyro.ShowInTerminal = false; gyro.ShowInToolbarConfig = false; } } public void Running() { if (Common.IsNullC(G)) return; if (SCIFM.Enabled) { foreach (var gyro in G) if (gyro.Enabled != SCIFM.EnabledGyros) gyro.Enabled = SCIFM.EnabledGyros; if (!SCIFM.EnabledGyros) return; foreach (var gyro in G) { if (!gyro.GyroOverride) gyro.GyroOverride = true; var result = CMF.VTransWorld2LocalD(gyro.WorldMatrix, SCIFM.GCS) * PS3A; gyro.Roll = (float)result.Z; gyro.Yaw = (float)result.Y; gyro.Pitch = (float)result.X; } } else { foreach (var gyro in G) { if (gyro == null) continue; if (!gyro.Enabled) gyro.Enabled = true; gyro.Roll = gyro.Yaw = gyro.Pitch = 0; gyro.GyroOverride = false; } return; } } static bool NC(IMyGyro Gyro) { if (Gyro.CustomName.Contains("Klang") || Gyro.CustomName.Contains("Clang")) return false; return true; } }
        sealed class MySLManager { double h, _Th; float HTh; bool nply; double sl, _Ts; public MySLManager() { nply = false; h = sl = 0; _Ts = 0; _Th = 0; HTh = 100; } public float Current_Height => nply ? (float)h : 0; public float Current_SeaLevel => nply ? (float)sl : 0; public float DH => SCIFM.DH; public float Diff_Height => nply ? (float)((HO ?? _Th) - h) : 0; public float Diff_Level => (float)(nply ? (h < HTh) ? Diff_Height : Diff_SeaLevel : 0); public float Diff_SeaLevel => nply ? (float)((SLO ?? _Ts) - sl) : 0; public bool Force_PullUp => h < DH; public bool HeightChangedEnable { get { if (Common.IsNull(SCIFM.Me) || SCIFM.NoGravity) return true; switch (SCIFM.Role) { case MyRole.Aeroplane: return !SCIFM.CruiseMode; case MyRole.SpaceShip: case MyRole.VTOL: if (SCIFM.HoverMode) return SCIFM.MoveIndicator.Y != 0; else if (SCIFM.CruiseMode) return SCIFM.MoveIndicator.Y != 0; else return false; case MyRole.SeaShip: case MyRole.TrackVehicle: case MyRole.WheelVehicle: return true; default: return SCIFM.MoveIndicator.Y != 0; } } } public void ForceUpdateHeight() { if (Common.IsNull(SCIFM.Me)) return; if (SCIFM.NoGravity) { nply = false; h = sl = _Ts = _Th = 0; return; } var _nearplanetary = SCIFM.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out sl) && SCIFM.TryGetPlanetElevation(MyPlanetElevation.Surface, out h); if (!_nearplanetary) { nply = false; h = sl = _Ts = _Th = 0; return; } if (nply != _nearplanetary) { nply = _nearplanetary; _Th = h; _Ts = sl; return; } _Ts = sl; _Th = h; } public double? HO { get; set; } = null; public double? SLO { get; set; } = null; internal bool FirstRun { get; set; } = true; internal MySCDSP SCIFM { get; set; } public void InitParameters(float HeightThreshold = 100) { if (Common.IsNull(SCIFM.Me)) return; this.HTh = Math.Max(HeightThreshold, 20); if (!FirstRun) return; nply = SCIFM.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out sl) && SCIFM.TryGetPlanetElevation(MyPlanetElevation.Surface, out h); if (nply) { _Ts = sl; _Th = h; } else { h = sl = _Ts = _Th = 0; } FirstRun = false; } public override string ToString() { string str = $"[SealevelManager]\n\r" + $"Has Main Controller:{SCIFM?.Me != null}\n\r" + $"Target SeaLevel:{_Ts}m" + $"\n\rCurrent SeaLevel:{sl}m\n\r" + $"Target Height:{_Th}m" + $"\n\rCurrent Height:{h}m\n\r"; return str; } public void UpdateParameters() { if (Common.IsNull(SCIFM.Me)) return; if (SCIFM.NoGravity) { nply = false; h = sl = _Ts = _Th = 0; return; } var _nearplanetary = SCIFM.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out sl) && SCIFM.TryGetPlanetElevation(MyPlanetElevation.Surface, out h); if (!_nearplanetary) { nply = false; h = sl = _Ts = _Th = 0; return; } if (nply != _nearplanetary) { nply = _nearplanetary; _Th = h; _Ts = sl; return; } if (HeightChangedEnable) { _Ts = sl; _Th = h; } } }
        sealed class MyACDCR { List<MyACDT> TS { get; } = new List<MyACDT>(); public void Running() { foreach (var Timer in TS) { Timer.Running(); } } public void UpdateBlocks(IMyGridTerminalSystem GTS) { var dsg = GTS.GetBlockGroupWithName(ACDoorsGroupNM); if (dsg == null) return; var ds = Common.GetTs<IMyDoor>(dsg); foreach (var d in ds) { TS.Add(new MyACDT(d)); } } }
        sealed class MyACDT { const int g = 25; readonly IMyDoor d; int c; public MyACDT(IMyDoor Door) { d = Door; } public void Running() { if (d == null) return; switch (d.Status) { case DoorStatus.Opening: c = g; return; case DoorStatus.Open: if (c > 0) c--; else d.CloseDoor(); return; default: break; } } }
        static class Common { static readonly string[] BlackList_ShipController = new string[] { "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", "Clang" }; public static bool BlockInTurretGroup(IMyBlockGroup group, IMyTerminalBlock Me) { List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); group?.GetBlocks(blocks); if (blocks.Count < 1 || !blocks.Contains(Me)) return false; return true; } public static bool ExceptKeywords(IMyTerminalBlock block) { foreach (var item in BlackList_ShipController) { if (block.BlockDefinition.SubtypeId.Contains(item) || block.CustomName.Contains("Clang")) return false; } return true; } public static IMyTerminalBlock GetBlock(IMyGridTerminalSystem gridTerminalSystem, long EntIds = 0) => gridTerminalSystem?.GetBlockWithId(EntIds) as IMyTerminalBlock; public static List<IMyTerminalBlock> GetBlocks(IMyGridTerminalSystem gridTerminalSystem, List<long> EntIds = null) { if (gridTerminalSystem == null) return null; return EntIds?.ConvertAll(id => gridTerminalSystem.GetBlockWithId(id) as IMyTerminalBlock); } public static T GetT<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = GetTs(gridTerminalSystem, requst); if (IsNullC(Items)) return null; else return Items.First(); } public static T GetT<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class => GetTs(blockGroup, requst).FirstOrDefault(); public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (gridTerminalSystem == null) return Items; gridTerminalSystem.GetBlocksOfType(Items, requst); return Items; } public static List<T> GetTs<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (blockGroup == null) return Items; blockGroup.GetBlocksOfType(Items, requst); return Items; } public static Matrix GetWorldMatrix(IMyTerminalBlock ShipController) { Matrix me_matrix; ShipController.Orientation.GetMatrix(out me_matrix); return me_matrix; } public static IMyCameraBlock ID2Camera(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyCameraBlock; public static IMyMotorStator ID2Motor(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyMotorStator; public static IMyTerminalBlock ID2Weapon(IMyGridTerminalSystem GTS, long EntID) => GTS?.GetBlockWithId(EntID) as IMyTerminalBlock; public static bool IsNull(Vector3? Value) => Value == null || Value.Value == Vector3.Zero; public static bool IsNull(Vector3D? Value) => Value == null || Value.Value == Vector3D.Zero; public static bool IsNull<T>(T Value) where T : class => Value == null; public static bool IsNullC<T>(ICollection<T> Value) => (Value?.Count ?? 0) < 1; public static bool IsNullC<T>(IEnumerable<T> Value) => (Value?.Count() ?? 0) < 1; public static bool NullEntity<T>(T Ent) where T : IMyEntity => Ent == null; public static float SetInRange_AngularDampeners(float data) => MathHelper.Clamp(data, 0.01f, 20f); public static List<string> SpliteByQ(string context) => context?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? (new List<string>()); }
        static class CMF { public static double SetNaNZero(double Value) => double.IsNaN(Value) ? 0 : Value; public static double AngleBetweenD(Vector3D a, Vector3D b) { var dotProd = Vector3D.Dot(a, b); var lenProd = a.Length() * b.Length(); return Math.Acos(dotProd / lenProd); } public static Vector3D CalculateAvg(ICollection<Vector3D> Vectors) { if (Common.IsNullC(Vectors)) return Vector3D.Zero; Vector3D Value = Vector3D.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value /= Vectors.Count; } public static Vector3 CalculateAvg(ICollection<Vector3> Vectors) { if (Common.IsNullC(Vectors)) return Vector3.Zero; Vector3 Value = Vector3.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value /= Vectors.Count; } public static Vector3D CalculateSum(ICollection<Vector3D> Vectors) { if (Common.IsNullC(Vectors)) return Vector3D.Zero; Vector3D Value = Vector3D.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value; } public static Vector3 CalculateSum(ICollection<Vector3> Vectors) { if (Common.IsNullC(Vectors)) return Vector3.Zero; Vector3 Value = Vector3.Zero; foreach (var Vector in Vectors) { Value += Vector; } return Value; } public static Vector3 Normalize(Vector3 vector) { if (Vector3.IsZero(vector)) return Vector3.Zero; return Vector3.Normalize(vector); } public static Vector3 Normalize(Vector3 vector, float Epsional, float Multipy) { return Normalize(vector * (Vector3.One - Vector3.IsZeroVector(vector, Epsional))) * Multipy; } public static Vector3D NormalizeD(Vector3D vector) { if (Vector3D.IsZero(vector)) return Vector3D.Zero; return Vector3D.Normalize(vector); } public static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal, float Epsilon = 0) { if (Epsilon == 0) return Vector3.ProjectOnPlane(ref direction, ref planeNormal); var result = Vector3.ProjectOnPlane(ref direction, ref planeNormal); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static Vector3D ProjectOnPlaneD(Vector3D direction, Vector3D planeNormal, double Epsilon = 0) { if (Epsilon == 0) return Vector3D.ProjectOnPlane(ref direction, ref planeNormal); var result = Vector3D.ProjectOnPlane(ref direction, ref planeNormal); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static Vector3 ProjectOnVector(Vector3 vector, Vector3 direction, double Epsilon = 0) { if (Epsilon == 0) return Vector3.ProjectOnVector(ref vector, ref direction); var result = Vector3.ProjectOnVector(ref vector, ref direction); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static Vector3D ProjectOnVectorD(Vector3D vector, Vector3D direction, double Epsilon = 0) { if (Epsilon == 0) return Vector3D.ProjectOnVector(ref vector, ref direction); var result = Vector3D.ProjectOnVector(ref vector, ref direction); var length = result.Length(); if (Math.Abs(Epsilon) > length) return Vector3D.Zero; return result; } public static float SignedAngle(Vector3 A, Vector3 B, Vector3 Axis) { return MyMath.AngleBetween(A, B) * SignNonZero(A.Cross(B).Dot(Axis)); } public static double SignedAngleD(Vector3D A, Vector3D B, Vector3D Axis) { if (Vector3.IsZero(A) || Vector3.IsZero(A)) return 1; return AngleBetweenD(A, B) * SignNonZero(A.Cross(B).Dot(Axis)); } public static int SignNonZero(double Value) => (Value >= 0) ? 1 : -1; public static float SmoothValue(float value) { return value * Math.Abs(value); } public static double SmoothValueD(double value) { return value * Math.Abs(value); } public static Vector3 SmoothValue(Vector3 value) { return value * Vector3.Abs(value); } public static Vector3D SmoothValueD(Vector3D value) { return value * Vector3D.Abs(value); } public static Vector3D VTransWorld2LocalD(MatrixD Parent_World, Vector3D Vector_World) => Vector3D.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World)); public static Vector3D VTransLocal2WorldD(MatrixD Parent_World, Vector3D Vector_Local) => Vector3D.TransformNormal(Vector_Local, Parent_World); public static Vector3 VTransWorld2Local(MatrixD Parent_World, Vector3 Vector_World) => Vector3.TransformNormal(Vector_World, MatrixD.Transpose(Parent_World)); public static Vector3 VTransLocal2World(MatrixD Parent_World, Vector3 Vector_Local) => Vector3.TransformNormal(Vector_Local, Parent_World); }
        public sealed class MyCBPS { readonly IMyCameraBlock CameraBlock; public MyCBPS(IMyCameraBlock Block) { CameraBlock = Block; UpdateInfos(100); } public bool CCT { get; set; } public double CH { get; set; } public Vector3D D { get; set; } public Vector3D P { get; set; } public static List<MyCBPS> CreateCameraBlockParametersList(IMyTerminalBlock Me, IMyGridTerminalSystem GTS, string Tag = "TestDistance") => Common.GetTs<IMyCameraBlock>(GTS, b => Me.IsSameConstructAs(b) && b.CustomName.EndsWith(Tag))?.ConvertAll(b => new MyCBPS(b)); public static Vector3D GetTorqueByCameras(List<MyCBPS> THS, double TSSK) { double H = float.MaxValue; if (Common.IsNullC(THS)) return Vector3D.Zero; Vector3D Center = CMF.CalculateAvg(THS?.ConvertAll(T => T.P)); H = THS?.Average(b => b.CH) ?? 0; var h = H; return CMF.CalculateSum(THS?.ConvertAll(TH => { var diff = SCPF.GetSpring(TH.CH - h, TSSK); var force = diff * CMF.NormalizeD(TH.D); var arm = TH.P - Center; return force.Cross(arm); })); } public void UpdateInfos(float FloatHeight) { P = CameraBlock.GetPosition(); D = CameraBlock.WorldMatrix.Forward; if (!CameraBlock.Enabled) { CH = 0; CCT = false; return; } if (!CameraBlock.EnableRaycast) CameraBlock.EnableRaycast = true; var Target = CameraBlock.Raycast(FloatHeight); if (Target.IsEmpty() || !Target.HitPosition.HasValue) { CH = 0; CCT = false; return; } CH = Vector3D.Distance(P, Target.HitPosition.Value); CCT = true; } }
        static class SCPF { public static bool IsIonEngine(IMyThrust thrust) { if (thrust == null) return false; return IonThrustList.Contains(thrust.BlockDefinition.SubtypeId) || thrust.BlockDefinition.SubtypeId.Contains("Ion") || thrust.BlockDefinition.SubtypeId.Contains("ion"); } public static float ControlWithVector(Vector3 Axe, Vector3 Target, Vector3 Current, float Epsilon = 0) => CMF.SignedAngle(CMF.ProjectOnPlane(Target, Axe, Epsilon), Current, Axe); public static double ControlWithVectorD(Vector3D Axe, Vector3D Target, Vector3D Current, double Epsilon = 0) => CMF.SetNaNZero(CMF.SignedAngleD(CMF.ProjectOnPlaneD(Target, Axe, Epsilon), Current, Axe)); public static float ControlWithVectorLinear(Vector3 Axe, Vector3 Target, Vector3 Current, float Epsilon = 0) => CMF.SignedAngle(CMF.ProjectOnPlane(Target, Axe, Epsilon), Current, Axe); public static double ControlWithVectorLinearD(Vector3D Axe, Vector3D Target, Vector3D Current, double Epsilon = 0) => CMF.SetNaNZero(CMF.SignedAngleD(CMF.ProjectOnPlaneD(Target, Axe, Epsilon), Current, Axe)); public static Vector3 Dampener_Mutlipy_Vector(Vector3 Local_ReferNormal, Vector3 Normal) { Vector3 Local_Diff = Normal - Local_ReferNormal; Vector3 Dampener_Temp = new Vector3(Local_Diff.Y, Local_Diff.Z, Local_Diff.X); return Dampener_Temp * Dampener_Temp; } public static double GetSpring(double x, double k = 1) => k * x * Math.Abs(x); public static Vector3D GetSuspensionAntiRoll(ICollection<IMyMotorSuspension> Wheels) { if (Common.IsNullC(Wheels)) return Vector3D.Zero; Vector3D Center = CMF.CalculateAvg(Wheels.Where(b => b.Top != null)?.ToList()?.ConvertAll(b => b.GetPosition())); return CMF.CalculateSum(Wheels.Where(b => b.Top != null)?.ToList()?.ConvertAll(b => { var FL = CMF.ProjectOnVectorD(b.Top.GetPosition() - b.GetPosition(), b.WorldMatrix.Backward); var Arm = b.GetPosition() - Center; return FL.Cross(Arm); })); } static List<string> IonThrustList = new List<string>() { "SmallBlockSmallThrust", "SmallBlockLargeThrust", "LargeBlockSmallThrust", "LargeBlockLargeThrust", "SmallBlockSmallThrustSciFi", "SmallBlockLargeThrustSciFi", "LargeBlockSmallThrustSciFi", "LargeBlockLargeThrustSciFi" }; }
        enum MyRole : int { None, Aeroplane, Helicopter, VTOL, SpaceShip, SeaShip, Submarine, TrackVehicle, WheelVehicle, HoverVehicle }
        const string ACDoorsGroupNM = @"ACDoors"; const string BackwardNM = @"Backward"; const string BrakeNM = @"Brake"; const string HoverEngineNM = "Hover"; const string MotorOverrideId = @"Propulsion override"; const string SteerOverrideId = @"Steer override"; const string WheelsGroupNM = @"Wheels"; const string GravityDriveNM = @"GravityDrive"; const string ClangEngineTag = "Clang"; const string T_OnOffTag = @"ThrusterOnOff"; const string W_SetupTag = @"WheelsSetup"; const string STTag = @"SpeedTable"; const string CSTag = @"CommonSetup"; public const string StablizeBlockTag = "TestDistance"; public static float PitchLimitedValue = (float)Math.Sin(MathHelper.ToRadians(87)); public static float RollLimitedValue = (float)Math.Sin(MathHelper.ToRadians(50));
