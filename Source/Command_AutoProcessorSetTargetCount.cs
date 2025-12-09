using Verse;
using RimWorld;

namespace AnimaTech
{
    public class Command_AutoProcessorSetTargetCount : Command_Action
    {
        protected CompWorkTableAutomatic user;

        public Command_AutoProcessorSetTargetCount(CompWorkTableAutomatic user)
        {
            //this.user = user;
            defaultLabel = "AT_AdjustProcessorTargetCount".Translate();
            defaultDesc = "AT_AdjustProcessorTargetCount".Translate();
            //icon = UIAssets.ButtonStrength;
            action = delegate
            {
                Dialog_Slider window = new Dialog_Slider((int x) => "AT_AdjustProcessorTargetCountLabel".Translate(x, user.Count*x), 1, 100, delegate(int value)
                {
                    user.targetCount = value;
                }, user.targetCount);
                Find.WindowStack.Add(window);
            };
        }
    }
}