using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace AnimaTech
{
    public class CompAssignableToPawn_PsychicStorage : CompAssignableToPawn
    {
        public override IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!parent.Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }
                return from x in parent.Map.mapPawns.FreeColonistsSpawned
                    where x.HasPsylink
                    select x into p
                    orderby CanAssignTo(p).Accepted descending
                    select p;
            }
        }

        public override bool AssignedAnything(Pawn pawn)
        {
            return assignedPawns.Count() >= base.MaxAssignedPawnsCount;
        }

        public override void TryAssignPawn(Pawn pawn)
        {
            if (assignedPawns.Count() == base.MaxAssignedPawnsCount)
            {
                assignedPawns.Remove(assignedPawns.Last());
            }
            assignedPawns.Add(pawn);
        }

        public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
        {
            assignedPawns.Remove(pawn);
        }

        protected override bool ShouldShowAssignmentGizmo()
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                return true;
            }
            return false;
        }

        public override AcceptanceReport CanAssignTo(Pawn pawn)
        {
            if (!pawn.HasPsylink)
            {
                return AcceptanceReport.WasRejected;
            }
            return AcceptanceReport.WasAccepted;
        }

        public override bool IdeoligionForbids(Pawn pawn)
        {
            return false;
        }

        protected override string GetAssignmentGizmoLabel()
        {
            return "AnimaTech.GUI.SetPsycastersToMeditation_Label".Translate();
        }

        protected override string GetAssignmentGizmoDesc()
        {
            return "AnimaTech.GUI.SetPsycastersToMeditation_Desc".Translate();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Action command_Action = new()
            {
                defaultLabel = GetAssignmentGizmoLabel(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner"),
                defaultDesc = GetAssignmentGizmoDesc(),
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this));
                }
            };
            yield return command_Action;
        }

        public override void PostExposeData()
        {
            assignedPawns.RemoveAll((Pawn x) => !x.Spawned || x.Dead);
            base.PostExposeData();
        }
    }
}