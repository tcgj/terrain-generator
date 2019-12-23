using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    public Vector3Int position;

    [HideInInspector]
    public Mesh mesh;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public void DestroyChunk() {
        if (Application.isPlaying) {
            mesh.Clear();
            Destroy(gameObject);
        } else {
            DestroyImmediate(gameObject, false);
        }
    }

    public void InitializeMesh(Material mat, bool hasCollider) {

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
}
