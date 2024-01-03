using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    /*public class Toils_PsychicRefuel
    {
        public static Toil FinalizePsychicRefueling(TargetIndex refuelableInd)
        {
            Toil toil = ToilMaker.MakeToil("FinalizePsychicRefueling");
            toil.initAction = delegate
            {
                Job curJob = toil.actor.CurJob;

                Thing thing = curJob.GetTarget(refuelableInd).Thing;

                float amount = thing.TryGetComp<CompRefuelable>().GetFuelCountToFullyRefuel() / thing.TryGetComp<CompRefuelable>().Props.FuelMultiplierCurrentDifficulty;

                if(toil.actor.CurJob.placedThings.NullOrEmpty())
                {
                    //thing.TryGetComp<CompPsychicFuel>().Refuel(amount, toil.GetActor());
                }
            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;

            return toil;
        }
    }*/
}