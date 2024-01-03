using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    public class JobDriver_PsychicImbuement : JobDriver
    {
        private const TargetIndex RefuelableInd = TargetIndex.A;

        public const int RefuelingDuration = 240;

        protected Thing Refuelable => job.GetTarget(TargetIndex.A).Thing;

        protected CompPsychicStorage StorageComp => Refuelable.TryGetComp<CompPsychicStorage>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            AddEndCondition(() => (!StorageComp.IsFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
            AddFailCondition(() => !job.playerForced && !StorageComp.ShouldImbueNowIgnoringFuelPct);
            AddFailCondition(() => !StorageComp.Props.allowImbuement && !job.playerForced);

            yield return Toils_General.DoAtomic(delegate
            {
                job.count = StorageComp.GetFuelCountToFullyRefuel();
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(RefuelingDuration).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);
            yield return Toils_PsychicImbuement.FinalizePsychicImbuing(TargetIndex.A);
        }
    }
}