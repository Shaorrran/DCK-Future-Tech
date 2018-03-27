using BDArmory.Parts;
using System.Collections.Generic;
using BDArmory;
using UnityEngine;
using System.Collections;

namespace DCK_FutureTech
{
    public class ModuleMissileDetect : PartModule
    {

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Missile Warning System"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.Flight, disabledText = "Off", enabledText = "SCANNING")]
        public bool warn = false;


        [KSPField(isPersistant = true)]
        public bool myTeam = false;

        public bool launchDetected = false;
        public bool detecting = false;


        public override void OnStart(StartState state)
        {
            Setup();
            base.OnStart(state);
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (warn && !detecting && !launchDetected)
                {
                    Setup();
                    StartCoroutine(CheckForMissile());
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

        IEnumerator DetectingRoutine()
        {
            detecting = true;
            yield return new WaitForSeconds(10);
            detecting = false;
        }

        IEnumerator CheckForMissile()
        {
            StartCoroutine(DetectingRoutine());
            launchDetected = false;

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (!v.LandedOrSplashed && !launchDetected)
                {
                    List<MissileLauncher> missiles = new List<MissileLauncher>(200);
                    foreach (Part p in v.Parts)
                    {
                        missiles.AddRange(p.FindModulesImplementing<MissileLauncher>());
                    }
                    foreach (MissileLauncher missile in missiles)
                    {
                        var mass = v.totalMass;

                        if (missile.TimeFired >= 0)
                        {
                            launchDetected = true;
                            ScreenMsg("Missile Launch Detected");
                            yield return new WaitForSeconds(1.5f);
                            ScreenMsg("Missile Launch Detected");
                            yield return new WaitForSeconds(1.5f);
                            ScreenMsg("Missile Launch Detected");
                            warn = false;
                        }
                    }
                }
            }
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

    }
}