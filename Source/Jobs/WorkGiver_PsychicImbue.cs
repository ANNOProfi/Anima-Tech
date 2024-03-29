using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    public class WorkGiver_PsychicImbue : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.MeditationFocus);

        public override PathEndMode PathEndMode => PathEndMode.Touch; 

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.PsychicComp().imbuementCache;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn.Map.PsychicComp().imbuementCache.NullOrEmpty();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompPsychicStorage storageComp = t.TryGetComp<CompPsychicStorage>();
            CompPsychicPylon pylonComp = t.TryGetComp<CompPsychicPylon>();

            if(!pawn.HasPsylink || pawn.psychicEntropy.CurrentPsyfocus <= pawn.psychicEntropy.TargetPsyfocus || !pawn.CanReserve(t) || t.Faction != pawn.Faction)
            {
                return false;
            }
            if((pylonComp != null && pylonComp.Network.IsFull()) || (storageComp != null && storageComp.IsFull))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(AT_DefOf.PsychicImbuement, t);
        }
    }
}