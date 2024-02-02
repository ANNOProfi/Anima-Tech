using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;

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

        private bool canImbue = false;

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

        public bool AllowImbuement
        {
            get
            {
                if(Props.allowImbuement)
                {
                    if(Props.canToggleImbuement)
                    {
                        return canImbue;
                    }
                }
                return false;
            }
        }

        public PsychicMapComponent mapComponentRef;

        public PsychicMapComponent MapComponent
        {
            get
            {
                if (mapComponentRef == null)
                {
                    mapComponentRef = parent.Map.PsychicComp();
                }
                return mapComponentRef;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pylonComp = parent.GetComp<CompPsychicPylon>();
            storageComp = parent.GetComp<CompPsychicStorage>();
            flickComp = parent.GetComp<CompFlickable>();
            reportedFocusGeneration = Props.baseGenerationRate;

            MapComponent.RegisterGenerator(this.parent);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            MapComponent.DeregisterGenerator(this.parent);
        }

        public override string CompInspectStringExtra()
        {
            if (DebugSettings.godMode && Props.baseGenerationRate > 0)
            {
                return "AT_PsychicGeneratorGenerationRate".Translate(Props.baseGenerationRate.ToString("F")) + $", Efficiency: {focusGenerationFactor.ToString("P")}";
            }

            if(Props.baseGenerationRate > 0)
            {
                return "AT_PsychicGeneratorGenerationRate".Translate(reportedFocusGeneration.ToString("F"));
            }
            return "";
        }

        public override void CompTick()
        {
            if (!(focusGenerationFactor > 0f) || !flickComp.SwitchIsOn)
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if(Props.canToggleImbuement)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AT_GeneratorToggleDebugImbuement".Translate(),
                    defaultDesc = "AT_GeneratorToggleDebugImbuementDesc".Translate(),
                    action = delegate
                    {
                        Props.allowImbuement = !AllowImbuement;
                    }
                };
            }
            if(Props.canToggleMeditation)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AT_GeneratorToggleDebugMeditation".Translate(),
                    defaultDesc = "AT_GeneratorToggleDebugMeditationDesc".Translate(),
                    action = delegate
                    {
                        Props.isMeditatable = !Props.isMeditatable;
                    }
                };
            }
            if(Props.canToggleTransmission)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AT_GeneratorToggleDebugTransmission".Translate(),
                    defaultDesc = "AT_GeneratorToggleDebugTransmissionDesc".Translate(),
                    action = delegate
                    {
                        allowTransmission = !allowTransmission;
                    }
                };
            }
        }

        public void Imbue(float amount, Pawn pawn)
        {
            float adjustedAmount = amount * Props.FocusMultiplierCurrentDifficulty;

            float psyfocus = pawn.psychicEntropy.CurrentPsyfocus;

            if((psyfocus * 100) >= adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(adjustedAmount * Props.NeuralHeatFactor))
            {
                float amount2 = adjustedAmount;

                if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
                {
                    amount2 = pylonComp.networkRef.PushFocus(adjustedAmount);
                }
                if (storageComp != null && amount2 > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(amount2);
                }
                
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-adjustedAmount / 100);
                pawn.psychicEntropy.TryAddEntropy(adjustedAmount * Props.NeuralHeatFactor);

                //parent.BroadcastCompSignal("Refueled");
            }
            else if((psyfocus * 100) < adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(pawn.psychicEntropy.CurrentPsyfocus * Props.NeuralHeatFactor))
            {
                float amount2 = psyfocus * 100;

                if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
                {
                    amount2 = pylonComp.networkRef.PushFocus(amount2)/100;
                }
                if (storageComp != null && amount2 > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(amount2);
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
                        float amount2 = i;

                        if (CanTransmit && pylonComp != null && pylonComp.networkRef != null)
                        {
                            amount2 = pylonComp.networkRef.PushFocus(i);
                        }
                        if (storageComp != null && amount2 > 0f && storageComp.FocusSpace > 0f)
                        {
                            storageComp.StoreFocus(amount2);
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
            if ((!Props.isMeditatable && !comp.AssignedPawns.Contains(pawn)) || ((storageComp?.focusStored + amount >= storageComp?.Props.focusCapacity) && storageComp?.Props.focusCapacity > 0) || ((pylonComp?.networkRef.focusTotal + amount >= pylonComp?.networkRef.focusCapacity) && pylonComp?.networkRef.focusCapacity > 0))
            {
                return false;
            }

            float num = focusGenerationFactor * amount;
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

            return true;
        }
    }
}