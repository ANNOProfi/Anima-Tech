using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Verse.Sound;
using System.Text;

namespace AnimaTech
{
    public class CompWorkTableAutomatic : ThingComp, IThingHolder, INotifyHauledTo, ISearchableContents, IStoreSettingsParent
    {
        public CompProperties_WorkTableAutomatic Props => (CompProperties_WorkTableAutomatic)props;

        private CompPsychicUser userComp;

        public RecipeDef selectedRecipe;

        private StorageSettings storageSettings = new StorageSettings();

        public bool Active => userComp.IsActive;

        public int maximumModifier = 1;

        public int MaximumCount
        {
            get
            {
                return Mathf.Clamp(MinimumCount*maximumModifier, MinimumCount, 1000);
            }
        }

        public bool IsFull
        {
            get
            {
                return innerContainer.TotalStackCount >= MaximumCount;
            }
        }

        private bool restock = true;

        public bool NeedsRestock
        {
            get
            {
                if(restock && IsFull)
                {
                    restock = false;
                    return false;
                }

                if(!restock && (innerContainer.TotalStackCount < MinimumCount))
                {
                    restock = true;
                    return true;
                }

                return restock;
            }
        }

        public int CountToFull
        {
            get
            {
                return MaximumCount - innerContainer.TotalStackCount;
            }
        }

        private int ticksToCompletion = -1;

        public int minimumModifier = 1;

        public int MinimumCount
        {
            get
            {
                if(selectedRecipe != null)
                {
                    return (int)Count*minimumModifier;
                }

                return 0;
            }
        }

        public float Count
        {
            get
            {
                if(selectedRecipe != null)
                {
                    return selectedRecipe.ingredients[0].CountFor(selectedRecipe);
                }

                return 0;
            }
        }

        public ThingOwner SearchableContents => innerContainer;

        public bool StorageTabVisible => true;

        private ThingOwner innerContainer;

        private ThingOwner ingredientContainer;

        private List<Thing> ingredients = new List<Thing>();

        private BillStoreModeDef storeMode = BillStoreModeDefOf.BestStockpile;

        private ISlotGroup slotGroup;

	    private Sustainer workingSound;

        public int targetCount = 1;

        public CompWorkTableAutomatic()
        {
            innerContainer = new ThingOwner<Thing>(this);
            ingredientContainer = new ThingOwner<Thing>(this);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            userComp = parent.TryGetComp<CompPsychicUser>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if(Active)
            {
                if(ticksToCompletion == -1 && selectedRecipe != null && !innerContainer.NullOrEmpty() && selectedRecipe.WorkerCounter.CountProducts((Bill_Production)selectedRecipe.MakeNewBill()) < targetCount)
                {
                    BeginForming();
                }
                else if(ticksToCompletion > 0)
                {
                    if(workingSound == null || workingSound.Ended)
                    {
                        workingSound = selectedRecipe.soundWorking.TrySpawnSustainer(parent);
                    }

                    workingSound.Maintain();

                    ticksToCompletion--;
                }
                else if(ticksToCompletion == 0)
                {
                    if(workingSound != null || workingSound.Ended)
                    {
                        workingSound.End();
                        workingSound = null;
                    }

                    FormingCompleted();
                    ticksToCompletion = -1;
                }
            }
            else if(workingSound != null)
            {
                workingSound.End();
                workingSound = null;
            }
        }

        public void BeginForming()
        {
            float num = innerContainer.TotalStackCount;

            IngredientCount count = selectedRecipe.ingredients[0];

            if(num >= count.CountFor(selectedRecipe))
            {
                ticksToCompletion = selectedRecipe.formingTicks;

                num = 0;

                int i = 0;

                while(num < count.CountFor(selectedRecipe))
                {
                    innerContainer.TryTransferToContainer(innerContainer[i % innerContainer.Count], ingredientContainer, 1, false);
                    ingredients.Add(ingredientContainer.Last());

                    i++;
                    
                    if(count.filter.DisplayRootCategory.catDef == ThingCategoryDefOf.Foods)
                    {
                        num += ingredients.Last().GetStatValue(StatDefOf.Nutrition);
                    }
                    else
                    {
                        num++;
                    }
                }

                ingredients.Clear();
            }
        }

        public void FormingCompleted()
        {
            List<Thing> ingredients = new List<Thing>();
            HashSet<ThingDef> ingredientsDef = new HashSet<ThingDef>();

            for(int i = 0; i < ingredientContainer.Count; i++)
            {
                ingredients.Add(ingredientContainer[i]);
                ingredientsDef.Add(ingredientContainer[i].def);
            }

            ingredientContainer.ClearAndDestroyContents();

            foreach(ThingDefCountClass product in selectedRecipe.products)
            {
                for(int i = 0; i < product.count; i++)
                {
                    Thing thing = ThingMaker.MakeThing(product.thingDef);
                    if (thing != null)
                    {
                        CompIngredients ingredientComp = thing.TryGetComp<CompIngredients>();

                        if(ingredientComp != null)
                        {
                            foreach(ThingDef ingredientDef in ingredientsDef)
                            {
                                ingredientComp.RegisterIngredient(ingredientDef);
                            }
                        }

                        ingredientContainer.TryAdd(thing);
                    }
                }
            }
            //ModLog.Log(" Forming completed");
            //ModLog.Log(" ThingOwner contains "+innerContainer.ContentsString);
            EjectContents();

            //inUse = false;
        }

        public void EjectContents()
        {
            if(selectedRecipe != null)
            {

                IntVec3 foundCell = IntVec3.Invalid;

                WorkTableAutonomousTeleportExtension teleportExtension = selectedRecipe.GetModExtension<WorkTableAutonomousTeleportExtension>();

                if(ingredientContainer.Contains(selectedRecipe.ProducedThingDef) && teleportExtension != null && teleportExtension.teleportToInventory)
                {
                    TeleportToPawn(foundCell, teleportExtension.minimumItemCount);

                    if(ingredientContainer.Count == 0)
                    {
                        return;
                    }
                }

                if(storeMode == BillStoreModeDefOf.BestStockpile)
                {
                    StoreUtility.TryFindBestBetterStoreCellFor(ingredientContainer[0], null, parent.Map, StoragePriority.Unstored, parent.Faction, out foundCell);
                }
                else if(storeMode == BillStoreModeDefOf.SpecificStockpile)
                {
                    StoreUtility.TryFindBestBetterStoreCellForIn(ingredientContainer[0], null, parent.Map, StoragePriority.Unstored, parent.Faction, slotGroup, out foundCell);
                }
                
                if(storeMode == BillStoreModeDefOf.DropOnFloor || foundCell == IntVec3.Invalid)
                {
                    foundCell = parent.InteractionCell;
                }

                TeleportToStorage(foundCell);
            }
            else
            {
                ingredientContainer.TryDropAll(parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
            }

            Messages.Message("AT_MessageProductFinished".Translate(parent.Label), MessageTypeDefOf.TaskCompletion);
        }

        public void TeleportToStorage(IntVec3 foundCell)
        {
            FleckMaker.Static(foundCell, parent.Map, FleckDefOf.PsycastSkipInnerExit, 0.5f);
            FleckMaker.Static(foundCell, parent.Map, FleckDefOf.PsycastSkipOuterRingExit, 0.5f);

            ingredientContainer.TryDropAll(foundCell, parent.Map, ThingPlaceMode.Near);
        }

        public void TeleportToPawn(IntVec3 foundCell, int minimumItemCount)
        {
            Pawn foundPawn = null;

            List<Pawn> pawns = parent.Map.mapPawns.FreeColonists;

            for(int i = 0; i < ingredientContainer.Count; i++)
            {
                Thing thing = ingredientContainer[i];

                int stackCount = thing.stackCount;

                for(int j = 0; j < stackCount; j++)
                {
                    foundPawn = pawns.Where((Pawn pawn) => pawn.inventory.innerContainer.TotalStackCountOfDef(thing.def) < minimumItemCount).DefaultIfEmpty().RandomElement();

                    if(foundPawn == null)
                    {
                        break;
                    }

                    Thing thing2 = thing;

                    if(thing.stackCount > 1)
                    {
                        thing2 = thing.SplitOff(1);
                    }

                    foundCell = foundPawn.Position;

                    FleckMaker.Static(foundCell, parent.Map, FleckDefOf.PsycastSkipOuterRingExit, 0.5f);

                    Messages.Message("AT_MessageProductTeleportedToPawn".Translate(parent.Label, thing2.Label, foundPawn.Name.ToStringShort), MessageTypeDefOf.TaskCompletion);

                    if(thing2 != thing)
                    {
                        foundPawn.inventory.innerContainer.TryAdd(thing2);
                    }
                    else
                    {
                        ingredientContainer.TryTransferToContainer(thing2, foundPawn.inventory.innerContainer);
                    }
                }
            }
        }

        public void SelectRecipe(RecipeDef recipe)
        {
            selectedRecipe = recipe;

            ticksToCompletion = -1;

            if(!selectedRecipe.ingredients.NullOrEmpty())
            {
                storageSettings.filter.SetAllowAll(selectedRecipe.ingredients[0].filter);
            }
            else
            {
                storageSettings.filter.SetAllowAll(selectedRecipe.fixedIngredientFilter);
            }

            ingredientContainer.TryTransferAllToContainer(innerContainer);

            while(!innerContainer.NullOrEmpty())
            {
                Thing thing = innerContainer.Last();

                if(storageSettings.AllowedToAccept(thing))
                {
                    innerContainer.TryTransferToContainer(thing, ingredientContainer);
                }
            }

            innerContainer.TryDropAll(parent.InteractionCell, parent.Map, ThingPlaceMode.Near);

            ingredientContainer.TryTransferAllToContainer(innerContainer);
        }

        public void DeselectRecipe()
        {
            selectedRecipe = null;

            ticksToCompletion = -1;

            storageSettings.filter.SetDisallowAll();

            ingredientContainer.TryTransferAllToContainer(innerContainer);

            innerContainer.TryDropAll(parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspect = base.CompInspectStringExtra();
            
            if (!inspect.NullOrEmpty())
            {
                stringBuilder.AppendLine(inspect);
            }

            string productString = null;

            if(selectedRecipe != null)
            {
                foreach(ThingDefCountClass product in selectedRecipe.products)
                {
                    if(productString.NullOrEmpty())
                    {
                        productString = product.Label.Colorize(Color.cyan);
                    }
                    else
                    {
                        productString += ", "+product.Label.Colorize(Color.cyan);
                    }
                }
            }
            
            stringBuilder.AppendLine("Produces: "+productString);

            string inventory = null;

            Dictionary<ThingDef, int> defs = new Dictionary<ThingDef, int>();
            Dictionary<ThingDef, Color> colors = new Dictionary<ThingDef, Color>();

            foreach(Thing thing in innerContainer)
            {
                Color color = Color.white;

                if(!defs.ContainsKey(thing.def))
                {
                    defs.Add(thing.def, thing.stackCount);
                    if(thing.def.IsMeat)
                    {
                        color = Color.red;
                    }
                    else if(thing.def.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
                    {
                        color = Color.green;
                    }

                    colors.Add(thing.def, color);
                }
                else
                {
                    defs[thing.def] += thing.stackCount;
                }
            }

            foreach(ThingDef thing in defs.Keys)
            {
                if(inventory == null)
                {
                    inventory = (thing.label+" x"+defs[thing].ToString()).Colorize(colors[thing]);
                }
                else
                {
                    inventory += ", "+(thing.label+" x"+defs[thing].ToString()).Colorize(colors[thing]);
                }
            }

            if(innerContainer.NullOrEmpty())
            {
                inventory = "empty";
            }

            stringBuilder.AppendLine("Storage: "+inventory);

            if(ticksToCompletion > -1)
            {
                stringBuilder.AppendLine("Finished in: "+ticksToCompletion.ToStringTicksToPeriod().Colorize(Color.cyan));
            }
            else
            {
                stringBuilder.AppendLine("Finished in:");
            }
            
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_Action
            {
                defaultLabel = "Select Recipe".Translate(),
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach(RecipeDef recipe in Props.recipes)
                    {
                        list.Add(new FloatMenuOption(recipe.LabelCap, delegate
                        {
                            SelectRecipe(recipe);
                        }));
                    }
                    list.Add(new FloatMenuOption("None", delegate
                    {
                        DeselectRecipe();
                    }));
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Set Delivery Mode".Translate(),
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("Teleport to pawn/Drop", delegate
                    {
                        storeMode = BillStoreModeDefOf.DropOnFloor;
                    }));
                    list.Add(new FloatMenuOption("Teleport to stockpile", delegate
                    {
                        storeMode = BillStoreModeDefOf.BestStockpile;
                    }));
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            };

            foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings))
            {
                yield return item;
            }

            yield return new Command_AutoProcessorAdjustMinCount(this);

            yield return new Command_AutoProcessorAdjustMaxCount(this);

            yield return new Command_AutoProcessorSetTargetCount(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Defs.Look(ref selectedRecipe, "selectedRecipe");
            Scribe_Values.Look(ref ticksToCompletion, "ticksToCompletion", -1);
            Scribe_Deep.Look(ref storageSettings, "storageSettings");
            Scribe_Defs.Look(ref storeMode, "storeMode");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Deep.Look(ref ingredientContainer, "ingredientContainer", this);
            Scribe_Values.Look(ref targetCount, "targetCount", 1);

            if (storageSettings == null)
            {
                storageSettings = new StorageSettings(this);
                if (parent.def.building.defaultStorageSettings != null)
                {
                    storageSettings.CopyFrom(parent.def.building.defaultStorageSettings);
                }
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
		    SoundDefOf.Standard_Drop.PlayOneShot(parent);
        }

        public StorageSettings GetStoreSettings()
        {
            return storageSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return parent.def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        {
        }
    }
}