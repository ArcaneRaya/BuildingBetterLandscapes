using UnityEngine;
using System.Collections;
using System;

public class PlantData : MonoBehaviour {
    public string PlantTag {
        get {
            return plantTag;
        }
    }

    public virtual float MinDistanceToTrees {
        get {
            return minDistanceToTrees;
        }
    }

    [SerializeField] protected float minWaterTableDepth = 2;
    [SerializeField] protected float maxWaterTableDepth = 2;
    [SerializeField] protected float minSunAngle = 20;
    [SerializeField] protected float maxSunAngle = 60;
    [SerializeField] protected string plantTag = "someTag";
    [SerializeField] protected float minDistanceToTrees = 5;

    public bool IsDepthWithinRange(float depth) {
        return depth <= maxWaterTableDepth && depth >= minWaterTableDepth;
    }

    public bool IsSunAngleWithinRange(float sunAngle) {
        return sunAngle <= maxSunAngle && sunAngle >= minSunAngle;
    }
}
