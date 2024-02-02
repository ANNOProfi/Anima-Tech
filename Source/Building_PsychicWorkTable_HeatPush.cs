using Verse;
using RimWorld;

namespace AnimaTech
{
    public class Building_PsychicWorkTable_HeatPush : Building_PsychicWorkTable
    {
        private const int HeatPushInterval = 30;

        public override void UsedThisTick()
        {
            base.UsedThisTick();
            if (Find.TickManager.TicksGame % 30 == 4)
            {
                GenTemperature.PushHeat(this, def.building.heatPerTickWhileWorking * 30f);
            }
        }
    }
}