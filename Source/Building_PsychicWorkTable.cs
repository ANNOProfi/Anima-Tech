using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace AnimaTech
{
    public class Building_PsychicWorkTable : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
    {
        protected CompPsychicStorage storageComp;

        protected CompPsychicPylon pylonComp;

        protected CompFlickable flickComp;

        protected CompAssignableToPawn_PsychicStorage pawnComp;

        protected ModExtension_PsychicRune extension;

        //protected int ticksActive;

        //protected int ticksUntilProjectionChange;

        protected bool usedThisTick;

        protected bool usedThisTick2;

        protected bool isInUse;

	    protected bool isForcedOn;

        //protected int projectionIndex = -1;

        //protected Material projectionMaterial;

        //protected Material overlayMaterial;

        protected Vector3 runeDrawSize;

        protected Material runeFireMaterial;

        protected Material runeNetworkMaterial;

        protected Material runeMeditationMaterial;

        protected Material[] runeStorageMaterial = new Material[5];

        public bool IsConnected
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if (pylonComp != null && !pylonComp.isToggledOn)
                {
                    return false;
                }
                return true;
            }
        }

        public bool HasFocusStored
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if(storageComp != null && storageComp.IsEmpty)
                {
                    return false;
                }
                return true;
            }
        }

        public bool IsMeditated
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if(pawnComp != null && pawnComp.AssignedPawnsForReading.NullOrEmpty())
                {
                    return false;
                }
                return true;
            }
        }

        public bool IsOn
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if (flickComp != null && !flickComp.SwitchIsOn)
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
            pawnComp = GetComp<CompAssignableToPawn_PsychicStorage>();
            flickComp = GetComp<CompFlickable>();
            extension = def.GetModExtension<ModExtension_PsychicRune>() ?? new ModExtension_PsychicRune();
            InitializeOverlay();
            //projectionMaterial = UIAssets.GetTableProjectionMaterial(ref projectionIndex);
        }

        public override void Tick()
        {
            base.Tick();
            if (isForcedOn || usedThisTick || IsConnected || HasFocusStored || IsMeditated)
            {
                usedThisTick = false;
                //ticksActive++;
                //ticksUntilProjectionChange--;
                if (!isInUse)
                {
                    isInUse = true;
                    /*if (storageComp != null && storageComp.Props.idlePowerDraw != storageComp.Props.PowerConsumption)
                    {
                        storageComp.PowerOutput = 0f - storageComp.Props.PowerConsumption;
                    }*/
                    InitializeOverlay();
                    //UpdateProjection();
                }
                /*else if (ticksUntilProjectionChange < 1)
                {
                    UpdateProjection();
                }*/
            }
            else if (isInUse)
            {
                isInUse = false;
                //ticksActive = 0;
                /*if (storageComp != null && storageComp.Props.idlePowerDraw != storageComp.Props.PowerConsumption)
                {
                    storageComp.PowerOutput = 0f - storageComp.Props.idlePowerDraw;
                }*/
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.EnterNode("AnimaTechPsychicWorkTable"))
            {
                try
                {
                    //Scribe_Values.Look(ref ticksActive, "ticksActive", 0);
                    //Scribe_Values.Look(ref ticksUntilProjectionChange, "ticksUntilProjectionChange", 0);
                    Scribe_Values.Look(ref usedThisTick, "usedThisTick", defaultValue: false);
                    Scribe_Values.Look(ref isInUse, "isInUse", defaultValue: false);
                    Scribe_Values.Look(ref isForcedOn, "isForcedOn", defaultValue: false);
                    //Scribe_Values.Look(ref projectionIndex, "projectionIndex", 0);
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
            if (IsOn)
            {
                DrawRuneFire();
            }
            if(HasFocusStored)
            {
                DrawRuneStorage();
            }
            if(IsMeditated)
            {
                DrawRuneMeditation();
            }
            if(IsConnected)
            {
                DrawRuneNetwork();
            }
        }

        public void InitializeOverlay()
        {
            //Log.Message("Initializing Overlay");
            runeFireMaterial = extension.MaterialRuneFire(this);
            runeNetworkMaterial = extension.MaterialRuneNetwork(this);
            runeMeditationMaterial = extension.MaterialRuneMeditation(this);
            runeStorageMaterial = extension.MaterialRuneStorage(this);
            runeDrawSize = extension.overlayDrawSize;
            if (runeDrawSize.x != runeDrawSize.z && (base.Rotation == Rot4.East || base.Rotation == Rot4.West))
            {
                runeDrawSize.x = extension.overlayDrawSize.z;
                runeDrawSize.z = extension.overlayDrawSize.x;
            }
        }

        protected void DrawRuneFire()
        {
            if (runeFireMaterial != null)
            {
                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeFireMaterial, 0);
            }
        }

        protected void DrawRuneMeditation()
        {
            if (runeMeditationMaterial != null)
            {
                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeMeditationMaterial, 0);
            }
        }

        protected void DrawRuneNetwork()
        {
            if (runeNetworkMaterial != null)
            {
                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeNetworkMaterial, 0);
            }
        }

        protected void DrawRuneStorage()
        {
            if (runeStorageMaterial != null)
            {

                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);

                if(0<storageComp.focusStored && storageComp.focusStored<=(storageComp.Props.focusCapacity*0.25))
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[0], 0);
                }
                else if((storageComp.Props.focusCapacity*0.25)<storageComp.focusStored && storageComp.focusStored<=(storageComp.Props.focusCapacity*0.5))
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[1], 0);
                }
                else if((storageComp.Props.focusCapacity*0.5)<storageComp.focusStored && storageComp.focusStored<=(storageComp.Props.focusCapacity*0.75))
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[2], 0);
                }
                else if((storageComp.Props.focusCapacity*0.75)<storageComp.focusStored && storageComp.focusStored<storageComp.Props.focusCapacity)
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[3], 0);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[4], 0);                    
                }
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
        }*/

        /*public void UpdateProjection()
        {
            projectionMaterial = UIAssets.GetTableProjectionMaterial(ref projectionIndex, randomize: true);
            //ticksUntilProjectionChange = extension.projectionDuration.RandomInRange;
        }*/
    }
}