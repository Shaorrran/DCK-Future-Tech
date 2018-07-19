using System.Collections.Generic;
using BDArmory.Modules;
using UnityEngine;
using System.Collections;

namespace DCK_FutureTech
{
    public class ModuleFriendOrFoe : PartModule
    {

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Vessel ID System"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.Flight, disabledText = "Off", enabledText = "SCANNING")]
        public bool vesselIDcheck = false;

        [KSPField(isPersistant = true)]
        public bool myTeam = false;

        public bool vesselID = false;

        public override void OnStart(StartState state)
        {
            Setup();
            base.OnStart(state);
        }
        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (!vesselID && vesselIDcheck)
                {
                    Setup();
                    StartCoroutine(CheckVessels());
                }
            }
        }

        private void Setup()
        {
            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in vessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                myTeam = wmPart.team;
            }
        }


        IEnumerator CheckVessels()
        {
            vesselID = true;
            StartCoroutine(VesselRoutine());
            double aircraft = 0;
            double boat = 0;
//            double sub = 0;
            double ground = 0;
            double cutoff = vessel.altitude * 0.67;

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (!v.LandedOrSplashed && v.altitude < cutoff && !v.HoldPhysics)
                {
                    List<MissileFire> wmParts = new List<MissileFire>(200);
                    foreach (Part p in v.Parts)
                    {
                        wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
                    }
                    foreach (MissileFire wmPart in wmParts)
                    {
                        if (wmPart.team != myTeam)
                        {
                            aircraft += 1;
                            ScreenMsg4(v.vesselName + " Detected");
                            yield return new WaitForSeconds(1.5f);
                        }
                    }
                }

                if (v.Splashed && v.altitude >= -10 && !v.HoldPhysics)
                {
                    List<MissileFire> wmParts = new List<MissileFire>(200);
                    foreach (Part p in v.Parts)
                    {
                        wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
                    }
                    foreach (MissileFire wmPart in wmParts)
                    {
                        if (wmPart.team != myTeam)
                        {
                            boat += 1;
                            ScreenMsg4(v.vesselName + " Detected");
                            yield return new WaitForSeconds(1.5f);
                        }
                    }
                }
                /*
                if (v.Splashed && v.altitude <= -25 && !v.HoldPhysics)
                {
                    List<MissileFire> wmParts = new List<MissileFire>(200);
                    foreach (Part p in v.Parts)
                    {
                        wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
                    }
                    foreach (MissileFire wmPart in wmParts)
                    {
                        if (wmPart.team != myTeam)
                        {
                            sub += 1;
                            ScreenMsg4(v.vesselName + " Detected");
                            yield return new WaitForSeconds(1.5f);
                        }
                    }
                }
                */
                if (v.Landed && !v.HoldPhysics)
                {
                    List<MissileFire> wmParts = new List<MissileFire>(200);
                    foreach (Part p in v.Parts)
                    {
                        wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
                    }
                    foreach (MissileFire wmPart in wmParts)
                    {
                        if (wmPart.team != myTeam)
                        {
                            ground += 1;
                            ScreenMsg4(v.vesselName + " Detected");
                            yield return new WaitForSeconds(1.5f);
                        }
                    }
                }
            }
            vesselIDcheck = false;
            ScreenMsg3(ground + " Air Contacts Found");
            yield return new WaitForSeconds(2);
            ScreenMsg3(ground + " Ground Contacts Found");
            yield return new WaitForSeconds(2);
            ScreenMsg3(boat + " Surface Contacts Found");
//            yield return new WaitForSeconds(2);
//            ScreenMsg3(ground + " Submarine Contacts Found");
        }

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 1.25f, ScreenMessageStyle.UPPER_RIGHT));
        }

        private void ScreenMsg2(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 4, ScreenMessageStyle.UPPER_RIGHT));
        }

        private void ScreenMsg3(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 8, ScreenMessageStyle.UPPER_LEFT));
        }

        private void ScreenMsg4(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 3, ScreenMessageStyle.UPPER_CENTER));
        }

        IEnumerator VesselRoutine()
        {
            vesselID = true;
            yield return new WaitForSeconds(30);
            vesselID = false;
        }
    }
}