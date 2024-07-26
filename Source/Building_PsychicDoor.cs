using RimWorld;
using Verse;
using UnityEngine;

namespace AnimaTech
{
    public class Building_PsychicDoor : Building_Door
    {
        public CompPsychicUser userComp;

        /*public CompPsychicPylon pylonComp;

        protected ModExtension_PsychicRune extension;

        protected Vector3 runeDrawSize;

        protected Material runeActiveMaterial;

        protected Material runeNetworkMaterial;*/

        public new bool DoorPowerOn
        {
            get
            {
                if(userComp != null)
                {
                    return userComp.IsPoweredOn;
                }

                return false;
            }
        }

        /*public bool IsConnected
        {
            get
            {
                if (!base.Spawned || pylonComp == null)
                {
                    return false;
                }
                if (pylonComp != null)
                {
                    return pylonComp.isToggledOn;
                }
                return true;
            }
        }

        private float OpenPct => Mathf.Clamp01((float)ticksSinceOpen / (float)TicksToOpenNow);*/

        public override void PostMake()
        {
            base.PostMake();
            userComp = GetComp<CompPsychicUser>();
            //pylonComp = GetComp<CompPsychicPylon>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            userComp = GetComp<CompPsychicUser>();
            /*pylonComp = GetComp<CompPsychicPylon>();
            extension = def.GetModExtension<ModExtension_PsychicRune>() ?? new ModExtension_PsychicRune();
            InitializeOverlay();*/
            
        }

        /*public override void Draw()
        {
            
            if(AT_Utilities.Settings.useInteractiveRunes)
            {
                if (OpenPct == 0f)
                {
                    DrawRuneActive();
                    if(IsConnected)
                    {
                        DrawRuneNetwork();
                    }
                }
            }
            else
            {
                DrawAllRunes();
            }

            base.Draw();
        }

        public void InitializeOverlay()
        {
            runeActiveMaterial = extension.MaterialRuneActive(this);
            runeNetworkMaterial = extension.MaterialRuneNetwork(this);
            runeDrawSize = extension.overlayDrawSize;
            if (runeDrawSize.x != runeDrawSize.z && (base.Rotation == Rot4.East || base.Rotation == Rot4.West))
            {
                runeDrawSize.x = extension.overlayDrawSize.z;
                runeDrawSize.z = extension.overlayDrawSize.x;
            }
        }

        protected void DrawRuneActive()
        {
            if (runeActiveMaterial != null)
            {
                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);
                if(base.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeActiveMaterial, 0);
                    return;
                }
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeActiveMaterial, 0);
            }
        }

        protected void DrawRuneNetwork()
        {
            if (runeNetworkMaterial != null)
            {
                Vector3 pos = DrawPos + extension.overlayDrawOffset;
                pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeActiveMaterial, 0);
            }
        }

        protected void DrawAllRunes()
        {
            Vector3 pos = DrawPos + extension.overlayDrawOffset;
            pos.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, runeDrawSize);

            if (runeActiveMaterial != null)
            {
                if(base.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeActiveMaterial, 0);
                    return;
                }
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeActiveMaterial, 0);
            }
            if (runeNetworkMaterial != null)
            {
                if(base.Rotation == Rot4.West)
                {
                    Graphics.DrawMesh(MeshPool.plane10Flip, matrix, runeNetworkMaterial, 0);
                    return;
                }
                Graphics.DrawMesh(MeshPool.plane10, matrix, runeNetworkMaterial, 0);
            }
        }*/
    }
}