using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;
using BDArmory.Core.Module;

namespace DCK_FutureTech
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class HP_Armor_Editor : MonoBehaviour
    {
        private const float WindowWidth = 140;
        private const float DraggableHeight = 40;
        private const float LeftIndent = 12;
        private const float ContentTop = 20;
        public static HP_Armor_Editor Fetch;
        public static bool GuiEnabled;
        public static bool HasAddedButton;
        private readonly float _incrButtonWidth = 26;
        private readonly float contentWidth = WindowWidth - 2 * LeftIndent;
        private readonly float entryHeight = 20;
        private float _contentWidth;
        private bool _gameUiToggle;
        private float _windowHeight = 230;
        private Rect _windowRect;

        public string _guiHP = String.Empty;
        public string _guiArmor = String.Empty;
        public string _percent = String.Empty;

        private bool MLS;
        private bool MCS;
        private bool allParts;
        private bool fuelTank;
        private bool command;
        private bool shipHull;
        private bool structural;
        private bool showHPtotal;
        private float HP = 0.0f;
        private float vesselHPtotal = 0.0f;


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
            _guiArmor = "";
            _guiHP = "";
        }

        private void OnGUI()
        {
            if (GuiEnabled && _gameUiToggle)
                _windowRect = GUI.Window(320, _windowRect, GuiWindow, "");
        }

        public void Update()
        {
            if (showHPtotal)
            {
                GetHPTotal();
                ShowHPtotal();
            }
        }

        private void ShowHPtotal()
        {
            vesselHPtotal = HP;
            ScreenMsg("Total HP: " + vesselHPtotal);
        }

        private void GetHPTotal()
        {
            HP = 0.0f;

            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                HP += hpTracker.maxHitPoints;
            }
        }

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 0.005f, ScreenMessageStyle.UPPER_RIGHT));
        }


        #region HP/Armor Adjust
        /// <summary>
        /// HP/Armor Adjust
        /// </summary>
        private void AdjustHP()
        {
            if (_guiHP != "")
            {
                Part root = EditorLogic.RootPart;
                if (!root)
                    return;

                if (allParts)
                {
                    HPAllParts();
                }

                if (command)
                {
                    HPCommand();
                }

                if (fuelTank)
                {
                    HPFuelTank();
                }

                if (MCS)
                {
                    HPMCS();
                }

                if (MLS)
                {
                    HPMLS();
                }

                if (shipHull)
                {
                    HPShipHull();
                }

                if (structural)
                {
                    HPStructural();
                }
            }
        }

        private void AdjustArmor()
        {
            if (_guiHP != "")
            {
                Part root = EditorLogic.RootPart;
                if (!root)
                    return;

                if (allParts)
                {
                    ArmorAllParts();
                }

                if (command)
                {
                    ArmorCommand();
                }

                if (fuelTank)
                {
                    ArmorFuelTank();
                }

                if (MCS)
                {
                    ArmorMCS();
                }

                if (MLS)
                {
                    ArmorMLS();
                }

                if (shipHull)
                {
                    ArmorShipHull();
                }

                if (structural)
                {
                    ArmorStructural();
                }
            }
        }

        private void AdjustHPPercent()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (!hpTracker.part.Modules.Contains("ModuleEngines") && !hpTracker.part.Modules.Contains("ModuleEnginesFX")
                    && !hpTracker.part.Modules.Contains("ModuleDecouple") && !hpTracker.part.Modules.Contains("LaunchClamp")
                    && !hpTracker.part.Modules.Contains("ModuleParachute") && !hpTracker.part.Modules.Contains("ModuleAnchoredDecoupler"))
                {
                    var percent = float.Parse(_percent);
                    hpTracker.maxHitPoints = hpTracker.maxHitPoints * percent * 0.01f;
                    hpTracker.Hitpoints = hpTracker.Hitpoints * percent * 0.01f;
                }
            }
        }

        #region HP
        /// <summary>
        /// HP
        /// </summary>
        private void HPAllParts()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (!hpTracker.part.Modules.Contains("ModuleEngines") && !hpTracker.part.Modules.Contains("ModuleEnginesFX")
                    && !hpTracker.part.Modules.Contains("ModuleDecouple") && !hpTracker.part.Modules.Contains("LaunchClamp")
                    && !hpTracker.part.Modules.Contains("ModuleParachute") && !hpTracker.part.Modules.Contains("ModuleAnchoredDecoupler"))
                {
                    var guiHP = float.Parse(_guiHP);
                    hpTracker.maxHitPoints = guiHP;
                    hpTracker.Hitpoints = guiHP;
                }
            }
        }

        private void HPCommand()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleCommand"))
                {
                    var guiHP = float.Parse(_guiHP);
                    hpTracker.maxHitPoints = guiHP;
                    hpTracker.Hitpoints = guiHP;
                }
            }
        }

        private void HPFuelTank()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Resources.Contains("LiquidFuel") || hpTracker.part.Resources.Contains("Oxidizer")
                    || hpTracker.part.Resources.Contains("SolidFuel"))
                {
                    var guiHP = float.Parse(_guiHP);
                    hpTracker.maxHitPoints = guiHP;
                    hpTracker.Hitpoints = guiHP;
                }
            }
        }

        private void HPMCS()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleControlSurface"))
                {
                    var guiHP = float.Parse(_guiHP);
                    hpTracker.maxHitPoints = guiHP;
                    hpTracker.Hitpoints = guiHP;
                }
            }
        }

        private void HPMLS()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleLiftingSurface") && !hpTracker.part.Modules.Contains("ModuleControlSurface")
                    && !hpTracker.part.Modules.Contains("ModuleEngines") && !hpTracker.part.Modules.Contains("ModuleEnginesFX"))
                {
                    var guiHP = float.Parse(_guiHP);
                    hpTracker.maxHitPoints = guiHP;
                    hpTracker.Hitpoints = guiHP;
                }
            }
        }

        private void HPStructural()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleEngines") || hpTracker.part.Modules.Contains("ModuleEnginesFX")
                    || hpTracker.part.Modules.Contains("ModuleDecouple") || hpTracker.part.Modules.Contains("LaunchClamp")
                    || hpTracker.part.Modules.Contains("ModuleAnchoredDecoupler") || hpTracker.part.Modules.Contains("ModuleParachute")
                    || hpTracker.part.Modules.Contains("ModuleDCKShields"))
                {
                    return;
                }
                else if ((hpTracker.part.Modules.Contains("ModuleCommand") || hpTracker.part.Resources.Contains("LiquidFuel")
                    || hpTracker.part.Resources.Contains("SolidFuel") || hpTracker.part.Resources.Contains("Oxidizer"))
                    || (hpTracker.part.Modules.Contains("ModuleLiftingSurface") && hpTracker.part.Modules.Contains("ModuleCargoBay")) 
                    || hpTracker.part.Modules.Contains("ModuleLiftingSurface"))
                {
                    var guiHP = float.Parse(_guiHP);
                    hpTracker.maxHitPoints = guiHP;
                    hpTracker.Hitpoints = guiHP;
                }
            }
        }

        private void HPShipHull()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Resources.Contains("SeaWater") || hpTracker.part.Resources.Contains("BallastWater"))
                {
                    var guiHP = float.Parse(_guiHP);
                    hpTracker.maxHitPoints = guiHP;
                    hpTracker.Hitpoints = guiHP;
                }
            }
        }

        #endregion

        #region Armor
        /// <summary>
        /// Armor
        /// </summary>
        private void ArmorAllParts()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (!hpTracker.part.Modules.Contains("ModuleEngines") && !hpTracker.part.Modules.Contains("ModuleEnginesFX")
                    && !hpTracker.part.Modules.Contains("ModuleDecouple") && !hpTracker.part.Modules.Contains("LaunchClamp")
                    && !hpTracker.part.Modules.Contains("ModuleParachute") && !hpTracker.part.Modules.Contains("ModuleAnchoredDecoupler"))
                {
                    var guiArmor = float.Parse(_guiHP);
                    hpTracker.ArmorThickness = guiArmor;
                    hpTracker.Armor = guiArmor;
                }
            }
        }

        private void ArmorCommand()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleCommand"))
                {
                    var guiArmor = float.Parse(_guiHP);
                    hpTracker.ArmorThickness = guiArmor;
                    hpTracker.Armor = guiArmor;
                }
            }
        }

        private void ArmorFuelTank()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Resources.Contains("LiquidFuel") || hpTracker.part.Resources.Contains("Oxidizer")
                    || hpTracker.part.Resources.Contains("SolidFuel"))
                {
                    var guiArmor = float.Parse(_guiHP);
                    hpTracker.ArmorThickness = guiArmor;
                    hpTracker.Armor = guiArmor;
                }
            }
        }

        private void ArmorMCS()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleControlSurface"))
                {
                    var guiArmor = float.Parse(_guiHP);
                    hpTracker.ArmorThickness = guiArmor;
                    hpTracker.Armor = guiArmor;
                }
            }
        }

        private void ArmorMLS()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleLiftingSurface") && !hpTracker.part.Modules.Contains("ModuleControlSurface")
                    && !hpTracker.part.Modules.Contains("ModuleEngines") && !hpTracker.part.Modules.Contains("ModuleEnginesFX"))
                {
                    var guiArmor = float.Parse(_guiHP);
                    hpTracker.ArmorThickness = guiArmor;
                    hpTracker.Armor = guiArmor;
                }
            }
        }

        private void ArmorStructural()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Modules.Contains("ModuleEngines") || hpTracker.part.Modules.Contains("ModuleEnginesFX")
                    || hpTracker.part.Modules.Contains("ModuleDecouple") || hpTracker.part.Modules.Contains("LaunchClamp")
                    || hpTracker.part.Modules.Contains("ModuleParachute") || hpTracker.part.Modules.Contains("KerbalEVA")
                    || hpTracker.part.Modules.Contains("ModuleCommand") || hpTracker.part.Resources.Contains("LiquidFuel")
                    || hpTracker.part.Resources.Contains("Oxidizer") || hpTracker.part.Resources.Contains("SeaWater")
                    || hpTracker.part.Resources.Contains("BallastWater") || hpTracker.part.Modules.Contains("ModuleDCKShields")
                    || hpTracker.part.Modules.Contains("ModuleDCKACS"))
                {
                    return;
                }
                else
                {
                    var guiArmor = float.Parse(_guiHP);
                    hpTracker.ArmorThickness = guiArmor;
                    hpTracker.Armor = guiArmor;
                }
            }
        }

        private void ArmorShipHull()
        {
            List<HitpointTracker> hpTrackers = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                hpTrackers.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in hpTrackers)
            {
                if (hpTracker.part.Resources.Contains("SeaWater") || hpTracker.part.Resources.Contains("BallastWater"))
                {
                    var guiArmor = float.Parse(_guiHP);
                    hpTracker.ArmorThickness = guiArmor;
                    hpTracker.Armor = guiArmor;
                }
            }
        }

        #endregion

        #endregion

        #region GUI
        /// <summary>
        /// GUI
        /// </summary>
        private void GuiWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            float line = 0;
            _contentWidth = WindowWidth - 2 * LeftIndent;

            DrawTitle();
            DrawText(line);
            line++;
            AllParts(line);
            line++;
            moduleCommand(line);
            line++;
            moduleLiftingSurface(line);
            line++;
            moduleControlSurface(line);
            line++;
            FuelTanks(line);
//            line++;
//            Structural(line);
            line++;
            ShipHull(line);
            line++;
            line++;
            DrawHP(line);
            line++;
            DrawSaveHP(line);
            line++;
            DrawSaveArmor(line);
            line++;
            line++;
            DrawHPPercent(line);
            line++;
            DrawSavePercent(line);

            _windowHeight = ContentTop + line * entryHeight + entryHeight + (entryHeight / 2);
            _windowRect.height = _windowHeight;
        }

        private void AddToolbarButton()
        {
            string textureDir = "DCK_FutureTech/Plugin/";

            if (!HasAddedButton)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture(textureDir + "DCK_FT_HP", false); //texture to use for the button
                ApplicationLauncher.Instance.AddModApplication(EnableGui, DisableGui, Dummy, Dummy, Dummy, Dummy,
                    ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB, buttonTexture);
                HasAddedButton = true;
            }
        }

        private void EnableGui()
        {
            GuiEnabled = true;
            Debug.Log("[HP_Armor_Editor]: Showing HP/Armor GUI");
        }

        private void DisableGui()
        {
            GuiEnabled = false;
            Debug.Log("[HP_Armor_Editor]: Hiding HP/Armor GUI");
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
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(0, 0, WindowWidth, 20), "HP/Armor Editor", titleStyle);
        }

        private void DrawHP(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "HP/Armor",
                leftLabel);
            float textFieldWidth =55;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiHP = GUI.TextField(fwdFieldRect, _guiHP);
        }

        private void DrawHPPercent(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "HP %",
                leftLabel);
            float textFieldWidth = 55;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _percent = GUI.TextField(fwdFieldRect, _percent);
        }

        private void DrawSaveHP(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Save HP"))
            {
                AdjustHP();
            }
        }

        private void DrawSaveArmor(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Save Armor"))
            {
                AdjustArmor();
            }
        }

        private void DrawSavePercent(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Save HP %"))
            {
                AdjustHPPercent();
            }
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
                "Select Parts to Adjust",
                titleStyle);
        }

        private void AllParts(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (allParts)
            {
                if (GUI.Button(saveRect, "All Parts   [On]"))
                    allParts = false;
            }
            else
            {
                if (GUI.Button(saveRect, "All Parts  [Off]"))
                    allParts = true;

            }
        }

        private void ShowHP(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (showHPtotal)
            {
                if (GUI.Button(saveRect, "Hide HP Total"))
                    showHPtotal = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Show HP Total"))
                    showHPtotal = true;
            }
        }

        private void moduleCommand(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (command)
            {
                if (GUI.Button(saveRect, "Command  [On]"))
                    command = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Command [Off]"))
                    command = true;
            }
        }

        private void moduleLiftingSurface(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (MLS)
            {
                if (GUI.Button(saveRect, "Wings   [On]"))
                    MLS = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Wings  [Off]"))
                    MLS = true;
            }
        }

        private void moduleControlSurface(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (MCS)
            {
                if (GUI.Button(saveRect, "Ctrl Surf  [On]"))
                    MCS = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Ctrl Surf [Off]"))
                    MCS = true;
            }
        }

        private void FuelTanks(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (fuelTank)
            {
                if (GUI.Button(saveRect, "Fuel  [On]"))
                    fuelTank = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Fuel  [Off]"))
                    fuelTank = true;
            }
        }

        private void Structural(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (structural)
            {
                if (GUI.Button(saveRect, "Structural [On]"))
                    structural = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Structural [Off]"))
                    structural = true;
            }
        }

        private void ShipHull(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (shipHull)
            {
                if (GUI.Button(saveRect, "Ship Hull [On]"))
                    shipHull = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Ship Hull [Off]"))
                    shipHull = true;
            }
        }

        #endregion

        private void Dummy()
        {
        }
    }
}