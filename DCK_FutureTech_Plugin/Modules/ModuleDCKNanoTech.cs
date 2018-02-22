using System.Collections.Generic;

namespace DCK_FutureTech
{
    public class ModuleDCKNanoTech : ModuleResourceConverter
    {
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "DCK NanoTech"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Off", enabledText = "On")]
        public bool autoRepair = false;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Nanite Mass - Master"),
         UI_FloatRange(controlEnabled = true, scene = UI_Scene.All, minValue = 0.0f, maxValue = 1.0f, stepIncrement = 0.05f)]
        public float naniteMass = 0.05f;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Nanite Mass"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Set", enabledText = "")]
        public bool overrideNanites = false;

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                part.force_activate();
            }
        }

        public override void OnFixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (autoRepair)
                {
                    StartResourceConverter();
                    NanoRepairOn();
                }
                else
                {
                    NanoRepairOff();
                }

                if (overrideNanites)
                {
                    OverrideNanites();
                    overrideNanites = false;
                }

            }
            base.OnFixedUpdate();
        }

        public void OverrideNanites()
        {
            List<ModuleDCKNanites> nanoParts = new List<ModuleDCKNanites>(200);
            foreach (Part p in vessel.Parts)
            {
                nanoParts.AddRange(p.FindModulesImplementing<ModuleDCKNanites>());
            }
            foreach (ModuleDCKNanites nanoPart in nanoParts)
            {
                nanoPart.naniteMass = naniteMass;
            }
        }

        public void NanoRepairOn()
        {
            List<ModuleDCKNanites> nanoParts = new List<ModuleDCKNanites>(200);
            foreach (Part p in vessel.Parts)
            {
                nanoParts.AddRange(p.FindModulesImplementing<ModuleDCKNanites>());
            }
            foreach (ModuleDCKNanites nanoPart in nanoParts)
            {
                if (!nanoPart.autoRepair)
                {
                    nanoPart.autoRepair = true;
                }
            }
        }

        public void NanoRepairOff()
        {
            List<ModuleDCKNanites> nanoParts = new List<ModuleDCKNanites>(200);
            foreach (Part p in vessel.Parts)
            {
                nanoParts.AddRange(p.FindModulesImplementing<ModuleDCKNanites>());
            }
            foreach (ModuleDCKNanites nanoPart in nanoParts)
            {
                if (nanoPart.autoRepair)
                {
                    nanoPart.autoRepair = false;
                }
            }
        }
    }
}