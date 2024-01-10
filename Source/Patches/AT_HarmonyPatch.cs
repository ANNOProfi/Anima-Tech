using HarmonyLib;
using Verse;

namespace AnimaTech
{
    [StaticConstructorOnStartup]
    public static class AT_HarmonyPatch
    {
        static AT_HarmonyPatch()
        {
            new Harmony("ANNOProfi.AnimaTech").PatchAll();
        }
    }
}