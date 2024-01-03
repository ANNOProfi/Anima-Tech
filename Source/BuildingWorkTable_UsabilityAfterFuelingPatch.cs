using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(Building_WorkTable))]
    [HarmonyPatch("UsableForBillsAfterFueling")]
    public class BuildingWorkTable_UsabilityAfterFuelingPatch
    {
        private static bool Postfix(bool result, ref Building_WorkTable __instance)
        {
            CompPsychicStorage compPsychicStorage = __instance.GetComp<CompPsychicStorage>();
            if (!(compPsychicStorage == null) && (compPsychicStorage == null || !compPsychicStorage.HasFuel))
            {
                return false;
            }
            return result;
        }
    }
}