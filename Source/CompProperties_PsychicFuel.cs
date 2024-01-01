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

        /*public float fuelConsumptionRate = 1f;

        public float fuelCapacity = 2f;

        public float initialFuelPercent;

        public float autoRefuelPercent = 0.3f;

        public float fuelConsumptionPerTickInRain;

        public bool consumeFuelOnlyWhenUsed;

        public bool showFuelGizmo;

        public bool initialAllowAutoRefuel = true;

        public bool showAllowAutoRefuelToggle;

        public bool allowRefuelIfNotEmpty = true;

        public bool drawOutOfFuelOverlay = true;

        public float minimumFueledThreshold;

        private float fuelMultiplier = 1f;

        public bool factorByDifficulty;

        [MustTranslate]
        public string fuelLabel;

        [MustTranslate]
        public string fuelGizmoLabel;

        [MustTranslate]
        public string outOfFuelMessage;

        public string fuelIconPath;

        public bool externalTicking;

        private Texture2D fuelIcon;

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

        public string FuelGizmoLabel
        {
            get
            {
                if (fuelGizmoLabel.NullOrEmpty())
                {
                    return "Fuel".TranslateSimple();
                }
                return fuelGizmoLabel;
            }
        }

        /*public Texture2D FuelIcon
        {
            get
            {
                if (fuelIcon == null)
                {
                    if (!fuelIconPath.NullOrEmpty())
                    {
                        fuelIcon = ContentFinder<Texture2D>.Get(fuelIconPath);
                    }
                    else
                    {
                        ThingDef thingDef = ((fuelFilter.AnyAllowedDef == null) ? ThingDefOf.Chemfuel : fuelFilter.AnyAllowedDef);
                        fuelIcon = thingDef.uiIcon;
                    }
                }
                return fuelIcon;
            }
        }

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

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if ((!consumeFuelOnlyWhenUsed || fuelConsumptionPerTickInRain > 0f) && parentDef.tickerType != TickerType.Normal)
            {
                yield return $"Refuelable component set to consume fuel per tick, but parent tickertype is {parentDef.tickerType} instead of {TickerType.Normal}";
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }
            if (((ThingDef)req.Def).building.IsTurret)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "ShotsBeforeRearm".Translate(), ((int)fuelCapacity).ToString(), "ShotsBeforeRearmExplanation".Translate(), 3171);
            }
        }*/
    }
}