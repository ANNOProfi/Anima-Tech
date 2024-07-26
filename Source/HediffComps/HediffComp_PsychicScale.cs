using RimWorld;
using Verse;
using UnityEngine;
using System;
using UnityEngine.Animations;

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

        private int ticksUntilNextCheck = 60;

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

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            this.Def.addedPartProps.partEfficiency = Math.Max(Props.minimumEfficiency, Props.originalEfficiency * PsychicSensitivity);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if(ticksUntilNextCheck <= 0)
            {
                if(psychicSensitivityCached != Pawn.GetStatValue(StatDefOf.PsychicSensitivity))
                {
                    this.Def.addedPartProps.partEfficiency = Math.Max(Props.minimumEfficiency, Props.originalEfficiency * PsychicSensitivity);
                }
                ticksUntilNextCheck = 60;
            }
            ticksUntilNextCheck--;
        }
    }
}