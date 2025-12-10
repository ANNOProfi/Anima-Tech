using Verse;
using RimWorld;

namespace AnimaTech
{
    public class CompProperties_Teleporter : CompProperties
    {
        public CompProperties_Teleporter()
        {
            compClass = typeof(CompTeleporter);
        }

        public float radius;

        public int range;

        public int ticksUntilTeleport = 120;

        public int focusToUse;
    }
}