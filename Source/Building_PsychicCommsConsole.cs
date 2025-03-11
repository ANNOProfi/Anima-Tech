using Verse;
using RimWorld;
using Verse.AI;
using System.Linq;
using System.Collections.Generic;

namespace AnimaTech
{
    public class Building_PsychicCommsConsole : Building_CommsConsole
    {
        private CompPsychicUser userComp;

        private CompNotWithoutFacilities facilityComp;

        public new bool CanUseCommsNow
        {
            get
            {
                if(userComp != null)
                {
                    if(facilityComp != null)
                    {
                        /*if(facilityComp.linkedFacilities.NullOrEmpty())
                        {
                            return false;
                        }

                        foreach(Thing facility in facilityComp.linkedFacilities)
                        {
                            if(facilityComp.Props.linkableFacilities.Contains(facility.def))
                            {
                                return facility.TryGetComp<CompEnabledByFacility>().AmIFacilityAndActive && userComp.IsActive;
                            }
                        }*/

                        return userComp.IsActive && facilityComp.CanUse;
                    }
                    
                    return userComp.IsActive;
                }

                return true;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            userComp = GetComp<CompPsychicUser>();

            facilityComp = GetComp<CompNotWithoutFacilities>();

		    LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
		    LessonAutoActivator.TeachOpportunity(ConceptDefOf.OpeningComms, OpportunityType.GoodToKnow);

            if (CanUseCommsNow)
            {
                LongEventHandler.ExecuteWhenFinished(AnnounceTradeShips);
            }
        }

        private void UseAct(Pawn myPawn, ICommunicable commTarget)
        {
            Job job = JobMaker.MakeJob(JobDefOf.UseCommsConsole, this);
            job.commTarget = commTarget;
            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
        }

        private FloatMenuOption GetFailureReason(Pawn myPawn)
        {
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
            {
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            }
            if (userComp != null && !userComp.IsActive)
            {
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            }
            if (facilityComp != null && !facilityComp.CanUse)
            {
                return new FloatMenuOption("AT_CannotUseNoFacility".Translate(), null);
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, myPawn.Named("PAWN"))), null);
            }
            if (!GetCommTargets(myPawn).Any())
            {
                return new FloatMenuOption("CannotUseReason".Translate("NoCommsTarget".Translate()), null);
            }
            if (!CanUseCommsNow)
            {
                Log.Error(string.Concat(myPawn, " could not use comm console for unknown reason."));
                return new FloatMenuOption("Cannot use now", null);
            }
            return null;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            FloatMenuOption failureReason = GetFailureReason(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
                yield break;
            }
            foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption2;
            }
        }

        private void AnnounceTradeShips()
        {
            foreach (TradeShip item in from s in base.Map.passingShipManager.passingShips.OfType<TradeShip>()
                where !s.WasAnnounced
                select s)
            {
                TaggedString baseLetterText = "TraderArrival".Translate(item.name, item.def.label, (item.Faction == null) ? "TraderArrivalNoFaction".Translate() : "TraderArrivalFromFaction".Translate(item.Faction.Named("FACTION")));
                IncidentParms incidentParms = new IncidentParms();
                incidentParms.target = base.Map;
                incidentParms.traderKind = item.TraderKind;
                IncidentWorker.SendIncidentLetter(item.def.LabelCap, baseLetterText, LetterDefOf.PositiveEvent, incidentParms, LookTargets.Invalid, null);
                item.WasAnnounced = true;
            }
        }
    }
}