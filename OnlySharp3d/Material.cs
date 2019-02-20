using System.Numerics;

namespace OnlySharp3d
{
    internal struct Material
    {
        public float RefractiveIndex { get; }

        public Albedo Albedo { get; }

        public Vector3 DiffuseColor { get; }

        public float SpecularExponent { get; }

        internal Material(float refractiveIndex, Albedo albedo, Vector3 diffuseColor, float specularExponent)
        {
            RefractiveIndex = refractiveIndex;
            DiffuseColor = diffuseColor;
            Albedo = albedo;
            SpecularExponent = specularExponent;
        }
    }
}
