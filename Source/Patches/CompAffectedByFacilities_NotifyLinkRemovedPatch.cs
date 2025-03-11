using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    /*[HarmonyPatch(typeof(CompAffectedByFacilities))]
    [HarmonyPatch("Notify_LinkRemoved")]
    public class CompAffectedByFacilities_NotifyLinkRemovedPatch
    {
        public static void Postfix(ref CompAffectedByFacilities __instance)
        {
            __instance.parent.TryGetComp<CompNotWithoutFacilities>()?.IsConnected();
        }
    }*/
}