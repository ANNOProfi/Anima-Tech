using Verse;

namespace AnimaTech
{
    public class HediffCompProperties_PsychicScale : HediffCompProperties
    {
        public HediffCompProperties_PsychicScale()
        {
            this.compClass = typeof(HediffComp_PsychicScale);
        }

        public float minimumEfficiency = 1f;
    }
}