using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;
using BDArmory.Core.Module;

// ReSharper disable NotAccessedField.Local

namespace DCK_FutureTech
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class DCK_FutureTech : MonoBehaviour
    {
        private const float WindowWidth = 150;
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
        private string _guiHP = String.Empty;
        private string _guiArmor = String.Empty;
        private float _windowHeight = 250;
        private Rect _windowRect;
        private bool addTotal;
        private HitpointTracker hpTracker;

        private bool MLF;
        private bool engines;
        private bool allParts;

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

        // ReSharper disable once InconsistentNaming
        private void OnGUI()
        {
            if (GuiEnabled && _gameUiToggle)
                _windowRect = GUI.Window(320, _windowRect, GuiWindow, "");
        }

        private void GuiWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            float line = 0;
            _contentWidth = WindowWidth - 2 * LeftIndent;

            DrawTitle();
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

        private void DrawText(float line)
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(0, ContentTop + line * entryHeight, WindowWidth, 20),
                "Parts to Adjust",
                titleStyle);
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
            var saveRect = new Rect(LeftIndent, ContentTop + line * entryHeight, WindowWidth / 2, entryHeight);
            if (GUI.Button(saveRect, "Apply"))
                ApplyHP();
        }

        private void DrawSaveArmor(float line)
        {
            var saveRect = new Rect(LeftIndent, ContentTop + line * entryHeight, WindowWidth / 2, entryHeight);
            if (GUI.Button(saveRect, "Apply"))
                ApplyArmor();
        }

        private void selectPartModule()
        {
            Part root = EditorLogic.RootPart;
            if (!root)
                return;

            if (allParts)
            {
//                AllParts();
                return;
            }

            if (MLF)
            {
                moduleLiftingSurface();
            }

            if (engines)
            {
                moduleEngines();
                moduleEnginesFX();
            }
        }

        private void ApplyHP()
        {
            List<HitpointTracker> HPtracker = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                HPtracker.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in HPtracker)
            {
                // HitPoints
                if (hpTracker.Hitpoints >= 0)
                {
                    var hpString = _guiHP;
                    var hpTotal = float.Parse(hpString);
                    hpTracker.maxHitPoints = hpTotal;
                    hpTracker.Hitpoints = hpTotal;

                    if (hpTracker.Hitpoints <= 1)
                    {
                        hpTracker.maxHitPoints = 1;
                        hpTracker.Hitpoints = 1;
                    }
                }
            }
        }

        private void ApplyArmor()
        {
            List<HitpointTracker> HPtracker = new List<HitpointTracker>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                HPtracker.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpTracker in HPtracker)
            {
                // Armor
                if (hpTracker.Armor >= 0)
                {
                    var armorString = _guiArmor;
                    float ArmorValue = float.Parse(armorString);
                    hpTracker.ArmorThickness = ArmorValue;
                    hpTracker.Armor = ArmorValue;

                    if (hpTracker.Armor <= 1)
                    {
                        hpTracker.ArmorThickness = 1;
                        hpTracker.Armor = 1;
                    }
                }
            }
        }


        private void moduleLiftingSurface()
        {
            Part root = EditorLogic.RootPart;
            if (!root)
                return;

            List<ModuleLiftingSurface> AeroParts = new List<ModuleLiftingSurface>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                AeroParts.AddRange(p.FindModulesImplementing<ModuleLiftingSurface>());
            }
            foreach (ModuleLiftingSurface aeroPart in AeroParts)
            {
                if (aeroPart != null)
                {
                    List<HitpointTracker> HPtracker = new List<HitpointTracker>(200);
                    foreach (HitpointTracker hpTracker in HPtracker)
                    {
                        // HitPoints
                        if (hpTracker.Hitpoints >= 0)
                        {
                            var hpString = _guiHP;
                            var hpTotal = float.Parse(hpString);
                            hpTracker.maxHitPoints = hpTotal;
                            hpTracker.Hitpoints = hpTotal;

                            if (hpTracker.Hitpoints <= 1)
                            {
                                hpTracker.maxHitPoints = 1;
                                hpTracker.Hitpoints = 1;
                            }
                        }

                        // Armor
                        if (hpTracker.Armor >= 0)
                        {
                            var armorString = _guiArmor;
                            float ArmorValue = float.Parse(armorString);
                            hpTracker.ArmorThickness = ArmorValue;
                            hpTracker.Armor = ArmorValue;

                            if (hpTracker.Armor <= 1)
                            {
                                hpTracker.ArmorThickness = 1;
                                hpTracker.Armor = 1;
                            }
                        }
                    }
                }
            }
        }

        private void moduleEngines()
        {
            Part root = EditorLogic.RootPart;
            if (!root)
                return;

            List<ModuleEngines> EngineParts = new List<ModuleEngines>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                EngineParts.AddRange(p.FindModulesImplementing<ModuleEngines>());
            }
            foreach (ModuleEngines enginePart in EngineParts)
            {
                if (enginePart != null)
                {
                    List<HitpointTracker> HPtracker = new List<HitpointTracker>(200);
                    foreach (HitpointTracker hpTracker in HPtracker)
                    {
                        // HitPoints
                        if (hpTracker.Hitpoints >= 0)
                        {
                            var hpString = _guiHP;
                            var hpTotal = float.Parse(hpString);
                            hpTracker.maxHitPoints = hpTotal;
                            hpTracker.Hitpoints = hpTotal;

                            if (hpTracker.Hitpoints <= 1)
                            {
                                hpTracker.maxHitPoints = 1;
                                hpTracker.Hitpoints = 1;
                            }
                        }

                        // Armor
                        if (hpTracker.Armor >= 0)
                        {
                            var armorString = _guiArmor;
                            float ArmorValue = float.Parse(armorString);
                            hpTracker.ArmorThickness = ArmorValue;
                            hpTracker.Armor = ArmorValue;

                            if (hpTracker.Armor <= 1)
                            {
                                hpTracker.ArmorThickness = 1;
                                hpTracker.Armor = 1;
                            }
                        }
                    }
                }
            }
        }

        private void moduleEnginesFX()
        {
            Part root = EditorLogic.RootPart;
            if (!root)
                return;

            List<ModuleEngines> EngineParts = new List<ModuleEngines>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                EngineParts.AddRange(p.FindModulesImplementing<ModuleEngines>());
            }
            foreach (ModuleEngines enginePart in EngineParts)
            {
                if (enginePart != null)
                {
                    List<HitpointTracker> HPtracker = new List<HitpointTracker>(200);
                    foreach (HitpointTracker hpTracker in HPtracker)
                    {
                        // HitPoints
                        if (hpTracker.Hitpoints >= 0)
                        {
                            var hpString = _guiHP;
                            var hpTotal = float.Parse(hpString);
                            hpTracker.maxHitPoints = hpTotal;
                            hpTracker.Hitpoints = hpTotal;

                            if (hpTracker.Hitpoints <= 1)
                            {
                                hpTracker.maxHitPoints = 1;
                                hpTracker.Hitpoints = 1;
                            }
                        }

                        // Armor
                        if (hpTracker.Armor >= 0)
                        {
                            var armorString = _guiArmor;
                            float ArmorValue = float.Parse(armorString);
                            hpTracker.ArmorThickness = ArmorValue;
                            hpTracker.Armor = ArmorValue;

                            if (hpTracker.Armor <= 1)
                            {
                                hpTracker.ArmorThickness = 1;
                                hpTracker.Armor = 1;
                            }
                        }
                    }
                }
            }
        }

        private void AllParts(float line)
        {
            var saveRect = new Rect(LeftIndent, ContentTop + line * entryHeight, contentWidth, entryHeight);


            if (allParts)
            {
                if (GUI.Button(saveRect, "All{On}"))
                    allParts = false;
            }
            else
            {
                if (GUI.Button(saveRect, "All{Off}"))
                    allParts = true;
            }
        }

        private void DrawEngines(float line)
        {
            var saveRect = new Rect(LeftIndent, ContentTop + line * entryHeight, contentWidth, entryHeight);


            if (engines)
            {
                if (GUI.Button(saveRect, "Engine{On}"))
                    engines = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Engine{Off}"))
                    engines = true;
            }
        }

        private void DrawLiftingSurface(float line)
        {
            var saveRect = new Rect(LeftIndent, ContentTop + line * entryHeight, contentWidth, entryHeight);


            if (MLF)
            {
                if (GUI.Button(saveRect, "Wings{On}"))
                    MLF = false;
            }
            else
            {
                if (GUI.Button(saveRect, "Wings{Off}"))
                    MLF = true;
            }
        }

        private void DrawTitle()
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = {textColor = Color.white}
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(0, 0, WindowWidth, 20), "HP/Armor Adjustment", titleStyle);
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

        private void Dummy()
        {
        }

        private void GameUiEnable()
        {
            _gameUiToggle = true;
        }

        private void GameUiDisable()
        {
            _gameUiToggle = false;
        }
    }
}