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

        public CompPsychicGenerator generatorComp;

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

        public int Tick => Find.TickManager.TicksGame;

        /*public void RuneGlow()
        {
            if (!((double)focusStored <= 0.01))
            {
                float x = focusStored / Props.focusCapacity * 100f;
                SimpleCurve simpleCurve = new SimpleCurve(new List<CurvePoint>
                {
                    new CurvePoint(0f, 70f),
                    new CurvePoint(100f, 20f)
                });

                if (Tick % (int)simpleCurve.Evaluate(x) == 0)
                {
                    MoteMaker.MakeStaticMote(parent.TrueCenter(), parent.Map, AT_DefOf.AT_RunesGlow);
                }
            }
        }*/

        public override void CompTick()
        {
            //RuneGlow();
        }

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
            //generatorComp = parent.GetComp<CompPsychicGenerator>();
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
            return "AT_PsychicStorage".Translate(focusStored.ToString("F"), Props.focusCapacity.ToString("F"));
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
                        defaultLabel = "AT_StorageAutoFill".Translate(),
                        defaultDesc = "AT_StorageAutoFillDesc".Translate(),
                        icon = UIAssets.ButtonChargeFill,
                        isActive = () => allowFill,
                        toggleAction = delegate
                        {
                            allowFill = !allowFill;
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "AT_StorageFillMin".Translate(),
                        defaultDesc = "AT_StorageFillMinDesc".Translate(),
                        icon = UIAssets.ButtonChargeFillMin,
                        getPercent = () => (100f * autoFillThreshold).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window4 = new Dialog_Slider((int x) => "AT_StorageFillMinLabel".Translate(x), 0, 100, delegate(int value)
                            {
                                autoFillThreshold = (float)value / 100f;
                            }, Mathf.RoundToInt(100f * autoFillThreshold));
                            Find.WindowStack.Add(window4);
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "AT_StorageFillMax".Translate(),
                        defaultDesc = "AT_StorageFillMaxDesc".Translate(),
                        icon = UIAssets.ButtonChargeFillMax,
                        getPercent = () => (100f * autoFillMaximum).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window3 = new Dialog_Slider((int x) => "AT_StorageFillMaxLabel".Translate(x), 0, 100, delegate(int value)
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
                        defaultLabel = "AT_StorageAutoEmpty".Translate(),
                        defaultDesc = "AT_StorageAutoEmptyDesc".Translate(),
                        icon = UIAssets.ButtonChargeEmpty,
                        isActive = () => allowEmpty,
                        toggleAction = delegate
                        {
                            allowEmpty = !allowEmpty;
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "AT_StorageEmptyMax".Translate(),
                        defaultDesc = "AT_StorageEmptyMaxDesc".Translate(),
                        icon = UIAssets.ButtonChargeEmptyMax,
                        getPercent = () => (100f * autoEmptyThreshold).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window2 = new Dialog_Slider((int x) => "AT_StorageEmptyMaxLabel".Translate(x), 0, 100, delegate(int value)
                            {
                                autoEmptyThreshold = (float)value / 100f;
                            }, Mathf.RoundToInt(100f * autoEmptyThreshold));
                            Find.WindowStack.Add(window2);
                        }
                    };
                    yield return new Command_ShowPercentage
                    {
                        defaultLabel = "AT_StorageEmptyMin".Translate(),
                        defaultDesc = "AT_StorageEmptyMinDesc".Translate(),
                        icon = UIAssets.ButtonChargeEmptyMin,
                        getPercent = () => (100f * autoEmptyMinimum).ToString("F0"),
                        action = delegate
                        {
                            Dialog_Slider window = new Dialog_Slider((int x) => "AT_StorageEmptyMinLevel".Translate(x), 0, 100, delegate(int value)
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
                    defaultLabel = "AT_StorageDebugFill".Translate(),
                    defaultDesc = "AT_StorageDebugFillDesc".Translate(),
                    action = delegate
                    {
                        FillFocus();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "AT_StorageDebugEmpty".Translate(),
                    defaultDesc = "AT_StorageDebugEmptyDesc".Translate(),
                    action = delegate
                    {
                        DrainFocus();
                    }
                };
            }
        }
    }
}