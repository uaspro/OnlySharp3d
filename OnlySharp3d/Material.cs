namespace OnlySharp3d
{
    internal struct Material
    {
        public ColorF DiffuseColor { get; }

        public Albedo Albedo { get; }

        public float SpecularExponent { get; }

        public float RefractiveIndex { get; }

        internal Material(ColorF diffuseColor, Albedo albedo, float specularExponent, float refractiveIndex)
        {
            DiffuseColor = diffuseColor;
            Albedo = albedo;
            SpecularExponent = specularExponent;
            RefractiveIndex = refractiveIndex;
        }
    }
}
