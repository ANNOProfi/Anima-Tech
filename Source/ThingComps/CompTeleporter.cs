using Verse;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;
using Verse.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse.AI;
using Steamworks;

namespace AnimaTech
{
    [StaticConstructorOnStartup]
    public class CompTeleporter : ThingComp, IThingHolder
    {
        public CompProperties_Teleporter Props => (CompProperties_Teleporter)props;

        public bool Active => storageComp.focusStored + pylonComp.Network.focusTotal >= storageComp.Props.minimumFocusThreshold;
        public CompPsychicStorage storageComp;

        public CompPsychicPylon pylonComp;

        public ThingOwner innerContainer;

        public readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment");

        private string arrivalMessageKey = "MessageTransportPodsArrived";

        public List<CompTeleporter> Teleporters = new List<CompTeleporter>();

        private static List<Pawn> pawns = new List<Pawn>();

        private static List<Thing> things = new List<Thing>();

        private Dictionary<Thing, Vector3> effectPositions = new Dictionary<Thing, Vector3>();

        private PlanetTile cachedClosest;

        private PlanetTile cachedOrigin;

        private PlanetLayer cachedLayer;

        private bool targeting = false;

        private ThingActiveTeleporter activeTeleporter;

        private int ticksUntilArrived = -1;

        //private Mote mote;

        //private Sustainer sound;

        private bool siteHasMap = false;

        public CompTeleporter()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            storageComp = parent.TryGetComp<CompPsychicStorage>();
            pylonComp = parent.TryGetComp<CompPsychicPylon>();
        }

        public override void PostPostMake()
        {
            Teleporters.Add(this);
        }

        public override void CompTick()
        {
            if(targeting && !Find.Targeter.IsTargeting && !Find.WorldTargeter.IsTargeting)
            {
                DropThings();
                targeting = false;
            }
 
            if(ticksUntilArrived == 60)
            {
                /*FleckMaker.Static(parent.Position, parent.Map, FleckDefOf.PsycastSkipFlashEntry, Props.radius);
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
                
                sound?.End();
                mote?.DeSpawn();*/

                if(activeTeleporter != null)
                {
                    //activeTeleporter.Destroy();
                    activeTeleporter = null;
                }

                ticksUntilArrived = -1;
            }
            /*else if(ticksUntilArrived == 0)
            {
                if (activeTeleporter.arrivalAction != null && siteHasMap)
                {
                    LongEventHandler.QueueLongEvent(DoArrivalAction, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
                }
                else
                {
                    DoArrivalAction();
                }
                //ticksUntilArrived = -1;
            }*/
            
            if(ticksUntilArrived != 60)
            {
                /*mote?.Maintain();
                if(sound != null && !sound.Ended)
                {
                    sound.Maintain();
                }*/
                ticksUntilArrived--;
            }
            /*else
            {
                if(activeTeleporter != null)
                {
                    //activeTeleporter.Destroy();
                    activeTeleporter = null;
                }
            }*/
        }

        public void DoArrivalAction()
        {
            //activeTeleporter.Arrived();
            siteHasMap = false;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if(activeTeleporter != null)
            {
                for (int i = 0; i < effectPositions.Count(); i++)
                {
                    activeTeleporter.Contents.innerContainer[i].DrawNowAt(effectPositions[activeTeleporter.Contents.innerContainer[i]]);
                }
            }
            else if(!innerContainer.NullOrEmpty())
            {
                for (int i = 0; i < effectPositions.Count(); i++)
                {
                    innerContainer[i].DrawNowAt(effectPositions[innerContainer[i]]);
                }
            }
        }

        public void Teleport(PlanetTile destination, TransportersArrivalAction arrivalAction)
        {
            float remaining = storageComp.DrainFocus(storageComp.Props.minimumFocusThreshold);

            pylonComp.TryDrawFocus(remaining);

            if(arrivalAction != null)
            {
                ticksUntilArrived = Props.ticksUntilTeleport+60;
            }

            CameraJumper.TryHideWorld();

            activeTeleporter = (ThingActiveTeleporter)GenSpawn.Spawn(ThingMaker.MakeThing(AT_DefOf.AT_ActiveTeleporter), parent.Position, parent.Map);

            activeTeleporter.Contents = new ActiveTransporterInfo();
            innerContainer.TryTransferAllToContainer(activeTeleporter.Contents.innerContainer, false);
            activeTeleporter.arrivalAction = arrivalAction;
            activeTeleporter.destination = destination;
            activeTeleporter.siteHasMap = siteHasMap;
            activeTeleporter.radius = (int)Props.radius;
            activeTeleporter.Begin(Props.ticksUntilTeleport+60);

            /*mote = MoteMaker.MakeStaticMote(parent.Position, parent.Map, ThingDefOf.Mote_Bestow, Props.radius/3);
            mote?.Maintain();

            sound = AT_DefOf.PsycastCastLoop.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(parent.Position, parent.Map), MaintenanceType.PerTick));
            sound?.Maintain();*/
        }

        public void GatherThings()
        {
            bool homeMap = parent.Map.IsPlayerHome;

            effectPositions.Clear();

            foreach (Thing item in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, Props.radius, useCenter: true))
            {
                if (item.def.EverHaulable || (item is Pawn pawn && !pawn.IsQuestLodger() && (pawn.IsColonist || pawn.IsPrisonerOfColony || (!homeMap && pawn.IsAnimal && pawn.Faction != null && pawn.Faction.IsPlayer) || (pawn.IsMutant && pawn.Faction != null && pawn.Faction.IsPlayer))))
                {
                    effectPositions.Add(item, item.DrawPos);
                    innerContainer.TryAdd(item.SplitOff(item.stackCount), false);
                    things.Add(item);
                    if(item is Pawn pawn2)
                    {
                        pawns.Add(pawn2);
                    }
                    continue;
                }
            }
        }

        public void DropThings()
        {
            if(!innerContainer.NullOrEmpty())
            {
                int count = effectPositions.Count;
                for(int i = count; i > 0; i--)
                {
                    Thing thing = innerContainer.Last();
                    innerContainer.TryDrop(thing, effectPositions[thing].ToIntVec3(), parent.Map, ThingPlaceMode.Near, thing.stackCount, out var _);
                    effectPositions.Remove(thing);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_UseTeleporter(this)
            {
                defaultLabel = "Select Destination".Translate(),
                action = delegate
                {
                    StartChoosingDestination(Teleport);
                }
            };
        }

        public void StartChoosingDestination(Action<PlanetTile, TransportersArrivalAction> launchAction)
        {
            PlanetTile tile = parent.Tile;
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            GatherThings();

            Find.WorldTargeter.BeginTargeting((GlobalTargetInfo t) => ChoseWorldTarget(t, launchAction), canTargetTiles: true, TargeterMouseAttachment, false, delegate
            {
                PlanetTile planetTile;
                if (cachedLayer != Find.WorldSelector.SelectedLayer || cachedOrigin != tile)
                {
                    cachedLayer = Find.WorldSelector.SelectedLayer;
                    cachedOrigin = tile;
                    planetTile = cachedClosest = Find.WorldSelector.SelectedLayer.GetClosestTile_NewTemp(tile);
                }
                else
                {
                    planetTile = cachedClosest;
                }
                int num = Props.range;
                GenDraw.DrawWorldRadiusRing(planetTile, num, CompPilotConsole.GetThrusterRadiusMat(planetTile));
                targeting = true;
            }, (GlobalTargetInfo target) => TargetingLabelGetter(target, tile, Props.range, Teleporters, launchAction));
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target, Action<PlanetTile, TransportersArrivalAction> launchAction)
        {
            cachedClosest = cachedOrigin = PlanetTile.Invalid;
            cachedLayer = null;
            PlanetTile tile = parent.Tile;
            return ChoseWorldTarget(target, tile, Teleporters, Props.range, launchAction, this);
        }

        public bool ChoseWorldTarget(GlobalTargetInfo target, PlanetTile tile, IEnumerable<IThingHolder> teleporters, int maxLaunchDistance, Action<PlanetTile, TransportersArrivalAction> launchAction, CompTeleporter teleporter, float? overrideFuelLevel = null)
        {
            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            if (target.HasWorldObject && !target.WorldObject.def.validLaunchTarget)
            {
                Messages.Message("MessageWorldObjectIsInvalid".Translate(target.WorldObject.Named("OBJECT")), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
            if (maxLaunchDistance > 0 && num > maxLaunchDistance)
            {
                Messages.Message("TransportPodDestinationBeyondMaximumProps.range".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            IEnumerable<FloatMenuOption> source = GetOptionsForTile(target, teleporters, launchAction);
            if (!source.Any())
            {
                if (Find.World.Impassable(target.Tile))
                {
                    Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    return false;
                }
                launchAction(target.Tile, null);
                return true;
            }
            if (source.Count() == 1)
            {
                if (!source.First().Disabled)
                {
                    source.First().action();
                    return true;
                }
                return false;
            }
            Find.WindowStack.Add(new FloatMenu(source.ToList()));
            return false;
        }

        public IEnumerable<FloatMenuOption> GetOptionsForTile(GlobalTargetInfo target, IEnumerable<IThingHolder> teleporters, Action<PlanetTile, TransportersArrivalAction> launchAction)
        {
            bool anything = false;
            //ModLog.Log("Can form caravan: "+TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(teleporters, target.Tile));
            if (TransportersArrivalAction_FormCaravan.CanFormCaravanAt(teleporters, target.Tile) && !Find.WorldObjects.AnySettlementBaseAt(target.Tile) && !Find.WorldObjects.AnySiteAt(target.Tile))
            {
                anything = true;
                string stringCaravanTooHeavy = string.Empty;

                if(CollectionsMassCalculator.Capacity(pawns) < CollectionsMassCalculator.MassUsage(things, IgnorePawnsInventoryMode.DontIgnore))
                {
                    stringCaravanTooHeavy = " (Caravan would be too heavy to move)";
                }

                yield return new FloatMenuOption("FormCaravanHere".Translate()+stringCaravanTooHeavy, delegate
                {
                    launchAction(target.Tile, new TransportersArrivalAction_FormCaravan("MessageShuttleArrived"));
                });
            }
            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < worldObjects.Count; i++)
            {
                if (worldObjects[i].Tile != target.Tile)
                {
                    continue;
                }

                if (worldObjects[i].GetType() == typeof(Site))
                {
                    Site site = worldObjects[i] as Site;
                    foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitSiteTeleport.GetFloatMenuOptions(launchAction, teleporters, site))
                    {
                        anything = true;

                        if (!site.HasMap)
                        {
                            siteHasMap = true;
                        }

                        yield return floatMenuOption;
                    }
                    
                }
                else if(worldObjects[i].GetType() == typeof(Settlement))
                {
                    Settlement settlement = worldObjects[i] as Settlement;

                    foreach(FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitSettlement.GetFloatMenuOptions(launchAction, teleporters, settlement, false))
                    {
                        anything = true;

                        if (!settlement.HasMap)
                        {
                            siteHasMap = true;
                        }

                        yield return floatMenuOption;
                    }

                    foreach(FloatMenuOption floatMenuOption in TransportersArrivalAction_GiveGift.GetFloatMenuOptions(launchAction, teleporters, settlement))
                    {
                        anything = true;
                        yield return floatMenuOption;
                    }

                    foreach(FloatMenuOption floatMenuOption in TransportersArrivalAction_AttackSettlementTeleport.GetFloatMenuOptions(launchAction, teleporters, settlement))
                    {
                        anything = true;

                        if (!settlement.HasMap)
                        {
                            siteHasMap = true;
                        }

                        yield return floatMenuOption;
                    }
                }
                else if(worldObjects[i].GetType() == typeof(Caravan))
                {
                    Caravan caravan = worldObjects[i] as Caravan;
                    foreach(FloatMenuOption floatMenuOption in TransportersArrivalAction_GiveToCaravan.GetFloatMenuOptions(launchAction, teleporters, caravan))
                    {
                        anything = true;
                        yield return floatMenuOption;
                    }
                }

                if (worldObjects[i] is MapParent mapParent)
                {
                    if (!TransportersArrivalAction_LandInSpecificCell.CanLandInSpecificCell(teleporters, mapParent))
                    {
                        yield break;
                    }
                    anything = true;
                    yield return new FloatMenuOption("LandInExistingMap".Translate(mapParent.Label), delegate
                    {
                        Map map = mapParent.Map;
                        Current.Game.CurrentMap = map;
                        CameraJumper.TryHideWorld();
                        Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate (LocalTargetInfo x)
                        {
                            launchAction(mapParent.Tile, new TransportersArrivalAction_TeleportToSpecificCell(mapParent, x.Cell, Rot4.North, landInShuttle: false));
                        }, null, null, CompLaunchable.TargeterMouseAttachment);
                    });
                }
            }
            if (!anything && !Find.World.Impassable(target.Tile))
            {
                yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate
                {
                    launchAction(target.Tile, null);
                });
            }
        }

        public string TargetingLabelGetter(GlobalTargetInfo target, PlanetTile tile, int maxLaunchDistance, IEnumerable<IThingHolder> teleporters, Action<PlanetTile, TransportersArrivalAction> launchAction)
        {
            if (!target.IsValid)
            {
                return null;
            }
            if (target.Tile.Layer != tile.Layer && !tile.Layer.HasConnectionPathTo(target.Tile.Layer))
            {
                GUI.color = ColorLibrary.RedReadable;
                return "TransportPodDestinationNoPath".Translate(target.Tile.Layer.Def.Named("LAYER"));
            }
            int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
            if (maxLaunchDistance > 0 && num > maxLaunchDistance)
            {
                GUI.color = ColorLibrary.RedReadable;
                return "TransportPodDestinationBeyondMaximumProps.range".Translate();
            }
            if (target.HasWorldObject && !target.WorldObject.def.validLaunchTarget)
            {
                return string.Empty;
            }
            IEnumerable<FloatMenuOption> source = GetOptionsForTile(target, teleporters, launchAction);
            if (!source.Any())
            {
                return string.Empty;
            }
            if (source.Count() == 1)
            {
                if (source.First().Disabled)
                {
                    GUI.color = ColorLibrary.RedReadable;
                }
                return source.First().Label;
            }
            if (target.WorldObject is MapParent mapParent)
            {
                return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
            }
            return "ClickToSeeAvailableOrders_Empty".Translate();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (Props.radius > 0)
            {
                GenDraw.DrawRadiusRing(parent.Position, Props.radius, Color.white);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilArrived, "ticksUntilArrived", -1);
            Scribe_Deep.Look(ref activeTeleporter, "activeTeleporter");
            //Scribe_References.Look(ref mote, "more");
            //Scribe_Values.Look(ref sound, "sound");
        }
    }
}