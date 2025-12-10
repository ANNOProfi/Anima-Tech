using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(CompDeepScanner))]
    [HarmonyPatch("ShouldShowDeepResourceOverlay")]
    public class CompDeepScanner_ShouldShowDeepResourceOverlayPatch
    {
        private static void Postfix(ref CompDeepScanner __instance, ref bool __result)
        {
            CompPsychicUser userComp = __instance.parent.TryGetComp<CompPsychicUser>();

            if(userComp != null)
            {
                __result = userComp.IsActive;
            }

            CompNotWithoutFacilities facilityComp = __instance.parent.TryGetComp<CompNotWithoutFacilities>();

            if(__result && facilityComp != null && !facilityComp.CanUse)
            {
                __result = false;
            }
        }
    }
}