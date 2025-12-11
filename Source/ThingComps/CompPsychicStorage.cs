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

        //private CompFlickable flickComp;

        public CompPsychicGenerator generatorComp;

        public CompPsychicPylon pylonComp;

        //public bool allowImbuement = true;

        public float focusStored;

        public float autoEmptyThreshold = 1f;

        public float autoEmptyMinimum;

        public bool allowEmpty;

        public float autoFillThreshold = 0.75f;

        public float autoFillMaximum = 1f;

        public bool allowFill = true;

        public int selectedFuelIndex;

        protected ModExtension_PsychicRune extension;

        protected Vector3 runeDrawSize;

        protected Material[] runeStorageMaterial = new Material[5];

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

        public float FocusCapacity => Props.focusCapacity;

        public float FocusSpace => Math.Max(0f, FocusCapacity - focusStored);

        public bool IsEmpty => focusStored <= 0f;

        public bool CanBeEmptied
        {
            get
            {
                if (Props.canBeEmptied && allowEmpty )
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
                if (Props.canBeEmptied && allowEmpty && AmountToAutoEmpty > 0)
                {
                    return focusStored >= FocusCapacity * autoEmptyThreshold;
                }
                return false;
            }
        }

        public int AmountToEmpty => Mathf.FloorToInt(focusStored);

        public int AmountToAutoEmpty => Mathf.FloorToInt(focusStored - FocusCapacity * autoEmptyMinimum);

        public bool CanBeFilled
        {
            get
            {
                if (Props.canBeFilled && allowFill)
                {
                    return !IsFull;
                }
                return false;
            }
        }

        public bool CanBeAutoFilled
        {
            get
            {
                if (Props.canBeFilled && allowFill && AmountToAutoFill > 0)
                {
                    return focusStored <= (FocusCapacity * autoFillThreshold);
                }
                return false;
            }
        }

        public int AmountToFill => Mathf.CeilToInt(FocusCapacity - focusStored);

        public int AmountToAutoFill => Mathf.CeilToInt(FocusCapacity * autoFillMaximum - focusStored);

        public bool IsFull
        {
            get
            {
                if(FocusCapacity > 0f)
            {
                return focusStored == FocusCapacity;
            }
            return false;
            }
        }

        public bool AcceptsTransmittedFocus => Props.canAcceptTransmitted;

        public bool CanBeInteractedWith
        {
            get
            {
                if (pylonComp != null && pylonComp.isToggledOn)
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
                    if (pylonComp != null && pylonComp.isToggledOn)
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

        public virtual void SetFuel(int index)
        {
            selectedFuelIndex = Mathf.Clamp(index, 0, Props.fuelThingDefs.Count - 1);
            //parent.BroadcastCompSignal("ARR.AethericFuelChanged");
        }

        public void FillFocus()
        {
            focusStored = FocusCapacity;
        }

        public void StoreFocus(float amount)
        {
            focusStored = Mathf.Min(focusStored + amount, FocusCapacity);
            if(IsFull)
            {
                allowFill = false;
            }
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

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (!runeStorageMaterial.NullOrEmpty() && !IsEmpty)
            {
                Vector3 pos = parent.DrawPos + extension.overlayDrawOffset;
                pos += Vector3.up * 0.01f;
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);

                if(0f<focusStored && focusStored<=(FocusCapacity*0.25f))
                {
                    if(parent.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[0], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[0], 0);
                    return;
                }
                else if(focusStored<=(FocusCapacity*0.5f))
                {
                    if(parent.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[1], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[1], 0);
                    return;
                }
                else if(focusStored<=(FocusCapacity*0.75f))
                {
                    if(parent.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[2], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[2], 0);
                    return;
                }
                else if(focusStored<(FocusCapacity*0.995f))
                {
                    if(parent.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[3], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[3], 0);
                    return;
                }
                else
                {  
                    if(parent.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[4], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[4], 0);
                    return;
                }
            }
        }

        public float DrainFocus(float amount)
        {
            float num = Mathf.Min(focusStored, amount);
            focusStored = Mathf.Max(0f, focusStored - num);

            if(AmountToFill >= FocusCapacity*0.25)
            {
                allowFill = true;
            }
            return amount - num;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            pylonComp = parent.GetComp<CompPsychicPylon>();

            extension = parent.def.GetModExtension<ModExtension_PsychicRune>() ?? new ModExtension_PsychicRune();

            if(extension != null)
            {
                runeStorageMaterial = extension.MaterialRuneStorage(parent);
                runeDrawSize = extension.overlayDrawSize;
                if (runeDrawSize.x != runeDrawSize.z && (parent.Rotation == Rot4.East || parent.Rotation == Rot4.West))
                {
                    runeDrawSize.x = extension.overlayDrawSize.z;
                    runeDrawSize.z = extension.overlayDrawSize.x;
                }
            }
            
            //generatorComp = parent.GetComp<CompPsychicGenerator>();
            if (!respawningAfterLoad && (!Props.disableInteractionIfPylonAvailableOnSpawn || !MapComponent.NetworkPresentAt(parent.Position)))
            {
                allowFill = Props.canBeFilled;
                allowEmpty = Props.canBeEmptied;
            }
            MapComponent.RegisterStorage(this);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
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
            if(FocusCapacity > 0)
            {
                return "AT_PsychicStorage".Translate(focusStored.ToString("F1"), FocusCapacity.ToString("F1"));
            }
            return "";
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