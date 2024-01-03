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

        //public static ThingDef AT_RunicStove;

        public static JobDef PsychicImbuement;

        //public static WorkGiverDef PsychicRefuel;
    }
}