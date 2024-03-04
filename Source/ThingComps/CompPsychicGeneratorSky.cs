using Verse;
using UnityEngine;

namespace AnimaTech
{
    public class CompPsychicGeneratorSky : CompPsychicGenerator
    {
        protected override float DesiredFocusGeneration => OutDoorGenerationRate;

        private float OutDoorGenerationRate
        {
            get
            {
                if(Props.isDayTimeGenerator && Props.isNightTimeGenerator)
                {
                    return Mathf.Lerp(Props.baseGenerationRate/3, Props.baseGenerationRate, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;
                }
                if(Props.isDayTimeGenerator)
                {
                    return Mathf.Lerp(0f, Props.baseGenerationRate, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;
                }
                if(Props.isNightTimeGenerator)
                {
                    return Mathf.Lerp(0f, Props.baseGenerationRate, Mathf.Abs(parent.Map.skyManager.CurSkyGlow-1)) * RoofedPowerOutputFactor;
                }
                return 0;
            }
        }

        private float RoofedPowerOutputFactor
        {
            get
            {
                int num = 0;
                int num2 = 0;
                foreach (IntVec3 item in parent.OccupiedRect())
                {
                    num++;
                    if (parent.Map.roofGrid.Roofed(item))
                    {
                        num2++;
                    }
                }
                return (float)(num - num2) / (float)num;
            }
        }
    }
}