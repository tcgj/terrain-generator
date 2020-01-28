using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkGenerator : MonoBehaviour {

    [Header("General Settings")]
    public DensityGenerator densityGenerator;
    public Material material;
    public bool editorAutoUpdate = true;
    public bool generateColliders;
    public Transform viewer;
    public float viewDistance = 500;

    [Header("Fixed Map Size Settings")]
    public bool mapSizeFixed;
    [ConditionalHide(nameof(mapSizeFixed), true, true)]
    public Vector3Int numberOfChunks = Vector3Int.one;

    [Header("Chunk Settings")]
    public float surfaceLevel;
    [Range(2, 64)]
    public int chunkSize = 2; // At highest detail
    public Vector3 densityOffset;
    [Range(1, 64)]
    public int resolution = 32;
    [Range(0, 9)]
    public int levelsOfDetail = 4;

    [Header("Gizmos")]
    public bool drawChunkGizmos;
    [ConditionalHide(nameof(drawChunkGizmos), true, true)]
    public Color chunkGizmosColor = Color.white;

    // Chunk Data Structures
    string chunkContainerName = "Chunk Container";
    GameObject chunkContainer;
    List<Chunk> chunks;
    Queue<JobData> jobQueue;

    // Flags
    bool settingsUpdated;

    void OnValidate() {
        settingsUpdated = true;
    }

    void Awake() {
        InitChunkDS();
        if (Application.isPlaying && !mapSizeFixed) {
            var existingChunks = FindObjectsOfType<Chunk>();
            foreach (Chunk chunk in existingChunks) {
                chunk.DestroySelf();
            }
        }
    }

    void Start() {
        InitChunks();
    }

    void Update() {
        if (Application.isPlaying) { // Playing
            Run();
        } else if (settingsUpdated || densityGenerator.settingsUpdated) { // Settings were updated
            if (!Application.isPlaying && editorAutoUpdate) {
                InitChunks();
                Run();
            }
            settingsUpdated = false;
            densityGenerator.settingsUpdated = false;
        }
    }

    void Run() {
        if (mapSizeFixed) {
            UpdateAllChunks();
        } else if (Application.isPlaying) {
            // Initialise only those visible ones
        }

        int jobCount = jobQueue.Count;
        for (int i = 0; i < jobCount; i++) {
            JobData jobData = jobQueue.Dequeue();
            if (jobData.jobHandle.IsCompleted) {
                UpdateChunkMesh(jobData);
            } else {
                jobQueue.Enqueue(jobData);
            }
        }
    }

    void UpdateAllChunks() {
        foreach (Chunk chunk in chunks) {
            RequestUpdateChunk(chunk);
        }
    }

    void RequestUpdateChunk(Chunk chunk) {
        // Determine if chunk should be at maximum detail
        if (chunk.WithinRadius(viewer.position, viewDistance)) {
            // is chunk already at max subdivision
            if (chunk.lod > 0) {
                if (chunk.children == null) {
                    chunk.Split();
                    chunk.mesh.Clear();
                }
                foreach (Chunk child in chunk.children) {
                    RequestUpdateChunk(child);
                }
                return;
            }
        } else {
            chunk.Merge();
        }

        // Only render if any changes are made to the chunk
        if (!chunk.dirty) {
            return;
        }

        int numVertsPerAxis = resolution + 1;
        int numVerts = numVertsPerAxis * numVertsPerAxis * numVertsPerAxis;
        int numVoxels = resolution * resolution * resolution;
        Vector3 mapSize = (Vector3)numberOfChunks * (chunkSize << levelsOfDetail); // Only used for "edge solidification".
        float vertSpacing = (float)chunk.size / resolution;
        Vector3 center = chunk.position;

        var vertexBuffer = new NativeArray<Vector3>(numVerts, Allocator.TempJob);
        var densityBuffer = new NativeArray<float>(numVerts, Allocator.TempJob);
        var triangleBuffer = new NativeQueue<Triangle>(Allocator.TempJob);

        // Generate vertex density values
        JobHandle densityJobHandle = densityGenerator.Generate(vertexBuffer, densityBuffer, numVertsPerAxis, chunk.size,
                vertSpacing, mapSize, center, densityOffset);

        var marchJob = new MarchingCubesJob(vertexBuffer, densityBuffer, triangleBuffer.AsParallelWriter(), resolution, surfaceLevel);
        JobHandle marchJobHandle = marchJob.Schedule(numVoxels, 128, densityJobHandle);

        jobQueue.Enqueue(new JobData(chunk, marchJobHandle, triangleBuffer));
        chunk.dirty = false;
    }

    void UpdateChunkMesh(JobData jobData) {
        // Generate mesh
        jobData.jobHandle.Complete();
        Chunk chunk = jobData.chunk;
        if (chunk != null && !chunk.IsDestroyed()) {
            Mesh mesh = chunk.mesh;
            NativeQueue<Triangle> triangleBuffer = jobData.triangleBuffer;
            int numTriangles = triangleBuffer.Count;
            Vector3[] vertices = new Vector3[numTriangles * 3];
            int[] triangles = new int[numTriangles * 3];

            for (int triIndex = 0; triIndex < numTriangles; triIndex++) {
                Triangle tri = triangleBuffer.Dequeue();
                for (int cornerIndex = 0; cornerIndex < 3; cornerIndex++) {
                    int vertIndex = triIndex * 3 + cornerIndex;
                    vertices[vertIndex] = tri[cornerIndex];
                    triangles[vertIndex] = vertIndex;
                }
            }

            // Set mesh data
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            chunk.UpdateCollider();
        }

        // Release buffers
        jobData.ReleaseBuffers();
    }

    // If container exists, re-obtain reference. Otherwise, create it.
    void GetChunkContainer() {
        chunkContainer = GameObject.Find(chunkContainerName);
        if (chunkContainer == null) {
            chunkContainer = new GameObject(chunkContainerName);
        }
    }

    Vector3 GetChunkCenterFromPosition(Vector3Int position) {
        return (Vector3)position;
    }

    void InitChunkDS() {
        chunks = new List<Chunk>();
        jobQueue = new Queue<JobData>();
    }

    // (Re)Initialize chunks
    // Adds new chunks as necessary, and removes those unnecessary.
    void InitChunks() {
        GetChunkContainer();
        chunks = new List<Chunk>();
        foreach (Chunk chunk in FindObjectsOfType<Chunk>()) {
            chunk.DestroySelf();
        }
        for (int x = 0; x < numberOfChunks.x; x++) {
            for (int y = 0; y < numberOfChunks.y; y++) {
                for (int z = 0; z < numberOfChunks.z; z++) {
                    Vector3Int position = new Vector3Int(x, y, z) * (chunkSize << levelsOfDetail);
                    Chunk chunkToAdd;
                    chunkToAdd = CreateChunk(position);
                    chunkToAdd.InitializeMesh(material, generateColliders);
                    chunks.Add(chunkToAdd);
                }
            }
        }
    }

    Chunk CreateChunk(Vector3Int position) {
        GameObject chunkObj = new GameObject($"Chunk@({position.x}, {position.y}, {position.z})");
        chunkObj.transform.parent = chunkContainer.transform;
        Chunk chunk = chunkObj.AddComponent<Chunk>();
        chunk.lod = levelsOfDetail;
        chunk.size = chunkSize << levelsOfDetail;
        chunk.position = position;

        return chunk;
    }

    void OnDrawGizmos() {
        if (drawChunkGizmos) {
            foreach (Chunk chunk in chunks) {
                DrawChunkBoundaries(chunk);
            }
        }
    }

    void DrawChunkBoundaries(Chunk chunk) {
        if (chunk.children != null) {
            foreach (Chunk child in chunk.children) {
                DrawChunkBoundaries(child);
            }
        } else {
            Vector3 chunkCenter = chunk.position;
            Gizmos.color = chunkGizmosColor;
            Gizmos.DrawWireCube(chunkCenter, Vector3.one * chunk.size);
        }
    }

    struct JobData {
        public Chunk chunk;
        public JobHandle jobHandle;
        public NativeQueue<Triangle> triangleBuffer;

        public JobData(Chunk chunk, JobHandle jobHandle, NativeQueue<Triangle> triangleBuffer) {
            this.chunk = chunk;
            this.jobHandle = jobHandle;
            this.triangleBuffer = triangleBuffer;
        }

        public void ReleaseBuffers() {
            if (triangleBuffer.IsCreated) {
                triangleBuffer.Dispose();
            }
        }
    }
}