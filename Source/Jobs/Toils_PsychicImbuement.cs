using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    public class Toils_PsychicImbuement
    {
        public static Toil FinalizePsychicImbuing(TargetIndex refuelableInd)
        {
            Toil toil = ToilMaker.MakeToil("FinalizePsychicRefueling");
            toil.initAction = delegate
            {
                Job curJob = toil.actor.CurJob;

                Thing thing = curJob.GetTarget(refuelableInd).Thing;

                CompPsychicGenerator generatorComp = thing.TryGetComp<CompPsychicGenerator>();

                CompPsychicPylon pylonComp = thing.TryGetComp<CompPsychicPylon>();

                float amount = thing.TryGetComp<CompPsychicStorage>().AmountToFill / generatorComp.Props.FocusMultiplierCurrentDifficulty;

                float amount2 = pylonComp.networkRef.AmountToFill() / generatorComp.Props.FocusMultiplierCurrentDifficulty;

                if(toil.actor.CurJob.placedThings.NullOrEmpty())
                {
                    generatorComp.Imbue(Math.Max(amount, amount2), toil.GetActor());
                }
            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;

            return toil;
        }
    }
}