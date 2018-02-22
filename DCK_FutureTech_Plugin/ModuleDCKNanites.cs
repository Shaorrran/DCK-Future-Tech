using BDArmory.Core;
using BDArmory.Core.Module;
using BDArmory.Core.Extension;
using UnityEngine;

namespace DCK_FutureTech
{
    public class ModuleDCKNanites : PartModule
    {
        const string modName = "[DCK_Nanites]";

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "DCK Nanites"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Off", enabledText = "On")]
        public bool autoRepair = false;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Nanite Mass"),
         UI_FloatRange(controlEnabled = true, scene = UI_Scene.All, minValue = 0.05f, maxValue = 1f, stepIncrement = 0.05f)]
        public float naniteMass = 0.05f;

        private float Armor = 0.0f;
        private bool setMaxHP = true;
        private float hpMax = 0.0f;
        private float armorMax = 0.0f;
        private float RequiredOoze = 0.0f;
        private HitpointTracker hpTracker;
        private readonly float hitpointMultiplier = BDArmorySettings.HITPOINT_MULTIPLIER;

        public override void OnStart(StartState state)
        {
            hpTracker = HPControl();

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (setMaxHP)
                {
                    SetMaxHP();
                    setMaxHP = false;
                }
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                CheckArmorMax();
                part.force_activate();
            }
            base.OnStart(state);
        }

        public void LateUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (autoRepair)
                {
                    CheckNanites();
                }
            }
        }

        #region Core
        /// <summary>
        /// Core
        /// </summary>
        private void CheckNanites()
        {
            if (hpTracker.Hitpoints < hpTracker.maxHitPoints * 0.99f)
            {
                GenerateHP();
            }
        }

        private float MaxHPcalc()
        {
            float hitpoints;

            if (!part.IsMissile())
            {
                //1. Density of the dry mass of the part.
                var density = part.GetDensity();

                //2. Incresing density based on crash tolerance
                density += Mathf.Clamp(part.crashTolerance, 10f, 30f);

                //12 square meters is the standard size of MK1 fuselage using it as a base
                var areaExcess = Mathf.Max(part.GetArea() - 12f, 0);
                var areaCalculation = Mathf.Min(12f, part.GetArea()) + Mathf.Pow(areaExcess, (1f / 3f));

                //3. final calculations 
                hitpoints = areaCalculation * density * hitpointMultiplier;
                hitpoints = Mathf.Round(hitpoints / 500) * 500;

                if (hitpoints <= 0) hitpoints = 500;
            }
            else
            {
                hitpoints = 5;
                Armor = 2;
            }
            
            if (hitpoints <= 0) hitpoints = 500;
            return hitpoints;
        }

        private void SetMaxHP()
        {
            hpMax = hpTracker.maxHitPoints;

            if (hpMax == 0)
            {
                var _maxHP = MaxHPcalc();
                hpTracker.maxHitPoints = _maxHP;
                hpTracker.Hitpoints = _maxHP;
                part.RefreshAssociatedWindows();
            }
        }

        private void CheckArmorMax()
        {
            hpMax = hpTracker.maxHitPoints;
            armorMax = hpTracker.ArmorThickness;
        }

        private HitpointTracker HPControl()
        {
            HitpointTracker hpControl = null;

            hpControl = part.FindModuleImplementing<HitpointTracker>();

            return hpControl;
        }
        #endregion

        #region Resources
        /// <summary>
        /// Resources
        /// </summary>
        public void GenerateHP()
        {
            float HPtoAdd = 0.0f;
            if (hpTracker.Hitpoints < hpMax * 0.99f)
            {
                RequiredOoze = Time.deltaTime * naniteMass;
                float AcquiredOoze = part.RequestResource("NaniteOoze", RequiredOoze);

                HPtoAdd = (RequiredOoze * 10) * naniteMass * 100;

                if (HPtoAdd > 0)
                {
                    hpTracker.Hitpoints += HPtoAdd;
                }
            }
        }

        public void GenerateArmor()
        {
            float ArmorToAdd = 0.0f;
            if (hpTracker.Armor < armorMax * 0.99)
            {
                RequiredOoze = Time.deltaTime * naniteMass * 100;
                float AcquiredOoze = part.RequestResource("NaniteOoze", RequiredOoze);

                ArmorToAdd = RequiredOoze * naniteMass;

                if (ArmorToAdd > 0)
                {
                    hpTracker.Armor += ArmorToAdd;
                }
            }
        }
        #endregion
    }
}