using System.Collections.Generic;
using BDArmory.Core.Module;

namespace DCK_FutureTech
{
    public class ModuleHullIntegrity : PartModule
    {
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Hull Integrity Field"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Off", enabledText = "On")]
        public bool ballanceHP = true;

        public float vesselHPmax = 0.0f;
        public float vesselHPtotal = 0.0f;

        private float HP = 0.0f;

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                CheckParts();
            }
            base.OnStart(state);
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (ballanceHP)
                {
                    CheckHP();
                    BallanceHP();
                }
            }
        }

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 0.005f, ScreenMessageStyle.UPPER_RIGHT));
        }

        private void CheckHP()
        {
            HP = 0;

            List<HitpointTracker> hpParts = new List<HitpointTracker>(200);
            foreach (Part p in vessel.Parts)
            {
                hpParts.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpPart in hpParts)
            {
                if (!hpPart.part.Modules.Contains("ModuleEngines") && !hpPart.part.Modules.Contains("ModuleEnginesFX")
                    && !hpPart.part.Modules.Contains("ModuleDecouple"))
                {
                    if (!hpPart.part.Modules.Contains("ModuleParachute") && !hpPart.part.Modules.Contains("KerbalEVA")
                         && !hpPart.part.Modules.Contains("LaunchClamp"))
                    {
                        if (!hpPart.part.Modules.Contains("ModuleWeapon") && !hpPart.part.Modules.Contains("ModuleTurret")
                             && !hpPart.part.Modules.Contains("BDExplosivePart") && !hpPart.part.Modules.Contains("ModuleDCKShields"))
                        {
                            HP += hpPart.maxHitPoints - hpPart.Hitpoints;
                        }
                    }
                }
            }
        }

        public void BallanceHP()
        {
            var _vesselHPtotal = vesselHPmax - HP;

            if (_vesselHPtotal <= vesselHPtotal)
            {
                vesselHPtotal = _vesselHPtotal;
                var _HPpercent = vesselHPtotal / vesselHPmax;

                List<HitpointTracker> hpParts = new List<HitpointTracker>(200);
                foreach (Part p in vessel.Parts)
                {
                    hpParts.AddRange(p.FindModulesImplementing<HitpointTracker>());
                }
                foreach (HitpointTracker hpPart in hpParts)
                {
                    if (!hpPart.part.Modules.Contains("ModuleEngines") && !hpPart.part.Modules.Contains("ModuleEnginesFX")
                        && !hpPart.part.Modules.Contains("ModuleDecouple") && !hpPart.part.Modules.Contains("ModuleAnchoredDecoupler"))
                    {
                        if (!hpPart.part.Modules.Contains("ModuleParachute") && !hpPart.part.Modules.Contains("KerbalEVA")
                             && !hpPart.part.Modules.Contains("LaunchClamp"))
                        {
                            if (!hpPart.part.Modules.Contains("ModuleWeapon") && !hpPart.part.Modules.Contains("ModuleTurret")
                                 && !hpPart.part.Modules.Contains("BDExplosivePart") && !hpPart.part.Modules.Contains("ModuleDCKShields"))
                            {
                                hpPart.Hitpoints = hpPart.maxHitPoints * _HPpercent;
                            }
                        }
                    }
                }
            }
        }

        private void CheckParts()
        {
            List<HitpointTracker> hpParts = new List<HitpointTracker>(200);
            foreach (Part p in vessel.Parts)
            {
                hpParts.AddRange(p.FindModulesImplementing<HitpointTracker>());
            }
            foreach (HitpointTracker hpPart in hpParts)
            {
                if (!hpPart.part.Modules.Contains("ModuleEngines") && !hpPart.part.Modules.Contains("ModuleEnginesFX")
                     && !hpPart.part.Modules.Contains("ModuleDecouple") && !hpPart.part.Modules.Contains("ModuleAnchoredDecoupler"))
                {
                    if (!hpPart.part.Modules.Contains("ModuleParachute") && !hpPart.part.Modules.Contains("KerbalEVA")
                         && !hpPart.part.Modules.Contains("LaunchClamp"))
                    {
                        if (!hpPart.part.Modules.Contains("ModuleWeapon") && !hpPart.part.Modules.Contains("ModuleTurret")
                             && !hpPart.part.Modules.Contains("BDExplosivePart") && !hpPart.part.Modules.Contains("ModuleDCKShields"))
                        {
                            hpPart.maxHitPoints = hpPart.Hitpoints;
                            vesselHPmax += hpPart.maxHitPoints;
                            vesselHPtotal += hpPart.Hitpoints;
                        }
                    }
                }
            }
        }
    }
}