using Verse;

namespace AnimaTech
{
    public static class AT_Utilities
    {
        public static AT_Settings Settings => LoadedModManager.GetMod<AnimaTechMod>().GetSettings<AT_Settings>();
    }
}