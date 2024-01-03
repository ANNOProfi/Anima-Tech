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

        public float minimumFueledThreshold;

        public float NeuralHeatFactor = 0.5f;

        public bool drawOutOfFuelOverlay = true;

        public string fuelIconPath;

        private float fuelMultiplier = 1f;

        public float autoRefuelPercent = 0.3f;

        public bool allowImbuement = true;

        public bool factorByDifficulty;

        public float FuelMultiplierCurrentDifficulty
        {
            get
            {
                if (factorByDifficulty && Find.Storyteller?.difficulty != null)
                {
                    return fuelMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
                }
                return fuelMultiplier;
            }
        }

        private Texture2D fuelIcon;

        public Texture2D FuelIcon
        {
            get
            {
                if (fuelIcon == null)
                {
                    if (!fuelIconPath.NullOrEmpty())
                    {
                        fuelIcon = ContentFinder<Texture2D>.Get(fuelIconPath);
                    }
                }
                return fuelIcon;
            }
        }
    }
}