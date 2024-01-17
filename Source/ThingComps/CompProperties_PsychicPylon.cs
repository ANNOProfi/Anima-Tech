using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class CompProperties_PsychicPylon : CompProperties
    {
        public int pylonRadius;

        public bool canBeToggled = true;

        public bool defaultToggleState = true;

        public bool disableIfNotInRangeOfNetworkOnSpawn;

        //public Vector3 linkPositionOffset = Vector3.zero;

        public CompProperties_PsychicPylon()
        {
            compClass = typeof(CompPsychicPylon);
        }
    }
}