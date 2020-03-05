using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace VelcroGrass {
    // SOME USEFUL EXTENSIONS
    static class Extensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Say(this string message) { Console.WriteLine(message); }

        public static void DrawLine(this SpriteBatch spriteBatch, Texture2D tex, Rectangle pixel, Vector2 begin, Vector2 end, Color color, int thick = 1)
        {
            Vector2 delta = end - begin;
            float rot = (float)Math.Atan2(delta.Y, delta.X);
            if (pixel.Width > 0) { pixel.Width = 1; pixel.Height = 1; }
            spriteBatch.Draw(tex, begin, pixel, color, rot, new Vector2(0, 0.5f), new Vector2(delta.Length(), thick), SpriteEffects.None, 0);
        }        
        public static void DrawRectLines(this SpriteBatch spriteBatch, Texture2D tex, Rectangle pixel, Rectangle r, Color color, int thick = 1)
        {
            spriteBatch.Draw(tex, new Rectangle(r.X, r.Y, r.Width, thick), pixel, color);
            spriteBatch.Draw(tex, new Rectangle(r.X + r.Width, r.Y, thick, r.Height), pixel, color);
            spriteBatch.Draw(tex, new Rectangle(r.X, r.Y + r.Height, r.Width, thick), pixel, color);
            spriteBatch.Draw(tex, new Rectangle(r.X, r.Y, thick, r.Height), pixel, color);
        }

        // The following version is for single pixel texture instead of a pixel from a sprite-sheet (not recommended if causes lots of texture switching)
        public static void DrawLine(this SpriteBatch spriteBatch, Texture2D tex, Vector2 begin, Vector2 end, Color color, int thick = 1)
        {
            Vector2 delta = end - begin;
            float rot = (float)Math.Atan2(delta.Y, delta.X);
            spriteBatch.Draw(tex, begin, null, color, rot, new Vector2(0, 0.5f), new Vector2(delta.Length(), thick), SpriteEffects.None, 0);
        }        


        public static float ToAngle(this Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }
        public static float NextFloat(this Random rand, float minValue, float maxValue) //get random float between the 2 numbers
        {
            return (float)rand.NextDouble() * (maxValue - minValue) + minValue;
        }

        public static uint ColorToUInt(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
        }
        public static Color UIntToColor(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return new Color(r, g, b, a);
        }
    }
}
