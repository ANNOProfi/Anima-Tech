using RimWorld;
using Verse;

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

        private float partEfficiencyCached;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            partEfficiencyCached = this.parent.def.addedPartProps.partEfficiency;

            if(Props.minimumEfficiency < partEfficiencyCached * PsychicSensitivity)
            {
                this.parent.def.addedPartProps.partEfficiency = partEfficiencyCached * PsychicSensitivity;
            }
            else
            {
                this.parent.def.addedPartProps.partEfficiency = Props.minimumEfficiency;
            }
            
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if(psychicSensitivityCached != Pawn.GetStatValue(StatDefOf.PsychicSensitivity))
            {
                if(Props.minimumEfficiency < partEfficiencyCached * PsychicSensitivity)
                {
                    this.parent.def.addedPartProps.partEfficiency = partEfficiencyCached * PsychicSensitivity;
                }
                else
                {
                    this.parent.def.addedPartProps.partEfficiency = Props.minimumEfficiency;
                }
            }
        }
    }
}