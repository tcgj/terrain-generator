using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkGenerator : MonoBehaviour {

    [Header("General Settings")]
    public DensityGenerator densityGenerator;
    public Material material;
    public ComputeShader chunkShader;
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

    // Compute Buffers
    ComputeBuffer vertexBuffer;
    ComputeBuffer triangleBuffer;
    ComputeBuffer numTriangleBuffer;

    // Flags
    bool settingsUpdated;

    void OnValidate() {
        settingsUpdated = true;
    }

    void Awake() {
        if (Application.isPlaying && !mapSizeFixed) {
            InitChunkDS();

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
        InitBuffers();

        if (mapSizeFixed) {
            UpdateAllChunks();
        } else if (Application.isPlaying) {
            // Initialise only those visible ones
        }
        if (!Application.isPlaying) {
            ReleaseBuffers();
        }
    }

    void UpdateAllChunks() {
        foreach (Chunk chunk in chunks) {
            UpdateChunk(chunk);
        }
    }

    void UpdateChunk(Chunk chunk) {
        // Determine if chunk should be at maximum detail
        if (chunk.WithinRadius(viewer.position, viewDistance)) {
            // is chunk already at max subdivision
            if (chunk.lod > 0) {
                if (chunk.children == null) {
                    chunk.Split();
                    chunk.mesh.Clear();
                }
                foreach (Chunk child in chunk.children) {
                    UpdateChunk(child);
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
        Vector3 mapSize = (Vector3)numberOfChunks * (chunkSize << levelsOfDetail); // Only used for "edge solidification".
        float vertSpacing = (float)chunk.size / resolution;
        Vector3Int position = chunk.position;
        Vector3 center = GetChunkCenterFromPosition(position);

        // Generate vertex density values
        densityGenerator.Generate(vertexBuffer, numVertsPerAxis, chunk.size, vertSpacing, mapSize, center, densityOffset);

        // Set up compute shader for contouring
        // Currently uses Marching Cubes shader
        int kernelIndex = chunkShader.FindKernel("Contour");
        triangleBuffer.SetCounterValue(0);
        chunkShader.SetBuffer(kernelIndex, "vertexBuffer", vertexBuffer);
        chunkShader.SetBuffer(kernelIndex, "triangleBuffer", triangleBuffer);
        chunkShader.SetInt("resolution", resolution);
        chunkShader.SetFloat("surfaceLevel", surfaceLevel);
        chunkShader.Dispatch(kernelIndex, 8, 8, 8);

        // Obtain vertex result
        int[] numTriangleOut = { 0 };
        ComputeBuffer.CopyCount(triangleBuffer, numTriangleBuffer, 0);
        numTriangleBuffer.GetData(numTriangleOut);
        int numTriangles = numTriangleOut[0];

        Triangle[] triangleList = new Triangle[numTriangles];
        triangleBuffer.GetData(triangleList, 0, 0, numTriangles);

        // Generate mesh
        Mesh mesh = chunk.mesh;
        Vector3[] vertices = new Vector3[numTriangles * 3];
        int[] triangles = new int[numTriangles * 3];

        for (int triIndex = 0; triIndex < numTriangles; triIndex++) {
            for (int cornerIndex = 0; cornerIndex < 3; cornerIndex++) {
                int vertIndex = triIndex * 3 + cornerIndex;
                vertices[vertIndex] = triangleList[triIndex][cornerIndex];
                triangles[vertIndex] = vertIndex;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        chunk.dirty = false;
        chunk.UpdateCollider();
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
    }

    void InitBuffers() {
        // Values per chunk
        int numVertsPerAxis = resolution + 1;
        int numVerts = numVertsPerAxis * numVertsPerAxis * numVertsPerAxis;
        int numVoxels = resolution * resolution * resolution;
        int maxTriangleCount = numVoxels * 5;

        if (!Application.isPlaying || vertexBuffer == null || numVerts != vertexBuffer.count) {
            if (Application.isPlaying) {
                ReleaseBuffers();
            }

            // vertex buffer has size of 4 floats to account for density value;
            vertexBuffer = new ComputeBuffer(numVerts, sizeof(float) * 4);
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            numTriangleBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }
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

    void ReleaseBuffers() {
        if (vertexBuffer != null) {
            vertexBuffer.Release();
        }
        if (triangleBuffer != null) {
            triangleBuffer.Release();
        }
        if (numTriangleBuffer != null) {
            numTriangleBuffer.Release();
        }
    }

    void OnDestroy() {
        ReleaseBuffers();
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
            Vector3 chunkCenter = GetChunkCenterFromPosition(chunk.position);
            Gizmos.color = chunkGizmosColor;
            Gizmos.DrawWireCube(chunkCenter, Vector3.one * chunk.size);
        }
    }
}