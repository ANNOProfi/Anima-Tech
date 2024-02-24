using RimWorld;

namespace AnimaTech
{
    public class CompProperties_PsychicGlower : CompProperties_Glower
    {
        public bool conditionalOnFocusUse = true;

        public bool conditionalOnActivePylon;

        public bool conditionalOnNonEmptyStorage;

        public CompProperties_PsychicGlower()
        {
            compClass = typeof(CompPsychicGlower);
        }
    }
}