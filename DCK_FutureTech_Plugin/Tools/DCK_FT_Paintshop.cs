using KSP.UI.Screens;
using System.Collections.Generic;
using UnityEngine;

namespace DCK_FutureTech
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class DCK_FT_Paintshop : MonoBehaviour
    {

        public static DCK_FT_Paintshop Instance = null;
        private ApplicationLauncherButton toolbarButton = null;
        private bool showWindow = false;
        private Rect windowRect;

        void Awake()
        {
        }

        void Start()
        {
            Instance = this;
            windowRect = new Rect(Screen.width - 215, Screen.height - 500, 173, 75);  //default size and coordinates, change as suitable
            AddToolbarButton();
        }

        private void OnDestroy()
        {
            if (toolbarButton)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
                toolbarButton = null;
            }
        }

        void AddToolbarButton()
        {
            string textureDir = "DCK_FutureTech/Plugin/";

            if (toolbarButton == null)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture(textureDir + "DCKFT_shields", false); //texture to use for the button
                toolbarButton = ApplicationLauncher.Instance.AddModApplication(ShowToolbarGUI, HideToolbarGUI, Dummy, Dummy, Dummy, Dummy, 
                    ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB, buttonTexture);
            }
        }

        public void ShowToolbarGUI()
        {
            showWindow = true;
        }

        public void HideToolbarGUI()
        {
            showWindow = false;
        }

        void Dummy()
        { }

        void OnGUI()
        {
            if (showWindow)
            {
                windowRect = GUI.Window(this.GetInstanceID(), windowRect, DCKFTWindow, "FutureTech Shields", HighLogic.Skin.window);   //change title as suitable
            }
        }

        void DCKFTWindow(int windowID)
        {
            if (GUI.Button(new Rect(10, 25, 75, 20), "Prev", HighLogic.Skin.button))    //change rect here for button size, position and text
            {
                SendEventShields(false);
            }

            if (GUI.Button(new Rect(90, 25, 75, 20), "Next", HighLogic.Skin.button))       //change rect here for button size, position and text
            {
                SendEventShields(true);
            }

            if (GUI.Button(new Rect(10, 50, 75, 20), "Deploy", HighLogic.Skin.button))    //change rect here for button size, position and text
            {
                deployShields();
            }

            if (GUI.Button(new Rect(90, 50, 75, 20), "Retract", HighLogic.Skin.button))       //change rect here for button size, position and text
            {
                retractShields();
            }

            GUI.DragWindow();
        }

        public void sanityCheck()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
            }
        }


        public void deployShields()
        {
            Part root = EditorLogic.RootPart;
            if (!root)
                return;            // find all ModuleDeployableRadiator modules on all parts
            List<ModuleDeployableRadiator> shieldParts = new List<ModuleDeployableRadiator>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                shieldParts.AddRange(p.FindModulesImplementing<ModuleDeployableRadiator>());
            }
            foreach (ModuleDeployableRadiator shieldPart in shieldParts)
            {
                shieldPart.Extend();
            }
        }

        public void retractShields()
        {
            Part root = EditorLogic.RootPart;
            if (!root)
                return;            // find all ModuleDeployableRadiator modules on all parts
            List<ModuleDeployableRadiator> shieldParts = new List<ModuleDeployableRadiator>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                shieldParts.AddRange(p.FindModulesImplementing<ModuleDeployableRadiator>());
            }
            foreach (ModuleDeployableRadiator shieldPart in shieldParts)
            {
                shieldPart.Retract();
            }
        }
        
        void SendEventShields(bool next)  //true: next texture, false: previous texture
        {
            Part root = EditorLogic.RootPart;
            if (!root)
                return;            // find all DCKtextureswitch2 modules on all parts
            List<DCKFTtextureswitch2> shieldParts = new List<DCKFTtextureswitch2>(200);
            foreach (Part p in EditorLogic.fetch.ship.Parts)
            {
                shieldParts.AddRange(p.FindModulesImplementing<DCKFTtextureswitch2>());
            }
            foreach (DCKFTtextureswitch2 shieldPart in shieldParts)
            {
                shieldPart.updateSymmetry = false;             //FIX symmetry problems because DCK also applies its own logic here
                                                            // send previous or next command
                if (next)
                    shieldPart.nextTextureEvent();
                else
                    shieldPart.previousTextureEvent();
            }
        }
    }
}