namespace OnlySharp3d
{
    internal struct ColorF
    {
        public static ColorF White = new ColorF(1f, 1f, 1f);

        public float R { get; }

        public float G { get; }

        public float B { get; }

        public ColorF(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static ColorF operator *(ColorF color, float multiplier)
        {
            return new ColorF(color.R * multiplier, color.G * multiplier, color.B * multiplier);
        }

        public static ColorF operator +(ColorF colorLeft, ColorF colorRight)
        {
            return new ColorF(colorLeft.R + colorRight.R, colorLeft.G + colorRight.G, colorLeft.B + colorRight.B);
        }
    }
}
