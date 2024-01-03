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

        public float focus;

        public int lastMeditationTick;

        public int Tick => Find.TickManager.TicksGame;

        public bool meditationActive = true;

        public bool HasFuel
        {
            get
            {
                if (focus > 0f)
                {
                    return focus >= Props.minimumFueledThreshold;
                }
                return false;
            }
        }

        public float FuelPercentOfTarget => focus / Props.minimumFueledThreshold;

        public float FuelPercentOfMax => focus / Props.focusMax;

        public bool IsFull
        {
            get
            {
                if (HasFuel)
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
                if (FuelPercentOfTarget <= Props.autoRefuelPercent && !IsFull && Props.minimumFueledThreshold > 0f)
                {
                    return ShouldAutoRefuelNowIgnoringFuelPct;
                }
                return false;
            }
        }

        public bool ShouldAutoRefuelNowIgnoringFuelPct
        {
            get
            {
                if (!parent.IsBurning() && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Flick) == null)
                {
                    return parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Deconstruct) == null;
                }
                return false;
            }
        }

        public bool TryAddFocus(float focus, Pawn pawn, CompAssignableToPawn_PsychicStorage comp)
        {
            if (!comp.AssignedPawns.Contains(pawn) || this.focus + focus >= Props.focusMax)
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
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!HasFuel && Props.drawOutOfFuelOverlay)
            {
                parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.OutOfFuel);
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            stringBuilder.AppendLineIfNotEmpty();
            stringBuilder.AppendLine("AnimaObelisk.GUI.FocusLevel".Translate(Math.Round(focus, 3), Props.focusMax));
            return stringBuilder.ToString();
        }

        public void Imbue(float amount, Pawn pawn)
        {
            float adjustedAmount = amount * Props.FuelMultiplierCurrentDifficulty;

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
            return Mathf.Max(Mathf.CeilToInt((Props.focusMax - focus) / Props.FuelMultiplierCurrentDifficulty), 1);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Debug: fill obelisk (100%)";
                command_Action.action = delegate
                {
                    focus = Props.focusMax;
                };
                yield return command_Action;

                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "Debug: fill obelisk (80%)";
                command_Action2.action = delegate
                {
                    focus = Props.focusMax / 100f * 80f;
                };
                yield return command_Action2;

                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "Debug: fill obelisk (60%)";
                command_Action3.action = delegate
                {
                    focus = Props.focusMax / 100f * 60f;
                };
                yield return command_Action3;

                Command_Action command_Action4 = new Command_Action();
                command_Action4.defaultLabel = "Debug: fill obelisk (40%)";
                command_Action4.action = delegate
                {
                    focus = Props.focusMax / 100f * 40f;
                };
                yield return command_Action4;

			Command_Action command_Action5 = new Command_Action();
			command_Action5.defaultLabel = "Debug: fill obelisk (20%)";
			command_Action5.action = delegate
			{
				focus = Props.focusMax / 100f * 20f;
			};
			yield return command_Action5;

			Command_Action command_Action6 = new Command_Action();
			command_Action6.defaultLabel = "Debug: fill obelisk (0%)";
			command_Action6.action = delegate
			{
				focus = 0f;
			};
			yield return command_Action6;
		}
		Command_Action command_Action7 = new Command_Action();
		command_Action7.defaultLabel = "AnimaObelisk.GUI.MeditationActiveSwitch_Label".Translate();
		command_Action7.defaultDesc = "AnimaObelisk.GUI.MeditationActiveSwitch_Desc".Translate();
		command_Action7.icon = (meditationActive ? ContentFinder<Texture2D>.Get("UI/Commands/AnimaObelisk_ModeMeditate") : ContentFinder<Texture2D>.Get("UI/Commands/AnimaObelisk_ModeMeditateDisable"));
		command_Action7.action = delegate
		{
			meditationActive = !meditationActive;
		};
		yield return command_Action7;
	}
    }
}