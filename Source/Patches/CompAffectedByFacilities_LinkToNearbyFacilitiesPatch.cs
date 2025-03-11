using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    /*[HarmonyPatch(typeof(CompAffectedByFacilities))]
    [HarmonyPatch("LinkToNearbyFacilities")]
    public class CompAffectedByFacilities_LinkToNearbyFacilitiesPatch
    {
        public static void Postfix(ref CompAffectedByFacilities __instance)
        {
            __instance.parent.TryGetComp<CompNotWithoutFacilities>()?.IsConnected();
        }
    }*/
}