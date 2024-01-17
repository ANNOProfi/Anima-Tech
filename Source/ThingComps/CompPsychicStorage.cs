using Verse;
using RimWorld;
using System;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace AnimaTech
{
    public class CompPsychicStorage : ThingComp
    {
        public CompProperties_PsychicStorage Props => (CompProperties_PsychicStorage)props;

        public PsychicMapComponent mapComponentRef;

        private CompFlickable flickComp;

        public CompPsychicPylon pylonComp;

        //public bool allowImbuement = true;

        public float focusStored;

        public float autoEmptyThreshold = 1f;

        public float autoEmptyMinimum;

        public bool allowEmpty;

        public float autoFillThreshold;

        public float autoFillMaximum = 1f;

        public bool allowFill;

        public int selectedFuelIndex;

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

        public float FocusSpace => Math.Max(0f, Props.focusCapacity - focusStored);

        public bool IsEmpty => focusStored <= 0f;

        public bool CanBeEmptied
        {
            get
            {
                if (Props.canBeEmptied && allowEmpty && CanBeInteractedWith)
                {
                    return AmountToEmpty > 0;
                }
                return false;
            }
        }

        public bool CanBeAutoEmptied
        {
            get
            {
                if (Props.canBeEmptied && allowEmpty && CanBeInteractedWith && AmountToAutoEmpty > 0)
                {
                    return focusStored >= Props.focusCapacity * autoEmptyThreshold;
                }
                return false;
            }
        }

        public int AmountToEmpty => Mathf.FloorToInt(focusStored);

        public int AmountToAutoEmpty => Mathf.FloorToInt(focusStored - Props.focusCapacity * autoEmptyMinimum);

        public bool CanBeFilled
        {
            get
            {
                if (Props.canBeFilled && allowFill && CanBeInteractedWith)
                {
                    return AmountToFill > 0;
                }
                return false;
            }
        }

        public bool CanBeAutoFilled
        {
            get
            {
                if (Props.canBeFilled && allowFill && CanBeInteractedWith && AmountToAutoFill > 0)
                {
                    return focusStored <= Props.focusCapacity * autoFillThreshold;
                }
                return false;
            }
        }

        public int AmountToFill => Mathf.CeilToInt(Props.focusCapacity - focusStored);

        public int AmountToAutoFill => Mathf.CeilToInt(Props.focusCapacity * autoFillMaximum - focusStored);

        public bool IsFull => focusStored < Props.focusCapacity;

        public bool AcceptsTransmittedFocus => Props.canAcceptTransmitted;

        public bool CanBeInteractedWith
        {
            get
            {
                if (pylonComp != null)
                {
                    return !pylonComp.ShouldFormLinks;
                }
                return true;
            }
        }

        public virtual ThingDef FuelThingDef
        {
            get
            {
                if (Props.fuelThingDefs.NullOrEmpty())
                {
                    return Props.fuelThingDef;
                }
                return Props.fuelThingDefs[Mathf.Clamp(selectedFuelIndex, 0, Props.fuelThingDefs.Count - 1)];
            }
        }

        public virtual bool HasFuelOptions
        {
            get
            {
                if (Props.canBeFilled || Props.canBeEmptied)
                {
                    return !Props.fuelThingDefs.NullOrEmpty();
                }
                return false;
            }
        }

        public virtual bool ShouldShowFuelOptions
        {
            get
            {
                if (HasFuelOptions)
                {
                    if (pylonComp != null)
                    {
                        return !pylonComp.ShouldFormLinks;
                    }
                    return true;
                }
                return false;
            }
        }

        public bool HasMinimumFocus => focusStored >= Props.minimumFocusThreshold;

        public virtual void SetFuel(int index)
        {
            selectedFuelIndex = Mathf.Clamp(index, 0, Props.fuelThingDefs.Count - 1);
            //parent.BroadcastCompSignal("ARR.AethericFuelChanged");
        }

        public void FillFocus()
        {
            focusStored = Props.focusCapacity;
        }

        public void StoreFocus(float amount)
        {
            focusStored = Mathf.Min(focusStored + amount, Props.focusCapacity);
        }

        public bool HasFocus(float amount)
        {
            return focusStored >= amount;
        }

        public float DrainFocus()
        {
            float result = focusStored;
            focusStored = 0f;
            return result;
        }

        public float DrainFocus(float amount)
        {
            float num = Mathf.Min(focusStored, amount);
            focusStored = Mathf.Max(0f, focusStored - num);
            return amount - num;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pylonComp = parent.GetComp<CompPsychicPylon>();
            if (!respawningAfterLoad && (!Props.disableInteractionIfPylonAvailableOnSpawn || !MapComponent.NetworkPresentAt(parent.Position)))
            {
                allowFill = Props.canBeFilled;
                allowEmpty = Props.canBeEmptied;
            }
            MapComponent.RegisterStorage(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            MapComponent.DeregisterStorage(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref focusStored, "focusStored", 0f);
            Scribe_Values.Look(ref autoEmptyThreshold, "autoEmptyThreshold", 1f);
            Scribe_Values.Look(ref autoEmptyMinimum, "autoEmptyMinimum", 0f);
            Scribe_Values.Look(ref allowEmpty, "allowEmpty", defaultValue: false);
            Scribe_Values.Look(ref autoFillThreshold, "autoFillThreshold", 0f);
            Scribe_Values.Look(ref autoFillMaximum, "autoFillMaximum", 1f);
            Scribe_Values.Look(ref allowFill, "allowFill", defaultValue: false);
            Scribe_Values.Look(ref selectedFuelIndex, "selectedFuelIndex", 0);
        }

        public override string CompInspectStringExtra()
        {
            return "ARR_AetherStorage".Translate(focusStored.ToString("F1"), Props.focusCapacity.ToString("F1"));
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (ShouldShowFuelOptions)
            {
                //yield return new Command_SwitchCapacitorFuelOption(this);
            }
            /*if (CanBeInteractedWith)
            {
                if (Props.canBeFilled)
                {
                    yield return new Command_Toggle
                    {
                        defaultLabel = "ARR_CapacitorAutoFill".Translate(),
                        defaultDesc = "ARR_CapacitorAutoFillDesc".Translate(),
                        icon = UIAssets.ButtonChargeFill,
                        isActive = () => allowFill,
                        toggleAction = delegate
                        {
                            allowFill = !allowFill;
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "ARR_CapacitorFillMin".Translate(),
                        defaultDesc = "ARR_CapacitorFillMinDesc".Translate(),
                        icon = UIAssets.ButtonChargeFillMin,
                        getPercent = () => (100f * autoFillThreshold).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window4 = new Dialog_Slider((int x) => "ARR_CapacitorFillMinLabel".Translate(x), 0, 100, delegate(int value)
                            {
                                autoFillThreshold = (float)value / 100f;
                            }, Mathf.RoundToInt(100f * autoFillThreshold));
                            Find.WindowStack.Add(window4);
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "ARR_CapacitorFillMax".Translate(),
                        defaultDesc = "ARR_CapacitorFillMaxDesc".Translate(),
                        icon = UIAssets.ButtonChargeFillMax,
                        getPercent = () => (100f * autoFillMaximum).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window3 = new Dialog_Slider((int x) => "ARR_CapacitorFillMaxLabel".Translate(x), 0, 100, delegate(int value)
                            {
                                autoFillMaximum = (float)value / 100f;
                            }, Mathf.RoundToInt(100f * autoFillMaximum));
                            Find.WindowStack.Add(window3);
                        }
                    };
                }
                if (Props.canBeEmptied)
                {
                    yield return new Command_Toggle
                    {
                        defaultLabel = "ARR_CapacitorAutoEmpty".Translate(),
                        defaultDesc = "ARR_CapacitorAutoEmptyDesc".Translate(),
                        icon = UIAssets.ButtonChargeEmpty,
                        isActive = () => allowEmpty,
                        toggleAction = delegate
                        {
                            allowEmpty = !allowEmpty;
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "ARR_CapacitorEmptyMax".Translate(),
                        defaultDesc = "ARR_CapacitorEmptyMaxDesc".Translate(),
                        icon = UIAssets.ButtonChargeEmptyMax,
                        getPercent = () => (100f * autoEmptyThreshold).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window2 = new Dialog_Slider((int x) => "ARR_CapacitorEmptyMaxLabel".Translate(x), 0, 100, delegate(int value)
                            {
                                autoEmptyThreshold = (float)value / 100f;
                            }, Mathf.RoundToInt(100f * autoEmptyThreshold));
                            Find.WindowStack.Add(window2);
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "ARR_CapacitorEmptyMin".Translate(),
                        defaultDesc = "ARR_CapacitorEmptyMinDesc".Translate(),
                        icon = UIAssets.ButtonChargeEmptyMin,
                        getPercent = () => (100f * autoEmptyMinimum).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window = new Dialog_Slider((int x) => "ARR_CapacitorEmptyMinLevel".Translate(x), 0, 100, delegate(int value)
                            {
                                autoEmptyMinimum = (float)value / 100f;
                            }, Mathf.RoundToInt(100f * autoEmptyMinimum));
                            Find.WindowStack.Add(window);
                        }
                    };
                }
            }*/
            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "ARR_CapacitorDebugFill".Translate(),
                    defaultDesc = "ARR_CapacitorDebugFillDesc".Translate(),
                    action = delegate
                    {
                        FillFocus();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "ARR_CapacitorDebugEmpty".Translate(),
                    defaultDesc = "ARR_CapacitorDebugEmptyDesc".Translate(),
                    action = delegate
                    {
                        DrainFocus();
                    }
                };
            }
        }

        /*public int lastMeditationTick;

        public int Tick => Find.TickManager.TicksGame;

        public bool meditationActive = true;

        public bool HasFocus
        {
            get
            {
                if (focus > 0f)
                {
                    return focus >= Props.minimumFocusThreshold;
                }
                return false;
            }
        }

        public float FocusPercentOfTarget => focus / Props.minimumFocusThreshold;

        public float FocusPercentOfMax => focus / Props.focusMax;

        public bool IsFull
        {
            get
            {
                if (HasFocus)
                {
                    return focus < Props.focusMax;
                }
                return false;
            }
        }

        public bool ShouldAutoRefuelNow
        {
            get
            {
                if (FocusPercentOfTarget <= Props.imbue && !IsFull && Props.minimumFocusThreshold > 0f)
                {
                    return ShouldImbueNowIgnoringFuelPct;
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

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            flickComp = parent.GetComp<CompFlickable>();
        }

        public bool TryAddFocus(float focus, Pawn pawn, CompAssignableToPawn_PsychicStorage comp)
        {
            if (!meditationActive || !comp.AssignedPawns.Contains(pawn) || this.focus + focus >= Props.focusMax)
            {
                return false;
            }
            this.focus = Mathf.Clamp(this.focus + focus, 0f, Props.focusMax);
            
            parent.BroadcastCompSignal("Refueled");
            //lastMeditationTick = Tick;
            return true;
        }

        public void TryAddFocus(float amount)
        {
            this.focus = Mathf.Clamp(this.focus + amount, 0f, Props.focusMax);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

		    Scribe_Values.Look(ref focus, "focus", 0f);
            Scribe_Values.Look(ref allowImbuement, "allowImbuement", defaultValue: true);
            Scribe_Values.Look(ref meditationActive, "meditationActive", defaultValue: true);
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (!allowImbuement)
            {
                parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.ForbiddenRefuel);
            }
            else if (!HasFocus && Props.drawOutOfFocusOverlay)
            {
                parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.OutOfFuel);
            }
        }

        public override string CompInspectStringExtra()
        {*/
            /*StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            stringBuilder.AppendLineIfNotEmpty();
            stringBuilder.AppendLine("AnimaObelisk.GUI.FocusLevel".Translate(Math.Round(focus, 3), Props.focusMax));
            return stringBuilder.ToString();*/

            /*string text = Props.FocusLabel + ": " + focus.ToStringDecimalIfSmall() + " / " + Props.focusMax.ToStringDecimalIfSmall();

            return text;
        }

        public void Imbue(float amount, Pawn pawn)
        {
            float adjustedAmount = amount * Props.FocusMultiplierCurrentDifficulty;

            float psyfocus = pawn.psychicEntropy.CurrentPsyfocus;

            if((psyfocus * 100) >= adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(adjustedAmount * Props.NeuralHeatFactor))
            {
                TryAddFocus(amount);
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-(adjustedAmount) / 100);
                pawn.psychicEntropy.TryAddEntropy(adjustedAmount * Props.NeuralHeatFactor);

                parent.BroadcastCompSignal("Refueled");
            }
            else if((psyfocus * 100) < adjustedAmount && !pawn.psychicEntropy.WouldOverflowEntropy(pawn.psychicEntropy.CurrentPsyfocus * Props.NeuralHeatFactor))
            {
                TryAddFocus(psyfocus * 100);
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-psyfocus);
                pawn.psychicEntropy.TryAddEntropy((psyfocus * 100) * Props.NeuralHeatFactor);

                parent.BroadcastCompSignal("Refueled");
            }
            else
            {
                for(int i=1; i<adjustedAmount; i++)
                {
                    if(!pawn.psychicEntropy.WouldOverflowEntropy(i * Props.NeuralHeatFactor))
                    {
                        TryAddFocus(i);
                        pawn.psychicEntropy.OffsetPsyfocusDirectly(-i / 100);
                        pawn.psychicEntropy.TryAddEntropy(i * Props.NeuralHeatFactor);

                        parent.BroadcastCompSignal("Refueled");
                    }
                }
            }
        }

        public int GetFuelCountToFullyRefuel()
        {
            return Mathf.Max(Mathf.CeilToInt((Props.focusMax - focus) / Props.FocusMultiplierCurrentDifficulty), 1);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new()
                {
                    defaultLabel = "Debug: fill psyfocus (100%)",
                    action = delegate
                    {
                        focus = Props.focusMax;
                    }
                };
                yield return command_Action;

                Command_Action command_Action2 = new()
                {
                    defaultLabel = "Debug: fill psyfocus (80%)",
                    action = delegate
                    {
                        focus = Props.focusMax / 100f * 80f;
                    }
                };
                yield return command_Action2;

                Command_Action command_Action3 = new()
                {
                    defaultLabel = "Debug: fill psyfocus (60%)",
                    action = delegate
                    {
                        focus = Props.focusMax / 100f * 60f;
                    }
                };
                yield return command_Action3;

                Command_Action command_Action4 = new()
                {
                    defaultLabel = "Debug: fill psyfocus (40%)",
                    action = delegate
                    {
                        focus = Props.focusMax / 100f * 40f;
                    }
                };
                yield return command_Action4;

                Command_Action command_Action5 = new()
                {
                    defaultLabel = "Debug: fill psyfocus (20%)",
                    action = delegate
                    {
                        focus = Props.focusMax / 100f * 20f;
                    }
                };
                yield return command_Action5;

                Command_Action command_Action6 = new()
                {
                    defaultLabel = "Debug: fill psyfocus (0%)",
                    action = delegate
                    {
                        focus = 0f;
                    }
                };
                yield return command_Action6;
            }
            Command_Action command_Action7 = new()
            {
                defaultLabel = "AnimaTech.GUI.MeditationActiveSwitch_Label".Translate(),
                defaultDesc = "AnimaTech.GUI.MeditationActiveSwitch_Desc".Translate(),
                icon = (meditationActive ? ContentFinder<Texture2D>.Get("UI/Commands/AnimaObelisk_ModeMeditate") : ContentFinder<Texture2D>.Get("UI/Commands/AnimaObelisk_ModeMeditateDisable")),
                action = delegate
                {
                    meditationActive = !meditationActive;
                }
            };
            yield return command_Action7;

            Command_Action command_Action8 = new()
            {
                defaultLabel = "AnimaTech.GUI.ImbuementActiveSwitch_Label".Translate(),
                defaultDesc = "AnimaTech.GUI.ImbuementActiveSwitch_Desc".Translate(),
                icon = (meditationActive ? ContentFinder<Texture2D>.Get("UI/Commands/AnimaObelisk_ModeMeditate") : ContentFinder<Texture2D>.Get("UI/Commands/AnimaObelisk_ModeMeditateDisable")),
                action = delegate
                {
                    allowImbuement = !allowImbuement;
                }
            };
            yield return command_Action8;
        }*/
    }
}