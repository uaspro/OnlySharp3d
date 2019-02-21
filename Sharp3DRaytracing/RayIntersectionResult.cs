using System.Numerics;

namespace Sharp3DRaytracing
{
    internal struct RayIntersectionResult
    {
        public bool IsIntersect { get; }

        public float Distance { get; }

        public Vector3 HitPoint { get; }

        public Vector3 NormalVector { get; }

        public Material Material { get; }

        public RayIntersectionResult(
            bool isIntersect, float distance, Vector3 hitPoint, Vector3 normalVector, Material material)
        {
            IsIntersect = isIntersect;
            Distance = distance;
            HitPoint = hitPoint;
            NormalVector = normalVector;
            Material = material;
        }
    }
}
