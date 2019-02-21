namespace OnlySharp3d
{
    internal struct Material
    {
        public float RefractiveIndex { get; }

        public Albedo Albedo { get; }

        public ColorF DiffuseColor { get; }

        public float SpecularExponent { get; }

        internal Material(float refractiveIndex, Albedo albedo, ColorF diffuseColor, float specularExponent)
        {
            RefractiveIndex = refractiveIndex;
            DiffuseColor = diffuseColor;
            Albedo = albedo;
            SpecularExponent = specularExponent;
        }
    }
}
