using Verse;

namespace AnimaTech
{
    public class AT_Settings : ModSettings
    {
        public bool useInteractiveRunes = true;
        
        public int tickInterval = 60;

        public float conversionFactor = 0.5f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useInteractiveRunes, "useInteractiveRunes", defaultValue: true);
            Scribe_Values.Look(ref tickInterval, "tickInterval", defaultValue: 60);
            Scribe_Values.Look(ref conversionFactor, "conversionFactor", defaultValue: 0.5f);
        }
    }
}