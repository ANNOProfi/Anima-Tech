using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimaTech
{
    [HarmonyPatch(typeof(MeditationUtility))]
    [HarmonyPatch("FindMeditationSpot")]
    public class MeditationUtility_GetMeditationJobPatch
    {
        public static void Postfix(ref MeditationSpotAndFocus __result, Pawn pawn)
        {
            float num = float.MaxValue;
            LocalTargetInfo spot = __result.spot;
            LocalTargetInfo focus = __result.focus;

            PsychicMapComponent mapComp = pawn.Map.PsychicComp();

            if(mapComp.meditationCache.NullOrEmpty() || !pawn.HasPsylink)
            {
                return;
            }

            foreach (Building item in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MeditationSpot))
			{
				if (item.DestroyedOrNull() || item.IsForbidden(pawn) || !item.Position.Standable(item.Map))
                {
                    continue;
                }
                if (item.GetAssignedPawn() == null || item.GetAssignedPawn() != pawn)
                {
                    continue;
                }
                if(!pawn.CanReserveAndReach(item, PathEndMode.OnCell, pawn.NormalMaxDanger()))
                {
                    continue;
                }

                spot = item;

                foreach(Thing thing in GenRadial.RadialDistinctThingsAround(item.TrueCenter().ToIntVec3(), item.Map, 3.9f, useCenter: false))
                {
                    float num2 = float.MaxValue;
                    if(!mapComp.meditationCache.Contains(thing))
                    {
                        continue;
                    }

                    CompPsychicStorage storageComp = thing.TryGetComp<CompPsychicStorage>();
                    if(thing != null && (!thing.TryGetComp<CompPsychicGenerator>().canMeditate || !thing.TryGetComp<CompAssignableToPawn_PsychicStorage>().AssignedPawnsForReading.Contains(pawn) || (storageComp != null && !storageComp.CanBeFilled)))
                    {
                        continue;
                    }

                    if(storageComp != null && num2 > storageComp.AmountToEmpty && storageComp.FocusCapacity > 0)
                    {
                        num2 = storageComp.focusStored;
                    }

                    CompPsychicPylon pylonComp = thing.TryGetComp<CompPsychicPylon>();

                    if(pylonComp != null && pylonComp.Network != null && num2 > pylonComp.Network.focusTotal)
                    {
                        num2 = pylonComp.Network.focusTotal;
                    }

                    if(num2 < num)
                    {
                        focus = thing;
                        num = num2;
                    }
                }
			}

            if(__result.focus != focus)
            {
                return;
            }

            foreach(LocalTargetInfo item in mapComp.meditationCache)
            {
                float num2 = float.MaxValue;

                CompPsychicStorage storageComp = item.Thing.TryGetComp<CompPsychicStorage>();

                if(item.Thing != null && (!item.Thing.TryGetComp<CompPsychicGenerator>().canMeditate || !item.Thing.TryGetComp<CompAssignableToPawn_PsychicStorage>().AssignedPawnsForReading.Contains(pawn) || (storageComp != null && !storageComp.CanBeFilled)))
                {
                    continue;
                }

                if(storageComp != null && num2 > storageComp.AmountToEmpty && storageComp.FocusCapacity > 0)
                {
                    num2 = storageComp.focusStored;
                }

                CompPsychicPylon pylonComp = item.Thing.TryGetComp<CompPsychicPylon>();

                if(pylonComp != null && pylonComp.Network != null && num2 > pylonComp.Network.focusTotal)
                {
                    num2 = pylonComp.Network.focusTotal;
                }

                if(num2 < num)
                {
                    focus = item.Thing;
                    num = num2;
                }
            }

            if(focus != __result.focus)
            {
                __result.spot = focus.Thing.OccupiedRect().ExpandedBy(2).Where((IntVec3 cell) => !cell.IsForbidden(pawn) && pawn.CanReserveAndReach(cell, PathEndMode.OnCell, pawn.NormalMaxDanger()) && cell.Standable(pawn.Map) && cell != focus.Thing.InteractionCell).RandomElementWithFallback(IntVec3.Invalid);
            }

            __result.focus = focus;
        }
    }
}