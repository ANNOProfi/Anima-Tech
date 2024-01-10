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

                float amount = thing.TryGetComp<CompPsychicStorage>().GetFuelCountToFullyRefuel() / thing.TryGetComp<CompPsychicStorage>().Props.FocusMultiplierCurrentDifficulty;

                if(toil.actor.CurJob.placedThings.NullOrEmpty())
                {
                    thing.TryGetComp<CompPsychicStorage>().Imbue(amount, toil.GetActor());
                }
            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;

            return toil;
        }
    }
}