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

        private CompRefuelable refuelComp;

        public float MinimumFuel => 1/refuelComp.Props.fuelCapacity;

        public float NeuralHeatFactor = 0.5f;
        
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            refuelComp = parent.GetComp<CompRefuelable>();
        }

        public void Refuel(float amount, Pawn pawn)
        {
            float adjustedAmount = amount * refuelComp.Props.FuelMultiplierCurrentDifficulty;

            if((pawn.psychicEntropy.CurrentPsyfocus * 100) >= adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(adjustedAmount*NeuralHeatFactor))
            {
                refuelComp.Refuel(amount);
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-((adjustedAmount))/100);
                pawn.psychicEntropy.TryAddEntropy(adjustedAmount*NeuralHeatFactor);
            }
            else if((pawn.psychicEntropy.CurrentPsyfocus * 100) < adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(pawn.psychicEntropy.CurrentPsyfocus*NeuralHeatFactor))
            {
                refuelComp.Refuel(pawn.psychicEntropy.CurrentPsyfocus * 100);
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-pawn.psychicEntropy.CurrentPsyfocus);
                pawn.psychicEntropy.TryAddEntropy((pawn.psychicEntropy.CurrentPsyfocus * 100)*NeuralHeatFactor);
            }
            else
            {
                for(int i=1; i<adjustedAmount; i++)
                {
                    if(!pawn.psychicEntropy.WouldOverflowEntropy(i*NeuralHeatFactor))
                    {
                        refuelComp.Refuel(i);
                        pawn.psychicEntropy.OffsetPsyfocusDirectly(-i/100);
                        pawn.psychicEntropy.TryAddEntropy(i*NeuralHeatFactor);
                    }
                }
            }
        }
    }
}