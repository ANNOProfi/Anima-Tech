using Verse;

namespace AnimaTech
{
    public class AT_Settings : ModSettings
    {
        public bool useInteractiveRunes = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useInteractiveRunes, "useInteractiveRunes", defaultValue: true);
        }
    }
}