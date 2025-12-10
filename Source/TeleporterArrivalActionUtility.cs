using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;

namespace AnimaTech
{
    public static class TeleporterArrivalActionUtility
    {
        public static void DoTeleport(ActiveTransporterInfo teleporter, IntVec3 pos, Map map, int radius)
        {
            TransportersArrivalActionUtility.RemovePawnsFromWorldPawns(new List<ActiveTransporterInfo>{teleporter});

            ThingActiveTeleporter activeTeleporter = (ThingActiveTeleporter)GenSpawn.Spawn(ThingMaker.MakeThing(AT_DefOf.AT_ActiveTeleporter), pos, map);

            activeTeleporter.Contents = teleporter;
            activeTeleporter.arriving = true;
            activeTeleporter.radius = radius;

            activeTeleporter.Begin(180);
        }

        public static void DoTeleport(List<Pawn> pawns, IncidentParms parms, int radius)
        {
            ThingActiveTeleporter activeTeleporter = (ThingActiveTeleporter)GenSpawn.Spawn(ThingMaker.MakeThing(AT_DefOf.AT_ActiveTeleporter), parms.spawnCenter, (Map)parms.target);

            activeTeleporter.Contents = new ActiveTransporterInfo();
            foreach(Thing thing in pawns)
            {
                activeTeleporter.Contents.innerContainer.TryAddOrTransfer(thing, 1, false);
            }
            activeTeleporter.arriving = true;
            activeTeleporter.radius = radius;

            activeTeleporter.Begin(180);
        }
    }
}