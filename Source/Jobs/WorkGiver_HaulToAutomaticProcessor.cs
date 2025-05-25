using System;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace AnimaTech
{
    public class WorkGiver_HaulToAutomaticProcessor : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.ThingHolder);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsForbidden(pawn))
            {
                return false;
            }

            CompWorkTableAutomatic compWorkTableAutomatic = t.TryGetComp<CompWorkTableAutomatic>();

            if(compWorkTableAutomatic == null)
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            return FindIngredients(pawn, compWorkTableAutomatic).Thing != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompWorkTableAutomatic compWorkTableAutomatic = t.TryGetComp<CompWorkTableAutomatic>();
            if (compWorkTableAutomatic == null || compWorkTableAutomatic.IsFull)
            {
                return null;
            }

            ThingCount thingCount = FindIngredients(pawn, compWorkTableAutomatic);
            if (thingCount.Thing != null)
            {
                Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
                job.count = Mathf.Min(job.count, thingCount.Count);
                return job;
            }
            return null;
        }

        private ThingCount FindIngredients(Pawn pawn, CompWorkTableAutomatic comp)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
            if (thing == null)
            {
                return default(ThingCount);
            }

            return new ThingCount(thing, Mathf.Clamp(Mathf.Min(thing.stackCount, comp.CountToFull), comp.MinimumCount, comp.MaximumCount));

            bool Validator(Thing x)
            {
                if (x.IsForbidden(pawn) || !pawn.CanReserve(x) || comp.selectedRecipe == null || !comp.NeedsRestock)
                {
                    return false;
                }

                return comp.selectedRecipe.ingredients[0].filter.Allows(x.def);
            }
        }
    }
}