using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    public class JobDriver_PsychicRefuel : JobDriver
    {
        private const TargetIndex RefuelableInd = TargetIndex.A;

        public const int RefuelingDuration = 240;

        protected Thing Refuelable => job.GetTarget(TargetIndex.A).Thing;

        protected CompRefuelable RefuelableComp => Refuelable.TryGetComp<CompRefuelable>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Log.Message("Making Toils");
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            AddEndCondition(() => (!RefuelableComp.IsFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
            AddFailCondition(() => !job.playerForced && !RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct);
            AddFailCondition(() => !RefuelableComp.allowAutoRefuel && !job.playerForced);

            yield return Toils_General.DoAtomic(delegate
            {
                job.count = RefuelableComp.GetFuelCountToFullyRefuel();
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);
            //Log.Message("Making final Toil");
            yield return Toils_PsychicRefuel.FinalizePsychicRefueling(TargetIndex.A);
        }
    }
}