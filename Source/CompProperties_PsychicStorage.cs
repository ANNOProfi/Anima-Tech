using Verse;
using RimWorld;
using UnityEngine;

namespace AnimaTech
{
    public class CompProperties_PsychicStorage : CompProperties
    {
        public CompProperties_PsychicStorage()
        {
            compClass = typeof(CompPsychicStorage);
        }

        public float focusMax = 10;

        public float minimumFocusThreshold;

        public float NeuralHeatFactor = 0.5f;

        public bool drawOutOfFocusOverlay = true;

        public string focusIconPath;

        private float focusMultiplier = 1f;

        public float imbue = 0.3f;

        public bool allowImbuement = true;

        public bool factorByDifficulty;

        public float FocusMultiplierCurrentDifficulty
        {
            get
            {
                if (factorByDifficulty && Find.Storyteller?.difficulty != null)
                {
                    return focusMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
                }
                return focusMultiplier;
            }
        }

        private Texture2D focusIcon;

        public Texture2D FocusIcon
        {
            get
            {
                if (focusIcon == null)
                {
                    if (!focusIconPath.NullOrEmpty())
                    {
                        focusIcon = ContentFinder<Texture2D>.Get(focusIconPath);
                    }
                }
                return focusIcon;
            }
        }
    }
}