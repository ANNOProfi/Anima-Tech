using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AnimaTech
{
    public class CompPsychicGenerator : ThingComp
    {
        public CompPsychicPylon pylonComp;

        public CompPsychicStorage storageComp;

        public CompFlickable flickComp;

        protected virtual float DesiredFocusGeneration => Props.baseGenerationRate;

        public float reportedFocusGeneration;

        public bool allowTransmission = true;

        public bool canImbue = true;

        public bool canMeditate = true;

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
                    return true;
                }
                return false;
            }
        }

        public bool AllowMeditation
        {
            get
            {
                if(Props.isMeditatable)
                {
                    if(Props.canToggleMeditation)
                    {
                        return canMeditate;
                    }
                    return true;
                }
                return false;
            }
        }

        private int ticksUntilNextGeneration;

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

            MapComponent.RegisterGenerator(this.parent);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            MapComponent.DeregisterGenerator(this.parent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref allowTransmission, "allowTransmission", defaultValue: true);
            Scribe_Values.Look(ref canImbue, "canImbue", defaultValue: true);
            Scribe_Values.Look(ref canMeditate, "canMeditate", defaultValue: true);
        }

        public override string CompInspectStringExtra()
        {
            if (DebugSettings.godMode && reportedFocusGeneration > 0f)
            {
                return "AT_PsychicGeneratorGenerationRate".Translate(reportedFocusGeneration.ToString("F1")) + $", Meditation efficiency: {Props.FocusMultiplierCurrentDifficulty.ToString("P")}";
            }

            if(reportedFocusGeneration > 0f)
            {
                return "AT_PsychicGeneratorGenerationRate".Translate(reportedFocusGeneration.ToString("F1"));
            }
            return "";
        }

        public virtual void UpdateGenerationRate()
        {
            if(flickComp != null &&!flickComp.SwitchIsOn)
            {
                reportedFocusGeneration = 0f;
            }
            else
            {
                reportedFocusGeneration = DesiredFocusGeneration;
            }
        }

        public override void CompTick()
        {
            UpdateGenerationRate();

            if (reportedFocusGeneration == 0f)
            {
                return;
            }

            if(ticksUntilNextGeneration < 1)
            {
                float num = reportedFocusGeneration / 1000f;

                if (num > 0f)
                {
                    float num2 = num;
                    if (CanTransmit && pylonComp != null && pylonComp.Network != null)
                    {
                        num2 = pylonComp.Network.PushFocus(num);
                    }
                    if (storageComp != null && num2 > 0f && storageComp.FocusSpace > 0f)
                    {
                        storageComp.StoreFocus(num);
                    }
                }

                ticksUntilNextGeneration = 60;
            }
            else
            {
                ticksUntilNextGeneration--;
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
                    defaultLabel = "AT_GeneratorToggleImbuement".Translate(),
                    defaultDesc = "AT_GeneratorToggleImbuementDesc".Translate(),
                    action = delegate
                    {
                        canImbue = !canImbue;
                    }
                };
            }
            if(Props.canToggleMeditation)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AT_GeneratorToggleMeditation".Translate(),
                    defaultDesc = "AT_GeneratorToggleMeditationDesc".Translate(),
                    action = delegate
                    {
                        canMeditate = !canMeditate;
                    }
                };
            }
            if(Props.canToggleTransmission)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AT_GeneratorToggleTransmission".Translate(),
                    defaultDesc = "AT_GeneratorToggleTransmissionDesc".Translate(),
                    action = delegate
                    {
                        allowTransmission = !allowTransmission;
                    }
                };
            }
        }

        public void Imbue(float amount, Pawn pawn)
        {
            float psyfocus = pawn.psychicEntropy.CurrentPsyfocus;

            if((psyfocus * 100) >= amount && !pawn.psychicEntropy.WouldOverflowEntropy(amount * Props.neuralHeatFactor))
            {
                float amount2 = amount;

                if (CanTransmit && pylonComp != null && pylonComp.Network != null)
                {
                    amount2 = pylonComp.Network.PushFocus(amount);
                }
                if (storageComp != null && amount2 > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(amount2);
                }
                
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-amount / 100);
                pawn.psychicEntropy.TryAddEntropy(amount * Props.neuralHeatFactor);

                //parent.BroadcastCompSignal("Refueled");
            }
            else if((psyfocus * 100) < amount && !pawn.psychicEntropy.WouldOverflowEntropy(pawn.psychicEntropy.CurrentPsyfocus * Props.neuralHeatFactor))
            {
                float amount2 = psyfocus * 100;

                if (CanTransmit && pylonComp != null && pylonComp.Network != null)
                {
                    amount2 = pylonComp.Network.PushFocus(amount2)/100;
                }
                if (storageComp != null && amount2 > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(amount2);
                }

                pawn.psychicEntropy.OffsetPsyfocusDirectly(-psyfocus);
                pawn.psychicEntropy.TryAddEntropy((psyfocus * 100) * Props.neuralHeatFactor);

                //parent.BroadcastCompSignal("Refueled");
            }
            else
            {
                for(float i=1f; i<amount; i++)
                {
                    if(!pawn.psychicEntropy.WouldOverflowEntropy(i * Props.neuralHeatFactor))
                    {
                        float amount2 = i;

                        if (CanTransmit && pylonComp != null && pylonComp.Network != null)
                        {
                            amount2 = pylonComp.Network.PushFocus(i);
                        }
                        if (storageComp != null && amount2 > 0f && storageComp.FocusSpace > 0f)
                        {
                            storageComp.StoreFocus(amount2);
                        }

                        pawn.psychicEntropy.OffsetPsyfocusDirectly(-i / 100);
                        pawn.psychicEntropy.TryAddEntropy(i * Props.neuralHeatFactor);

                        //parent.BroadcastCompSignal("Refueled");
                    }
                }
            }
        }

        public bool TryStoreFocus(float amount, Pawn pawn, CompAssignableToPawn_PsychicStorage comp)
        {
            if (!Props.isMeditatable || !comp.AssignedPawnsForReading.Contains(pawn) || (pylonComp == null && (storageComp.focusStored + amount >= storageComp.FocusCapacity) && storageComp.FocusCapacity > 0) || (storageComp == null && (pylonComp.Network.focusTotal + amount >= pylonComp.Network.focusCapacity) && pylonComp.Network.focusCapacity > 0))
            {
                return false;
            }

            float num = Props.FocusMultiplierCurrentDifficulty * amount * 100;
            if (num > 0f)
            {
                float num2 = num;
                if (CanTransmit && pylonComp != null && pylonComp.Network != null)
                {
                    num2 = pylonComp.Network.PushFocus(num);
                }
                if (storageComp != null && num2 > 0f && storageComp.FocusSpace > 0f)
                {
                    storageComp.StoreFocus(num2);
                }
            }

            return true;
        }
    }
}