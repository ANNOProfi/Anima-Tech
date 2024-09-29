using RimWorld;
using Verse;

namespace AnimaTech
{
    public class Hediff_PsychicAddedPart : Hediff_AddedPart
    {
        public HediffStage stage = new HediffStage();

        private float psychicSensitivityCached = 1f;

        private int ticksUntilNextCheck = 0;

        public override void Tick()
        {
            if(ticksUntilNextCheck > 0)
            {
                ticksUntilNextCheck--;
                
                return;
            }

            psychicSensitivityCached = pawn.GetStatValue(StatDefOf.PsychicSensitivity);

            ticksUntilNextCheck = 60;
        }

        public override HediffStage CurStage
        {
            get
            {
                HediffStage curStage;

                if (def.stages.NullOrEmpty<HediffStage>())
                {
                    curStage = stage;
                }
                else
                {
                    curStage = def.stages[CurStageIndex];
                }

                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    if (comps[i] is HediffComp_PsychicScale scaleComp)
                    {
                        curStage = scaleComp.GetStage(curStage, psychicSensitivityCached);
                    }
                }

                return curStage;
            }
        }
    }
}