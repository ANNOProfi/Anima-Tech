using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(Building_CommsConsole))]
    [HarmonyPatch("GetFailureReason")]
    public class Building_CommsConsole_GetFailureReasonPatch
    {
        private static void Postfix(ref Building_CommsConsole __instance, ref FloatMenuOption __result)
        {
            if(__result != null && __instance.Spawned && __instance.Map.gameConditionManager.ElectricityDisabled(__instance.Map) && __instance.TryGetComp<CompPsychicUser>() != null)
            {
                __result = null;
            }
        }
    }
}