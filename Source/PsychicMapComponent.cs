using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AnimaTech
{
    public class PsychicMapComponent : MapComponent
    {
        public static PsychicMapComponent localCachedComponent = null;

        public static Dictionary<int, PsychicMapComponent> components = new Dictionary<int, PsychicMapComponent>();

        private static IComparer<CompPsychicPylon> pylonsByPylonRadius = new ByPylonRadius();

        public List<PsychicNetwork> psychicNetworks = new List<PsychicNetwork>();

        public List<CompPsychicPylon> pylonCache = new List<CompPsychicPylon>();

        public List<CompPsychicStorage> storageCache = new List<CompPsychicStorage>();

        public List<ThingWithComps> imbuementCache = new List<ThingWithComps>();

        public List<ThingWithComps> meditationCache = new List<ThingWithComps>();

        public Dictionary<IntVec3, CompPsychicPylon> pylonsByLocation = new Dictionary<IntVec3, CompPsychicPylon>();

        public HashSet<PsychicNetwork>[] networkGrid;

        public CellIndices cellIndices;

        public int nextId = 1;

        public bool dirty = true;

        public PsychicMapComponent(Map map)
		: base(map)
        {
            components[base.map.uniqueID] = this;
            cellIndices = map.cellIndices;
            networkGrid = new HashSet<PsychicNetwork>[cellIndices.NumGridCells];
            localCachedComponent = null;
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
            RegenGrid(forceReset: true);
        }

        public override void MapRemoved()
        {
            base.MapRemoved();
            components.Remove(map.uniqueID);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RegenGrid();
        }

        public override void MapComponentUpdate()
        {
            if (dirty)
            {
                RegenGrid();
            }
        }

        public override void MapComponentTick()
        {
            if (dirty)
            {
                RegenGrid();
            }
            foreach (PsychicNetwork psychicNetwork in psychicNetworks)
            {
                if (psychicNetwork.Tick())
                {
                    UpdateCellsFromNetwork(psychicNetwork);
                }
            }
        }

        public void RegenGrid(bool forceReset = false)
        {
            ResetGrid(forceReset);
            InitGrid();
        }

        private void ResetGrid(bool forceReset = false)
        {
            psychicNetworks.Clear();
            for (int i = 0; i < networkGrid.Length; i++)
            {
                networkGrid[i] = null;
            }
            pylonsByLocation.Clear();
            //edges.Clear();
            if (forceReset)
            {
                pylonCache.Clear();
            }
        }

        private void InitGrid()
        {
            List<CompPsychicPylon> list = pylonCache.ToList();
            list.Sort(pylonsByPylonRadius);
            foreach (CompPsychicPylon item in list)
            {
                InitLink(item);
            }
            dirty = false;
        }

        private void InitLink(CompPsychicPylon link, bool onlyEnable = false)
        {
            ThingWithComps parent = link.parent;
            if (pylonsByLocation.ContainsKey(parent.Position))
            {
                if (!onlyEnable)
                {
                    Log.Error($"Link overlap detected at {parent.Position}.");
                    return;
                }
            }
            else
            {
                pylonsByLocation[parent.Position] = link;
            }
            //link.edges.Clear();
            if (!link.ShouldFormLinks)
            {
                return;
            }
            PsychicNetwork psychicNetwork = null;
            IEnumerable<IntVec3> enumerable = null;
            foreach (IntVec3 item in parent.OccupiedRect())
            {
                HashSet<PsychicNetwork> hashSet = networkGrid[cellIndices.CellToIndex(item)];
                if (hashSet == null)
                {
                    continue;
                }
                foreach (PsychicNetwork item2 in hashSet.ToList())
                {
                    if (psychicNetwork == null)
                    {
                        psychicNetwork = item2;
                        enumerable = psychicNetwork.AddThing(parent);
                    }
                    else if (psychicNetwork.networkId != item2.networkId)
                    {
                        MergeNetworks(psychicNetwork, item2);
                    }
                }
            }
            if (psychicNetwork == null)
            {
                psychicNetwork = CreateNewNetwork();
                enumerable = psychicNetwork.AddThing(parent);
            }
            JoinToLinksInRange(link, psychicNetwork, enumerable);
            if (enumerable != null)
            {
                UpdateCellsFromNetwork(psychicNetwork, enumerable);
            }
            //edges.UnionWith(psychicNetwork.edges);
        }

        protected void JoinToLinksInRange(CompPsychicPylon link, PsychicNetwork parentNetwork = null, IEnumerable<IntVec3> affectedCells = null)
        {
            if (parentNetwork == null)
            {
                if (link.networkRef == null)
                {
                    return;
                }
                parentNetwork = link.networkRef;
            }
            if (affectedCells == null)
            {
                affectedCells = link.GetAffectedCells();
            }
            foreach (IntVec3 affectedCell in affectedCells)
            {
                if (pylonsByLocation.TryGetValue(affectedCell, out var value) && value != link && value.ShouldFormLinks && !parentNetwork.Equals(value.networkRef))
                {
                    MergeNetworks(parentNetwork, value.networkRef);
                }
            }
        }

        private void RemoveLink(CompPsychicPylon link, bool onlyDisable = false)
        {
            ThingWithComps parent = link.parent;
            link.networkRef?.RemoveThing(parent);
            if (!onlyDisable && pylonsByLocation[parent.Position] == link)
            {
                pylonsByLocation.Remove(parent.Position);
            }
            dirty = true;
        }

        private PsychicNetwork CreateNewNetwork()
        {
            PsychicNetwork psychicNetwork = new PsychicNetwork
            {
                mapComponent = this,
                networkId = nextId++
            };
            psychicNetworks.Add(psychicNetwork);
            return psychicNetwork;
        }

        private void UpdateCellsFromNetwork(PsychicNetwork network, IEnumerable<IntVec3> affectedCells = null)
        {
            if (network == null)
            {
                return;
            }
            if (affectedCells == null)
            {
                affectedCells = network.cells;
            }
            foreach (IntVec3 affectedCell in affectedCells)
            {
                int num = cellIndices.CellToIndex(affectedCell);
                if (affectedCell.InBounds(map))
                {
                    HashSet<PsychicNetwork> hashSet = networkGrid[num];
                    if (hashSet == null)
                    {
                        hashSet = (networkGrid[num] = new HashSet<PsychicNetwork>());
                    }
                    hashSet.Add(network);
                }
            }
            //edges.UnionWith(network.edges);
        }

        public void RegisterPylon(CompPsychicPylon pylon, bool onlyEnable = false)
        {
            if (pylonCache.Contains(pylon) && !onlyEnable)
            {
                Log.Error($"AT: Attempted to register a new psychic pylon at {pylon.parent.Position}, but it was already in the cache.");
                return;
            }
            if (!pylonCache.Contains(pylon))
            {
                pylonCache.Add(pylon);
            }
            InitLink(pylon, onlyEnable);
        }

        public void DeregisterPylon(CompPsychicPylon pylon, bool onlyDisable = false)
        {
            if (!pylonCache.Contains(pylon))
            {
                Log.Error($"AT: Attempted to deregister a psychic pylon at {pylon.parent.Position}, but it wasn't in the cache.");
                return;
            }
            RemoveLink(pylon, onlyDisable);
            if (!onlyDisable && pylonCache.Contains(pylon))
            {
                pylonCache.Remove(pylon);
            }
        }

        public void RegisterStorage(CompPsychicStorage storage)
        {
            if (storageCache.Contains(storage))
            {
                Log.Error($"AT: Attempted to register a new psychic storage at {storage.parent.Position}, but it was already in the cache.");
            }
            else
            {
                storageCache.Add(storage);
            }
        }

        public void DeregisterStorage(CompPsychicStorage storage)
        {
            if (storageCache.Contains(storage))
            {
                storageCache.Remove(storage);
            }
            else
            {
                Log.Error($"AT: Attempted to deregister a psychic storage at {storage.parent.Position}, but it wasn't in the cache.");
            }
        }

        public void RegisterGenerator(ThingWithComps generator)
        {
            if(imbuementCache.Contains(generator) || meditationCache.Contains(generator))
            {
                Log.Error($"AT: Attempted to register a new psychic generator at {generator.Position}, but it was already in the cache.");
            }
            else
            {
                if(generator.TryGetComp<CompPsychicGenerator>().AllowImbuement)
                {
                    imbuementCache.Add(generator);
                }

                if(generator.TryGetComp<CompPsychicGenerator>().AllowMeditation)
                {
                    meditationCache.Add(generator);
                }
            }
        }

        public void DeregisterGenerator(ThingWithComps generator)
        {
            if(generator.TryGetComp<CompPsychicGenerator>().AllowImbuement)
            {
                if (imbuementCache.Contains(generator))
                {
                    imbuementCache.Remove(generator);
                }
                else
                {
                    Log.Error($"AT: Attempted to deregister a psychic imbuement generator at {generator.Position}, but it wasn't in the cache.");
                }
            }

            if(generator.TryGetComp<CompPsychicGenerator>().AllowMeditation)
            {
                if (meditationCache.Contains(generator))
                {
                    meditationCache.Remove(generator);
                }
                else
                {
                    Log.Error($"AT: Attempted to deregister a psychic meditation generator at {generator.Position}, but it wasn't in the cache.");
                }
            }
        }

        protected void MergeNetworks(PsychicNetwork network, PsychicNetwork other)
        {
            if (network == null || other == null || network.Equals(other))
            {
                return;
            }
            HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
            foreach (ThingWithComps networkedThing in other.networkedThings)
            {
                hashSet.UnionWith(network.AddThing(networkedThing));
            }
            foreach (IntVec3 item in hashSet)
            {
                int num = cellIndices.CellToIndex(item);
                networkGrid[num].Add(network);
                networkGrid[num].Remove(other);
            }
            psychicNetworks.Remove(other);
        }

        public bool NetworkPresentAt(IntVec3 location)
        {
            int num = cellIndices.CellToIndex(location);
            if (networkGrid[num] != null)
            {
                return networkGrid[num].Count > 0;
            }
            return false;
        }
    }
}
