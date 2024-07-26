using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class ModExtension_PsychicRune : DefModExtension
    {
        [Unsaved(false)]
        public Material staticOverlay;

        [Unsaved(false)]
        public Material rotatedOverlayEast;

        [Unsaved(false)]
        public Material rotatedOverlaySouth;

        public string staticOverlayPath;

        public string rotatedOverlayPath;

        [Unsaved(false)]
        public Material staticRuneActive;

        [Unsaved(false)]
        public Material rotatedRuneActiveEast;

        [Unsaved(false)]
        public Material rotatedRuneActiveSouth;

        [Unsaved(false)]
        public Material rotatedRuneActiveNorth;

        public string staticRuneActivePath;

        public string rotatedRuneActivePath;

        [Unsaved(false)]
        public Material staticRuneMeditation;

        [Unsaved(false)]
        public Material rotatedRuneMeditationEast;

        [Unsaved(false)]
        public Material rotatedRuneMeditationSouth;

        [Unsaved(false)]
        public Material rotatedRuneMeditationNorth;

        public string staticRuneMeditationPath;

        public string rotatedRuneMeditationPath;

        [Unsaved(false)]
        public Material[] staticRuneStorage = new Material[5];

        [Unsaved(false)]
        public Material[] rotatedRuneStorageEast = new Material[5];

        [Unsaved(false)]
        public Material[] rotatedRuneStorageSouth = new Material[5];

        [Unsaved(false)]
        public Material[] rotatedRuneStorageNorth = new Material[5];

        public string staticRuneStoragePath;

        public string rotatedRuneStoragePath;

        [Unsaved(false)]
        public Material staticRuneNetwork;

        [Unsaved(false)]
        public Material rotatedRuneNetworkEast;

        [Unsaved(false)]
        public Material rotatedRuneNetworkSouth;

        [Unsaved(false)]
        public Material rotatedRuneNetworkNorth;

        public string staticRuneNetworkPath;

        public string rotatedRuneNetworkPath;

        [Unsaved(false)]
        public Material[] staticRuneGenerator = new Material[5];

        public string staticRuneGeneratorPath;

        public Vector3 overlayDrawSize = new Vector3(2f, 0f, 2f);

        public Vector3 overlayDrawOffset = new Vector3(0f, 0f, 0f);

        //public Vector3 projectionDrawSize = new Vector3(2f, 0f, 2f);

        //public Vector3 projectionDrawOffset = new Vector3(0f, 0f, 0.35f);

        //public IntRange projectionDuration = new IntRange(120, 240);

        public ModExtension_PsychicRune()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (!staticRuneActivePath.NullOrEmpty())
                {
                    staticRuneActive = MaterialPool.MatFrom(staticRuneActivePath, ShaderDatabase.Transparent);
                }
                if (!rotatedRuneActivePath.NullOrEmpty())
                {
                    rotatedRuneActiveNorth = MaterialPool.MatFrom(rotatedRuneActivePath + "_north", ShaderDatabase.Transparent);
                    rotatedRuneActiveEast = MaterialPool.MatFrom(rotatedRuneActivePath + "_east", ShaderDatabase.Transparent);
                    rotatedRuneActiveSouth = MaterialPool.MatFrom(rotatedRuneActivePath + "_south", ShaderDatabase.Transparent);
                }

                if (!staticRuneNetworkPath.NullOrEmpty())
                {
                    staticRuneNetwork = MaterialPool.MatFrom(staticRuneNetworkPath, ShaderDatabase.Transparent);
                }
                if (!rotatedRuneNetworkPath.NullOrEmpty())
                {
                    rotatedRuneNetworkNorth = MaterialPool.MatFrom(rotatedRuneNetworkPath + "_north", ShaderDatabase.Transparent);
                    rotatedRuneNetworkEast = MaterialPool.MatFrom(rotatedRuneNetworkPath + "_east", ShaderDatabase.Transparent);
                    rotatedRuneNetworkSouth = MaterialPool.MatFrom(rotatedRuneNetworkPath + "_south", ShaderDatabase.Transparent);
                }

                if (!staticRuneMeditationPath.NullOrEmpty())
                {
                    staticRuneMeditation = MaterialPool.MatFrom(staticRuneMeditationPath, ShaderDatabase.Transparent);
                }
                if (!rotatedRuneMeditationPath.NullOrEmpty())
                {
                    rotatedRuneMeditationNorth = MaterialPool.MatFrom(rotatedRuneMeditationPath + "_north", ShaderDatabase.Transparent);
                    rotatedRuneMeditationEast = MaterialPool.MatFrom(rotatedRuneMeditationPath + "_east", ShaderDatabase.Transparent);
                    rotatedRuneMeditationSouth = MaterialPool.MatFrom(rotatedRuneMeditationPath + "_south", ShaderDatabase.Transparent);
                }

                if (!staticRuneStoragePath.NullOrEmpty())
                {
                    for(int i=1; i<6; i++)
                    {
                        staticRuneStorage[i-1] = MaterialPool.MatFrom(staticRuneStoragePath + $"{i}", ShaderDatabase.Transparent);
                    }
                }
                if (!rotatedRuneStoragePath.NullOrEmpty())
                {
                    for(int i=1; i<6; i++)
                    {
                        rotatedRuneStorageNorth[i-1] = MaterialPool.MatFrom(rotatedRuneStoragePath + $"{i}_north", ShaderDatabase.Transparent);
                        rotatedRuneStorageEast[i-1] = MaterialPool.MatFrom(rotatedRuneStoragePath + $"{i}_east", ShaderDatabase.Transparent);
                        rotatedRuneStorageSouth[i-1] = MaterialPool.MatFrom(rotatedRuneStoragePath + $"{i}_south", ShaderDatabase.Transparent);
                    }
                }

                if(!staticRuneGeneratorPath.NullOrEmpty())
                {
                    for(int i=1; i<6; i++)
                    {
                        staticRuneGenerator[i-1] = MaterialPool.MatFrom(staticRuneGeneratorPath + $"{i}", ShaderDatabase.Transparent);
                    }
                }
            });
        }

        public Material MaterialFor(Thing thing)
        {
            if (rotatedOverlayPath != null)
            {
                if (thing.Rotation == Rot4.East || thing.Rotation == Rot4.West)
                {
                    return rotatedOverlayEast;
                }
                return rotatedOverlaySouth;
            }
            return staticOverlay;
        }

        public Material MaterialRuneActive(Thing thing)
        {
            if (rotatedRuneActivePath != null)
            {
                if (thing.Rotation == Rot4.East || thing.Rotation == Rot4.West)
                {
                    return rotatedRuneActiveEast;
                }
                if(thing.Rotation == Rot4.North)
                {
                    return rotatedRuneActiveNorth;
                }
                return rotatedRuneActiveSouth;
            }
            return staticRuneActive;
        }

        public Material MaterialRuneNetwork(Thing thing)
        {
            if (rotatedRuneNetworkPath != null)
            {
                if (thing.Rotation == Rot4.East || thing.Rotation == Rot4.West)
                {
                    return rotatedRuneNetworkEast;
                }
                if(thing.Rotation == Rot4.North)
                {
                    return rotatedRuneNetworkNorth;
                }
                return rotatedRuneNetworkSouth;
            }
            return staticRuneNetwork;
        }

        public Material MaterialRuneMeditation(Thing thing)
        {
            if (rotatedRuneMeditationPath != null)
            {
                if (thing.Rotation == Rot4.East || thing.Rotation == Rot4.West)
                {
                    return rotatedRuneMeditationEast;
                }
                if(thing.Rotation == Rot4.North)
                {
                    return rotatedRuneMeditationNorth;
                }
                return rotatedRuneMeditationSouth;
            }
            return staticRuneMeditation;
        }

        public Material[] MaterialRuneStorage(Thing thing)
        {
            if (rotatedRuneStoragePath != null)
            {
                if (thing.Rotation == Rot4.East || thing.Rotation == Rot4.West)
                {
                    return rotatedRuneStorageEast;
                }
                if(thing.Rotation == Rot4.North)
                {
                    return rotatedRuneStorageNorth;
                }
                return rotatedRuneStorageSouth;
            }
            return staticRuneStorage;
        }

        public Material[] MaterialRuneGenerator(Thing thing)
        {
            /*if (rotatedRuneGeneratorPath != null)
            {
                if (thing.Rotation == Rot4.East || thing.Rotation == Rot4.West)
                {
                    return rotatedRuneGeneratorEast;
                }
                if(thing.Rotation == Rot4.North)
                {
                    return rotatedRuneGeneratorNorth;
                }
                return rotatedRuneGeneratorSouth;
            }*/
            return staticRuneGenerator;
        }
    }
}