using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AnimaTech
{
    public class TransportersArrivalAction_AttackSettlementTeleport : TransportersArrivalAction_AttackSettlement
    {
        private Settlement settlement;

	    private PawnsArrivalModeDef arrivalMode;

        public TransportersArrivalAction_AttackSettlementTeleport()
        {
        }

        public TransportersArrivalAction_AttackSettlementTeleport(Settlement settlement, PawnsArrivalModeDef arrivalMode)
        {
            this.settlement = settlement;
            this.arrivalMode = arrivalMode;
        }

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(transporters);
            bool num = !settlement.HasMap;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString letterText = "LetterTransportPodsLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
            SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
            if (num)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
            }
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTarget, settlement.Faction);
            arrivalMode.Worker.TravellingTransportersArrived(transporters, orGenerateMap);
        }

        public static new IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Settlement settlement)
        {
            foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportersArrivalAction_AttackSettlementTeleport(settlement, AT_DefOf.AT_EdgeTeleport), "AttackAndDropAtEdge".Translate(settlement.Label), launchAction, settlement.Tile))
            {
                yield return floatMenuOption;
            }
            foreach (FloatMenuOption floatMenuOption2 in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(pods, settlement), () => new TransportersArrivalAction_AttackSettlementTeleport(settlement, AT_DefOf.AT_CenterTeleport), "AttackAndDropInCenter".Translate(settlement.Label), launchAction, settlement.Tile))
            {
                yield return floatMenuOption2;
            }
        }
    }
}