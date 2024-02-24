using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class PlaceWorker_ShowPsychicPylons : PlaceWorker
    {
        protected PsychicMapComponent mapComponent;

        //protected HashSet<AetherLinkEdge> directEdges = new HashSet<AetherLinkEdge>();

        //protected HashSet<AetherLinkEdge> networkEdges = new HashSet<AetherLinkEdge>();

        //protected HashSet<AetherLinkEdge> inactiveEdges = new HashSet<AetherLinkEdge>();

        protected HashSet<PsychicNetwork> linkableNetworks = new HashSet<PsychicNetwork>();

        protected int cellIndex;

        protected void FindDirectLinks(CompProperties_PsychicPylon props, ThingDef def, IntVec3 center, Rot4 rot)
        {
            if (mapComponent.networkGrid[cellIndex] != null)
            {
                linkableNetworks.UnionWith(mapComponent.networkGrid[cellIndex]);
            }
            foreach (IntVec3 item in GenRadial.RadialCellsAround(center, props.pylonRadius, useCenter: false))
            {
                if (!mapComponent.pylonsByLocation.TryGetValue(item, out var value))
                {
                    continue;
                }
                if (value.ShouldFormLinks)
                {
                    //directEdges.Add(new AetherLinkEdge(value, GenThing.TrueCenter(center, rot, def.size, def.Altitude)));
                    if (value.Network != null)
                    {
                        linkableNetworks.Add(value.Network);
                    }
                }
                /*else
                {
                    //inactiveEdges.Add(new AetherLinkEdge(value, GenThing.TrueCenter(center, rot, def.size, def.Altitude)));
                }*/
            }
        }

        protected void FindNetworkLinks()
        {
            foreach (PsychicNetwork linkableNetwork in linkableNetworks)
            {
                //networkEdges.UnionWith(linkableNetwork.edges);
                foreach (CompPsychicPylon pylon in linkableNetwork.pylons)
                {
                    if (pylon.PylonRadius > 0)
                    {
                        GenDraw.DrawRadiusRing(pylon.parent.Position, pylon.PylonRadius);
                    }
                }
            }
        }

        /*protected void drawEdges()
        {
            foreach (AetherLinkEdge inactiveEdge in inactiveEdges)
            {
                GenDraw.DrawLineBetween(inactiveEdge.first, inactiveEdge.second, UIAssets.AetherLinkInactiveLineMaterial);
            }
            foreach (AetherLinkEdge networkEdge in networkEdges)
            {
                GenDraw.DrawLineBetween(networkEdge.first, networkEdge.second, UIAssets.AetherLinkLineMaterial);
            }
            foreach (AetherLinkEdge directEdge in directEdges)
            {
                GenDraw.DrawLineBetween(directEdge.first, directEdge.second, UIAssets.AetherLinkBrightLineMaterial);
            }
        }*/

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            mapComponent = Find.CurrentMap.PsychicComp();
            CompProperties_PsychicPylon compProperties = def.GetCompProperties<CompProperties_PsychicPylon>();
            if (compProperties != null && mapComponent != null && !mapComponent.dirty)
            {
                //directEdges.Clear();
                //networkEdges.Clear();
                //inactiveEdges.Clear();
                linkableNetworks.Clear();
                cellIndex = mapComponent.cellIndices.CellToIndex(center);
                FindDirectLinks(compProperties, def, center, rot);
                FindNetworkLinks();
                //drawEdges();
                if (compProperties.pylonRadius > 0)
                {
                    GenDraw.DrawRadiusRing(center, compProperties.pylonRadius, new Color(0.67f, 0.80f, 0.70f));
                }
            }
        }
    }
}