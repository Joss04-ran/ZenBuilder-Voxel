using UnityEngine;
using System;

// CaveSettings now controls worm-based cave generation instead of threshold noise.
// Worm caves produce organic tunnels by steering a path through space using Perlin noise.
// All worm parameters are serializable so they can be tuned directly in the Unity Inspector.
[Serializable]
public class CaveSettings
{
    [Header("Worm Count")]
    [Tooltip("How many worms attempt to carve per chunk")]
    [Range(1, 10)]
    public int WormCount = 3;

    [Header("Worm Shape")]
    [Tooltip("How many steps each worm takes")]
    [Range(10, 200)]
    public int WormLength = 80;

    [Tooltip("Carve sphere radius at each step")]
    [Range(0.5f, 5f)]
    public float WormRadius = 2.2f;

    [Tooltip("Distance the worm moves per step")]
    [Range(0.5f, 3f)]
    public float StepSize = 1.5f;

    [Header("Worm Direction")]
    [Tooltip("How sharply the worm can turn per step (degrees)")]
    [Range(5f, 90f)]
    public float TurnSpeed = 35f;

    [Tooltip("Scale of the noise that drives direction changes")]
    [Range(0.5f, 10f)]
    public float DirectionNoiseScale = 3f;

    [Header("Height Limits")]
    [Tooltip("Minimum Y where caves can appear")]
    public int MinCaveY = 1;

    [Tooltip("Maximum Y where caves can reach")]
    public int MaxCaveY = 18;

    [Tooltip("World seed offset for cave noise")]
    public float Seed = 500f;
}