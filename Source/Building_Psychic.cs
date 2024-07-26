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

        protected CompPsychicGenerator generatorComp;

        protected ModExtension_PsychicRune extension;

        protected int ticksActive;

        protected int ticksUntilProjectionChange;

        protected bool usedThisTick;

        protected bool isInUse;

	    protected bool isForcedOn;

        protected int projectionIndex = -1;

        protected Material[] runeStorageMaterial = new Material[5];

        protected Material[] runeGeneratorMaterial = new Material[5];

        protected Vector3 runeDrawSize;

        public bool HasFocusStored
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if(storageComp == null || (storageComp != null && storageComp.IsEmpty))
                {
                    return false;
                }
                return true;
            }
        }

        public bool IsGenerating
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                if(generatorComp == null || (generatorComp != null && generatorComp.GenerationRate == 0f) || (generatorComp != null && generatorComp.reportedFocusGeneration == 0f))
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
            generatorComp = GetComp<CompPsychicGenerator>();
            extension = def.GetModExtension<ModExtension_PsychicRune>() ?? new ModExtension_PsychicRune();
            InitializeOverlay();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.EnterNode("AnimaTechBuildingPsychic"))
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

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (HasFocusStored)
            {
                DrawRuneStorage();
            }
            if(IsGenerating)
            {
                DrawRuneGenerator();
            }
        }

        public void InitializeOverlay()
        {
            if(extension != null)
            {
                runeStorageMaterial = extension.MaterialRuneStorage(this);
                runeGeneratorMaterial = extension.MaterialRuneGenerator(this);
                runeDrawSize = extension.overlayDrawSize;
                if (runeDrawSize.x != runeDrawSize.z && (base.Rotation == Rot4.East || base.Rotation == Rot4.West))
                {
                    runeDrawSize.x = extension.overlayDrawSize.z;
                    runeDrawSize.z = extension.overlayDrawSize.x;
                }
            }
        }

        protected void DrawRuneStorage()
        {
            if (!runeStorageMaterial.NullOrEmpty())
            {

                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);

                if(0<storageComp.focusStored && storageComp.focusStored<=(storageComp.FocusCapacity*0.25))
                {
                    if(base.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[0], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[0], 0);
                }
                else if((storageComp.FocusCapacity*0.25)<storageComp.focusStored && storageComp.focusStored<=(storageComp.FocusCapacity*0.5))
                {
                    if(base.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[1], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[1], 0);
                }
                else if((storageComp.FocusCapacity*0.5)<storageComp.focusStored && storageComp.focusStored<=(storageComp.FocusCapacity*0.75))
                {
                    if(base.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[2], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[2], 0);
                }
                else if((storageComp.FocusCapacity*0.75)<storageComp.focusStored && storageComp.focusStored<(storageComp.FocusCapacity*0.995))
                {
                    if(base.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[3], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[3], 0);
                }
                else
                {  
                    if(base.Rotation == Rot4.West)
                    {
                        Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[4], 0);
                        return;
                    }
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[4], 0);
                }
            }
        }

        protected void DrawRuneGenerator()
        {
            if (!runeGeneratorMaterial.NullOrEmpty())
            {
                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);

                if(0<generatorComp.reportedFocusGeneration && generatorComp.reportedFocusGeneration<=(generatorComp.GenerationRate*0.25))
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeGeneratorMaterial[0], 0);
                }
                else if((generatorComp.GenerationRate*0.25)<generatorComp.reportedFocusGeneration && generatorComp.reportedFocusGeneration<=(generatorComp.GenerationRate*0.5))
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeGeneratorMaterial[1], 0);
                }
                else if((generatorComp.GenerationRate*0.5)<generatorComp.reportedFocusGeneration && generatorComp.reportedFocusGeneration<=(generatorComp.GenerationRate*0.75))
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeGeneratorMaterial[2], 0);
                }
                else if((generatorComp.GenerationRate*0.75)<generatorComp.reportedFocusGeneration && generatorComp.reportedFocusGeneration<(generatorComp.GenerationRate*0.995))
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeGeneratorMaterial[3], 0);
                }
                else
                {
                    Graphics.DrawMesh(MeshPool.plane10, matrix, runeGeneratorMaterial[4], 0);
                }
            }
        }

        protected void DrawAllRunes()
        {
            Vector3 pos = DrawPos + extension.overlayDrawOffset;
            pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);

            if(!runeStorageMaterial.NullOrEmpty())
            {
                if(base.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeStorageMaterial[4], 0);
                    return;
                }
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeStorageMaterial[4], 0);
            }
            if(!runeGeneratorMaterial.NullOrEmpty())
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeGeneratorMaterial[4], 0);
            }
        }
    }
}