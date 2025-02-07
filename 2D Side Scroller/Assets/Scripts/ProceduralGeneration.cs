using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ProceduralGeneration : MonoBehaviour
{
    public SpriteShapeController spriteShapeController;
    public Transform player;

    [Header("Terrain Settings")]
    public int segmentCount = 10;   // Points per chunk
    public float segmentWidth = 2f; // Distance between points
    public float noiseScale = 0.5f; // Controls smoothness
    public float heightMultiplier = 5f;

    [Header("Infinite Scrolling")]
    public int chunkSize = 5; // How many chunks exist at a time
    private List<Vector3> points = new List<Vector3>();
    private float lastXPosition = 0f;

    void Start()
    {
        GenerateInitialChunks();
    }

    void Update()
    {
        // Check if player moved forward, generate new chunk
        if (player.position.x > lastXPosition - (chunkSize * segmentWidth))
        {
            GenerateChunk();
        }
    }

    void GenerateInitialChunks()
    {
        for (int i = 0; i < chunkSize; i++)
        {
            GenerateChunk();
        }
    }

    void GenerateChunk()
    {
        Spline spline = spriteShapeController.spline;
        int startIndex = spline.GetPointCount();

        for (int i = 0; i < segmentCount; i++)
        {
            float x = lastXPosition + (i * segmentWidth);
            float y = Mathf.PerlinNoise(x * noiseScale, 0) * heightMultiplier;
            points.Add(new Vector3(x, y, 0));
        }

        // Add points to the Sprite Shape
        for (int i = startIndex; i < points.Count; i++)
        {
            spline.InsertPointAt(i, points[i]);
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        lastXPosition += segmentCount * segmentWidth;

        // Remove old chunks to optimize performance
        if (points.Count > chunkSize * segmentCount)
        {
            points.RemoveRange(0, segmentCount);
            for (int i = 0; i < spline.GetPointCount() - segmentCount; i++)
            {
                spline.SetPosition(i, points[i]);
            }
            for (int i = spline.GetPointCount() - segmentCount; i < spline.GetPointCount(); i++)
            {
                spline.RemovePointAt(i);
            }
        }

        spriteShapeController.BakeCollider(); // Update physics
    }
}
