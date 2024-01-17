using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    /*public class CompProperties_PsychicFuel : CompProperties
    {
        public CompProperties_PsychicFuel()
        {
            compClass = typeof(CompPsychicFuel);
        }

        public bool consumeFocusOnlyWhenUsed;

        public float focusConsumptionRate = 1f;

        public float focusConsumptionPerTickInRain;

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

        [MustTranslate]
	    public string focusLabel;

        public string FocusLabel
        {
            get
            {
                if (focusLabel.NullOrEmpty())
                {
                    return "Focus".TranslateSimple();
                }
                return focusLabel;
            }
        }

        [MustTranslate]
	    public string outOfFocusMessage;

        public bool externalTicking;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if ((focusConsumptionPerTickInRain > 0f) && parentDef.tickerType != TickerType.Normal)
            {
                yield return $"Refocusable component set to consume focus per tick, but parent tickertype is {parentDef.tickerType} instead of {TickerType.Normal}";
            }
            if(parentDef.GetCompProperties<CompProperties_PsychicStorage>()==null)
            {
                yield return $"PsychicFuel comp without PsychicStorage comp";
            }
        }
    }*/
}