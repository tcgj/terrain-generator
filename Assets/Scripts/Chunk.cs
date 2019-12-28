using UnityEngine;

public class Chunk : MonoBehaviour {

    public Vector3Int position;

    [HideInInspector]
    public Mesh mesh;
    [HideInInspector]
    public float[] densities;

    // LOD
    [HideInInspector]
    public int lod;
    [HideInInspector]
    public int size;
    [HideInInspector]
    public Chunk[] children = new Chunk[8];

    // Mesh
    bool hasCollider;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public void DestroyChunk() {
        foreach (Chunk child in children) {
            if (child != null) {
                child.DestroyChunk();
            }
        }
        if (Application.isPlaying) {
            mesh.Clear();
            Destroy(gameObject);
        } else {
            DestroyImmediate(gameObject, false);
        }
    }

    public void InitializeMesh(Material mat, bool hasCollider) {
        this.hasCollider = hasCollider;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        if (meshFilter == null) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (meshRenderer == null) {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (meshCollider == null && hasCollider) {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        } else if (meshCollider != null && !hasCollider) {
            DestroyImmediate(meshCollider);
        }

        mesh = meshFilter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }

        meshRenderer.material = mat;

        if (hasCollider) {
            // force update
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    public bool WithinRadius(Vector3 viewerPos, float radius) {
        if (Vector3.Distance(viewerPos, position) < radius + size / 2f) {
            return true;
        }
        return false;
    }

    public void Split() {
        int childSize = size / 2;
        for (int i = 0; i < 8; i++) {
            GameObject chunkChild = new GameObject(gameObject.name + $"#child{i}");
            chunkChild.transform.parent = transform;
            children[i] = chunkChild.AddComponent<Chunk>();
            children[i].size = childSize;
            children[i].lod = lod - 1;
            children[i].InitializeMesh(meshRenderer.sharedMaterial, hasCollider);
        }
        int childHalfLength = childSize / 2;
        children[0].position = new Vector3Int(position.x + childHalfLength, position.y + childHalfLength, position.z + childHalfLength);
        children[1].position = new Vector3Int(position.x + childHalfLength, position.y + childHalfLength, position.z - childHalfLength);
        children[2].position = new Vector3Int(position.x - childHalfLength, position.y + childHalfLength, position.z - childHalfLength);
        children[3].position = new Vector3Int(position.x - childHalfLength, position.y + childHalfLength, position.z + childHalfLength);
        children[4].position = new Vector3Int(position.x + childHalfLength, position.y - childHalfLength, position.z + childHalfLength);
        children[5].position = new Vector3Int(position.x + childHalfLength, position.y - childHalfLength, position.z - childHalfLength);
        children[6].position = new Vector3Int(position.x - childHalfLength, position.y - childHalfLength, position.z - childHalfLength);
        children[7].position = new Vector3Int(position.x - childHalfLength, position.y - childHalfLength, position.z + childHalfLength);
    }
}
