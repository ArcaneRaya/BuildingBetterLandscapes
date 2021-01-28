using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamplePoint : MonoBehaviour {
    [Header("Info")]
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private float sunAngle = 0;
    [Space]
    [Header("References")]
    [SerializeField] private Terrain terrain = null;
    [SerializeField] private Transform sunTransform = null;

    private void OnValidate() {
        if (terrain != null) {
            position.y = terrain.SampleHeight(position);
            transform.position = position;

            if (sunTransform != null) {
                float normalizedXPos = (position.x - terrain.GetPosition().x) / terrain.terrainData.size.x;
                float normalizedZPos = (position.z - terrain.GetPosition().z) / terrain.terrainData.size.y;
                sunAngle = Vector3.Angle(terrain.terrainData.GetInterpolatedNormal(normalizedXPos, normalizedZPos), sunTransform.up);
            }
        }
    }
}
