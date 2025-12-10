using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.Linq;

namespace AnimaTech
{
    public class PawnsArrivalModeWorker_EdgeTeleport : PawnsArrivalModeWorker
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
            IntVec3 spot = DropCellFinder.FindRaidDropCenterDistant(map, allowRoofed: false, !transporters.IsShuttle());

            DropCellFinder.TryFindDropSpotNear(spot, map, out var result, allowFogged: false, canRoofPunch: true);

            TeleporterArrivalActionUtility.DoTeleport(transporters[0], result, map, DefaultRadius);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!parms.spawnCenter.IsValid)
            {
                parms.spawnCenter = DropCellFinder.FindRaidDropCenterDistant(map);
            }
            parms.spawnRotation = Rot4.Random;
            return true;
        }
    }
}