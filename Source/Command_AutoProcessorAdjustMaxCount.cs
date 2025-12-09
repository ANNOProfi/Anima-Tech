using Verse;
using RimWorld;
using UnityEngine;

namespace AnimaTech
{
    public class Command_AutoProcessorAdjustMaxCount : Command_Action
    {
        protected CompWorkTableAutomatic user;

        public Command_AutoProcessorAdjustMaxCount(CompWorkTableAutomatic user)
        {
            this.user = user;
            defaultLabel = "AT_AdjustProcessorCountMax".Translate();
            defaultDesc = "AT_AdjustProcessorCountMaxDesc".Translate();
            //icon = UIAssets.ButtonStrength;
            action = delegate
            {
                Dialog_Slider window = new Dialog_Slider((int x) => "AT_AdjustProcessorCountMaxLabel".Translate(x, Mathf.Clamp(user.MinimumCount*x, user.MinimumCount, 1000)), 1, 10, delegate(int value)
                {
                    user.maximumModifier = value;
                }, user.maximumModifier);
                Find.WindowStack.Add(window);
            };
        }
    }
}