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
                ConsumeFuel(ConsumptionRatePerTick);
            }
            if (Props.focusConsumptionPerTickInRain > 0f && parent.Spawned && parent.Map.weatherManager.RainRate > 0.4f && !parent.Map.roofGrid.Roofed(parent.Position) && !Props.externalTicking)
            {
                ConsumeFuel(Props.focusConsumptionPerTickInRain);
            }
        }

        public void Notify_UsedThisTick()
        {
            ConsumeFuel(ConsumptionRatePerTick);
        }

        public void ConsumeFuel(float amount)
        {
            if(storageComp.focus <= 0f)
            {
                return;
            }
            storageComp.focus -= amount;

            if (storageComp.focus <= 0f)
            {
                storageComp.focus = 0f;
                parent.BroadcastCompSignal("RanOutOfFuel");
            }
        }
    }
}