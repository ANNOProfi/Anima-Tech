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
            //Log.Message("Calling meditate prefix");
            Pawn pawn = __instance.pawn;
            List<Building> list = __instance.pawn.Map.listerBuildings.allBuildingsColonist.Where((Building x) => x.GetComp<CompAssignableToPawn_PsychicStorage>() != null && (x.GetComp<CompPsychicStorage>() != null || x.GetComp<CompPsychicPylon>() != null) && x.GetComp<CompPsychicGenerator>() != null).ToList();
            if (list.NullOrEmpty())
            {
                return true;
            }

            List<Building> source = list.Where(delegate(Building x)
            {
                CompPsychicStorage comp = x.GetComp<CompPsychicStorage>();
                CompPsychicGenerator comp2 = x.GetComp<CompPsychicGenerator>();
                CompPsychicPylon comp3 = x.GetComp<CompPsychicPylon>();

                return comp2.Props.isMeditatable && x.GetComp<CompAssignableToPawn_PsychicStorage>().AssignedPawns.Contains(pawn) && pawn.CanReach(x, PathEndMode.Touch, Danger.None);
            }).ToList();

            source.TryRandomElement(out var result);

            if (result != null)
            {
                Job curJob = pawn.jobs.curJob;
                if (curJob.targetC.Thing != null && source.Contains(curJob.targetC.Thing))
                {
                    //Log.Message("Job already on thing");
                    return false;
                }

                /*if(result.def.hasInteractionCell)
                {
                    foreach(IntVec3 cell in cells)
                    {
                        if(result.TrueCenter().ToIntVec3()-result.def.interactionCellOffset == cell)
                        {
                            Log.Message("Removing interaction spot");
                            cells.Remove(cell);
                        }
                    }
                }*/

                //Log.Message("Selecting target");
                (from x in GenRadial.RadialCellsAround(result.Position, 5f, false)
                    where pawn.CanReach(x, PathEndMode.OnCell, Danger.None)
                    select x).TryRandomElement(out var result2);
                //Log.Message("Making new Job");
                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Meditate, result2, null, result), JobTag.Misc, requestQueueing: false);
                return false;
            }
            Log.Message("No building found");
            return true;
        }
    }
}