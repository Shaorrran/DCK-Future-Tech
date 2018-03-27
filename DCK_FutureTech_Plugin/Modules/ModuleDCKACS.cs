/*Copywrite © 2018, DoctorDavinci
 * 
 * 
 * All code used from the Cloaking Device mod has been absorbed into this code via
 * one-way compatibility from CC BY-SA 4.0 to GPLv3 and is released as such
 * <https://creativecommons.org/2015/10/08/cc-by-sa-4-0-now-one-way-compatible-with-gplv3/>
 * 

 Attribution and previous license.....
--------------------------------------------------------------------------------------------------
 * Copyright © 2016, wasml
 Licensed under the Attribution-ShareAlike 4.0 (CC BY-SA 4.0)
 creative commons license. See <https://creativecommons.org/licenses/by-nc-sa/4.0/>
 for full details.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
--------------------------------------------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.Linq;
using BDArmory;
using BDArmory.Radar;
using UnityEngine;
using System.Collections;

namespace DCK_FutureTech
{
    public class ModuleDCKACS : PartModule
    {
        private static string modName = "[ModuleDCKACS]";

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Auto Deploy"),
         UI_Toggle(controlEnabled = true, scene = UI_Scene.All, disabledText = "Off", enabledText = "On")]
        public bool autoDeploy = true;

        private float maxfade = 0.1f; // invisible:0 to uncloaked:1
        private float surfaceAreaToCloak = 0.0f;
        private float RequiredEC = 0.0f;

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false)]
        public bool cloakOn = false;

        public ModuleDCKStealth stealthCheck;

        private static float UNCLOAKED = 1.0f;
        private static float RENDER_THRESHOLD = 0.0f;
        private bool targeted = false;
        private float fadePerTime = 0.5f;
        private bool currentShadowState = true;
        private bool pauseRoutine;
        private bool coolDown = false;
        private bool resourceAvailable;
        private bool fullRenderHide = true;
        private bool recalcCloak = true;
        private float visiblilityLevel = UNCLOAKED;
        private float areaExponet = 0.5f;
        private float ECPerSec = 1.0f; // Electric charge per second
        private float fadeTime = 1.0f; // In seconds
        private float shadowCutoff = 0.0f;
        private bool selfCloak = true;

        //---------------------------------------------------------------------

        [KSPAction("Cloak Toggle")]
        public void actionToggleCloak(KSPActionParam param)
        {
            cloakOn = !cloakOn;
            UpdateCloakField(null, null);
        }

        [KSPAction("Cloak On")]
        public void actionCloakOn(KSPActionParam param)
        {
            cloakOn = true;
            UpdateCloakField(null, null);
        }

        [KSPAction("Cloak Off")]
        public void actionCloakOff(KSPActionParam param)
        {
            cloakOn = false;
            UpdateCloakField(null, null);
        }

        //---------------------------------------------------------------------

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 3, ScreenMessageStyle.UPPER_CENTER));
        }

        private void ScreenMsg2(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 3, ScreenMessageStyle.LOWER_CENTER));
        }

        private ModuleDCKStealth CheckRCS()
        {
            ModuleDCKStealth checkRCS = null;

            try
            {
                checkRCS = part.FindModulesImplementing<ModuleDCKStealth>().SingleOrDefault();
            }
            catch (System.Exception x)
            {
                Debug.Log(string.Format("ERROR:", modName, x.Message));
            }
            return checkRCS;
        }


        public override string GetInfo()
        {
            // Editor "More Info" display
            string st;
            st = "Fade/sec: " + fadePerTime.ToString("F1") + "\n" +
                 "EC = Area * Fade% * " + ECPerSec.ToString("F1") + " ^ " + areaExponet.ToString("F1");
            return st;
        }

        //---------------------------------------------------------------------

        public override void OnStart(StartState state)
        {
            GameEvents.onVesselWasModified.Add(ReconfigureEvent);
            recalcSurfaceArea();
        }

        public override void OnUpdate()
        {
            checkresourceAvailable();

            if (autoDeploy)
            {
                BDAcCheck();
            }

            if (cloakOn)
            {
                BDAcJammerRCS0();
                drawEC();
                radarOff();
            }
            else
            {
                BDAcJammerRCS1();
            }

            if (IsTransitioning())
            {
                recalcCloak = false;
                calcNewCloakLevel();

                foreach (Part p in vessel.parts)
                if (selfCloak || (p != part))
                {
                    p.SetOpacity(visiblilityLevel);
                    SetRenderAndShadowStates(p, visiblilityLevel > shadowCutoff, visiblilityLevel > RENDER_THRESHOLD);
                }
            }            
            base.OnUpdate();
        }

        public override void OnFixedUpdate()
        {
            if (autoDeploy)
            {
                UnderFirecheck();
            }
            base.OnFixedUpdate();
        }

        public void OnDestroy()
        {
            GameEvents.onVesselWasModified.Remove(ReconfigureEvent);
        }

        #region Resources

        /// <summary>
        /// Resources
        /// </summary>
        /// 
        protected void drawEC()
        {
            RequiredEC = Time.deltaTime * (1 - visiblilityLevel) * (float)Math.Pow(surfaceAreaToCloak * ECPerSec, areaExponet);

            float AcquiredEC = part.RequestResource("ElectricCharge", RequiredEC);
            if (AcquiredEC < RequiredEC * 0.8f)
            {
                if (vessel.isActiveVessel)
                {
                    ScreenMsg("Not Enough Electrical Charge");
                }
                BDAcJammerDisable();
                disengageCloak();
            }

            foreach (var p in vessel.parts)
            {
                double totalAmount = 0;
                double maxAmount = 0;
                PartResource r = p.Resources.Where(n => n.resourceName == "ElectricCharge").FirstOrDefault();
                if (r != null)
                {
                    totalAmount += r.amount;
                    maxAmount += r.maxAmount;
                    if (totalAmount < maxAmount * 0.02)
                    {
                        if (vessel.isActiveVessel)
                        {
                            ScreenMsg("Not Enough Electrical Charge");
                        }
                        BDAcJammerDisable();
                        disengageCloak();
                        StartCoroutine(CoolDownRoutine());
                    }
                }
            }
        }

        private void checkresourceAvailable()
        {
            foreach (var p in vessel.parts)
            {
                double totalAmount = 0;
                double maxAmount = 0;
                PartResource r = p.Resources.Where(n => n.resourceName == "ElectricCharge").FirstOrDefault();
                if (r != null)
                {
                    totalAmount += r.amount;
                    maxAmount += r.maxAmount;
                    if (totalAmount < maxAmount * 0.2)
                    {
                        resourceAvailable = false;
                    }
                    else
                    {
                        resourceAvailable = true;
                    }
                }
            }
        }
#endregion

        #region BDAc Integration

        /// <summary>
        /// BDAc Integration
        /// </summary>
        /// 

        IEnumerator PauseRoutine()
        {
            pauseRoutine = true;
            yield return new WaitForSeconds(10);
            BDAcJammerDisable();
            pauseRoutine = false;
            StartCoroutine(CoolDownRoutine());
        }

        IEnumerator CoolDownRoutine()
        {
            coolDown = true;
            yield return new WaitForSeconds(10);
            coolDown = false;
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

        private void BDAcCheck()
        {
            UnderFirecheck();

            stealthCheck = CheckRCS();

            if (targeted && !coolDown && !pauseRoutine && !stealthCheck.jammerEnabled)
            {
                BDAcJammerEnable();
                StartCoroutine(PauseRoutine());
            }

            if (stealthCheck.jammerEnabled && !cloakOn)
            {
                engageCloak();
            }

            if (!stealthCheck.jammerEnabled && cloakOn)
            {
                disengageCloak();
            }

            if (coolDown)
            {
                BDAcJammerDisable();
            }
        }

        private void BDAcJammerEnable()
        {
            stealthCheck = CheckRCS();

            if (!stealthCheck.jammerEnabled)
            {
                stealthCheck.EnableJammer();
            }
        }

        private void BDAcJammerDisable()
        {
            stealthCheck = CheckRCS();

            if (stealthCheck.jammerEnabled)
            {
                stealthCheck.DisableJammer();
            }
        }

        private void BDAcJammerRCS0()
        {
            stealthCheck = CheckRCS();

            stealthCheck.lockBreaker = true;
            stealthCheck.rcsReductionFactor = 0;
            stealthCheck.jammerStrength = 2000;
            stealthCheck.lockBreakerStrength = 2000;
        }

        private void BDAcJammerRCS1()
        {
            stealthCheck = CheckRCS();

            stealthCheck.lockBreaker = false;
            stealthCheck.rcsReductionFactor = 1;
            stealthCheck.jammerStrength = 0;
            stealthCheck.lockBreakerStrength = 0;
        }

        private void radarOn()
        {
            List<ModuleRadar> radarParts = new List<ModuleRadar>(200);
            foreach (Part p in vessel.Parts)
            {
                radarParts.AddRange(p.FindModulesImplementing<ModuleRadar>());
            }
            foreach (ModuleRadar radarPart in radarParts)
            {
                if (!radarPart.radarEnabled)
                {
                    radarPart.EnableRadar();
                }
            }
        }

        private void radarOff()
        {
            List<ModuleRadar> radarParts = new List<ModuleRadar>(200);
            foreach (Part p in vessel.Parts)
            {
                radarParts.AddRange(p.FindModulesImplementing<ModuleRadar>());
            }
            foreach (ModuleRadar radarPart in radarParts)
            {
                if (radarPart.radarEnabled)
                {
                    radarPart.DisableRadar();
                }
            }
        }
        #endregion

        #region Cloak
        /// <summary>
        /// Cloak code
        /// </summary>
        /// 
        private void engageCloak()
        {
            if (resourceAvailable)
            {
                if (vessel.isActiveVessel)
                {
                    ScreenMsg("Active Camouflage engaging");
                }
                cloakOn = true;
                UpdateCloakField(null, null);
            }
            else
            {
                if (vessel.isActiveVessel)
                {
                    ScreenMsg("EC too low ... Active Camouflage unable to engage");
                }
                BDAcJammerDisable();
            }
        }

        private void disengageCloak()
        {
            if (cloakOn)
            {
                if (vessel.isActiveVessel)
                {
                    ScreenMsg("Active Camouflage Disengaging");
                }
                cloakOn = false;
                UpdateCloakField(null, null);
            }
        }

        protected void UpdateSelfCloakField(BaseField field, object oldValueObj)
        {
            if (selfCloak)
            {
                SetRenderAndShadowStates(part, visiblilityLevel > shadowCutoff, visiblilityLevel > RENDER_THRESHOLD);
            }
            else
            {
                SetRenderAndShadowStates(part, true, true);
            }
            recalcCloak = true;
        }

        protected void UpdateCloakField(BaseField field, object oldValueObj)
        {
            // Update in case its been changed
            calcFadeTime();
            recalcSurfaceArea();
            recalcCloak = true;
        }

        private void calcFadeTime()
        {
            // In case fadeTime == 0
            try
            { fadePerTime = (1 - maxfade) / fadeTime; }
            catch (Exception)
            { fadePerTime = 10.0f; }
        }

        private void recalcSurfaceArea()
        {
            Part p;

            if (vessel != null)
            {
                surfaceAreaToCloak = 0.0f;
                for (int i = 0; i < vessel.parts.Count; i++)
                {
                    p = vessel.parts[i];
                    if (p != null)
                        if (selfCloak || (p != part))
                            surfaceAreaToCloak = (float)(surfaceAreaToCloak + p.skinExposedArea);
                }
            }
        }

        private void SetRenderAndShadowStates(Part p, bool shadowsState, bool renderState)
        {
            if (p.gameObject != null)
            {
                int i;

                MeshRenderer[] MRs = p.GetComponentsInChildren<MeshRenderer>();
                for (i = 0; i < MRs.GetLength(0); i++)
                    MRs[i].enabled = renderState;// || !fullRenderHide;

                SkinnedMeshRenderer[] SMRs = p.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (i = 0; i < SMRs.GetLength(0); i++)
                    SMRs[i].enabled = renderState;// || !fullRenderHide;

                if (shadowsState != currentShadowState)
                {
                    for (i = 0; i < MRs.GetLength(0); i++)
                    {
                        if (shadowsState)
                            MRs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        else
                            MRs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    for (i = 0; i < SMRs.GetLength(0); i++)
                    {
                        if (shadowsState)
                            SMRs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        else
                            SMRs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    currentShadowState = shadowsState;
                }
            }
        }

        private void ReconfigureEvent(Vessel v)
        {
            if (v == null) { return; }

            if (v == vessel)
            {   // This is the cloaking vessel - recalc EC required based on new configuration (unless this is a dock event)
                recalcCloak = true;
                recalcSurfaceArea();
            }
            else
            {   // This is the added/removed part - reset it to normal
                ModuleDCKACS mc = null;
                foreach (Part p in v.parts)
                    if ((p != null) &&
                        ((p != part) || selfCloak))
                    {
                        //p.setOpacity(UNCLOAKED); // 1.1.3
                        p.SetOpacity(UNCLOAKED); // 1.2.2 and up
                        SetRenderAndShadowStates(p, true, true);
                        Debug.Log(modName + "Uncloak " + p.name);

                        // If the other vessel has a cloak device let it know it needs to do a refresh
                        mc = p.FindModuleImplementing<ModuleDCKACS>();
                        if (mc != null)
                            mc.recalcCloak = true;
                    }
            }
        }

        protected void calcNewCloakLevel()
        {
            calcFadeTime();
            float delta = Time.deltaTime * fadePerTime;
            if (cloakOn && (visiblilityLevel > maxfade))
                delta = -delta;

            visiblilityLevel = visiblilityLevel + delta;
            visiblilityLevel = Mathf.Clamp(visiblilityLevel, maxfade, UNCLOAKED);
        }

        protected bool IsTransitioning()
        {
            return (cloakOn && (visiblilityLevel > maxfade)) ||     // Cloaking in progress
                   (!cloakOn && (visiblilityLevel < UNCLOAKED)) ||  // Uncloaking in progress
                   recalcCloak;                                     // A forced refresh 
        }
#endregion
    }
}
