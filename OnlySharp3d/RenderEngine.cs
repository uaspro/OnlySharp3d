using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace OnlySharp3d
{
    internal class RenderEngine
    {
        private static readonly ColorF DefaultColor = ColorF.White;

        public int FrameWidth { get; }

        public int FrameHeight { get; }

        public float Fov { get; }

        public int RaycastDepth { get; }

        public float RenderDistance { get; set; }

        internal RenderEngine(
            int frameWidth, int frameHeight, float fov = MathF.PI / 2, float renderDistance = 1000f,
            int raycastDepth = 3)
        {
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            Fov = fov;
            RenderDistance = renderDistance;
            RaycastDepth = raycastDepth;
        }

        public ImageF Render(
            Vector3 cameraPosition, List<IObject3D> scene, List<LightSource> lightSources, ImageF environmentMap = null)
        {
            var frame = new ImageF(FrameWidth, FrameHeight);
            Parallel.For(
                0, FrameHeight, j =>
                {
                    for (var i = 0; i < FrameWidth; i++)
                    {
                        var x = (2 * (i + 1e-3f) / FrameWidth - 1) * MathF.Tanh(Fov / 2f) *
                            (FrameWidth / (float) FrameHeight);

                        var y = -(2 * (j + 1e-3f) / FrameHeight - 1) * MathF.Tanh(Fov / 2f);

                        var projectionDirection = Vector3.Normalize(new Vector3(x, y, -1f));
                        frame.Buffer[i][j] = CastRay(
                            ref cameraPosition, ref projectionDirection, scene, lightSources, environmentMap);
                    }
                });

            return frame;
        }

        private ColorF CastRay(
            ref Vector3 origin, ref Vector3 projectionDirection, List<IObject3D> scene,
            List<LightSource> lightSources, ImageF environmentMap = null, int depth = 0)
        {
            RayIntersectionResult rayIntersectionResult;
            if (depth > RaycastDepth ||
                !(rayIntersectionResult = SceneIntersect(ref origin, ref projectionDirection, scene)).IsIntersect)
            {
                return environmentMap == null
                    ? DefaultColor
                    : GetColorFromEnvironmentMap(projectionDirection, environmentMap);
            }

            var reflectionColor = GetReflectionColor(
                projectionDirection, scene, lightSources, environmentMap, depth, ref rayIntersectionResult);

            var refractionColor = GetRefractionColor(
                projectionDirection, scene, lightSources, environmentMap, depth, ref rayIntersectionResult);

            CalculateLightIntensity(
                projectionDirection, scene, lightSources, rayIntersectionResult,
                out var diffuseLightIntensity,
                out var specularLightIntensity);

            return rayIntersectionResult.Material.DiffuseColor * diffuseLightIntensity * rayIntersectionResult.Material.Albedo.DiffuseKoef +
                ColorF.White * specularLightIntensity * rayIntersectionResult.Material.Albedo.SpecularKoef +
                reflectionColor * rayIntersectionResult.Material.Albedo.ReflectKoef +
                refractionColor * rayIntersectionResult.Material.Albedo.RefractKoef;
        }

        private void CalculateLightIntensity(
            Vector3 projectionDirection, List<IObject3D> scene, List<LightSource> lightSources,
            RayIntersectionResult rayIntersectionResult, out float diffuseLightIntensity,
            out float specularLightIntensity)
        {
            diffuseLightIntensity = 0f;
            specularLightIntensity = 0f;

            foreach (var lightSource in lightSources)
            {
                var lightDirection = Vector3.Normalize(lightSource.Position - rayIntersectionResult.HitPoint);
                var lightDistance = Vector3.Distance(lightSource.Position, rayIntersectionResult.HitPoint);

                var shadowOrigin = Vector3.Dot(lightDirection, rayIntersectionResult.NormalVector) < 0
                    ? rayIntersectionResult.HitPoint - rayIntersectionResult.NormalVector * 1e-3f
                    : rayIntersectionResult.HitPoint + rayIntersectionResult.NormalVector * 1e-3f;

                RayIntersectionResult shadowIntersectionResult;
                if ((shadowIntersectionResult = SceneIntersect(ref shadowOrigin, ref lightDirection, scene))
                   .IsIntersect &&
                    Vector3.Distance(shadowIntersectionResult.HitPoint, shadowOrigin) < lightDistance)
                {
                    continue;
                }

                diffuseLightIntensity += lightSource.Intensity * MathF.Max(
                    0f, Vector3.Dot(lightDirection, rayIntersectionResult.NormalVector));

                specularLightIntensity +=
                    MathF.Pow(
                        MathF.Max(
                            0f,
                            -Vector3.Dot(
                                Vector3.Reflect(-lightDirection, rayIntersectionResult.NormalVector),
                                projectionDirection)),
                        rayIntersectionResult.Material.SpecularExponent) * lightSource.Intensity;
            }
        }

        private ColorF GetColorFromEnvironmentMap(Vector3 projectionDirection, ImageF environmentMap)
        {
            var x = (int) MathF.Max(
                0f,
                MathF.Min(
                    environmentMap.Width - 1,
                    (int) ((Math.Atan2(projectionDirection.Z, projectionDirection.X) / (2 * MathF.PI) + 0.5f) *
                        environmentMap.Width)));

            var y = (int) MathF.Max(
                0f,
                MathF.Min(
                    environmentMap.Height - 1,
                    (int) (MathF.Acos(projectionDirection.Y) / MathF.PI * environmentMap.Height)));

            return environmentMap.Buffer[x][y];
        }

        private ColorF GetReflectionColor(
            Vector3 projectionDirection, List<IObject3D> scene, List<LightSource> lightSources, ImageF environmentMap,
            int depth, ref RayIntersectionResult rayIntersectionResult)
        {
            var reflectDirection =
                Vector3.Normalize(Vector3.Reflect(projectionDirection, rayIntersectionResult.NormalVector));

            var reflectOrigin = Vector3.Dot(reflectDirection, rayIntersectionResult.NormalVector) < 0
                ? rayIntersectionResult.HitPoint - rayIntersectionResult.NormalVector * 1e-3f
                : rayIntersectionResult.HitPoint + rayIntersectionResult.NormalVector * 1e-3f;

            return CastRay(ref reflectOrigin, ref reflectDirection, scene, lightSources, environmentMap, depth + 1);
        }

        private ColorF GetRefractionColor(
            Vector3 projectionDirection, List<IObject3D> scene, List<LightSource> lightSources, ImageF environmentMap,
            int depth, ref RayIntersectionResult rayIntersectionResult)
        {
            var refractDirection =
                Vector3.Normalize(
                    Refract(
                        projectionDirection, rayIntersectionResult.NormalVector,
                        rayIntersectionResult.Material.RefractiveIndex));

            var refractOrigin = Vector3.Dot(refractDirection, rayIntersectionResult.NormalVector) < 0
                ? rayIntersectionResult.HitPoint - rayIntersectionResult.NormalVector * 1e-3f
                : rayIntersectionResult.HitPoint + rayIntersectionResult.NormalVector * 1e-3f;

            return CastRay(ref refractOrigin, ref refractDirection, scene, lightSources, environmentMap, depth + 1);
        }

        private Vector3 Refract(Vector3 inputVector, Vector3 normalVector, float refractiveIndex)
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

        private RayIntersectionResult SceneIntersect(
            ref Vector3 origin, ref Vector3 projectionDirection, List<IObject3D> scene)
        {
            var result = default(RayIntersectionResult);
            var minObjectDistance = float.MaxValue;
            foreach (var object3D in scene)
            {
                var rayIntersectionResult = object3D.RayIntersect(origin, projectionDirection);
                if (rayIntersectionResult.IsIntersect && rayIntersectionResult.Distance < minObjectDistance)
                {
                    result = rayIntersectionResult;
                    minObjectDistance = rayIntersectionResult.Distance;
                }
            }

            return minObjectDistance < RenderDistance ? result : default(RayIntersectionResult);
        }
    }
}
