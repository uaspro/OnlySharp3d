using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace OnlySharp3d
{
    internal class Program
    {
        private const int Width = 1920 * 4;
        private const int Height = 1080 * 4;
        private const float Fov = MathF.PI / 2;
        private const int CastRayRecursionDepth = 3;

        private const string EnvMapPath = @"envmap.jpg";
        private const string ResultFramePath = @"ResultFrame.bmp";

        private static void Main(string[] args)
        {
            var materialIvory = new Material(1f, new Albedo(0.6f, 0.3f, 0.1f, 0f), new Vector3(0.4f, 0.4f, 0.3f), 50f);
            var materialRed = new Material(1f, new Albedo(0.9f, 0.1f, 0.0f, 0f), new Vector3(0.3f, 0.1f, 0.1f), 10f);
            var materialMirror = new Material(1f, new Albedo(0f, 10f, 0.8f, 0f), new Vector3(1.0f, 1.0f, 1.0f), 1425f);
            var materialGlass = new Material(
                1.5f, new Albedo(0f, 0.5f, 0.1f, 0.8f), new Vector3(0.6f, 0.7f, 0.8f), 125f);

            var scene = new List<IObject3D>
            {
                new Sphere(new Vector3(-3f, 0f, -16f), 2, materialIvory),
                new Sphere(new Vector3(-1f, -1.5f, -12f), 2, materialGlass),
                new Sphere(new Vector3(1.5f, -0.5f, -18f), 3, materialRed),
                new Sphere(new Vector3(7f, 5f, -18f), 4, materialMirror)
            };

            var lightSources = new List<LightSource>
            {
                new LightSource(new Vector3(-20f, 20f, 20f), 1.5f),
                new LightSource(new Vector3(30f, 50f, -25f), 1.8f),
                new LightSource(new Vector3(30f, 20f, 30f), 1.7f)
            };

            Render(Vector3.Zero, scene, lightSources);
        }

        private static void Render(Vector3 cameraPosition, List<IObject3D> scene, List<LightSource> lightSources)
        {
            var frame = new ImageF(Width, Height);
            var envMap = new ImageF(EnvMapPath);
            Parallel.For(
                0, Height, j =>
                {
                    for (var i = 0; i < Width; i++)
                    {
                        var x = (2 * (i + 1e-3f) / Width - 1) * MathF.Tanh(Fov / 2f) * (Width / (float) Height);
                        var y = -(2 * (j + 1e-3f) / Height - 1) * MathF.Tanh(Fov / 2f);

                        var projectionDirection = Vector3.Normalize(new Vector3(x, y, -1f));
                        frame.Buffer[i][j] = CastRay(envMap, cameraPosition, projectionDirection, scene, lightSources);
                    }
                });

            frame.SaveToFile(ResultFramePath);
        }

        private static Vector3 CastRay(
            ImageF envMap, Vector3 origin, Vector3 projectionDirection, List<IObject3D> scene,
            List<LightSource> lightSources, int depth = 0)
        {
            if (depth > CastRayRecursionDepth ||
                !SceneIntersect(
                    origin, projectionDirection, scene, out var hitPoint, out var normalVector, out var material))
            {
                var x = (int) MathF.Max(
                    0f,
                    MathF.Min(
                        envMap.Width - 1,
                        (int) ((Math.Atan2(projectionDirection.Z, projectionDirection.X) / (2 * MathF.PI) + 0.5f) *
                            envMap.Width)));

                var y = (int) MathF.Max(
                    0f,
                    MathF.Min(envMap.Height - 1, (int) (MathF.Acos(projectionDirection.Y) / MathF.PI * envMap.Height)));

                return envMap.Buffer[x][y];
            }

            var reflectDirection = Vector3.Normalize(Vector3.Reflect(projectionDirection, normalVector));
            var refractDirection =
                Vector3.Normalize(Refract(projectionDirection, normalVector, material.RefractiveIndex));

            var reflectOrigin = Vector3.Dot(reflectDirection, normalVector) < 0
                ? hitPoint - normalVector * 1e-3f
                : hitPoint + normalVector * 1e-3f;

            var refractOrigin = Vector3.Dot(refractDirection, normalVector) < 0
                ? hitPoint - normalVector * 1e-3f
                : hitPoint + normalVector * 1e-3f;

            var reflectColor = CastRay(envMap, reflectOrigin, reflectDirection, scene, lightSources, depth + 1);
            var refractColor = CastRay(envMap, refractOrigin, refractDirection, scene, lightSources, depth + 1);

            var diffuseLightIntensity = 0f;
            var specularLightIntensity = 0f;
            foreach (var lightSource in lightSources)
            {
                var lightDirection = Vector3.Normalize(lightSource.Position - hitPoint);
                var lightDistance = Vector3.Distance(lightSource.Position, hitPoint);

                var shadowOrigin = Vector3.Dot(lightDirection, normalVector) < 0
                    ? hitPoint - normalVector * 1e-3f
                    : hitPoint + normalVector * 1e-3f;

                if (SceneIntersect(
                        shadowOrigin, lightDirection, scene,
                        out var shadowHitPoint, out var shadowNormalVector, out var shadowObjectMaterial) &&
                    Vector3.Distance(shadowHitPoint, shadowOrigin) < lightDistance)
                {
                    continue;
                }

                diffuseLightIntensity += lightSource.Intensity * MathF.Max(
                    0f, Vector3.Dot(lightDirection, normalVector));

                specularLightIntensity +=
                    MathF.Pow(
                        MathF.Max(
                            0f, -Vector3.Dot(Vector3.Reflect(-lightDirection, normalVector), projectionDirection)),
                        material.SpecularExponent) * lightSource.Intensity;
            }

            return material.DiffuseColor * diffuseLightIntensity * material.Albedo.DiffuseKoef +
                Vector3.One * specularLightIntensity * material.Albedo.SpecularKoef +
                reflectColor * material.Albedo.ReflectKoef +
                refractColor * material.Albedo.RefractKoef;
        }

        private static Vector3 Refract(Vector3 inputVector, Vector3 normalVector, float refractiveIndex)
        {
            var cosi = -MathF.Max(-1f, MathF.Min(1f, Vector3.Dot(inputVector, normalVector)));
            var etai = 1f;
            var etat = refractiveIndex;

            var normal = normalVector;
            if (cosi < 0)
            {
                cosi = -cosi;

                etai += etat;
                etat = etai - etat;
                etai -= etat;

                normal = -normal;
            }

            var eta = etai / etat;
            var k = 1 - eta * eta * (1 - cosi * cosi);
            return k < 0 ? Vector3.Zero : inputVector * eta + normal * (eta * cosi - MathF.Sqrt(k));
        }

        private static bool SceneIntersect(
            Vector3 origin, Vector3 projectionDirection, List<IObject3D> scene, out Vector3 hitPoint,
            out Vector3 normalVector,
            out Material material)
        {
            hitPoint = default(Vector3);
            normalVector = default(Vector3);
            material = default(Material);

            var minSphereDistance = float.MaxValue;
            foreach (var object3D in scene)
            {
                if (object3D.RayIntersect(origin, projectionDirection, out var distance) &&
                    distance < minSphereDistance)
                {
                    hitPoint = origin + projectionDirection * distance;
                    normalVector = Vector3.Normalize(hitPoint - object3D.Center);
                    material = object3D.Material;

                    minSphereDistance = distance;
                }
            }

            return minSphereDistance < 1000f;
        }
    }
}
