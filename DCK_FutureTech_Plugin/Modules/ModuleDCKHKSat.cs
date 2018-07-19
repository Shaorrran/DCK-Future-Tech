using System.Collections;
using UnityEngine;
using System;
using BDArmory.Modules;

namespace DCK_FutureTech
{
    public class ModuleDCKHKSat : ModuleCommand
    {
        private bool detecting = false;
        private bool start = false;
        private bool chasing = false;
        private double speed = 0;

        private float proximity = 15;
        private int count = 0;
        private bool detonating = false;
        public Guid id;

        public BDExplosivePart mine;
        private BDExplosivePart GetMine()
        {
            BDExplosivePart m = null;

            m = part.FindModuleImplementing<BDExplosivePart>();

            return m;
        }

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                part.force_activate();
                mine = GetMine();
            }
            base.OnStart(state);
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (this.vessel.parts.Count == 1)
                {
                    if (!chasing)
                    {
                        chasing = true;
                        StartCoroutine(Chase());
                    }
                }
            }
        }


        IEnumerator DetonateMineRoutine()
        {
            detonating = true;
            UnityEngine.Debug.Log("[OrX Bonus Ball] Waldo Attack " + this.vessel.vesselName + " Detonating ... ");

            mine = GetMine();
            mine.ArmAG(new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate));
            yield return new WaitForFixedUpdate();
            mine.DetonateAG(new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate));
            part.explode();
        }

        IEnumerator Chase()
        {
            var count = 0;

            foreach (Vessel v in FlightGlobals.Vessels)
            {
                double targetDistance = Vector3d.Distance(this.vessel.GetWorldPos3D(), v.GetWorldPos3D());

                if (targetDistance <= 10000 && count == 0)
                {
                    count += 1;

                    if (targetDistance <= 100)
                    {
                        StartCoroutine(DetonateMineRoutine());
                    }
                    else
                    {
                        speed = v.srfSpeed * 1.5f;
                        mine.tntMass = 100;
                        var heading = (v.GetWorldPos3D() - this.part.vessel.GetWorldPos3D()).normalized;
                        this.part.GetComponent<Rigidbody>().velocity = heading * speed;
                    }
                }
            }

            yield return new WaitForSeconds(2);

            chasing = false;
        }
    }
}