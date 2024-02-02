using RimWorld;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class Building_Psychic : Building
    {
        protected CompPsychicStorage storageComp;

        protected CompPsychicPylon pylonComp;

        protected CompPsychicUser userComp;

        protected ModExtension_PsychicRune extension;

        protected int ticksActive;

        protected int ticksUntilProjectionChange;

        protected bool usedThisTick;

        protected bool isInUse;

	    protected bool isForcedOn;

        protected int projectionIndex = -1;

        protected Material projectionMaterial;

        protected Material overlayMaterial;

        protected Vector3 overlayDrawSize;

        public bool IsActive
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if ((storageComp != null && !storageComp.HasMinimumFocus) || (pylonComp != null && pylonComp.networkRef.IsEmpty() && pylonComp.networkRef != null))
                {
                    return false;
                }
                return true;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            storageComp = GetComp<CompPsychicStorage>();
            pylonComp = GetComp<CompPsychicPylon>();
            extension = def.GetModExtension<ModExtension_PsychicRune>() ?? new ModExtension_PsychicRune();
            InitializeOverlay();
            //projectionMaterial = UIAssets.GetTableProjectionMaterial(ref projectionIndex);
        }

        public override void Tick()
        {
            base.Tick();
            if ((isForcedOn || usedThisTick) && IsActive)
            {
                usedThisTick = false;
                ticksActive++;
                ticksUntilProjectionChange--;
                if (!isInUse)
                {
                    isInUse = true;
                    /*if (storageComp != null && storageComp.Props.idlePowerDraw != storageComp.Props.PowerConsumption)
                    {
                        storageComp.PowerOutput = 0f - storageComp.Props.PowerConsumption;
                    }*/
                    InitializeOverlay();
                    //RandomizeProjection();
                }
                else if (ticksUntilProjectionChange < 1)
                {
                    //RandomizeProjection();
                }
            }
            else if (isInUse)
            {
                isInUse = false;
                ticksActive = 0;
                /*if (storageComp != null && storageComp.Props.idlePowerDraw != storageComp.Props.PowerConsumption)
                {
                    storageComp.PowerOutput = 0f - storageComp.Props.idlePowerDraw;
                }*/
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.EnterNode("EccentricSmartTable"))
            {
                try
                {
                    Scribe_Values.Look(ref ticksActive, "ticksActive", 0);
                    Scribe_Values.Look(ref ticksUntilProjectionChange, "ticksUntilProjectionChange", 0);
                    Scribe_Values.Look(ref usedThisTick, "usedThisTick", defaultValue: false);
                    Scribe_Values.Look(ref isInUse, "isInUse", defaultValue: false);
                    Scribe_Values.Look(ref isForcedOn, "isForcedOn", defaultValue: false);
                    Scribe_Values.Look(ref projectionIndex, "projectionIndex", 0);
                }
                finally
                {
                    Scribe.ExitNode();
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (IsActive)
            {
                DrawOverlay();
                //DrawProjection();
            }
        }

        public void InitializeOverlay()
        {
            Log.Message("Initializing Overlay");
            overlayMaterial = extension.MaterialFor(this);
            overlayDrawSize = extension.overlayDrawSize;
            if (overlayDrawSize.x != overlayDrawSize.z && (base.Rotation == Rot4.East || base.Rotation == Rot4.West))
            {
                overlayDrawSize.x = extension.overlayDrawSize.z;
                overlayDrawSize.z = extension.overlayDrawSize.x;
            }
        }

        protected void DrawOverlay()
        {
            if (overlayMaterial != null)
            {
                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, overlayDrawSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, overlayMaterial, 0);
            }
        }

        /*protected void DrawProjection()
        {
            if (projectionMaterial != null)
            {
                Vector3 pos = DrawPos + extension.projectionDrawOffset;
                pos.y = AltitudeLayer.MoteOverheadLow.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, extension.projectionDrawSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, projectionMaterial, 0);
            }
        }

        public void RandomizeProjection()
        {
            //projectionMaterial = UIAssets.GetTableProjectionMaterial(ref projectionIndex, randomize: true);
            ticksUntilProjectionChange = extension.projectionDuration.RandomInRange;
        }*/
    }
}