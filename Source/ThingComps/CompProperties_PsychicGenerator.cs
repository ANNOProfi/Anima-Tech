using Verse;

namespace AnimaTech
{
    public class CompProperties_PsychicGenerator : CompProperties
    {
        public float baseGenerationRate;

        public bool canTransmitToNetwork;

        public bool canToggleTransmission;

        public CompProperties_PsychicGenerator()
        {
            compClass = typeof(CompPsychicGenerator);
        }

        private float focusMultiplier = 1f;

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

        public bool allowImbuement = false;

        public bool canToggleImbuement;

        public float NeuralHeatFactor = 0.5f;

        public bool isMeditatable = false;

        public bool canToggleMeditation;
    }
}