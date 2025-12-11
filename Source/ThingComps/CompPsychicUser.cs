using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using UnityEngine.Animations;
using Verse;

namespace AnimaTech
{
    public class CompPsychicUser : ThingComp
    {
        public CompProperties_PsychicUser Props => (CompProperties_PsychicUser)props;

        protected CompFlickable flickableComp;

        protected CompPsychicPylon pylonComp;

        protected CompPsychicStorage storageComp;

        protected CompPsychicPowerTrader powerTraderComp;

        protected int ticksUntilNextCheck;

        protected bool isConsumingStoredPower;

        protected bool isConsumingNetworkPower;

        protected float consumptionRateFactor = 1f;

        protected bool usedThisTick;

        protected ModExtension_PsychicRune extension;
        
        protected Vector3 runeDrawSize;

        protected Material runeActiveMaterial;

        private float focusConsumption = 0f;

        public float FocusConsumption
        {
            get
            {
                if(!Props.powerTrader)
                {
                    return Props.baseFocusConsumption * FocusConsumptionRate;
                }

                return focusConsumption * FocusConsumptionRate;
            }

            set
            {
                focusConsumption = value;
            }
        }

        public float FocusConsumptionForReading
        {
            get
            {
                if(IsActive)
                {
                    return FocusConsumption;
                }
                
                return 0f;
            }
        }

        public virtual float FocusConsumptionPerCheck => AT_Utilities.Settings.tickInterval * FocusConsumption / 60000f;

        public virtual bool IsUsingStoredPower => isConsumingStoredPower;

        public virtual bool IsUsingNetworkPower => isConsumingNetworkPower;

        public virtual bool IsActive => (IsUsingStoredPower || IsUsingNetworkPower) && ((Props.consumeOnlyWhenUsed && UsedThisTick) || !Props.consumeOnlyWhenUsed);

        public virtual float FocusConsumptionRate => consumptionRateFactor;

        public virtual bool UsedThisTick => usedThisTick;

        public virtual void SetConsumptionRate(float level)
        {
            consumptionRateFactor = Mathf.Clamp(level, Props.minimumConsumptionRate, Props.maximumConsumptionRate);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
             if (runeActiveMaterial != null && IsActive)
            {
                Vector3 pos = parent.DrawPos + extension.overlayDrawOffset;
                pos += Vector3.up * 0.01f;
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);

                if(parent.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeActiveMaterial, 0);
                    return;
                }
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeActiveMaterial, 0);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pylonComp = parent.GetComp<CompPsychicPylon>();
            storageComp = parent.GetComp<CompPsychicStorage>();
            flickableComp = parent.GetComp<CompFlickable>();
            powerTraderComp = parent.GetComp<CompPsychicPowerTrader>();

            extension = parent.def.GetModExtension<ModExtension_PsychicRune>() ?? new ModExtension_PsychicRune();

            if(extension != null)
            {
                runeActiveMaterial = extension.MaterialRuneNetwork(parent);
                runeDrawSize = extension.overlayDrawSize;
                if (runeDrawSize.x != runeDrawSize.z && (parent.Rotation == Rot4.East || parent.Rotation == Rot4.West))
                {
                    runeDrawSize.x = extension.overlayDrawSize.z;
                    runeDrawSize.z = extension.overlayDrawSize.x;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilNextCheck, "ticksUntilNextCheck", 0);
            Scribe_Values.Look(ref isConsumingStoredPower, "isConsumingStoredPower", defaultValue: false);
            Scribe_Values.Look(ref isConsumingNetworkPower, "isConsumingNetworkPower", defaultValue: false);
            Scribe_Values.Look(ref consumptionRateFactor, "consumptionRateFactor", 1f);
            Scribe_Values.Look(ref usedThisTick, "usedThisTick", defaultValue: false);
        }

        public override void CompTick()
        {
            if((Props.consumeOnlyWhenUsed && !usedThisTick) || (flickableComp != null && !flickableComp.SwitchIsOn) || (FocusConsumption == 0f && !Props.powerTrader))
            {
                return;
            }
            if (ticksUntilNextCheck > 0)
            {
                ticksUntilNextCheck--;
                return;
            }
            float num = FocusConsumptionPerCheck;
            if (Props.canUsePsychicPylon && pylonComp != null && pylonComp.isToggledOn && !pylonComp.Network.IsEmpty)
            {
                num = pylonComp.TryDrawFocus(num);
                if (num <= 0f && !isConsumingNetworkPower)
                {
                    isConsumingNetworkPower = true;
                    //parent.BroadcastCompSignal("ARR.AethericFuelChanged");
                }
                usedThisTick = false;
            }
            if (num > 0f && storageComp != null)
            {
                num = storageComp.DrainFocus(num);
                if (isConsumingNetworkPower)
                {
                    isConsumingNetworkPower = false;
                    //parent.BroadcastCompSignal("ARR.AethericFuelChanged");
                }
                usedThisTick = false;
            }
            if (num > 0f)
            {
                if (isConsumingStoredPower)
                {
                    isConsumingStoredPower = false;
                    parent.BroadcastCompSignal("AT.PsychicDeviceDectivated");
                }
                ticksUntilNextCheck = AT_Utilities.Settings.tickInterval;
                usedThisTick = false;
            }
            if(num <= 0f)
            {
                if (!isConsumingStoredPower)
                {
                    isConsumingStoredPower = true;
                    parent.BroadcastCompSignal("AT.PsychicDeviceActivated");
                }
                ticksUntilNextCheck = AT_Utilities.Settings.tickInterval;
                usedThisTick = false;
            }
        }

        public void Notify_UsedThisTick()
        {
            usedThisTick = true;
        }

        public override string CompInspectStringExtra()
        {
            if (IsActive)
            {
                int numTicks;
                if(storageComp != null && pylonComp == null)
                {
                    numTicks = (int)(storageComp.focusStored/FocusConsumption*60000);
                    return "AT_PsychicUserRatePerDay".Translate(FocusConsumptionForReading.ToString("F1"), numTicks.ToStringTicksToPeriod());
                }
                else if(pylonComp == null)
                {
                    return "AT_PsychicUser error, no storage or pylon Comp";
                }
                return "AT_PsychicUserRatePerDayPylon".Translate(FocusConsumptionForReading.ToString("F1"));
            }
            if(Props.powerTrader)
            {
                return "";
            }
            return "AT_PsychicUserRatePerDayInactive".Translate(FocusConsumptionForReading.ToString("F1"));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            /*if (Props.canAdjustConsumptionRate)
            {
                yield return new Command_FocusUserAdjustRate(this);
            }*/
        }
    }
}
