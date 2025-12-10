using System.Collections.Generic;
using Verse;
using RimWorld;

namespace AnimaTech
{
    public class CompProperties_PsychicUser : CompProperties
    {
        public CompProperties_PsychicUser()
        {
            compClass = typeof(CompPsychicUser);
        }

        public float baseFocusConsumption;

        public float minimumConsumptionRate = 1f;

        public float maximumConsumptionRate = 1f;

        public bool canUsePsychicPylon = true;

        public bool canAdjustConsumptionRate;

        //public int useTickPeriod = 60; // = 1sec

        //public int resetTickPeriod = 120; // = 2sec

        public bool consumeOnlyWhenUsed = false;

        public bool powerTrader = false;

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }
            if(baseFocusConsumption > 0)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "AT_PsychicUserStat".Translate(), baseFocusConsumption.ToString("F1"), "AT_PsychicUserConsumptionStatDesc".Translate(), 5000);
            }
        }
    }
}