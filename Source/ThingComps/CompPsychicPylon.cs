using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class CompPsychicPylon : ThingComp
    {
        public CompProperties_PsychicPylon Props => (CompProperties_PsychicPylon)props;

        public PsychicMapComponent mapComponentRef;

        public PsychicNetwork networkRef;

        public bool isToggledOn = true;

        public IEnumerable<IntVec3> affectedCells;

        public int PylonRadius => Props.pylonRadius;

        public PsychicMapComponent MapComponent
        {
            get
            {
                if (mapComponentRef == null)
                {
                    mapComponentRef = parent.Map.PsychicComp();
                }
                return mapComponentRef;
            }
        }

        public PsychicNetwork Network
        {
            get
            {
                if (networkRef == null)
                {
                    Log.Warning("AT: Manual regen triggered from inside CompPsychicPylon, this shouldn't happen.");
                    MapComponent.RegenGrid();
                }
                return networkRef;
            }
        }

        public bool ShouldFormLinks
        {
            get
            {
                if (Props.canBeToggled)
                {
                    return isToggledOn;
                }
                return true;
            }
        }

        public bool IsConsideredDisconnected
        {
            get
            {
                if (ShouldFormLinks && Props.pylonRadius < 1)
                {
                    if (networkRef != null)
                    {
                        return networkRef.pylons.Count < 2;
                    }
                    return true;
                }
                return false;
            }
        }

        public IEnumerable<IntVec3> GetAffectedCells(bool forceUpdate = false)
        {
            if ((forceUpdate || affectedCells == null) && PylonRadius > 0)
            {
                affectedCells = GenRadial.RadialCellsAround(parent.PositionHeld, PylonRadius, useCenter: true);
            }
            return affectedCells;
        }

        public float TryDrawFocus(float amount)
        {
            if (ShouldFormLinks)
            {
                return Network.PullFocus(amount);
            }
            return amount;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            isToggledOn = Props.defaultToggleState;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad && Props.disableIfNotInRangeOfNetworkOnSpawn && !MapComponent.NetworkPresentAt(parent.Position))
            {
                isToggledOn = false;
            }
            MapComponent.RegisterPylon(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            MapComponent.DeregisterPylon(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isToggledOn, "isToggledOn", defaultValue: false);
        }

        public override string CompInspectStringExtra()
        {
            if (networkRef != null)
            {
                if (IsConsideredDisconnected)
                {
                    return "ARR_AetherLinkDisconnected".Translate();
                }
                string text = "ARR_AetherNetworkStorage".Translate(networkRef.focusTotal.ToString("F1"), networkRef.focusCapacity.ToString("F1"), networkRef.generationTotal.ToString("F1"), (0f - networkRef.consumptionTotal).ToString("F1"));
                if (DebugSettings.godMode)
                {
                    return text + $"\nDebug: Network ID #{((networkRef == null) ? (-1) : networkRef.networkId)}";
                }
                return text;
            }
            return null;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (!Props.canBeToggled)
            {
                yield break;
            }
            Command_Toggle command_Toggle = new Command_Toggle();
            if (isToggledOn)
            {
                command_Toggle.defaultLabel = "ARR_AetherLinkEnabled".Translate();
                command_Toggle.defaultDesc = "ARR_AetherLinkEnabledDesc".Translate();
            }
            else
            {
                command_Toggle.defaultLabel = "ARR_AetherLinkDisabled".Translate();
                command_Toggle.defaultDesc = "ARR_AetherLinkDisabledDesc".Translate();
            }
            command_Toggle.hotKey = KeyBindingDefOf.Command_ItemForbid;
            //command_Toggle.icon = UIAssets.ButtonFocusLink;
            command_Toggle.isActive = () => isToggledOn;
            command_Toggle.toggleAction = delegate
            {
                isToggledOn = !isToggledOn;
                if (isToggledOn)
                {
                    MapComponent.RegisterPylon(this, onlyEnable: true);
                    //parent.BroadcastCompSignal("ARR.AethericLinkActivated");
                }
                else
                {
                    MapComponent.DeregisterPylon(this, onlyDisable: true);
                    //parent.BroadcastCompSignal("ARR.AethericLinkDeactivated");
                }
            };
            yield return command_Toggle;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (!ShouldFormLinks)
            {
                return;
            }
            /*if (networkRef != null)
            {
                foreach (AetherLinkEdge edge in networkRef.edges)
                {
                    GenDraw.DrawLineBetween(edge.first, edge.second, UIAssets.AetherLinkLineMaterial);
                }
            }*/
            if (PylonRadius > 0)
            {
                GenDraw.DrawRadiusRing(parent.Position, PylonRadius, new Color(0.51f, 0.61f, 0.55f));
            }
        }
    }
}