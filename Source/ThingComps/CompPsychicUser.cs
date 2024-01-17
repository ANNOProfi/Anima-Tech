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

        protected int ticksUntilNextCheck;

        protected bool isConsumingPower;

        protected bool isConsumingNetworkPower;

        protected float consumptionRateFactor = 1f;

        protected bool usedThisTick;

        public virtual float FocusConsumptionPerDay => Props.baseFocusConsumption * consumptionRateFactor;

        public virtual float FocusConsumptionPerCheck => (float)Props.useTickPeriod * FocusConsumptionPerDay / 60000f;

        public virtual bool IsPoweredOn => isConsumingPower;

        public virtual bool IsUsingNetWorkPower => isConsumingNetworkPower;

        public virtual float FocusConsumptionRate => consumptionRateFactor;

        public virtual bool UsedThisTick => usedThisTick;

        public virtual void SetConsumptionRate(float level)
        {
            consumptionRateFactor = Mathf.Clamp(level, Props.minimumConsumptionRate, Props.maximumConsumptionRate);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pylonComp = parent.GetComp<CompPsychicPylon>();
            storageComp = parent.GetComp<CompPsychicStorage>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilNextCheck, "ticksUntilNextCheck", 0);
            Scribe_Values.Look(ref isConsumingPower, "isConsumingPower", defaultValue: false);
            Scribe_Values.Look(ref isConsumingNetworkPower, "isConsumingNetworkPower", defaultValue: false);
            Scribe_Values.Look(ref consumptionRateFactor, "consumptionRateFactor", 1f);
        }

        public override void CompTick()
        {
            if(Props.consumeOnlyWhenUsed && !usedThisTick)
            {
                return;
            }
            if (ticksUntilNextCheck > 0)
            {
                ticksUntilNextCheck--;
                return;
            }
            float num = FocusConsumptionPerCheck;
            if (Props.canUsePsychicPylon && pylonComp != null && pylonComp.isToggledOn)
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
                if (isConsumingPower)
                {
                    isConsumingPower = false;
                    //parent.BroadcastCompSignal("ARR.AethericDeviceDeactivated");
                }
                ticksUntilNextCheck = Props.resetTickPeriod;
                usedThisTick = false;
            }
            else
            {
                if (!isConsumingPower)
                {
                    isConsumingPower = true;
                    //parent.BroadcastCompSignal("ARR.AethericDeviceActivated");
                }
                ticksUntilNextCheck = Props.useTickPeriod;
                usedThisTick = false;
            }
        }

        public void Notify_UsedThisTick()
        {
            usedThisTick = true;
        }

        public override string CompInspectStringExtra()
        {
            if (IsPoweredOn)
            {
                return "ARR_AetherUserRatePerDay".Translate(FocusConsumptionPerDay.ToString("F1"));
            }
            return "ARR_AetherUserRatePerDayInactive".Translate(FocusConsumptionPerDay.ToString("F1"));
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
