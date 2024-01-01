using RimWorld;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    public class WorkGiver_PsychicRefuel : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Refuelable);//ThingRequest.ForDef(AT_DefOf.RunicSmithy);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public virtual JobDef JobStandard => AT_DefOf.PsychicRefuel;

        public virtual bool CanRefuelThing(Thing t)
        {
            //Log.Message("CanRefuelThing");
            return !(t is Building_Turret);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //Log.Message("Checking for Job");
            if (CanRefuelThing(t))
            {
                return PsychicRefuelWorkGiverUtility.CanRefuel(pawn, t, forced);
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //Log.Message("Ordering new Job");
            return PsychicRefuelWorkGiverUtility.RefuelJob(t, JobStandard);
        }
    }
}