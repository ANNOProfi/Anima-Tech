using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(CompDeepDrill))]
    [HarmonyPatch("CanDrillNow")]
    public class CompDeepDrill_CanDrillNowPatch
    {
        private static void Postfix(ref CompDeepDrill __instance, ref bool __result)
        {
            CompPsychicUser userComp = __instance.parent.TryGetComp<CompPsychicUser>();

            if(__result && userComp != null && !userComp.IsActive)
            {
                __result = false;
            }
        }
    }
}