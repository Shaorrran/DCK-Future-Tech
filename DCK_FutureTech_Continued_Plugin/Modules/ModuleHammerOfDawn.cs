using BDArmory.Competition;
using BDArmory.Control;
using BDArmory.Targeting;
using BDArmory.Utils;
using BDArmory.Weapons;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace DCK_FutureTech
{
    public class ModuleHammerOfDawn : PartModule
    {

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Lock on Target"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.Flight, disabledText = "Off", enabledText = "On")]
        public bool lockTarget = false;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "BRING DOWN THE HAMMER"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.Flight, disabledText = "FIRE", enabledText = "HAMMERING")]
        public bool fireLaser = false;

        [KSPField(isPersistant = true)]
        public BDTeam myTeam;

        private bool targetLocked = false;
        private bool pauseRoutine = false;
        private bool scanning = false;

        private double altitude;
        private double longitude;
        private double latitude;

        public double _altitude = 0.0f;
        public double _longitude = 0.0f;
        public double _latitude = 0.0f;

        private bool firing = false;
        private double SatLat = 0.0f;
        private double SatLong = 0.0f;
        private double SatAlt = 0.0f;
        private double LatDiff = 0.0f;
        private double LongDiff = 0.0f;
        private double radius = 0.0f;
        private double circumference = 0.0f;
        private double distPerDeg = 0.0f;
        private double targetDistance = 0.0f;

        private double targetCount = 0;

        private ModuleTargetingCamera camera;
        private ModuleTargetingCamera GetCamera()
        {
            ModuleTargetingCamera c = null;

            c = part.FindModuleImplementing<ModuleTargetingCamera>();

            return c;
        }

        private MissileFire wm;
        private MissileFire GetWM()
        {
            MissileFire w = null;

            w = part.FindModuleImplementing<MissileFire>();

            return w;
        }

        private ModuleWeapon laser;
        private ModuleWeapon GetLaser()
        {
            ModuleWeapon l = null;

            l = part.FindModuleImplementing<ModuleWeapon>();

            return l;
        }


        public override void OnStart(StartState state)
        {
            laser = GetLaser();
            wm = GetWM();
            camera = GetCamera();
            Setup();
            base.OnStart(state);
        }


        private void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (lockTarget && !targetLocked && !scanning)
                {
                    LockTarget();
                }

                if (fireLaser && targetLocked)
                {
                    Fire();
                }
            }
        }

        public void Fire()
        {
            StartCoroutine(FireLaser());
        }

        public void LockTarget()
        {
            StartCoroutine(TargetGPS());
        }

        IEnumerator TargetGPS()
        {
            scanning = true;
            myTeam = wm.Team;
            camera.EnableCamera();

            List<MissileFire> wmParts = new List<MissileFire>(200);
            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                wmParts.AddRange(p.FindModulesImplementing<MissileFire>());
            }
            foreach (MissileFire wmPart in wmParts)
            {
                var getTarget = wmPart.designatedGPSCoords;
                var _latitude_ = getTarget.x;
                var _longitude_ = getTarget.y;
                var _altitude_ = getTarget.z;
                _latitude = _latitude_;
                _longitude = _longitude_;
                _altitude = _altitude_;
            }

            if (getTargetCoords != Vector3.zero)
            {
                targetLocked = true;
                camera.StartCoroutine(camera.PointToPositionRoutine(VectorUtils.GetWorldSurfacePostion(getTargetCoords, vessel.mainBody)));
                yield return new WaitForSeconds(1);
                ScreenMsg2("Hammer of Dawn Locked on Target");
                camera.currentFovIndex = 3;
            }
            else
            {
                ScreenMsg2("No GPS Targets to Lock");
                targetLocked = false;
                yield return new WaitForSeconds(1);
                lockTarget = false;
            }
            scanning = false;
        }

        IEnumerator FireLaser()
        {
            if (targetLocked)
            {
                ScreenMsg2("BRINGING DOWN THE HAMMER");
                yield return new WaitForSeconds(0.7f);
                firing = true;
                laser.EnableWeapon();
                laser.AGFireToggle(new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate));
                yield return new WaitForSeconds(5);
                laser.AGFireToggle(new KSPActionParam(KSPActionGroup.None, KSPActionType.Deactivate));
                fireLaser = false;
                laser.DisableWeapon();
                firing = false;
                lockTarget = false;
                targetLocked = false;
            }
            else
            {
                ScreenMsg2("No GPS Targets Locked");
            }
        }

        private void Setup()
        {
            myTeam = wm.Team;
            wm.guardRange = 200000;
            wm.gunRange = 200000;
            camera.maxRayDistance = 200000;
            laser.maxTargetingRange = 200000;
            laser.maxEffectiveDistance = 200000;
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

        private Vector3 getTargetCoords
        {
            get
            {
                return new Vector3d(_latitude, _longitude, _altitude);
            }

            set
            {
                string _latString = Convert.ToString(_latitude);
                string _longString = Convert.ToString(_longitude);
                string _altString = Convert.ToString(_altitude);

                var _latFloat = float.Parse(_latString);
                var _longFloat = float.Parse(_longString);
                var _altFloat = float.Parse(_altString);

                value.x = _latFloat;
                value.y = _longFloat;
                value.z = _altFloat;

                latitude = value.x;
                longitude = value.y;
                altitude = value.z;
            }
        }
    }
}