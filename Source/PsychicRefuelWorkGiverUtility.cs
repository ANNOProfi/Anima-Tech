using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    public static class PsychicRefuelWorkGiverUtility
    {
        public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
        {
            Log.Message("Checking for FailReasons");
            CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();

            if(!pawn.HasPsylink)
            {
                return false;
            }

            if (compRefuelable == null || compRefuelable.IsFull || (!forced && !compRefuelable.allowAutoRefuel))
            {
                return false;
            }

            if (compRefuelable.FuelPercentOfMax > 0f && !compRefuelable.Props.allowRefuelIfNotEmpty)
            {
                return false;
            }

            if (!forced && !compRefuelable.ShouldAutoRefuelNow)
            {
                return false;
            }

            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            if (t.Faction != pawn.Faction)
            {
                return false;
            }

            CompActivable compActivable = t.TryGetComp<CompActivable>();
            
            if (compActivable != null && compActivable.Props.cooldownPreventsRefuel && compActivable.OnCooldown)
            {
                JobFailReason.Is(compActivable.Props.onCooldownString.CapitalizeFirst());
                return false;
            }

            if (!HasEnoughPsyfocus(pawn, t))
            {
                JobFailReason.Is("NoPsyfocusToRefuel".Translate());
                return false;
            }

            return true;
        }

        public static Job RefuelJob(Thing target, JobDef customRefuelJob = null)
        {
            //Log.Message("Making new Job");
            return JobMaker.MakeJob(customRefuelJob ?? AT_DefOf.PsychicRefuel, target);
        }

        private static bool HasEnoughPsyfocus(Pawn pawn, Thing t)
        {
            if(pawn.psychicEntropy.CurrentPsyfocus >= t.TryGetComp<CompPsychicFuel>().MinimumFuel)
            {
                return true;
            }

            return false;
        }
    }
}