using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(CompScanner))]
    public static class CompScanner_CanUseNowPatch
    {
        [HarmonyPatch(nameof(CompScanner.CanUseNow), MethodType.Getter)]
        [HarmonyPostfix]
        private static void CanUseNowGetter(ref CompScanner __instance, ref AcceptanceReport __result)
        {
            CompForbiddable forbiddable = __instance.parent.TryGetComp<CompForbiddable>();

            CompPsychicUser userComp = __instance.parent.TryGetComp<CompPsychicUser>();

            CompNotWithoutFacilities facilityComp = __instance.parent.TryGetComp<CompNotWithoutFacilities>();
            
            if(!__result && userComp != null && RoofUtility.IsAnyCellUnderRoof(__instance.parent) && __instance.parent.Spawned && forbiddable != null && !forbiddable.Forbidden && __instance.parent.Faction == Faction.OfPlayer)
            {
                __result = true;
            }

            if(__result && userComp != null && !userComp.IsActive)
            {
                __result = false;
            }

            if(__result && facilityComp != null && !facilityComp.CanUse)
            {
                __result = false;
            }
        }
    }
}