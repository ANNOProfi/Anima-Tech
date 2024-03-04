using Verse;
using RimWorld;

namespace AnimaTech
{
    public class CompHeatPusherPsychic : CompHeatPusher
    {
        protected CompPsychicPylon pylonComp;

        protected CompPsychicStorage storageComp;

        protected override bool ShouldPushHeatNow
        {
            get
            {
                if(!base.ShouldPushHeatNow || !FlickUtility.WantsToBeOn(parent) || (pylonComp != null && pylonComp.Network.IsEmpty) || (storageComp != null && storageComp.IsEmpty))
                {
                    return false;
                }

                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pylonComp = parent.GetComp<CompPsychicPylon>();
            storageComp = parent.GetComp<CompPsychicStorage>();
        }
    }
}