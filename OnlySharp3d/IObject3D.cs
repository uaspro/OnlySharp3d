using System.Numerics;

namespace OnlySharp3d
{
    internal interface IObject3D
    {
        RayIntersectionResult RayIntersect(Vector3 origin, Vector3 projectionDirection);
    }
}
