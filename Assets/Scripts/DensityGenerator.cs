using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public abstract class DensityGenerator : MonoBehaviour {

    [HideInInspector]
    public bool settingsUpdated;

    void OnValidate() {
        settingsUpdated = true;
    }

    public abstract JobHandle Generate(NativeArray<Vector3> vertexBuffer, NativeArray<float> densityBuffer,
            int numVertsPerAxis, float chunkSize, float vertSpacing, Vector3 mapSize, Vector3 center, Vector3 offset);
}
