using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class TerrainController : MonoBehaviour {
    [SerializeField] private TreeData[] treeTypes = null;
    [SerializeField] private PlantData[] bottomTypes = null;
    [SerializeField] private Terrain terrain = null;
    [SerializeField] private Transform sunTransform = null;
    [SerializeField] private bool doShuffle = false;
    [SerializeField] private bool useRandomOffset = false;

    [ContextMenu("Populate Terrain")]
    public void PopulateTerrain() {
        ResetTerrain();

        Vector3 terrainPosition = terrain.GetPosition();
        Vector3 terrainSize = terrain.terrainData.size;
        PlantFoliage(terrainPosition, terrainSize);
    }

    private void PlantFoliage(Vector3 terrainPosition, Vector3 terrainSize) {
        Vector3 offset;
        Vector3 randomOffset;
        Vector3 samplePosition;

        List<Vector3> potentialPositions = new List<Vector3>();

        for (int x = 0; x < terrainSize.x; x++) {
            for (int z = 0; z < terrainSize.y; z++) {
                offset = new Vector3(x, 0, z);
                if (useRandomOffset) {
                    randomOffset = new Vector3(UnityEngine.Random.Range(-0.8f, 0.8f), 0, UnityEngine.Random.Range(-0.8f, 0.8f));
                    offset += randomOffset;
                }
                if (offset.x < 0 || offset.x > terrainSize.x || offset.z < 0 || offset.z > terrainSize.y) {
                    continue;
                }
                samplePosition = terrainPosition + offset;
                samplePosition.y = terrain.SampleHeight(samplePosition);
                potentialPositions.Add(samplePosition);
                //if (z != 0) {
                //    offset = new Vector3(x, 0, z);
                //    samplePosition = terrainPosition + offset;
                //    samplePosition.y = terrain.SampleHeight(samplePosition);
                //    float waterDepth = GetWaterDepth(samplePosition);
                //    TryPlantTree(samplePosition, waterDepth);
                //}
                //z += UnityEngine.Random.Range(0, 3);
            }
        }

        if (doShuffle) {
            potentialPositions.Shuffle();
        }

        foreach (var potentialPosition in potentialPositions) {
            float waterDepth = GetWaterDepth(potentialPosition);
            float sunAngle = GetSunAngle(potentialPosition);
            TryPlantTree(potentialPosition, waterDepth, sunAngle);
        }

        foreach (var potentialPosition in potentialPositions) {
            float waterDepth = GetWaterDepth(potentialPosition);
            float sunAngle = GetSunAngle(potentialPosition);
            TryPlantBottom(potentialPosition, waterDepth, sunAngle);
        }
    }

    private void TryPlantTree(Vector3 samplePosition, float waterDepth, float sunAngle) {
        List<TreeData> treeOptions = new List<TreeData>(treeTypes);
        if (doShuffle) {
            treeOptions.Shuffle();
        }
        while (treeOptions.Count > 0) {
            TreeData option = treeOptions[0];
            treeOptions.Remove(option);
            if (option.IsDepthWithinRange(waterDepth) && option.IsSunAngleWithinRange(sunAngle)) {
                TreeVariations variation = option.GetTreeVariation(sunAngle);
                bool canPlantHere = CheckTreeDistancesForPotentialTreePosition(option, variation, samplePosition);
                if (canPlantHere) {
                    GameObject InstantiatedObject = Instantiate(option.gameObject, samplePosition, Quaternion.identity, transform);
                    InstantiatedObject.GetComponent<TreeData>().SetVariation(variation);
#if UNITY_EDITOR
                    if (Application.isPlaying == false) {
                        // Register root object for undo.
                        Undo.RegisterCreatedObjectUndo(InstantiatedObject, "Create object");
                    }
#endif
                    return;
                }
            }
        }
    }

    private void TryPlantBottom(Vector3 samplePosition, float waterDepth, float sunAngle) {
        List<PlantData> bottomOptions = new List<PlantData>(bottomTypes);
        bottomOptions.Shuffle();
        while (bottomOptions.Count > 0) {
            PlantData option = bottomOptions[0];
            bottomOptions.Remove(option);
            if (option.IsDepthWithinRange(waterDepth)) {
                bool canPlantHere = CheckTreeDistancesForPotentialBottomPosition(option, samplePosition);
                if (canPlantHere) {
                    GameObject InstantiatedObject = Instantiate(option.gameObject, samplePosition, Quaternion.identity, transform);
#if UNITY_EDITOR
                    if (Application.isPlaying == false) {
                        // Register root object for undo.
                        Undo.RegisterCreatedObjectUndo(InstantiatedObject, "Create object");
                    }
#endif
                    return;
                }
            }
        }
    }

    private bool CheckTreeDistancesForPotentialBottomPosition(PlantData option, Vector3 samplePosition) {
        foreach (Collider coll in Physics.OverlapSphere(samplePosition, option.MinDistanceToTrees)) {
            TreeData plantData = coll.GetComponentInParent<TreeData>();
            if (plantData != null) {
                return false;
            }
        }
        return true;
    }

    private bool CheckTreeDistancesForPotentialTreePosition(TreeData option, TreeVariations variation, Vector3 samplePosition) {
        foreach (Collider coll in Physics.OverlapSphere(samplePosition, option.GetMinDistanceToTreesForVariation(variation))) {
            TreeData plantData = coll.GetComponentInParent<TreeData>();
            if (plantData != null) {
                return false;
            }
        }

        foreach (var treeType in treeTypes) {
            if (CheckCollisionsFor(samplePosition, treeType, TreeVariations.Big)) {
                return false;
            }
            if (CheckCollisionsFor(samplePosition, treeType, TreeVariations.Medium)) {
                return false;
            }
            if (CheckCollisionsFor(samplePosition, treeType, TreeVariations.Small)) {
                return false;
            }
        }

        return true;
    }

    private bool CheckCollisionsFor(Vector3 samplePosition, TreeData treeType, TreeVariations variation) {
        foreach (Collider coll in Physics.OverlapSphere(samplePosition, treeType.GetMinDistanceToTreesForVariation(variation))) {
            TreeData treeData = coll.GetComponentInParent<TreeData>();
            if (treeData != null) {
                if (treeData.PlantTag == treeType.PlantTag && treeData.activeVariation == variation) {
                    return true;
                }
            }
        }
        return false;
    }

    [ContextMenu("Reset Terrain")]
    public void ResetTerrain() {
        var plantObjects = GetComponentsInChildren<PlantData>();
        for (int i = plantObjects.Length - 1; i >= 0; i--) {
            if (Application.isPlaying) {
                Destroy(plantObjects[i].gameObject);
            } else {
                DestroyImmediate(plantObjects[i].gameObject);
            }
        }
    }

    private float GetWaterDepth(Vector3 samplePosition) {
        float terrainHeight = terrain.SampleHeight(samplePosition);
        return Mathf.Max(0, 2 + terrainHeight * 0.5f);
    }

    private float GetSunAngle(Vector3 samplePosition) {
        float normalizedXPos = (samplePosition.x) / terrain.terrainData.size.x;
        float normalizedZPos = (samplePosition.z) / terrain.terrainData.size.y;
        float sunAngle = Vector3.Angle(terrain.terrainData.GetInterpolatedNormal(normalizedXPos, normalizedZPos), sunTransform.up);
        return sunAngle;
    }
}


