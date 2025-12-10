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
        private static void Postfix(ref Building_WorkTable __instance, ref bool __result)
        {
            CompPsychicStorage compPsychicStorage = __instance.GetComp<CompPsychicStorage>();
            CompPsychicPylon compPsychicPylon = __instance.GetComp<CompPsychicPylon>();

            if ((!(compPsychicStorage == null) && (!compPsychicStorage.HasMinimumFocus) && !(compPsychicPylon == null) && (!compPsychicPylon.isToggledOn)) || compPsychicPylon.Network.IsEmpty || !compPsychicPylon.Network.HasFocus(compPsychicStorage.Props.minimumFocusThreshold))
            {
                __result = true;
            }
        }
    }
}