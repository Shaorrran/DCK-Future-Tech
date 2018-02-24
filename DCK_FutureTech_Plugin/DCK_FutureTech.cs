using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;


namespace DCK_FutureTech
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class DCK_FutureTech : MonoBehaviour
    {
        private const float WindowWidth = 140;
        private const float DraggableHeight = 40;
        private const float LeftIndent = 12;
        private const float ContentTop = 20;
        public static DCK_FutureTech Fetch;
        public static bool GuiEnabled;
        public static bool HasAddedButton;
        private readonly float _incrButtonWidth = 26;
        private readonly float contentWidth = WindowWidth - 2 * LeftIndent;
        private readonly float entryHeight = 20;
        private float _contentWidth;
        private bool _gameUiToggle;
        public string _guiHP = String.Empty;
        public string _guiArmor = String.Empty;
        private float _windowHeight = 250;
        private Rect _windowRect;
        private bool MLS;
        private bool MCS;
        private bool engines;
        private bool allParts;
        private bool fuelTank;
        private bool command;
        private bool shipHull;
        private bool structural;

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
            _guiArmor = "0";
            _guiHP = "0";
        }

        private void OnGUI()
        {
            if (GuiEnabled && _gameUiToggle)
                _windowRect = GUI.Window(320, _windowRect, GuiWindow, "");
        }

        #region HP/Armor Adjust
        /// <summary>
        /// HP/Armor Adjust
        /// </summary>
        private void AdjustHP()
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

            if (engines)
            {
                HPEngines();
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

        private void AdjustArmor()
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

            if (engines)
            {
                ArmorEngines();
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

        #region HP
        /// <summary>
        /// HP
        /// </summary>
        private void HPAllParts()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                var guiHP = float.Parse(_guiHP);
                identifiedPart.adjustedHP = guiHP;
                identifiedPart.AdjustHP();
            }
        }

        private void HPCommand()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.command)
                {
                    var guiHP = float.Parse(_guiHP);
                    identifiedPart.adjustedHP = guiHP;
                    identifiedPart.AdjustHP();
                }
            }
        }

        private void HPFuelTank()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.fuelTank)
                {
                    var guiHP = float.Parse(_guiHP);
                    identifiedPart.adjustedHP = guiHP;
                    identifiedPart.AdjustHP();
                }
            }
        }

        private void HPMCS()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.MCS)
                {
                    var guiHP = float.Parse(_guiHP);
                    identifiedPart.adjustedHP = guiHP;
                    identifiedPart.AdjustHP();
                }
            }
        }

        private void HPMLS()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.MLS)
                {
                    var guiHP = float.Parse(_guiHP);
                    identifiedPart.adjustedHP = guiHP;
                    identifiedPart.AdjustHP();
                }
            }
        }

        private void HPStructural()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.structural)
                {
                    var guiHP = float.Parse(_guiHP);
                    identifiedPart.adjustedHP = guiHP;
                    identifiedPart.AdjustHP();
                }
            }
        }

        private void HPEngines()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.engine)
                {
                    var guiHP = float.Parse(_guiHP);
                    identifiedPart.adjustedHP = guiHP;
                    identifiedPart.AdjustHP();
                }
            }
        }

        private void HPShipHull()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.shipHull || identifiedPart.ballastTank)
                {
                    var guiHP = float.Parse(_guiHP);
                    identifiedPart.adjustedHP = guiHP;
                    identifiedPart.AdjustHP();
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
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                var guiArmor = float.Parse(_guiArmor);
                identifiedPart.adjustedHP = guiArmor;
                identifiedPart.AdjustArmor();
            }
        }

        private void ArmorCommand()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.command)
                {
                    var guiArmor = float.Parse(_guiArmor);
                    identifiedPart.adjustedHP = guiArmor;
                    identifiedPart.AdjustArmor();
                }
            }
        }

        private void ArmorFuelTank()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.fuelTank)
                {
                    var guiArmor = float.Parse(_guiArmor);
                    identifiedPart.adjustedHP = guiArmor;
                    identifiedPart.AdjustArmor();
                }
            }
        }

        private void ArmorMCS()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.MCS)
                {
                    var guiArmor = float.Parse(_guiArmor);
                    identifiedPart.adjustedHP = guiArmor;
                    identifiedPart.AdjustArmor();
                }
            }
        }

        private void ArmorMLS()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.MLS)
                {
                    var guiArmor = float.Parse(_guiArmor);
                    identifiedPart.adjustedHP = guiArmor;
                    identifiedPart.AdjustArmor();
                }
            }
        }

        private void ArmorStructural()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.structural)
                {
                    var guiArmor = float.Parse(_guiArmor);
                    identifiedPart.adjustedHP = guiArmor;
                    identifiedPart.AdjustHP();
                }
            }
        }

        private void ArmorEngines()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.engine)
                {
                    var guiArmor = float.Parse(_guiArmor);
                    identifiedPart.adjustedHP = guiArmor;
                    identifiedPart.AdjustArmor();
                }
            }
        }

        private void ArmorShipHull()
        {
            List<ModuleDCKPartID> identifiedParts = new List<ModuleDCKPartID>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                identifiedParts.AddRange(p.FindModulesImplementing<ModuleDCKPartID>());
            }
            foreach (ModuleDCKPartID identifiedPart in identifiedParts)
            {
                if (identifiedPart.shipHull || identifiedPart.ballastTank)
                {
                    var guiArmor = float.Parse(_guiArmor);
                    identifiedPart.adjustedHP = guiArmor;
                    identifiedPart.AdjustArmor();
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
            line++;
            Engines(line);
            line++;
            Structural(line);
            line++;
            ShipHull(line);
            line++;
            line++;
            DrawHP(line);
            line++;
            DrawSaveHP(line);
            line++;
            line++;
            DrawArmor(line);
            line++;
            DrawSaveArmor(line);

            _windowHeight = ContentTop + line * entryHeight + entryHeight + entryHeight;
            _windowRect.height = _windowHeight;
        }

        private void AddToolbarButton()
        {
            string textureDir = "DCK_FutureTech/Plugin/";

            if (!HasAddedButton)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture(textureDir + "DCK_FT", false); //texture to use for the button
                ApplicationLauncher.Instance.AddModApplication(EnableGui, DisableGui, Dummy, Dummy, Dummy, Dummy,
                    ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
                HasAddedButton = true;
            }
        }

        private void EnableGui()
        {
            GuiEnabled = true;
            Debug.Log("[DCK_FutureTech]: Showing HP/Armor GUI");
        }

        private void DisableGui()
        {
            GuiEnabled = false;
            Debug.Log("[DCK_FutureTech]: Hiding HP/Armor GUI");
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
            GUI.Label(new Rect(0, 0, WindowWidth, 20), "HP/Armor Adjustment", titleStyle);
        }

        private void DrawHP(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "HitPoints",
                leftLabel);
            float textFieldWidth = 60;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiHP = GUI.TextField(fwdFieldRect, _guiHP);
        }

        private void DrawArmor(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "Armor",
                leftLabel);
            float textFieldWidth = 60;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiArmor = GUI.TextField(fwdFieldRect, _guiArmor);
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
                "Parts to Adjust",
                titleStyle);
        }

        private void AllParts(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (allParts)
            {
                if (GUI.Button(saveRect, "All Parts   {On}"))
                    allParts = false;
            }
            else
            {
                if (GUI.Button(saveRect, "All Parts  {Off}"))
                    allParts = true;

            }
        }

        private void moduleCommand(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (command)
            {
                if (GUI.Button(saveRect, "Command   {On}"))
                    command = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Command  {Off}"))
                    command = true;
            }
        }

        private void moduleLiftingSurface(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (MLS)
            {
                if (GUI.Button(saveRect, "Wings    {On}"))
                    MLS = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Wings    {Off}"))
                    MLS = true;
            }
        }

        private void moduleControlSurface(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (MCS)
            {
                if (GUI.Button(saveRect, "Ctrl Surf   {On}"))
                    MCS = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Ctrl Surf  {Off}"))
                    MCS = true;
            }
        }

        private void FuelTanks(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (fuelTank)
            {
                if (GUI.Button(saveRect, "Fuel Tank {On}"))
                    fuelTank = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Fuel Tank {Off}"))
                    fuelTank = true;
            }
        }

        private void Structural(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (structural)
            {
                if (GUI.Button(saveRect, "Structural {On}"))
                    structural = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Structural{Off}"))
                    structural = true;
            }
        }

        private void Engines(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (engines)
            {
                if (GUI.Button(saveRect, "Engines  {On}"))
                    engines = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Engines {Off}"))
                    engines = true;
            }
        }

        private void ShipHull(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (shipHull)
            {
                if (GUI.Button(saveRect, "Ship Hull  {On}"))
                    shipHull = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Ship Hull {Off}"))
                    shipHull = true;
            }
        }

        #endregion

        private void Dummy()
        {
        }
    }
}