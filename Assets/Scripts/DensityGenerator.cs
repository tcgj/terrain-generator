using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DensityGenerator : MonoBehaviour {

    [HideInInspector]
    public bool settingsUpdated;
    public ComputeShader densityShader;

    protected List<ComputeBuffer> additionalBuffers;

    const int threadGroupsPerAxis = 8;

    void OnValidate() {
        settingsUpdated = true;
    }

    public virtual ComputeBuffer Generate(ComputeBuffer vertexBuffer, int numVertsPerAxis, float chunkSize,
            float vertSpacing, Vector3 mapSize, Vector3 center, Vector3 offset) {
        int numThreadsPerAxis = Mathf.CeilToInt((float)numVertsPerAxis / threadGroupsPerAxis);

        int kernelIndex = densityShader.FindKernel("CalculateDensity");
        densityShader.SetBuffer(kernelIndex, "vertexBuffer", vertexBuffer);
        densityShader.SetInt("numVertsPerAxis", numVertsPerAxis);
        densityShader.SetFloat("chunkSize", chunkSize);
        densityShader.SetFloat("vertSpacing", vertSpacing);
        densityShader.SetVector("mapSize", mapSize);
        densityShader.SetVector("center", center);
        densityShader.SetVector("offset", offset);

        densityShader.Dispatch(kernelIndex, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        if (additionalBuffers != null) {
            foreach (ComputeBuffer buffer in additionalBuffers) {
                buffer.Release();
            }
        }

        return vertexBuffer;
    }
}
