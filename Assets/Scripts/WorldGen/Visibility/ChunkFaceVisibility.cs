public sealed class ChunkFaceVisibility
{
    public const int FaceCount = 6;

    private readonly bool[,] _graph = new bool[FaceCount, FaceCount];

    public bool CanSeeThrough(int fromFace, int toFace)
        => _graph[fromFace, toFace];

    public void Connect(int a, int b)
    {
        _graph[a, b] = true;
        _graph[b, a] = true;
    }
    public static int Opposite(int face) => face ^ 1;
    public static readonly int[] HorizontalFaces = { 0, 1, 4, 5 };

    public static readonly int[] NDX = { 0, 0, 0, 0, -1, 1 };
    public static readonly int[] NDZ = { -1, 1, 0, 0, 0, 0 };
}