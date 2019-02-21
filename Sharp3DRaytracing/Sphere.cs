using System;
using System.Numerics;

namespace Sharp3DRaytracing
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

        public RayIntersectionResult RayIntersect(Vector3 origin, Vector3 projectionDirection)
        {
            var originToSphereCenterVector = Center - origin;
            var distanceFromOriginToIntersectionPoint = Vector3.Dot(originToSphereCenterVector, projectionDirection);
            var distanceFromSprereCenterToIntersectionPointSquared =
                Vector3.Dot(originToSphereCenterVector, originToSphereCenterVector) -
                distanceFromOriginToIntersectionPoint * distanceFromOriginToIntersectionPoint;

            if (distanceFromSprereCenterToIntersectionPointSquared > Radius * Radius)
            {
                return default(RayIntersectionResult);
            }

            var distanceFromIntersectionPointToSphereSurface =
                MathF.Sqrt(Radius * Radius - distanceFromSprereCenterToIntersectionPointSquared);

            var distance = distanceFromOriginToIntersectionPoint - distanceFromIntersectionPointToSphereSurface;

            var distance2 = distanceFromOriginToIntersectionPoint + distanceFromIntersectionPointToSphereSurface;
            if (distance < 0)
            {
                distance = distance2;
            }

            if (distance < 0)
            {
                return default(RayIntersectionResult);
            }

            var hitPoint = origin + projectionDirection * distance;
            var normalVector = Vector3.Normalize(hitPoint - Center);
            return new RayIntersectionResult(true, distance, hitPoint, normalVector, Material);
        }
    }
}
