using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    /*[HarmonyPatch(typeof(CompAffectedByFacilities))]
    [HarmonyPatch("Notify_NewLink")]
    public class CompAffectedByFacilities_NotifyNewLinkPatch
    {
        public static void Postfix(ref CompAffectedByFacilities __instance)
        {
            __instance.parent.TryGetComp<CompNotWithoutFacilities>()?.IsConnected();
        }
    }*/
}