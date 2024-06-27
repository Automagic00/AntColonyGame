using UnityEngine;

public static class BoundsIntExt
{
    public static bool Intersects(this BoundsInt a, BoundsInt b)
    {
        return (a.xMin < b.xMax) && (a.xMax > b.xMin) &&
            (a.yMin < b.yMax) && (a.yMax > b.yMin) &&
            (a.zMin < b.zMax) && (a.zMax > b.zMin);
    }
}