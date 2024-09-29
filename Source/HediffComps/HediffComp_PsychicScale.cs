using RimWorld;
using Verse;
using UnityEngine;

namespace AnimaTech
{
    public class HediffComp_PsychicScale : HediffComp
    {
        public HediffCompProperties_PsychicScale Props
        {
            get
            {
                return (HediffCompProperties_PsychicScale)this.props;
            }
        }

        private float psychicSensitivityCached;

	    private int psychicSensitivityCachedTick = -1;

        private float partEfficiencyCached = 1f;

        public float PsychicSensitivity
        {
            get
            {
                if (psychicSensitivityCachedTick != Find.TickManager.TicksGame)
                {
                    psychicSensitivityCached = Pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                    psychicSensitivityCachedTick = Find.TickManager.TicksGame;
                }
                return psychicSensitivityCached;
            }
        }

        public HediffStage GetStage(HediffStage stage, float psychicSensitivity)
        {
            partEfficiencyCached = Def.addedPartProps.partEfficiency;

            stage.partEfficiencyOffset = partEfficiencyCached * psychicSensitivity - 1f;

            if(partEfficiencyCached * psychicSensitivity < Props.minimumEfficiency)
            {
                stage.partEfficiencyOffset = Props.minimumEfficiency - 1f;
            }

            return stage;
        }
    }
}