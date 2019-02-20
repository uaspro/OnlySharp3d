using System;
using System.Numerics;

namespace OnlySharp3d
{
    internal struct Sphere : IObject3D
    {
        public Vector3 Center { get; }

        public float Radius { get; }

        public Material Material { get; }

        internal Sphere(Vector3 center, float radius, Material material)
        {
            Center = center;
            Radius = radius;

            Material = material;
        }

        public bool RayIntersect(Vector3 origin, Vector3 projectionDirection, out float distance)
        {
            var originToSphereCenterVector = Center - origin;
            var distanceFromOriginToIntersectionPoint = Vector3.Dot(originToSphereCenterVector, projectionDirection);
            var distanceFromSprereCenterToIntersectionPointSquared = 
                Vector3.Dot(originToSphereCenterVector, originToSphereCenterVector) -
                distanceFromOriginToIntersectionPoint * distanceFromOriginToIntersectionPoint;

            distance = float.NaN;
            if (distanceFromSprereCenterToIntersectionPointSquared > Radius * Radius)
            {
                return false;
            }

            var distanceFromIntersectionPointToSphereSurface =
                MathF.Sqrt(Radius * Radius - distanceFromSprereCenterToIntersectionPointSquared);
            distance = distanceFromOriginToIntersectionPoint - distanceFromIntersectionPointToSphereSurface;

            var distance2 = distanceFromOriginToIntersectionPoint + distanceFromIntersectionPointToSphereSurface;
            if (distance < 0)
            {
                distance = distance2;
            }

            return distance >= 0;
        }
    }
}
