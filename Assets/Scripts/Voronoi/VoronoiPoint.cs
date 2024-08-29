public class VoronoiPoint
{
    public float X { get; }
    public float Y { get; }
    public float Weight { get; }

    public VoronoiPoint(float x, float y, float weight = 1.0f)
    {
        X = x;
        Y = y;
        Weight = weight;
    }
}