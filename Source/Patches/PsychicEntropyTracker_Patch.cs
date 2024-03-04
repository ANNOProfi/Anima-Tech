using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker))]
    [HarmonyPatch("GainPsyfocus")]
    public class PsychicEntropyTracker_Patch
    {
        private static bool Prefix(Pawn_PsychicEntropyTracker __instance, ref Thing focus)
        {
            if (focus != null && !focus.Destroyed)
            {
                float focus2 = Mathf.Clamp(MeditationUtility.PsyfocusGainPerTick(__instance.Pawn, focus), 0f, 1f);

                CompPsychicStorage compPsychicStorage = focus.TryGetComp<CompPsychicStorage>();

                CompPsychicPylon compPsychicPylon = focus.TryGetComp<CompPsychicPylon>();

                CompPsychicGenerator compPsychicGenerator = focus.TryGetComp<CompPsychicGenerator>();

                CompAssignableToPawn_PsychicStorage compAssignableToPawn_PsychicStorage = focus.TryGetComp<CompAssignableToPawn_PsychicStorage>();

                if ((compPsychicStorage == null && compPsychicPylon == null) || compAssignableToPawn_PsychicStorage == null || compPsychicGenerator == null)
                {
                    return true;
                }
                if((bool)compPsychicPylon?.Network.IsFull() && !compPsychicStorage.CanBeFilled)
                {
                    return true;
                }
                if (!compPsychicGenerator.TryStoreFocus(focus2, __instance.Pawn, compAssignableToPawn_PsychicStorage))
                {
                    return true;
                }

                focus.TryGetComp<CompMeditationFocus>()?.Used(__instance.Pawn);
                return false;
            }
            return true;
        }
    }
}
    