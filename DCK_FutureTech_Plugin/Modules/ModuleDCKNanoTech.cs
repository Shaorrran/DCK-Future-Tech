using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BDArmory.Core.Module;

namespace DCK_FutureTech
{
    public class ModuleDCKNanoTech : ModuleResourceConverter
    {

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "DCK NanoTech"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Off", enabledText = "On")]
        public bool autoRepair = false;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Nanite Intensity"),
         UI_FloatRange(controlEnabled = true, scene = UI_Scene.All, minValue = 0.0f, maxValue = 1.0f, stepIncrement = 0.05f)]
        public float naniteMass = 0.05f;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Auto Generate G.L.U.E"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Off", enabledText = "On")]
        public bool generator = false;

        private float _naniteMass = 0.0f;
        private float RequiredGLUE = 0.0f;


        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                part.force_activate();
            }
            base.OnStart(state);
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (generator)
                {
                    CheckGLUEGen();
                    CheckEC();
                }

                if (autoRepair)
                {
                    GenerateHP();
                    CheckGLUE();
                }
            }
        }

        private void GenerateHP()
        {
            List<HitpointTracker> nanoParts = new List<HitpointTracker>(200);
            foreach (Part p in vessel.Parts)
            {
                nanoParts.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker nanoPart in nanoParts)
            {
                if (!nanoPart.part.Modules.Contains("ModuleEngines") && !nanoPart.part.Modules.Contains("ModuleEnginesFX")
                    && !nanoPart.part.Modules.Contains("ModuleDecouple") && !nanoPart.part.Modules.Contains("LaunchClamp"))
                {
                    if (!nanoPart.part.Modules.Contains("ModuleParachute") && !nanoPart.part.Modules.Contains("ModuleAnchoredDecoupler")
                        && !nanoPart.part.Modules.Contains("ModuleDCKShields"))
                    {
                        if (nanoPart.Hitpoints < nanoPart.maxHitPoints)
                        {
                            float HPtoAdd = 0.0f;
                            RequiredGLUE = Time.deltaTime * naniteMass / 10;
                            float glue = part.RequestResource("GLUE", RequiredGLUE);
                            HPtoAdd = (glue * 10) * naniteMass * 10;

                            if (HPtoAdd > 0)
                            {
                                nanoPart.Hitpoints += HPtoAdd;
                            }
                        }
                    }
                }
            }
        }

        private void CheckGLUE()
        {
            double totalAmount = 0;
            double maxAmount = 0;

            foreach (var p in vessel.parts)
            {
                PartResource r = p.Resources.Where(pr => pr.resourceName == "GLUE").FirstOrDefault();
                if (r != null)
                {
                    totalAmount += r.amount;
                    maxAmount += r.maxAmount;
                }
            }

            if (totalAmount < maxAmount * 0.001f)
            {
                autoRepair = false;
            }
        }



        private void CheckEC()
        {
            double totalAmount = 0;
            double maxAmount = 0;

            foreach (var p in vessel.parts)
            {
                PartResource r = p.Resources.Where(pr => pr.resourceName == "ElectricCharge").FirstOrDefault();
                if (r != null)
                {
                    totalAmount += r.amount;
                    maxAmount += r.maxAmount;
                }
            }

            if (totalAmount < maxAmount * 0.15 && IsActivated)
            {
                StopResourceConverter();
            }
        }


        private void CheckGLUEGen()
        {
            double totalAmount = 0;
            double maxAmount = 0;

            PartResource r = part.Resources.Where(pr => pr.resourceName == "GLUE").FirstOrDefault();
            if (r != null)
            {
                totalAmount = r.amount;
                maxAmount = r.maxAmount;

                if (totalAmount < maxAmount * 0.75)
                {
                    StartResourceConverter();
                }

                if (totalAmount > maxAmount * 0.99)
                {
                    StopResourceConverter();
                }
            }
        }
    }
}