using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.Linq;

namespace AnimaTech
{
    public class PawnsArrivalModeWorker_CenterTeleport : PawnsArrivalModeWorker
    {
        public const int DefaultRadius = 10;

        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            int cellAmount;
            int radius = 0;

            do
            {
                radius++;
                cellAmount = GenRadial.NumCellsInRadius(radius);
            }
            while (cellAmount < pawns.Count);

            TeleporterArrivalActionUtility.DoTeleport(pawns, parms, radius);
        }

        public override void TravellingTransportersArrived(List<ActiveTransporterInfo> transporters, Map map)
        {
            if (!DropCellFinder.TryFindRaidDropCenterClose(out var spot, map))
            {
                spot = DropCellFinder.FindRaidDropCenterDistant(map);
            }

            DropCellFinder.TryFindDropSpotNear(spot, map, out var result, allowFogged: false, canRoofPunch: true);

            TeleporterArrivalActionUtility.DoTeleport(transporters[0], result, map, DefaultRadius);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!parms.raidArrivalModeForQuickMilitaryAid)
            {
                parms.podOpenDelay = 520;
            }
            parms.spawnRotation = Rot4.Random;
            if (!parms.spawnCenter.IsValid)
            {
                bool flag = parms.faction != null && parms.faction == Faction.OfMechanoids;
                bool flag2 = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
                if (Rand.Chance(0.4f) && !flag && map.listerBuildings.ColonistsHaveBuildingWithPowerOn(ThingDefOf.OrbitalTradeBeacon))
                {
                    parms.spawnCenter = DropCellFinder.TradeDropSpot(map);
                }
                else if (!DropCellFinder.TryFindRaidDropCenterClose(out parms.spawnCenter, map, !flag && flag2, !flag))
                {
                    parms.raidArrivalMode = AT_DefOf.AT_EdgeTeleport;
                    return parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
                }
            }
            return true;
        }
    }
}