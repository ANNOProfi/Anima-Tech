using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using Verse.Sound;
using System.Linq;

namespace AnimaTech
{
    public class ThingActiveTeleporter : Thing, IThingHolder
    {
        private ActiveTransporterInfo contents;

        public ActiveTransporterInfo Contents
        {
            get
            {
                return contents;
            }
            set
            {
                if (contents != null)
                {
                    contents.parent = null;
                }
                if (value != null)
                {
                    value.parent = this;
                }
                contents = value;
            }
        }

        public TransportersArrivalAction arrivalAction;

        public PlanetTile destination;

        public int ticksUntilCompletion = -10;

        public bool arriving = false;

        private Mote mote;

        private Sustainer sound;

        public bool siteHasMap;

        public int radius;

        protected override void Tick()
        {
            if(!arriving && ticksUntilCompletion > 60 && mote == null)
            {
                mote = MoteMaker.MakeStaticMote(Position, Map, ThingDefOf.Mote_Bestow, radius/3);
                mote?.Maintain();
            }
            if(arriving && ticksUntilCompletion % 60 == 0 && ticksUntilCompletion > 0)
            {
                FleckMaker.Static(Position, Map, FleckDefOf.PsycastSkipOuterRingExit, Mathf.Clamp(radius/(ticksUntilCompletion/60), 1, radius));
                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(Position, Map));
            }
            if(ticksUntilCompletion == 60)
            {
                if(!arriving)
                {
                    FleckMaker.Static(Position, Map, FleckDefOf.PsycastSkipFlashEntry, radius);
                    SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(Position, Map));
                }
                else
                {
                    FleckMaker.Static(Position, Map, FleckDefOf.PsycastSkipInnerExit, radius);
                    //FleckMaker.Static(Position, Map, FleckDefOf.PsycastSkipOuterRingExit, radius);
                    //SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(Position, Map));
                }

                if(arriving)
                { 
                    Arrival();
                }
                
                sound?.End();
                mote?.DeSpawn();
            }
            else if(ticksUntilCompletion == 0 && !arriving)
            {
                if (arrivalAction != null && siteHasMap)
                {
                    LongEventHandler.QueueLongEvent(Arrived, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
                }
                else
                {
                    Arrived();
                }
            }
            else if(ticksUntilCompletion == -10 && arriving)
            {
                if(Spawned)
                {
                    Destroy();
                }
            }
            
            if(ticksUntilCompletion != -10)
            {
                mote?.Maintain();
                if(sound != null && !sound.Ended)
                {
                    sound.Maintain();
                }
                ticksUntilCompletion--;
            }
            else
            {
                if(Spawned)
                {
                    Destroy();
                }
            }
        }

        public void Begin(int ticks)
        {
            ticksUntilCompletion = ticks;

            if(!arriving)
            {
                mote = MoteMaker.MakeStaticMote(Position, Map, ThingDefOf.Mote_Bestow, radius/3);
                mote?.Maintain();

                sound = AT_DefOf.PsycastCastLoop.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(Position, Map), MaintenanceType.PerTick));
                sound?.Maintain();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref contents, "contents", this);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return null;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
            if (contents != null)
            {
                outChildren.Add(contents);
            }
        }

        public void Arrived()
        {
            if(arrivalAction != null)
            {
                foreach(Thing thing in Contents.innerContainer)
                {
                    if(thing is Pawn pawn)
                    {
                        Find.WorldPawns.PassToWorld(pawn);
                    }
                }
                arrivalAction.Arrived(new List<ActiveTransporterInfo>{Contents}, destination);
                
                if(Spawned)
                {
                    Destroy();
                }
            }
        }

        private void Arrival()
        {
            ModLog.Log("Doing Arrival");
            List<Pawn> pawns = new List<Pawn>();

            for (int j = Contents.innerContainer.Count; j > 0; j--)
            {
                Thing thing = Contents.innerContainer.Last();
                
                IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(Position, Map, radius);

                Contents.innerContainer.TryDrop(thing, loc2, Map, ThingPlaceMode.Direct, thing.stackCount, out var _);

                /*FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(thing, FleckDefOf.PsycastSkipInnerExit, Vector3.zero);
                dataAttachedOverlay.link.detachAfterTicks = 5;
                map.flecks.CreateFleck(dataAttachedOverlay);

                dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(thing, FleckDefOf.PsycastSkipOuterRingExit, Vector3.zero);
                dataAttachedOverlay.link.detachAfterTicks = 5;
                map.flecks.CreateFleck(dataAttachedOverlay);

                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(loc2, thing.Map));*/

                if (thing is Pawn pawn)
                {
                    if ((pawn.IsColonist || pawn.RaceProps.packAnimal || pawn.IsColonyMech) && pawn.Map.IsPlayerHome)
                    {
                        pawn.inventory.UnloadEverything = true;
                    }
                }
            }
        }
    }
}