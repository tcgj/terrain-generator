using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

public struct TerrainDensityJob : IJobParallelFor {

    [ReadOnly]
    [DeallocateOnJobCompletion]
    public NativeArray<Vector3> octaveOffsetBuffer;
    public NativeArray<Vector3> vertexBuffer;
    public NativeArray<float> densityBuffer;

    public int numberOfOctaves;
    public int numVertsPerAxis;

    public float chunkSize;
    public float vertSpacing;
    public float lacunarity;
    public float persistence;
    public float scale;
    public float weight;
    public float surfaceOffset;
    public float weightMultiplier;
    public float bedrockHeight;
    public float bedrockWeight;
    public Vector3 center;
    public Vector3 offset;
    public Vector3 mapSize;
    public Vector3 terracing;

    public bool solidifyEdges;

    Vector3 ToVector3(int i) {
        int z = i % numVertsPerAxis;
        int temp = (i - z) / numVertsPerAxis;
        int y = temp % numVertsPerAxis;
        int x = temp / numVertsPerAxis;
        return new Vector3(x, y, z);
    }

    public void Execute(int id) {
        // position of current vertex in world space
        Vector3 offsetPos = Vector3.one * chunkSize / 2;
        Vector3 position = center - offsetPos + ToVector3(id) * vertSpacing;

        // Generate noise
        float density = 0;
        float frequency = scale / 100;
        float amplitude = 1;
        float octaveWeight = 1;
        for (int i = 0; i < numberOfOctaves; i++) {
            float val = SimplexNoise.Evaluate(position * frequency + octaveOffsetBuffer[i] + offset);
            val = 1 - Mathf.Abs(val);
            val *= val;
            val *= octaveWeight;
            density += val * amplitude;

            octaveWeight = Mathf.Max(Mathf.Min(val * weightMultiplier, 1), 0);
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        // Generate surface plane and add noise
        density = -(position.y + surfaceOffset) + density * weight;

        // Generate shelves/terraces
        if (terracing.z > 0) {
            density += (position.y % terracing.x) * terracing.y;
        }

        // Add bedrock layer
        if (position.y < bedrockHeight) {
            density += bedrockWeight;
        }

        // Close mesh edges
        if (solidifyEdges) {
            Vector3 edgeOffset = new Vector3(Mathf.Abs(position.x), Mathf.Abs(position.y), Mathf.Abs(position.z)) - mapSize / 2;
            // if current vertex is at chunk boundary, aka non-negative edge offset
            // then turn into air
            if (edgeOffset.x >= 0 || edgeOffset.y >= 0 || edgeOffset.z >= 0) {
                density = -100;
            }
        }

        vertexBuffer[id] = position;
        densityBuffer[id] = density;
    }
}