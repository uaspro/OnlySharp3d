using System;
using System.Collections.Generic;
using System.Numerics;

namespace OnlySharp3d
{
    internal class Program
    {
        private const int Width = 1920;
        private const int Height = 1080;

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

            var renderEngine = new RenderEngine(Width, Height);
            var environmentMap = new ImageF(EnvMapPath);
            var frame = renderEngine.Render(Vector3.Zero, scene, lightSources, environmentMap);
            frame.SaveToFile(ResultFramePath);
        }
    }
}
