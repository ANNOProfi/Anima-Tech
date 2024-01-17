using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace AnimaTech
{
    public class CompProperties_PsychicStorage : CompProperties
    {
        public CompProperties_PsychicStorage()
        {
            compClass = typeof(CompPsychicStorage);
        }

        public float minimumFocusThreshold;

        public float focusCapacity = 100f;

        public bool canBeTransmitted;

        public bool canAcceptTransmitted;

        public bool canBeEmptied;

        public bool canBeFilled = true;

        public bool disableInteractionIfPylonAvailableOnSpawn;

        public bool canOnlyChangeAspectWhileEmpty = true;

        public ThingDef fuelThingDef;

        public List<ThingDef> fuelThingDefs;

        /*public float focuCapacity = 10;

        public float minimumFocusThreshold;

        public float NeuralHeatFactor = 0.5f;

        public bool drawOutOfFocusOverlay = true;

        public string focusIconPath;

        [MustTranslate]
	    public string focusLabel;

        public string FocusLabel
        {
            get
            {
                if (focusLabel.NullOrEmpty())
                {
                    return "Focus".TranslateSimple();
                }
                return focusLabel;
            }
        }

        private float focusMultiplier = 1f;

        public float imbue = 0.3f;

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
        }*/
    }
}