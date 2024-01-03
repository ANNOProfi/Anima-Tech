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

        public bool drawOutOfFuelOverlay = true;

        public string fuelIconPath;

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