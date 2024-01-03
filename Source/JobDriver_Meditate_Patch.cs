using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    [HarmonyPatch(typeof(JobDriver_Meditate))]
    [HarmonyPatch("Notify_Starting")]
    public class JobDriver_Meditate_Patch
    {
        private static bool Prefix(ref JobDriver_Meditate __instance)
        {
            Pawn pawn = __instance.pawn;
            List<Building> list = __instance.pawn.Map.listerBuildings.allBuildingsColonist.Where((Building x) => x.GetComp<CompAssignableToPawn_PsychicStorage>() != null && x.GetComp<CompPsychicStorage>() != null).ToList();
            if (list.NullOrEmpty())
            {
                return true;
            }

            List<Building> source = list.Where(delegate(Building x)
            {
                CompPsychicStorage comp = x.GetComp<CompPsychicStorage>();
                return comp != null && comp.meditationActive && x.GetComp<CompAssignableToPawn_PsychicStorage>().AssignedPawns.Contains(pawn) && pawn.CanReach(x, PathEndMode.Touch, Danger.None);
            }).ToList();

            source.TryRandomElement(out var result);

            if (result != null)
            {
                Job curJob = pawn.jobs.curJob;
                if (curJob.targetC.Thing != null && source.Contains(curJob.targetC.Thing))
                {
                    return false;
                }

                (from x in GenRadial.RadialCellsAround(result.Position, 5f, useCenter: false)
                    where pawn.CanReach(x, PathEndMode.OnCell, Danger.None) && !(result.def.hasInteractionCell && (result.TrueCenter().ToIntVec3()-result.def.interactionCellOffset == x))
                    select x).TryRandomElement(out var result2);
                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Meditate, result2, null, result), JobTag.Misc);
                return false;
            }
            return true;
        }
    }
}