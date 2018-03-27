using System;
using KSP.UI.Screens;
using UnityEngine;
using BDArmory;
using BDArmory.UI;
using BDArmory.Parts;
using System.Collections.Generic;

namespace DCK_FutureTech
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class GPS_Editor : MonoBehaviour
    {
        private const float WindowWidth = 200;
        private const float DraggableHeight = 40;
        private const float LeftIndent = 12;
        private const float ContentTop = 20;
        public static GPS_Editor Fetch;
        public static bool GuiEnabled;
        public static bool HasAddedButton;
        private readonly float _incrButtonWidth = 26;
        private readonly float contentWidth = WindowWidth - 2 * LeftIndent;
        private readonly float entryHeight = 20;
        private float _contentWidth;
        private bool _gameUiToggle;
        public string _guiX = String.Empty;
        public string _guiY = String.Empty;
        public string _guiZ = String.Empty;
        private float _windowHeight = 250;
        private Rect _windowRect;

        public string Name = String.Empty;

        private double altitude;
        private double longitude;
        private double latitude;

        private double _altitude = 0.0f;
        private double _longitude = 0.0f;
        private double _latitude = 0.0f;
        private bool team;

        private void Awake()
        {        
            if (Fetch)
                Destroy(Fetch);

            Fetch = this;
        }

        private void Start()
        {
            _windowRect = new Rect(Screen.width - WindowWidth - 40, 100, WindowWidth, _windowHeight);
            AddToolbarButton();
            GameEvents.onHideUI.Add(GameUiDisable);
            GameEvents.onShowUI.Add(GameUiEnable);
            _gameUiToggle = true;
            _guiX = "0";
            _guiY = "0";
            _guiZ = "0";
        }

        private void OnGUI()
        {
            if (GuiEnabled && _gameUiToggle)
            {
                _windowRect = GUI.Window(200, _windowRect, GuiWindow, "");
            }
        }


        #region GPS 
        /// <summary>
        /// GPS 
        /// </summary>
        /// 
        private void SaveGPS()
        {
            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                var _latitude_ = double.Parse(_guiX);
                var _longitude_ = double.Parse(_guiY);
                var _altitude_ = double.Parse(_guiZ);
                _latitude = _latitude_;
                _longitude = _longitude_;
                _altitude = _altitude_;
                BDATargetManager.GPSTargets[BDATargetManager.BoolToTeam(wmPart.team)].Add(new GPSTargetInfo(getTargetCoords, "Saved GPS"));
                ScreenMsg("GPS Target Saved");
            }
        }

        private void LockGPS()
        {
            GetCoords();

            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                team = wmPart.team;
            }

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                List<ModuleHammerOfDawn> HoDParts = new List<ModuleHammerOfDawn>(200);
                foreach (Part p in v.Parts)
                {
                    HoDParts.AddRange(p.FindModulesImplementing<ModuleHammerOfDawn>());
                }
                foreach (ModuleHammerOfDawn HoDPart in HoDParts)
                {
                    if (HoDPart.myTeam == team)
                    {
                        HoDPart._latitude = _latitude;
                        HoDPart._longitude = _longitude;
                        HoDPart._altitude = _altitude;
                        HoDPart.lockTarget = true;
                    }
                }
            }
        }

        private void FireHoD()
        {
            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                team = wmPart.team;
            }

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                List<ModuleHammerOfDawn> HoDParts = new List<ModuleHammerOfDawn>(200);
                foreach (Part p in v.Parts)
                {
                    HoDParts.AddRange(p.FindModulesImplementing<ModuleHammerOfDawn>());
                }
                foreach (ModuleHammerOfDawn HoDPart in HoDParts)
                {
                    if (HoDParts != null)
                    {
                        if (HoDPart.myTeam == team)
                        {
                            HoDPart.fireLaser = true;
                        }
                    }
                    else
                    {
                        ScreenMsg("Hammer Of Dawn Not in Range");
                    }
                }
            }
        }

        private void ScanGPS()
        {
            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                team = wmPart.team;
            }

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                var satCount = 0;
                var HoDCount = 0;

                List<ModuleDCKGPSSat> gpsParts = new List<ModuleDCKGPSSat>(200);
                foreach (Part p in v.Parts)
                {
                    gpsParts.AddRange(p.FindModulesImplementing<ModuleDCKGPSSat>());
                }
                foreach (ModuleDCKGPSSat gpsPart in gpsParts)
                {
                    satCount += 1;
                    if (gpsPart.myTeam == team && satCount == 1)
                    {
                        gpsPart.scan = true;
                    }
                }

                List<ModuleHammerOfDawn> HoDParts = new List<ModuleHammerOfDawn>(200);
                foreach (Part p in v.Parts)
                {
                    HoDParts.AddRange(p.FindModulesImplementing<ModuleHammerOfDawn>());
                }
                foreach (ModuleHammerOfDawn HoDPart in HoDParts)
                {
                    HoDCount += 1;
                    if (HoDPart.myTeam == team && HoDCount == 1)
                    {
                        HoDPart.scan = true;
                    }
                }
            }
        }

        private void GetCoords()
        {
            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                var getTarget = wmPart.designatedGPSCoords;
                var _latitude_ = getTarget.x;
                var _longitude_ = getTarget.y;
                var _altitude_ = getTarget.z;
                _latitude = _latitude_;
                _longitude = _longitude_;
                _altitude = _altitude_;
            }

        }

        private Vector3 getTargetCoords
        {
            get
            {
                return new Vector3d(_latitude, _longitude, _altitude);
            }

            set
            {
                string _latString = Convert.ToString(_latitude);
                string _longString = Convert.ToString(_longitude);
                string _altString = Convert.ToString(_altitude);

                var _latFloat = float.Parse(_latString);
                var _longFloat = float.Parse(_longString);
                var _altFloat = float.Parse(_altString);

                value.x = _latFloat;
                value.y = _longFloat;
                value.z = _altFloat;

                latitude = value.x;
                longitude = value.y;
                altitude = value.z;
            }
        }

        #endregion

        #region GUI
        /// <summary>
        /// GUI
        /// </summary>

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 4, ScreenMessageStyle.UPPER_CENTER));
        }

        private void GuiWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            float line = 0;
            _contentWidth = WindowWidth - 2 * LeftIndent;

            DrawTitle();
            DrawText(line);
            line++;
            DrawX(line);
            line++;
            DrawY(line);
            line++;
            DrawZ(line);
            line++;
            DrawSaveGPS(line);
            line++;
            line++;
            DrawText2(line);
            line++;
            DrawScanGPS(line);
            line++;
            line++;
            DrawText3(line);
            line++;
            DrawLockGPS(line);
            line++;
            DrawFire(line);



            _windowHeight = ContentTop + line * entryHeight + entryHeight + (entryHeight / 2);
            _windowRect.height = _windowHeight;
        }

        private void AddToolbarButton()
        {
            string textureDir = "DCK_FutureTech/Plugin/";

            if (!HasAddedButton)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture(textureDir + "DCK_FT_icon", false); //texture to use for the button
                ApplicationLauncher.Instance.AddModApplication(EnableGui, DisableGui, Dummy, Dummy, Dummy, Dummy,
                    ApplicationLauncher.AppScenes.FLIGHT, buttonTexture);
                HasAddedButton = true;
            }
        }

        private void EnableGui()
        {
            GuiEnabled = true;
            Debug.Log("[DCK_FutureTech]: Showing GPS GUI");
        }

        private void DisableGui()
        {
            GuiEnabled = false;
            Debug.Log("[DCK_FutureTech]: Hiding GPS GUI");
        }

        private void GameUiEnable()
        {
            _gameUiToggle = true;
        }

        private void GameUiDisable()
        {
            _gameUiToggle = false;
        }

        private void DrawTitle()
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(0, 0, WindowWidth, 20), "DCK FutureTech", titleStyle);
        }

        private void DrawText(float line)
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(0, ContentTop + line * entryHeight, WindowWidth, 20),
                "Enter GPS Coords Below",
                titleStyle);
        }

        private void DrawX(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "X Coordinate",
                leftLabel);
            float textFieldWidth = 80;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiX = GUI.TextField(fwdFieldRect, _guiX);
        }

        private void DrawY(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "Y Coordinate",
                leftLabel);
            float textFieldWidth = 80;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiY = GUI.TextField(fwdFieldRect, _guiY);
        }

        private void DrawZ(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "Altitude",
                leftLabel);
            float textFieldWidth = 80;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiZ = GUI.TextField(fwdFieldRect, _guiZ);
        }

        private void DrawSaveGPS(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Save GPS"))
            {
                SaveGPS();
            }
        }

        private void DrawText2(float line)
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(0, ContentTop + line * entryHeight, WindowWidth, 20),
                "Satellite GPS System",
                titleStyle);
        }

        private void DrawScanGPS(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Scan For GPS Targets"))
            {
                ScanGPS();
            }
        }

        private void DrawText3(float line)
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(0, ContentTop + line * entryHeight, WindowWidth, 20),
                "Hammer of Dawn",
                titleStyle);
        }

        private void DrawLockGPS(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Lock Target"))
            {
                LockGPS();
            }
        }

        private void DrawFire(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Bring Down The Hammer"))
            {
                FireHoD();
            }
        }

        #endregion

        private void Dummy()
        {
        }
    }
}