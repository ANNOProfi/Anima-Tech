using Verse;
using System.Collections.Generic;
using RimWorld;
using System.Text;

namespace AnimaTech
{
    public class CompProperties_PsychicGenerator : CompProperties
    {
        public float baseGenerationRate = 0;

        public bool canTransmitToNetwork;

        public bool canToggleTransmission;

        public CompProperties_PsychicGenerator()
        {
            compClass = typeof(CompPsychicGenerator);
        }

        private float focusMultiplier = 1f;

        public bool factorByDifficulty;

        public float FocusMultiplierCurrentDifficulty
        {
            get
            {
                if (factorByDifficulty && Find.Storyteller?.difficulty != null)
                {
                    return focusMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
                }
                return focusMultiplier;
            }
        }

        public float techFactor = 1f;

        public bool allowImbuement = false;

        public bool canToggleImbuement;

        public float neuralHeatFactor = 0.5f;

        public bool isMeditatable = false;

        public bool canToggleMeditation;

        public bool isDayTimeGenerator = false;

        public bool isNightTimeGenerator = false;

        public bool powerTrader = false;

        public float maximumGenerationRate;

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }
            if(baseGenerationRate > 0)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "AT_PsychicGeneratorStat".Translate(), baseGenerationRate.ToString("F1"), "AT_PsychicGeneratorGenerationStatDesc".Translate(), 5000);
            }
        }
    }
}