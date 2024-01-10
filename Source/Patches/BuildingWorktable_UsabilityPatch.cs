using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(Building_WorkTable))]
    [HarmonyPatch("CurrentlyUsableForBills")]
    public class BuildingWorkTable_UsabilityPatch
    {
        private static bool Postfix(bool result, ref Building_WorkTable __instance)
        {
            CompPsychicStorage compPsychicStorage = __instance.GetComp<CompPsychicStorage>();
            if (!(compPsychicStorage == null) && (compPsychicStorage == null || !compPsychicStorage.HasFocus))
            {
                return false;
            }
            return result;
        }
    }
}