using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AnimaTech
{
    public class Building_PsychicTradeBeacon : Building_OrbitalTradeBeacon
    {
        public new static IEnumerable<Building_OrbitalTradeBeacon> AllPowered(Map map)
        {
            foreach(Building_OrbitalTradeBeacon item in map.listerBuildings.AllBuildingsColonistOfClass<Building_OrbitalTradeBeacon>())
            {
                CompPsychicUser userComp = item.GetComp<CompPsychicUser>();
                if (userComp == null || userComp.IsActive)
                {
                    yield return item;
                }
            }
        }
    }
}