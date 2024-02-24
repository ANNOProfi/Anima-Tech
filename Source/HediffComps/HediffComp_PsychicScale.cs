using RimWorld;
using Verse;
using UnityEngine;
using System;

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

        private int ticksUntilNextCheck = 0;

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
            partEfficiencyCached = this.Def.addedPartProps.partEfficiency;

            this.Def.addedPartProps.partEfficiency = Math.Max(Props.minimumEfficiency, partEfficiencyCached * PsychicSensitivity);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if(ticksUntilNextCheck <= 0)
            {
                if(psychicSensitivityCached != Pawn.GetStatValue(StatDefOf.PsychicSensitivity))
                {
                    this.Def.addedPartProps.partEfficiency = Math.Max(Props.minimumEfficiency, partEfficiencyCached * PsychicSensitivity);
                }
                ticksUntilNextCheck = 60;
            }
            ticksUntilNextCheck--;
        }
    }
}