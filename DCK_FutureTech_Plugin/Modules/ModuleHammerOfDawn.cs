using BDArmory.Parts;
using System.Collections.Generic;
using BDArmory.UI;
using BDArmory;
using BDArmory.Misc;
using UnityEngine;
using System.Collections;
using System;

namespace DCK_FutureTech
{
    public class ModuleHammerOfDawn : PartModule
    {

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "GPS - Scan For Targets"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.Flight, disabledText = "", enabledText = "SCANNING")]
        public bool scan = false;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Lock on Target"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.Flight, disabledText = "Off", enabledText = "On")]
        public bool lockTarget = false;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "HAMMER OF DAWN"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.Flight, disabledText = "FIRE", enabledText = "FIRING")]
        public bool fireLaser = false;

        [KSPField(isPersistant = true)]
        public bool myTeam = false;

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
                    StartCoroutine(TargetGPS());
                }

                if (fireLaser && targetLocked)
                {
                    StartCoroutine(FireLaser());
                }

                if (scan && !pauseRoutine && !scanning && !firing)
                {
                    GetSatInfo();

                    if (vessel.atmDensity < 0.000005f)
                    {
                        StartCoroutine(GPSRoutine());
                    }

                    if (vessel.atmDensity > 0.000005f)
                    {
                        var atm = vessel.atmDensity - 0.000005;
                        ScreenMsg2("Current Atmospheric Density too high");
                        ScreenMsg2("Reduce Density by : " + atm + " atm");
                        StartCoroutine(PauseRoutine());
                        scan = false;
                    }
                }
            }
        }

        IEnumerator TargetGPS()
        {
            scanning = true;
            if (getTargetCoords != Vector3.zero)
            {
                camera.StartCoroutine(camera.PointToPositionRoutine(VectorUtils.GetWorldSurfacePostion(getTargetCoords, vessel.mainBody)));
                yield return new WaitForSeconds(2);
                targetLocked = true;
                ScreenMsg2("Hammer of Dawn Locked on Target");
            }
            else
            {
                ScreenMsg2("No GPS Targets to Lock .....");
                lockTarget = false;
                targetLocked = false;
            }
            scanning = false;
        }

        IEnumerator FireLaser()
        {
            ScreenMsg2("BRINGING DOWN THE HAMMER");
            firing = true;
            laser.EnableWeapon();
            laser.AGFireToggle(new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate));
            yield return new WaitForSeconds(6);
            laser.AGFireToggle(new KSPActionParam(KSPActionGroup.None, KSPActionType.Deactivate));
            fireLaser = false;
            laser.DisableWeapon();
            wm.guardMode = false;
            scanning = false;
            firing = false;
            targetLocked = false;
        }
        private void GetSatInfo()
        {
            var _satLat = vessel.latitude;
            var _satLong = vessel.longitude;
            SatAlt = vessel.altitude;

            if (_satLat <= 0)
            {
                SatLat = _satLat + 360;
            }
            else
            {
                SatLat = _satLat;
            }

            if (SatLong <= 0)
            {
                SatLong = _satLong + 360;
            }
            else
            {
                SatLong = _satLong;
            }

            radius = vessel.mainBody.Radius;
            circumference = 3.14 * radius * radius;
            distPerDeg = circumference / 360;
            myTeam = wm.team;
            ScreenMsg3("GPS Sat Altitude : " + SatAlt);
        }

        private void Setup()
        {
            myTeam = wm.team;
            wm.guardMode = false;
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

        IEnumerator GPSRoutine()
        {
            scanning = true;
            targetCount = 0;

            ScreenMsg("Initializing Scan ......");
            yield return new WaitForSeconds(1.5f);
            ScreenMsg("Initializing Scan ......");
            yield return new WaitForSeconds(1.5f);
            ScreenMsg("Scanning in Progress ......");
            yield return new WaitForSeconds(1.5f);
            ScreenMsg("Scanning in Progress ......");
            yield return new WaitForSeconds(1.5f);
            ScreenMsg("Scanning in Progress ......");
            yield return new WaitForSeconds(1.5f);

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v.LandedOrSplashed && !v.HoldPhysics)
                {
                    List<MissileFire> targets = new List<MissileFire>(200);
                    foreach (Part t in v.Parts)
                    {
                        targets.AddRange(t.FindModulesImplementing<MissileFire>());
                    }
                    foreach (MissileFire target in targets)
                    {
                        if (myTeam != target.team)
                        {
                            _altitude = v.altitude;
                            _latitude = v.latitude;
                            _longitude = v.longitude;

                            if (this.vessel.isActiveVessel && targetCount == 0)
                            {
                                StartCoroutine(camera.PointToPositionRoutine(VectorUtils.GetWorldSurfacePostion(getTargetCoords, vessel.mainBody)));
                                ScreenMsg2("Locking onto " + v.vesselName);
                            }

                            targetCount += 1;
                            ScreenMsg2("Retrieving GPS Coords for " + v.vesselName);
                            yield return new WaitForSeconds(1.5f);
                            BDATargetManager.GPSTargets[BDATargetManager.BoolToTeam(myTeam)].Add(new GPSTargetInfo(getTargetCoords, v.vesselName));
                            ScreenMsg2(v.vesselName + " added to GPS Database");
                            yield return new WaitForSeconds(1.5f);

                        }
                    }
                }
            }
            yield return new WaitForSeconds(1.5f);
            ScreenMsg2("Scan Complete ... " + targetCount + " Targets added to GPS Database");
            scan = false;
            scanning = false;
        }

        IEnumerator PauseRoutine()
        {
            pauseRoutine = true;
            scan = false;
            yield return new WaitForSeconds(2);
            pauseRoutine = false;
            scanning = false;
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