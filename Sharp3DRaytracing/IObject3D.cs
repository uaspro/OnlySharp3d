using System.Numerics;

namespace Sharp3DRaytracing
{
    internal interface IObject3D
    {
        RayIntersectionResult RayIntersect(Vector3 origin, Vector3 projectionDirection);
    }
}
