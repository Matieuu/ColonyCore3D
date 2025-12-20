using System.Runtime.InteropServices;
using Silk.NET.Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Ray {

    public float OriginX;
    public float OriginY;
    public float OriginZ;

    public float DirectionX;
    public float DirectionY;
    public float DirectionZ;

    public static Ray FromVectors(Vector3D<float> origin, Vector3D<float> direction) => new Ray {
        OriginX = origin.X,
        OriginY = origin.Y,
        OriginZ = origin.Z,
        DirectionX = direction.X,
        DirectionY = direction.Y,
        DirectionZ = direction.Z
    };

}

[StructLayout(LayoutKind.Sequential)]
public struct RaycastResult {
    public byte Hit;
    public int X;
    public int Y;
    public int Z;
    public byte Face;
}
