using System.Linq;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace AnimaTech
{
    public class TransportersArrivalAction_TeleportToSpecificCell : TransportersArrivalAction_LandInSpecificCell
    {
        private MapParent mapParent;

        private IntVec3 cell;

        private Rot4 rotation;

        private bool landInShuttle;

        public override bool GeneratesMap => false;

        public const int DefaultRadius = 10;

        public TransportersArrivalAction_TeleportToSpecificCell()
        {
        }

        public TransportersArrivalAction_TeleportToSpecificCell(MapParent mapParent, IntVec3 cell)
        {
            this.mapParent = mapParent;
            this.cell = cell;
        }

        public TransportersArrivalAction_TeleportToSpecificCell(MapParent mapParent, IntVec3 cell, Rot4 rotation, bool landInShuttle)
        {
            this.mapParent = mapParent;
            this.cell = cell;
            this.rotation = rotation;
            this.landInShuttle = landInShuttle;
        }

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(transporters);

            TeleporterArrivalActionUtility.DoTeleport(transporters[0], cell, mapParent.Map, DefaultRadius);

            Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
        }
    }
}