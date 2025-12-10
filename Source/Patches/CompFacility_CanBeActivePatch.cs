using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(CompFacility))]
    public static class CompFacility_CanUseNowPatch
    {
        [HarmonyPatch(nameof(CompFacility.CanBeActive), MethodType.Getter)]
        [HarmonyPostfix]
        private static void CanBeActiveGetter(ref CompFacility __instance, ref bool __result)
        {
            CompPsychicUser userComp = __instance.parent.TryGetComp<CompPsychicUser>();

            if(__result && userComp != null && !userComp.IsActive)
            {
                __result = false;
            }
        }
    }
}