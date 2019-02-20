namespace OnlySharp3d
{
    public class Albedo
    {
        public float DiffuseKoef { get; }

        public float SpecularKoef { get; }

        public float ReflectKoef { get; }

        public float RefractKoef { get; }

        public Albedo(float diffuseKoef, float specularKoef, float reflectKoef, float refractKoef)
        {
            DiffuseKoef = diffuseKoef;
            SpecularKoef = specularKoef;
            ReflectKoef = reflectKoef;
            RefractKoef = refractKoef;
        }
    }
}
