using System;
using UnityEngine;

[Serializable]
public class MapData
{
    [Header("Globals Properties")]
    [Range(1, 50)]
    public int width;

    [Range(1, 50)]
    public int height;

    [Header("Squares Properties")]
    public SquareData[] grid;

    [Range(0, 100)]
    public int percentSquareLock = 50;

    [Header("Edges Properties")]
    public bool[] edgesHori;
    [Range(0, 100)]
    public int percenteEdgeHori = 50;

    public bool[] edgesVert;
    [Range(0, 100)]
    public int percentEdgeVert = 50;

}

public enum SquareState
{
    Normal,
    Lock,
    Water,
    Grass,
    Special
}

public enum Alignment
{
    Neutral,
    IA,
    Player
}

[Serializable]
public struct SquareData
{
    public SquareState state;

    public Alignment aligment;
}