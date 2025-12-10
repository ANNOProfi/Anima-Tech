using Verse;
using RimWorld;

namespace AnimaTech
{
    public class CompHeatPusherPsychic : CompHeatPusher
    {
        protected CompPsychicUser userComp;

        public override bool ShouldPushHeatNow
        {
            get
            {
                if(!base.ShouldPushHeatNow || !FlickUtility.WantsToBeOn(parent) || (userComp != null && !userComp.IsActive))
                {
                    return false;
                }

                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            userComp = parent.GetComp<CompPsychicUser>();
        }
    }
}