using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [StaticConstructorOnStartup]
    public class CompNotWithoutFacilities : CompAffectedByFacilities
    {
        public bool CanUse
        {
            get
            {
                if(LinkedFacilitiesListForReading.NullOrEmpty())
                {
                    //parent.def.SetStatBaseValue(StatDefOf.WorkTableWorkSpeedFactor, 0f);
                    return false;
                }

                foreach(Thing thing in LinkedFacilitiesListForReading)
                {
                    if(base.parent.def.GetCompProperties<CompProperties_AffectedByFacilities>().linkableFacilities.Contains(thing.def))
                    {
                        //parent.def.SetStatBaseValue(StatDefOf.WorkTableWorkSpeedFactor, statCached);
                        if(userComp != null)
                        {
                            return userComp.IsActive && thing.TryGetComp<CompFacility>().CanBeActive;
                        }

                        return thing.TryGetComp<CompFacility>().CanBeActive;
                    }
                    //parent.def.SetStatBaseValue(StatDefOf.WorkTableWorkSpeedFactor, 0f);
                }

                return false;
            }
        }

        public CompPsychicUser userComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            userComp = parent.TryGetComp<CompPsychicUser>();
        }

        /*public void IsConnected()
        {
            /*if(canUse == true && parent.def.GetStatValueAbstract(StatDefOf.WorkTableWorkSpeedFactor) > 0f)
            {
                statCached = parent.def.GetStatValueAbstract(StatDefOf.WorkTableWorkSpeedFactor);
            }
            
            if(LinkedFacilitiesListForReading.NullOrEmpty())
            {
                //parent.def.SetStatBaseValue(StatDefOf.WorkTableWorkSpeedFactor, 0f);
                canUse = false;
                return;
            }

            foreach(Thing thing in LinkedFacilitiesListForReading)
            {
                if(base.parent.def.GetCompProperties<CompProperties_AffectedByFacilities>().linkableFacilities.Contains(thing.def))
                {
                    //parent.def.SetStatBaseValue(StatDefOf.WorkTableWorkSpeedFactor, statCached);
                    canUse = true;
                    return;
                }

                canUse = false;
                //parent.def.SetStatBaseValue(StatDefOf.WorkTableWorkSpeedFactor, 0f);
            }
        }*/
    }
}