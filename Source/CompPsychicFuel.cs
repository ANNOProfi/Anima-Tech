using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [StaticConstructorOnStartup]
    public class CompPsychicFuel : ThingComp
    {
        public CompProperties_PsychicFuel Props => (CompProperties_PsychicFuel)props;

        private CompPsychicStorage storageComp;

        private CompFlickable flickComp;

        public const string RefueledSignal = "Refueled";

	    public const string RanOutOfFuelSignal = "RanOutOfFuel";

        private float ConsumptionRatePerTick => Props.focusConsumptionRate / 60000f;
        
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            storageComp = parent.GetComp<CompPsychicStorage>();

            flickComp = parent.GetComp<CompFlickable>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!Props.consumeFocusOnlyWhenUsed && (flickComp == null || flickComp.SwitchIsOn) && !Props.externalTicking)
            {
                storageComp.TryAddFocus(-ConsumptionRatePerTick);
            }
            if (Props.focusConsumptionPerTickInRain > 0f && parent.Spawned && parent.Map.weatherManager.RainRate > 0.4f && !parent.Map.roofGrid.Roofed(parent.Position) && !Props.externalTicking)
            {
                storageComp.TryAddFocus(-Props.focusConsumptionPerTickInRain);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = "";

            if (!Props.consumeFocusOnlyWhenUsed && storageComp.HasFocus)
            {
                int numTicks = (int)(storageComp.focus / Props.focusConsumptionRate * 60000f);
                text = "(" + numTicks.ToStringTicksToPeriod() + ")";
            }

            return text;
        }

        public void Notify_UsedThisTick()
        {
            storageComp.TryAddFocus(-ConsumptionRatePerTick);
        }
    }
}