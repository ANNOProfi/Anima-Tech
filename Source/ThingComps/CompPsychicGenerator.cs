using Verse;
using RimWorld;
using System.Linq;

namespace AnimaTech
{
    public class CompPsychicGenerator : ThingComp
    {
        public CompPsychicPylon pylonComp;

        public CompPsychicStorage storageComp;

        public CompFlickable flickComp;

        public float reportedFocusGeneration;

        public float focusGenerationFactor = 1f;

        public bool allowTransmission = true;

        public CompProperties_PsychicGenerator Props => (CompProperties_PsychicGenerator)props;

        public bool CanTransmit
        {
            get
            {
                if (Props.canTransmitToNetwork)
                {
                    if (Props.canToggleTransmission)
                    {
                        return allowTransmission;
                    }
                    return true;
                }
                return false;
            }
        }

        public bool ShouldImbueNowIgnoringFuelPct
        {
            get
            {
                if (!parent.IsBurning() && (flickComp == null || flickComp.SwitchIsOn) && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Flick) == null)
                {
                    return parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Deconstruct) == null;
                }
                return false;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pylonComp = parent.GetComp<CompPsychicPylon>();
            storageComp = parent.GetComp<CompPsychicStorage>();
            reportedFocusGeneration = Props.baseGenerationRate;
        }

        public override string CompInspectStringExtra()
        {
            if (DebugSettings.godMode)
            {
                return "ARR_AetherAccumulatorGenerationRate".Translate(reportedFocusGeneration.ToString("F1")) + $", Rate: {focusGenerationFactor}";
            }
            return "ARR_AetherAccumulatorGenerationRate".Translate(reportedFocusGeneration.ToString("F1"));
        }

        public override void CompTick()
        {
            if (!(focusGenerationFactor > 0f))
            {
                return;
            }
            float num = focusGenerationFactor * Props.baseGenerationRate / 60000f;
            if (num > 0f)
            {
                float num2 = num;
                if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
                {
                    num2 = pylonComp.networkRef.PushFocus(num);
                }
                if (storageComp != null && num2 > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(num);
                }
            }
        }

        public void Imbue(float amount, Pawn pawn)
        {
            float adjustedAmount = amount * Props.FocusMultiplierCurrentDifficulty;

            float psyfocus = pawn.psychicEntropy.CurrentPsyfocus;

            if((psyfocus * 100) >= adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(adjustedAmount * Props.NeuralHeatFactor))
            {
                if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
                {
                    adjustedAmount = pylonComp.networkRef.PushFocus(adjustedAmount);
                }
                if (storageComp != null && adjustedAmount > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(adjustedAmount);
                }
                
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-adjustedAmount / 100);
                pawn.psychicEntropy.TryAddEntropy(adjustedAmount * Props.NeuralHeatFactor);

                //parent.BroadcastCompSignal("Refueled");
            }
            else if((psyfocus * 100) < adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(pawn.psychicEntropy.CurrentPsyfocus * Props.NeuralHeatFactor))
            {
                if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
                {
                    psyfocus = pylonComp.networkRef.PushFocus(psyfocus * 100)/100;
                }
                if (storageComp != null && (psyfocus * 100) > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(psyfocus * 100);
                }

                pawn.psychicEntropy.OffsetPsyfocusDirectly(-psyfocus);
                pawn.psychicEntropy.TryAddEntropy((psyfocus * 100) * Props.NeuralHeatFactor);

                //parent.BroadcastCompSignal("Refueled");
            }
            else
            {
                for(float i=1f; i<adjustedAmount; i++)
                {
                    if(!pawn.psychicEntropy.WouldOverflowEntropy(i * Props.NeuralHeatFactor))
                    {
                        if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
                        {
                            i = pylonComp.networkRef.PushFocus(i);
                        }
                        if (storageComp != null && i > 0f && storageComp.FocusSpace > 0f)
                        {
                            storageComp.StoreFocus(i);
                        }

                        pawn.psychicEntropy.OffsetPsyfocusDirectly(-i / 100);
                        pawn.psychicEntropy.TryAddEntropy(i * Props.NeuralHeatFactor);

                        //parent.BroadcastCompSignal("Refueled");
                    }
                }
            }
        }

        public bool TryStoreFocus(float amount, Pawn pawn, CompAssignableToPawn_PsychicStorage comp)
        {
            if (!Props.isMeditatable || !comp.AssignedPawns.Contains(pawn) || storageComp.focusStored + amount >= storageComp.Props.focusCapacity)
            {
                return false;
            }

            /*if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
            {
                amount = pylonComp.networkRef.PushFocus(amount);
            }
            if (storageComp != null && amount > 0f && storageComp.FocusSpace > 0f)
            {
                storageComp.StoreFocus(amount);
            }*/

            focusGenerationFactor += amount;

            return true;
        }
    }
}