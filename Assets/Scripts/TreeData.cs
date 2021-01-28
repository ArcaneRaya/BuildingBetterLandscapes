using UnityEngine;
using System.Collections;

public enum TreeVariations {
    Big,
    Medium,
    Small
}

public class TreeData : PlantData {
    [SerializeField] public TreeVariations activeVariation = TreeVariations.Medium;
    [SerializeField] private GameObject bigVariationPrefab = null;
    [SerializeField] private GameObject mediumVariationPrefab = null;
    [SerializeField] private GameObject smallVariationPrefab = null;
    [SerializeField] private float minDistanceBigVariation = 5;
    [SerializeField] private float minDistanceMediumVariation = 3;
    [SerializeField] private float minDistanceSmallVariation = 1;

    public float GetMinDistanceToTreesForVariation(TreeVariations variation) {
        switch (variation) {
            case TreeVariations.Big:
                return minDistanceBigVariation;
            case TreeVariations.Medium:
                return minDistanceMediumVariation;
            case TreeVariations.Small:
                return minDistanceSmallVariation;
            default:
                return 1;
        }
    }

    public TreeVariations GetTreeVariation(float sunAngle) {
        float normalizedAngle = (sunAngle - minSunAngle) / (maxSunAngle - minSunAngle);
        normalizedAngle += Random.Range(-0.16f, 0.16f);
        normalizedAngle = Mathf.Clamp01(normalizedAngle);
        if (normalizedAngle < 0.33f) {
            return TreeVariations.Big;
        } else if (normalizedAngle < 0.66f) {
            return TreeVariations.Medium;
        } else {
            return TreeVariations.Small;
        }
    }

    public void SetVariation(TreeVariations variation) {
        activeVariation = variation;
        if (transform.childCount > 0) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        switch (variation) {
            case TreeVariations.Big:
                Instantiate(bigVariationPrefab, transform);
                break;
            case TreeVariations.Medium:
                Instantiate(mediumVariationPrefab, transform);
                break;
            case TreeVariations.Small:
                Instantiate(smallVariationPrefab, transform);
                break;
        }
    }

    [ContextMenu("UpdateVisual")]
    private void UpdateVisual() {
        if (bigVariationPrefab == null || mediumVariationPrefab == null || smallVariationPrefab == null) {
            return;
        }
        SetVariation(activeVariation);
    }
}
