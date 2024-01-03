using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class CompProperties_PsychicFuel : CompProperties
    {
        public CompProperties_PsychicFuel()
        {
            compClass = typeof(CompPsychicFuel);
        }

        public bool consumeFuelOnlyWhenUsed;

        public float fuelConsumptionRate = 1f;

        public float fuelConsumptionPerTickInRain;

        private float fuelMultiplier = 1f;

        public bool factorByDifficulty;

        public float FuelMultiplierCurrentDifficulty
        {
            get
            {
                if (factorByDifficulty && Find.Storyteller?.difficulty != null)
                {
                    return fuelMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
                }
                return fuelMultiplier;
            }
        }

        [MustTranslate]
	    public string fuelLabel;

        public string FuelLabel
        {
            get
            {
                if (fuelLabel.NullOrEmpty())
                {
                    return "Fuel".TranslateSimple();
                }
                return fuelLabel;
            }
        }

        [MustTranslate]
	    public string outOfFuelMessage;

        

        public bool externalTicking;

        

        

        

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if ((fuelConsumptionPerTickInRain > 0f) && parentDef.tickerType != TickerType.Normal)
            {
                yield return $"Refuelable component set to consume fuel per tick, but parent tickertype is {parentDef.tickerType} instead of {TickerType.Normal}";
            }
            if(parentDef.GetCompProperties<CompProperties_PsychicStorage>()==null)
            {
                yield return $"PsychicFuel comp without PsychicStorage comp";
            }
        }
    }
}