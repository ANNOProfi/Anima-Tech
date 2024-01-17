using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(Building_WorkTable))]
    [HarmonyPatch("UsedThisTick")]
    public class BuildingWorkTable_NotifyPatch
    {
        private static void Postfix(ref Building_WorkTable __instance)
        {
            CompPsychicUser compPsychicUser = __instance.GetComp<CompPsychicUser>();
            compPsychicUser?.Notify_UsedThisTick();
        }
    }
}