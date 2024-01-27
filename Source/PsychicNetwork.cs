using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class PsychicNetwork
    {
        private const int MAX_PUSH_ATTEMPTS = 1000;

        public int networkId;

        public PsychicMapComponent mapComponent;

        public HashSet<ThingWithComps> networkedThings = new HashSet<ThingWithComps>();

        public HashSet<IntVec3> cells = new HashSet<IntVec3>();

        public float focusCapacity;

        public float focusTotal;

        public float generationTotal;

        public float consumptionTotal;

        public int ticksUntilNextReport;

        public List<CompPsychicPylon> pylons = new List<CompPsychicPylon>();

        public List<CompPsychicGenerator> generators = new List<CompPsychicGenerator>();

        public List<CompPsychicStorage> storages = new List<CompPsychicStorage>();

        public List<CompPsychicUser> users = new List<CompPsychicUser>();

        public List<CompPsychicGenerator> shuffledGenerators = new List<CompPsychicGenerator>();

        public List<CompPsychicStorage> shuffledStorages = new List<CompPsychicStorage>();

        public void Clear()
        {
            networkedThings.Clear();
            cells.Clear();
            pylons.Clear();
            generators.Clear();
            storages.Clear();
            //edges.Clear();
        }

        protected IEnumerable<IntVec3> InitThing(ThingWithComps thing)
        {
            IEnumerable<IntVec3> result = null;
            foreach (ThingComp allComp in thing.AllComps)
            {
                if (allComp is CompPsychicPylon compPsychicPylon)
                {
                    pylons.Add(compPsychicPylon);
                    compPsychicPylon.networkRef = this;
                    result = PopulateAffectedCells(compPsychicPylon);
                }
                else if (allComp is CompPsychicGenerator item)
                {
                    generators.Add(item);
                }
                else if (allComp is CompPsychicStorage compPsychicStorage && compPsychicStorage.Props.canBeTransmitted)
                {
                    storages.Add(compPsychicStorage);
                }
                else if (allComp is CompPsychicUser compPsychicUser && compPsychicUser.Props.canUsePsychicPylon)
                {
                    users.Add(compPsychicUser);
                }
            }
            return result;
        }

        public IEnumerable<IntVec3> PopulateAffectedCells(CompPsychicPylon pylon)
        {
            if (pylon == null)
            {
                yield break;
            }
            ThingWithComps parent = pylon.parent;
            if (pylon.PylonRadius > 0 && !parent.Destroyed && parent.Spawned)
            {
                foreach (IntVec3 affectedCell in pylon.GetAffectedCells(forceUpdate: true))
                {
                    cells.Add(affectedCell);
                    /*if (mapComponent.pylonsByLocation.TryGetValue(affectedCell, out var value) && value != pylon && value.ShouldFormLinks)
                    {
                        PsychicPylonEdge item = new PsychicPylonEdge(pylon, value);
                        edges.Add(item);
                        pylon.edges.Add(item);
                        value.edges.Add(item);
                    }*/
                    yield return affectedCell;
                }
            }
            /*foreach (CompPsychicPylon pylon2 in pylons)
            {
                if (pylon != pylon2 && pylon2.ShouldFormLinks && pylon2.PylonRadius > pylon.PylonRadius && pylon2.GetAffectedCells().Contains(parent.Position))
                {
                    PsychicPylonEdge item = new PsychicPylonEdge(pylon, pylon2);
                    edges.Add(item);
                    pylon.edges.Add(item);
                    pylon2.edges.Add(item);
                }
            }*/
        }

        public bool Tick()
        {
            bool result = false;
            if (ticksUntilNextReport < 1)
            {
                focusCapacity = 0f;
                focusTotal = 0f;
                generationTotal = 0f;
                consumptionTotal = 0f;
                foreach (CompPsychicStorage capacitor in storages)
                {
                    focusCapacity += capacitor.Props.focusCapacity;
                    focusTotal += capacitor.focusStored;
                }
                foreach (CompPsychicGenerator generator in generators)
                {
                    generationTotal += generator.reportedFocusGeneration;
                }
                foreach (CompPsychicUser user in users)
                {
                    consumptionTotal += user.FocusConsumptionPerDay;
                }
                ticksUntilNextReport = 60;
            }
            else
            {
                ticksUntilNextReport--;
            }
            //effectTracker.Tick();
            return result;
        }

        public IEnumerable<IntVec3> AddThing(ThingWithComps thing)
        {
            if (networkedThings.Contains(thing))
            {
                Log.Error($"AT: Attempted to add {thing.def.LabelCap} at {thing.Position} to network {networkId}, but it was already there.");
                return null;
            }
            networkedThings.Add(thing);
            thing.Map.mapDrawer.MapMeshDirty(thing.PositionHeld, MapMeshFlag.Buildings, regenAdjacentCells: true, regenAdjacentSections: true);
            return InitThing(thing);
        }

        public void RemoveThing(ThingWithComps thing)
        {
            if (networkedThings.Contains(thing))
            {
                networkedThings.Remove(thing);
                thing.GetComp<CompPsychicPylon>().networkRef = null;
                mapComponent.dirty = true;
            }
            else
            {
                Log.Error($"AT: Attempted to remove {thing.def.LabelCap} at {thing.Position} from network {networkId}, but it wasn't there to begin with.");
            }
        }

        public bool IsFull()
        {
            return focusTotal == focusCapacity;
        }

        public bool IsEmpty()
        {
            return focusTotal == 0f;
        }

        public float AmountToFill()
        {
            return focusCapacity - focusTotal;
        }

        public bool HasFocus(float amount)
        {
            return focusTotal >= amount;
        }

        public float PullFocus(float amount)
        {
            if (amount <= 0f || !storages.Any())
            {
                return amount;
            }
            if (focusTotal < amount)
            {
                return amount;
            }
            float num = amount;
            shuffledStorages.Clear();
            shuffledStorages.AddRange(storages);
            shuffledStorages.Shuffle();
            foreach (CompPsychicStorage shuffledCapacitor in shuffledStorages)
            {
                float num2 = Math.Min(shuffledCapacitor.focusStored, num);
                shuffledCapacitor.DrainFocus(num2);
                if (float.IsNaN(shuffledCapacitor.focusStored))
                {
                    shuffledCapacitor.focusStored = 0f;
                    Log.Error("AT: NAN generated when attempting to draw aether from capacitor");
                }
                num -= num2;
                if (num <= 0f)
                {
                    break;
                }
            }
            shuffledStorages.Clear();
            if (num < amount)
            {
                focusTotal -= amount - num;
            }
            return num;
        }

        public float PushFocus(float amount)
        {
            List<CompPsychicStorage> list = storages.Where((CompPsychicStorage cap) => cap.AcceptsTransmittedFocus && cap.FocusSpace > 0f).ToList();
            if (list.NullOrEmpty())
            {
                return amount;
            }
            float num = amount;
            int num2 = 0;
            while (num > 0f)
            {
                num2++;
                list.RemoveAll((CompPsychicStorage tank) => tank.FocusSpace <= 0f);
                if (list.NullOrEmpty())
                {
                    return num;
                }
                float val = num / (float)list.Count;
                foreach (CompPsychicStorage item in list)
                {
                    float num3 = Math.Min(val, item.FocusSpace);
                    item.StoreFocus(num3);
                    if (float.IsNaN(item.focusStored))
                    {
                        item.focusStored = 0f;
                        Log.Error("AT: NaN generated while attempting to store aether to capacitors");
                    }
                    num -= num3;
                }
                if (num2 > 1000)
                {
                    Log.Warning("AT: Aborting aether capacitor storage attempt due to too many attempts");
                    return num;
                }
            }
            if (num < amount)
            {
                focusTotal += amount - num;
            }
            return num;
        }

        public override bool Equals(object obj)
        {
            if (obj is PsychicNetwork psychicNetwork)
            {
                return networkId == psychicNetwork.networkId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return networkId;
        }
    }
}