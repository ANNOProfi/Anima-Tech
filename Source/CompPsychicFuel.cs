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

        private float ConsumptionRatePerTick => Props.fuelConsumptionRate / 60000f;

        public float NeuralHeatFactor = 0.5f;

        
        
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            storageComp = parent.GetComp<CompPsychicStorage>();

            flickComp = parent.GetComp<CompFlickable>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!Props.consumeFuelOnlyWhenUsed && (flickComp == null || flickComp.SwitchIsOn) && !Props.externalTicking)
            {
                ConsumeFuel(ConsumptionRatePerTick);
            }
            if (Props.fuelConsumptionPerTickInRain > 0f && parent.Spawned && parent.Map.weatherManager.RainRate > 0.4f && !parent.Map.roofGrid.Roofed(parent.Position) && !Props.externalTicking)
            {
                ConsumeFuel(Props.fuelConsumptionPerTickInRain);
            }
        }

        /*public void Refuel(float amount, Pawn pawn)
        {
            float adjustedAmount = amount * Props.FuelMultiplierCurrentDifficulty;

            float psyfocus = pawn.psychicEntropy.CurrentPsyfocus;

            if((psyfocus * 100) >= adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(adjustedAmount * NeuralHeatFactor))
            {
                refuelComp.Refuel(amount);
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-(adjustedAmount) / 100);
                pawn.psychicEntropy.TryAddEntropy(adjustedAmount * NeuralHeatFactor);

                parent.BroadcastCompSignal("Refueled");
            }
            else if((psyfocus * 100) < adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(pawn.psychicEntropy.CurrentPsyfocus * NeuralHeatFactor))
            {
                refuelComp.Refuel(psyfocus * 100);
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-psyfocus);
                pawn.psychicEntropy.TryAddEntropy((psyfocus * 100)*NeuralHeatFactor);

                parent.BroadcastCompSignal("Refueled");
            }
            else
            {
                for(int i=1; i<adjustedAmount; i++)
                {
                    if(!pawn.psychicEntropy.WouldOverflowEntropy(i * NeuralHeatFactor))
                    {
                        refuelComp.Refuel(i);
                        pawn.psychicEntropy.OffsetPsyfocusDirectly(-i / 100);
                        pawn.psychicEntropy.TryAddEntropy(i * NeuralHeatFactor);

                        parent.BroadcastCompSignal("Refueled");
                    }
                }
            }
        }*/

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