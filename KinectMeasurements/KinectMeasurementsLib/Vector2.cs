using System;

namespace KinectMeasurementsLib
{
    [Serializable]
    public struct Vector2
    {
        public float X;
        public float Y;

        public static Vector2 Zero
        {
            get
            {
                return new Vector2(0, 0);
            }
        }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y);
            }
        }

        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2 operator *(Vector2 left, float value)
        {
            return new Vector2(left.X * value, left.Y * value);
        }

        public static Vector2 operator *(float value, Vector2 left)
        {
            return left * value;
        }

        public static Vector2 operator /(Vector2 left, float value)
        {
            return new Vector2(left.X / value, left.Y / value);
        }
    }
}