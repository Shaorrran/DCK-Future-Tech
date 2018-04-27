using System.Collections.Generic;
using System.Linq;
using BDArmory;
using BDArmory.Core.Module;
using System.Collections;
using UnityEngine;

namespace DCK_FutureTech
{
    public class ModuleDCKShields : PartModule
    {
        const string modName = "[DCK_Shields]";

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Auto Deploy"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Off", enabledText = "On")]
        public bool autoDeploy = true;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Field Intensity %"),
         UI_FloatRange(controlEnabled = true, scene = UI_Scene.All, minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float intensity = 100;

        private bool targeted;
        private bool hpAvailable;
        public bool shieldsDeployed;
        private bool coolDown = false;
        private bool resourceAvailable;
        private bool resourceCheck;
        private bool shieldDeployable;
        private float RequiredEC = 0.0f;
        private float surfaceArea = 0.0f;
        private float areaExponet = 0.5f;

        private ModuleActiveRadiator shieldState;
        private ModuleDeployableRadiator shieldCheck;
        private HitpointTracker hpTracker;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
            {
                shieldState = GetShieldState();
                shieldCheck = ShieldControl();
                hpTracker = GetHP();
                CheckShieldState();
                calcSurfaceArea();
            }
        }

        public void Update()
        {
            CheckShieldHP();
            CheckShieldState();
            CheckEC();
        }

        public void LateUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (shieldsDeployed)
                {
                    if (!resourceAvailable)
                    {
                        lowEC();
                        StartCoroutine(CoolDownRoutine());
                    }

                    if (!hpAvailable)
                    {
                        lowHP();
                        StartCoroutine(CoolDownRoutine());
                    }

                    if (!resourceCheck)
                    {
                        RetractShields();
                    }

                    if (coolDown)
                    {
                        lowHP();
                    }
                }

                if (autoDeploy)
                {
                    UnderFirecheck();

                    if (targeted && !shieldsDeployed && !coolDown)
                    {
                        EnableShields();
                    }
                }
            }
        }

        #region Core
        /// <summary>
        /// Core
        /// </summary>
        IEnumerator CoolDownRoutine()
        {
            coolDown = true;
            yield return new WaitForSeconds(10);
            coolDown = false;
        }

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 4, ScreenMessageStyle.UPPER_CENTER));
        }

        private void UnderFirecheck()
        {
            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in vessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                if (wmPart.underAttack)
                {
                    targeted = true;
                }
                else
                {
                    targeted = false;
                }
            }
        }

        private void CheckShieldState()
        {
            if (shieldState.IsCooling)
            {
                shieldsDeployed = true;
            }
            else
            {
                shieldsDeployed = false;
            }
        }

        private void calcSurfaceArea()
        {
            if (part != null)
            {
                surfaceArea = (float)(surfaceArea + part.skinExposedArea);
            }
        }

        private HitpointTracker GetHP()
        {
            HitpointTracker hp = null;

            hp = part.FindModuleImplementing<HitpointTracker>();

            return hp;
        }

        private ModuleDeployableRadiator ShieldControl()
        {
            ModuleDeployableRadiator shieldControl = null;

            shieldControl = part.FindModuleImplementing<ModuleDeployableRadiator>();

            return shieldControl;
        }

        private ModuleActiveRadiator GetShieldState()
        {
            ModuleActiveRadiator ShieldState = null;

            ShieldState = part.FindModuleImplementing<ModuleActiveRadiator>();

            return ShieldState;
        }

        #endregion

        #region Resources
        /// <summary>
        /// Resources
        /// </summary>

        private void GenerateHP()
        {
            float HPtoAdd = 0;
            if (hpTracker.Hitpoints < hpTracker.maxHitPoints * 0.95f)
            {
                RequiredEC = Time.deltaTime * intensity / 100;
                float AcquiredEC = part.RequestResource("ElectricCharge", RequiredEC);

                HPtoAdd = AcquiredEC * intensity;

                if (HPtoAdd > 0)
                {
                    hpTracker.Hitpoints += HPtoAdd;
                }
            }
        }

        private void CheckShieldHP()
        {
            hpTracker = GetHP();

            if (hpTracker.Hitpoints < hpTracker.maxHitPoints * 0.05)
            {
                hpAvailable = false;
            }
            else
            {
                hpAvailable = true;
            }

            if (hpTracker.Hitpoints < hpTracker.maxHitPoints * 0.35)
            {
                shieldDeployable = false;
            }
            else
            {
                shieldDeployable = true;
            }

            if (hpTracker.Hitpoints < hpTracker.maxHitPoints * 0.99f)
            {
                GenerateHP();
            }

            if (hpTracker.Hitpoints < hpTracker.maxHitPoints * 0.0001 || hpTracker.Armor < hpTracker.ArmorThickness * 0.0001)
            {
                part.explode();
            }
        }

        private void CheckEC()
        {
            foreach (var p in vessel.parts)
            {
                double totalAmount = 0;
                double maxAmount = 0;

                PartResource r = p.Resources.Where(pr => pr.resourceName == "ElectricCharge").FirstOrDefault();
                if (r != null)
                {
                    totalAmount += r.amount;
                    maxAmount += r.maxAmount;

                    if (totalAmount < maxAmount * 0.05)
                    {
                        resourceAvailable = false;
                    }
                    else
                    {
                        resourceAvailable = true;
                    }

                    if (totalAmount < maxAmount * 0.25)
                    {
                        resourceCheck = false;
                    }
                    else
                    {
                        resourceCheck = true;
                    }
                }
            }
        }

        private void lowEC()
        {
            if (vessel.isActiveVessel)
            {
                ScreenMsg("EC too low ...");
            }
            DisableShields();
        }

        private void lowHP()
        {
            if (vessel.isActiveVessel)
            {
                ScreenMsg("Shields Reinitializing ... Please Stand By");
            }
            RetractShields();
        }

        #endregion

        #region Shields
        /// <summary>
        /// Shields
        /// </summary>

        public void EnableShields()
        {
            if (resourceCheck && shieldDeployable)
            {
                if (vessel.isActiveVessel)
                {
                    ScreenMsg("Deploying Shields");
                }
                DeployShields();
            }
            else 
            {
                if (vessel.isActiveVessel && !resourceCheck)
                {
                    ScreenMsg("EC too low ... Shields unable to deploy");
                }

                if (vessel.isActiveVessel && !shieldDeployable)
                {
                    ScreenMsg("Shields Regenerating .. Please Stand By");
                }
            }
        }

        public void DisableShields()
        {
            if (vessel.isActiveVessel)
            {
                ScreenMsg("Retracting Shields");
            }
            RetractShields();
        }

        private void DeployShields()
        {
            if (shieldCheck != null)
            {
                shieldCheck.Extend();
            }
        }

        private void RetractShields()
        {
            if (shieldCheck != null)
            {
                shieldCheck.Retract();
            }
        }
        #endregion
    }
}
