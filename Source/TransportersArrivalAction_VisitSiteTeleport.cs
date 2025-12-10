using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AnimaTech
{
    public class TransportersArrivalAction_VisitSiteTeleport : TransportersArrivalAction_VisitSite
    {
        private Site site;

	    private PawnsArrivalModeDef arrivalMode;

        public TransportersArrivalAction_VisitSiteTeleport()
        {
        }

        public TransportersArrivalAction_VisitSiteTeleport(Site site, PawnsArrivalModeDef arrivalMode)
        {
            this.site = site;
            this.arrivalMode = arrivalMode;
        }

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(transporters);

            bool num = !site.HasMap;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, site.PreferredMapSize, null);
            if (num)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
            }
            if (site.Faction != null && site.Faction != Faction.OfPlayer && site.MainSitePartDef.considerEnteringAsAttack)
            {
                Faction.OfPlayer.TryAffectGoodwillWith(site.Faction, Faction.OfPlayer.GoodwillToMakeHostile(site.Faction), canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.AttackedSettlement);
            }
            if (transporters.IsShuttle())
            {
                Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
            }

            arrivalMode.Worker.TravellingTransportersArrived(transporters, orGenerateMap);
        }

        public static new IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Site site)
        {
            foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, site), () => new TransportersArrivalAction_VisitSiteTeleport(site,  AT_DefOf.AT_EdgeTeleport), "DropAtEdge".Translate(), launchAction, site.Tile, UIConfirmationCallback))
            {
                yield return floatMenuOption;
            }
            foreach (FloatMenuOption floatMenuOption2 in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, site), () => new TransportersArrivalAction_VisitSiteTeleport(site, AT_DefOf.AT_CenterTeleport), "DropInCenter".Translate(), launchAction, site.Tile, UIConfirmationCallback))
            {
                yield return floatMenuOption2;
            }
            void UIConfirmationCallback(Action action)
            {
                if (ModsConfig.OdysseyActive && site.Tile.LayerDef == PlanetLayerDefOf.Orbit)
                {
                    TaggedString text = "OrbitalWarning".Translate();
                    text += string.Format("\n\n{0}", "LaunchToConfirmation".Translate());
                    Find.WindowStack.Add(new Dialog_MessageBox(text, null, action, "Cancel".Translate(), delegate
                    {
                    }, null, buttonADestructive: true));
                }
                else
                {
                    action();
                }
            }
	    }
    }
}