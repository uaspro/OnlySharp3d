using System.Numerics;

namespace OnlySharp3d
{
    internal interface IObject3D
    {
        Vector3 Center { get; }
        Material Material { get; }

        bool RayIntersect(Vector3 origin, Vector3 projectionDirection, out float distance);
    }
}
