using System.Collections.Generic;
using Verse;

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

        public bool canUsePsychicPylon;

        public bool canAdjustConsumptionRate;

        public int useTickPeriod = 60; // = 1sec

        public int resetTickPeriod = 120; // = 2sec

        public bool consumeOnlyWhenUsed;
    }
}