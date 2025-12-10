using RimWorld;
using Verse;

namespace AnimaTech
{
    public class CompPsychicGlower : CompGlower
    {
        protected CompFlickable flickableComp;

        protected CompPsychicUser userComp;

        protected CompPsychicPylon pylonComp;

        protected CompPsychicStorage storageComp;

        protected CompProperties_PsychicGlower PsychicProps => (CompProperties_PsychicGlower)props;

        protected override bool ShouldBeLitNow
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                if (flickableComp != null && !flickableComp.SwitchIsOn)
                {
                    return false;
                }
                if (PsychicProps.conditionalOnFocusUse && userComp != null && !userComp.IsActive)
                {
                    return false;
                }
                if (PsychicProps.conditionalOnActivePylon && pylonComp != null && !pylonComp.ShouldFormLinks)
                {
                    return false;
                }
                if (PsychicProps.conditionalOnNonEmptyStorage && ((storageComp != null && !storageComp.IsEmpty) || (pylonComp != null && pylonComp.Network.IsEmpty)))
                {
                    return false;
                }
                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            flickableComp = parent.GetComp<CompFlickable>();
            userComp = parent.GetComp<CompPsychicUser>();
            pylonComp = parent.GetComp<CompPsychicPylon>();
            storageComp = parent.GetComp<CompPsychicStorage>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
            case "AT.PsychicNetworkEmpty":
            case "AT.PsychicNetworkNotEmpty":
            case "AT.PsychicDeviceActivated":
            case "AT.PsychicDeviceDeactivated":
            case "AT.PsychicPylonActivated":
            case "AT.PsychicPylonDeactivated":
                UpdateLit(parent.Map);
                break;
            default:
                base.ReceiveCompSignal(signal);
                break;
            }
        }
    }
}