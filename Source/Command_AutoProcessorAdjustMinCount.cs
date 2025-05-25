using Verse;
using RimWorld;

namespace AnimaTech
{
    public class Command_AutoProcessorAdjustMinCount : Command_Action
    {
        protected CompWorkTableAutomatic user;

        public Command_AutoProcessorAdjustMinCount(CompWorkTableAutomatic user)
        {
            this.user = user;
            defaultLabel = "AT_AdjustProcessorCountMin".Translate();
            defaultDesc = "AT_AdjustProcessorCountMinDesc".Translate();
            //icon = UIAssets.ButtonStrength;
            action = delegate
            {
                Dialog_Slider window = new Dialog_Slider((int x) => "AT_AdjustProcessorCountMinLabel".Translate(x, user.Count*x), 1, 10, delegate(int value)
                {
                    user.minimumModifier = value;
                }, user.minimumModifier);
                Find.WindowStack.Add(window);
            };
        }
    }
}