using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AnimaTech
{
    public class CompProperties_WorkTableAutomatic : CompProperties
    {
        public CompProperties_WorkTableAutomatic()
        {
            compClass = typeof(CompWorkTableAutomatic);
        }

        public List<RecipeDef> recipes;
    }
}