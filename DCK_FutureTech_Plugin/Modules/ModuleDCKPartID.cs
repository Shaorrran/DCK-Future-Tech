using BDArmory.Parts;
using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;
using BDArmory.Core.Module;


namespace DCK_FutureTech
{
    public class ModuleDCKPartID : PartModule
    {
        public float adjustedArmor = 0.0f;
        public float adjustedHP = 0.0f;

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                PartModuleID();
                CheckResources();
            }
            base.OnStart(state);
        }

        public bool fuelTank = false;
        public bool shipHull = false;
        public bool ballastTank = false;
        public bool solidFuel = false;
        public bool engine = false;
        public bool command = false;
        public bool MLS = false;
        public bool MCS = false;
        public bool structural = false;


        #region Module/Resource Identification
        /// <summary>
        /// Module/Resource Identification
        /// </summary>
        private void PartModuleID()
        {
            mls = CheckMLS();
            mcs = CheckMCS();
            moduleEngineFX = CheckMEFX();
            moduleEngine = CheckME();
            moduleCommand = CheckMC();

            if (mls !=null) // Identify if part is a lifting surface
            {
                MLS = true;
                fuelTank = false;
            }

            if (mcs != null)
            {
                MCS = true;
                fuelTank = false;
            }
            
            if (moduleEngine != null || moduleEngineFX != null) // Identify if the part is an engine
            {
                engine = true;
            }

            if (moduleCommand != null)
            {
                command = true;
            }

            if (moduleEngine == null || moduleEngineFX == null || moduleCommand == null || mcs == null || mls == null)
            {
                structural = true;
            }
        }

        private void CheckResources()
        {
            if (!MLS && (part.Resources.Contains("LiquidFuel") || part.Resources.Contains("Oxidizer"))) // Identify if part is a fuel tank
            {
                fuelTank = true;
            }

            if (part.Resources.Contains("SolidFuel")) // Identify parts with solid fuel
            {
                solidFuel = true;
            }

            if (part.Resources.Contains("SeaWater")) // Identify if part is a ship hull
            {
                shipHull = true;
                fuelTank = false;
            }

            if (part.Resources.Contains("BallastWater")) // Find ballast tanks so as to exclude them from HP adjustment
            {
                ballastTank = true;
            }
        }

        #endregion

        #region Part Module Interface
        /// <summary>
        /// Part Module Interface
        /// </summary>
        private ModuleEngines moduleEngine;
        private ModuleEngines CheckME()
        {
            ModuleEngines me = null;

            me = part.FindModuleImplementing<ModuleEngines>();

            return me;
        }
        private ModuleEnginesFX moduleEngineFX;
        private ModuleEnginesFX CheckMEFX()
        {
            ModuleEnginesFX mefx = null;

            mefx = part.FindModuleImplementing<ModuleEnginesFX>();

            return mefx;
        }

        private ModuleCommand moduleCommand;
        private ModuleCommand CheckMC()
        {
            ModuleCommand mc = null;

            mc = part.FindModuleImplementing<ModuleCommand>();

            return mc;
        }

        private ModuleLiftingSurface mls;
        private ModuleLiftingSurface CheckMLS()
        {
            ModuleLiftingSurface ls = null;

            ls = part.FindModuleImplementing<ModuleLiftingSurface>();

            return ls;
        }

        private ModuleControlSurface mcs;
        private ModuleControlSurface CheckMCS()
        {
            ModuleControlSurface cs = null;

            cs = part.FindModuleImplementing<ModuleControlSurface>();

            return cs;
        }

        public HitpointTracker hpTracker;
        private HitpointTracker GetTracker()
        {
            HitpointTracker hp = null;

            hp = part.FindModuleImplementing<HitpointTracker>();

            return hp;
        }
        #endregion

        #region HP/Armor Adjust
        /// <summary>
        /// HP/Armor Adjust
        /// </summary>
        public void AdjustArmor()
        {
            hpTracker = GetTracker();
            hpTracker.ArmorThickness = adjustedArmor;
            hpTracker.Armor = adjustedArmor;

            if (hpTracker.Armor <= 1)
            {
                hpTracker.ArmorThickness = 0;
                hpTracker.Armor = 0;
            }
        }

        public void AdjustHP()
        {
            hpTracker = GetTracker();
            hpTracker.maxHitPoints = adjustedHP;
            hpTracker.Hitpoints = adjustedHP;

            if (hpTracker.Hitpoints <= 1)
            {
                hpTracker.maxHitPoints = 0;
                hpTracker.Hitpoints = 0;
            }
        }
        #endregion
    }
}