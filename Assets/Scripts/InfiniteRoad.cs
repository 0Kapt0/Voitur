using System.Collections.Generic;
using UnityEngine;

public class InfiniteRoad : MonoBehaviour
{
    [Header("Route")]
    public Transform car;
    public Material roadMaterial;
    public int pointsPerChunk = 20;
    public float distanceBetweenPoints = 4f;
    public float curveStrength = 6f;
    public float curveSmoothness = 0.05f;
    public float roadWidth = 8f;
    public float textureRepeat = 0.1f;
    public float generateDistance = 300f;
    public float deleteDistance = 200f;

    [Header("Décor")]
    public Material barrierMaterial;
    public float barrierHeight = 0.8f;
    public GameObject treePrefab;
    public float minTreeSize = 0.7f;
    public float maxTreeSize = 1.5f;
    public float treeChance = 0.3f;
    public float treeMaxDistance = 12f;

    private List<GameObject> chunks = new List<GameObject>();
    private Vector3 nextPosition = Vector3.back * 20f;
    private float angle;
    private float seed;
    private int pointIndex;

    void Start()
    {
        seed = Random.value * 1000f;
        ManageChunks();
    }

    void Update()
    {
        ManageChunks();
    }

    void ManageChunks()
    {
        while (Vector3.Distance(car.position, nextPosition) < generateDistance)
            CreateChunk();

        while (chunks.Count > 0 && Vector3.Distance(car.position, chunks[0].transform.position) > deleteDistance)
        {
            Destroy(chunks[0].GetComponent<MeshFilter>().sharedMesh);
            Destroy(chunks[0]);
            chunks.RemoveAt(0);
        }
    }

    void CreateChunk()
    {
        Vector3 startPosition = nextPosition;
        Vector3 localPosition = Vector3.zero;
        Vector3[] vertices = new Vector3[(pointsPerChunk + 1) * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        List<int> triangles = new List<int>();

        for (int i = 0; i <= pointsPerChunk; i++)
        {
            Vector3 forward = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            vertices[i * 2] = localPosition - right * roadWidth / 2;
            vertices[i * 2 + 1] = localPosition + right * roadWidth / 2;

            float textureY = pointIndex * distanceBetweenPoints * textureRepeat;
            uvs[i * 2] = new Vector2(0, textureY);
            uvs[i * 2 + 1] = new Vector2(1, textureY);

            if (i < pointsPerChunk)
            {
                int a = i * 2;
                triangles.AddRange(new int[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });

                angle += (Mathf.PerlinNoise(pointIndex * curveSmoothness, seed) - 0.5f) * 2f * curveStrength;
                localPosition += forward * distanceBetweenPoints;
                pointIndex++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject chunk = new GameObject("RoadChunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        chunk.transform.position = startPosition;
        chunk.transform.SetParent(transform);
        chunk.GetComponent<MeshFilter>().sharedMesh = mesh;
        chunk.GetComponent<MeshRenderer>().sharedMaterial = roadMaterial;
        chunk.GetComponent<MeshCollider>().sharedMesh = mesh;

        Decorate(chunk.transform, vertices);

        chunks.Add(chunk);
        nextPosition = startPosition + localPosition;
    }

    void Decorate(Transform chunk, Vector3[] vertices)
    {
        for (int i = 0; i + 2 <= pointsPerChunk; i += 2)
        {
            for (int side = 0; side < 2; side++)
            {
                Vector3 start = vertices[i * 2 + side];
                Vector3 end = vertices[(i + 2) * 2 + side];
                Vector3 otherSide = vertices[i * 2 + 1 - side];
                Vector3 outward = (start - otherSide).normalized;

                CreateBarrier(chunk, start + outward * 0.3f, end + outward * 0.3f);

                if (treePrefab != null && Random.Range(0f, 1f) < treeChance)
                    CreateTree(chunk, start + outward * Random.Range(3f, treeMaxDistance));
            }
        }
    }

    void CreateBarrier(Transform chunk, Vector3 start, Vector3 end)
    {
        GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrier.transform.SetParent(chunk, false);
        barrier.transform.localPosition = (start + end) / 2 + Vector3.up * barrierHeight / 2;
        barrier.transform.localRotation = Quaternion.LookRotation(end - start);
        barrier.transform.localScale = new Vector3(0.3f, barrierHeight, (end - start).magnitude * 1.1f);

        if (barrierMaterial != null)
            barrier.GetComponent<Renderer>().sharedMaterial = barrierMaterial;
    }

    void CreateTree(Transform chunk, Vector3 position)
    {
        GameObject tree = Instantiate(treePrefab, chunk);
        tree.transform.localPosition = position;
        tree.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        tree.transform.localScale = treePrefab.transform.localScale * Random.Range(minTreeSize, maxTreeSize);
    }
}