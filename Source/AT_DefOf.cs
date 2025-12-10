using RimWorld;
using Verse;

namespace AnimaTech
{
    [DefOf]
    public static class AT_DefOf
    {
        static AT_DefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(AT_DefOf));
		}

        //public static ThingDef AT_RunesGlow;

        public static JobDef PsychicImbuement;

        //public static ThingCategoryDef FoodRaw;

        //public static WorkGiverDef PsychicRefuel;

        public static ThingDef AT_ActiveTeleporter;

        public static SoundDef PsycastCastLoop;

        public static PawnsArrivalModeDef AT_CenterTeleport;

        public static PawnsArrivalModeDef AT_EdgeTeleport;
    }
}