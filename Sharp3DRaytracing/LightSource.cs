﻿using System.Numerics;

namespace Sharp3DRaytracing
{
    internal struct LightSource
    {
        public Vector3 Position { get; }

        public float Intensity { get; set; }

        internal LightSource(Vector3 position, float intensity)
        {
            Position = position;
            Intensity = intensity;
        }
    }
}
