using System.Collections.Generic;

namespace AnimaTech
{
    public class ByPylonRadius : IComparer<CompPsychicPylon>
    {
        public int Compare(CompPsychicPylon left, CompPsychicPylon right)
        {
            return right.PylonRadius.CompareTo(left.PylonRadius);
        }
    }
}