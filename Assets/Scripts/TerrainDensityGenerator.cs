using Unity.Collections;
using Unity.Jobs;
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
    [ConditionalHide(nameof(terraceEffect), true, true)]
    public float terraceHeight = 1f;
    [ConditionalHide(nameof(terraceEffect), true, true)]
    public float terraceWeight = 1f;


    public override JobHandle Generate(NativeArray<Vector3> vertexBuffer,
            NativeArray<float> densityBuffer, int numVertsPerAxis, float chunkSize, float vertSpacing,
            Vector3 mapSize, Vector3 center, Vector3 offset) {

        System.Random gen = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[numberOfOctaves];
        // each octave offset goes from -1000 to 1000
        float offsetRange = 1000;
        for (int i = 0; i < numberOfOctaves; i++) {
            octaveOffsets[i] = new Vector3((float)gen.NextDouble() * 2 - 1,
                (float)gen.NextDouble() * 2 - 1, (float)gen.NextDouble() * 2 - 1) * offsetRange;
        }

        var densityJob = new TerrainDensityJob {
            vertexBuffer = vertexBuffer,
            densityBuffer = densityBuffer,
            octaveOffsetBuffer = new NativeArray<Vector3>(octaveOffsets, Allocator.TempJob),
            terracing = new Vector3(terraceHeight, terraceWeight, terraceEffect ? 1 : 0),
            numberOfOctaves = numberOfOctaves,
            numVertsPerAxis = numVertsPerAxis,
            chunkSize = chunkSize,
            vertSpacing = vertSpacing,
            mapSize = mapSize,
            center = center,
            offset = offset,
            lacunarity = lacunarity,
            persistence = persistence,
            scale = scale,
            weight = weight,
            weightMultiplier = weightMultiplier,
            surfaceOffset = surfaceOffset,
            bedrockHeight = bedrockHeight,
            bedrockWeight = bedrockWeight,
            solidifyEdges = solidifyEdges
        };

        return densityJob.Schedule(numVertsPerAxis * numVertsPerAxis * numVertsPerAxis, 128);
    }
}
