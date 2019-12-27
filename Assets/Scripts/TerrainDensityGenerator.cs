using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDensityGenerator : DensityGenerator {

    [Header("Noise Settings")]
    public int seed;
    [Range(1, 10)]
    public int numberOfOctaves = 4;
    public float lacunarity = 2f;
    public float persistence = 0.5f;
    public float scale = 1f;
    public float weight = 1f;
    public float weightMultiplier = 1f;

    [Header("Terrain Settings")]
    public float surfaceOffset = 1f;
    public float bedrockHeight;
    public float bedrockWeight;
    public bool solidifyEdges;
    public bool terraceEffect;
    [ConditionalHide("terraceEffect", true)]
    public float terraceHeight = 1f;
    [ConditionalHide("terraceEffect", true)]
    public float terraceWeight = 1f;


    public override ComputeBuffer Generate(ComputeBuffer vertexBuffer, int numVertsPerAxis, float chunkSize,
            float vertSpacing, Vector3 mapSize, Vector3 center, Vector3 offset) {
        additionalBuffers = new List<ComputeBuffer>();

        System.Random gen = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[numberOfOctaves];
        // each octave offset goes from -1000 to 1000
        float offsetRange = 1000;
        for (int i = 0; i < numberOfOctaves; i++) {
            octaveOffsets[i] = new Vector3((float)gen.NextDouble() * 2 - 1,
                (float)gen.NextDouble() * 2 - 1, (float)gen.NextDouble() * 2 - 1) * offsetRange;
        }

        int kernelIndex = densityShader.FindKernel("CalculateDensity");
        ComputeBuffer octaveOffsetBuffer = new ComputeBuffer(numberOfOctaves, sizeof(float) * 3);
        octaveOffsetBuffer.SetData(octaveOffsets);
        additionalBuffers.Add(octaveOffsetBuffer);
        densityShader.SetBuffer(kernelIndex, "octaveOffsetBuffer", octaveOffsetBuffer);
        densityShader.SetVector("terracing", new Vector3(terraceHeight, terraceWeight, terraceEffect ? 1 : 0));
        densityShader.SetInt("numberOfOctaves", numberOfOctaves);
        densityShader.SetFloat("lacunarity", lacunarity);
        densityShader.SetFloat("persistence", persistence);
        densityShader.SetFloat("scale", scale);
        densityShader.SetFloat("weight", weight);
        densityShader.SetFloat("weightMultiplier", weightMultiplier);
        densityShader.SetFloat("surfaceOffset", surfaceOffset);
        densityShader.SetFloat("bedrockHeight", bedrockHeight);
        densityShader.SetFloat("bedrockWeight", bedrockWeight);
        densityShader.SetBool("solidifyEdges", solidifyEdges);

        return base.Generate(vertexBuffer, numVertsPerAxis, chunkSize, vertSpacing, mapSize, center, offset);
    }
}
