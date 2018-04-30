using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if !UNITY_EDITOR

namespace MiniASM.Builtin
{
    /// <summary>
    /// Basic implementation of a Vector3
    /// for testing purposes
    /// </summary>
    public class Vector3
    {
        public static Vector3 zero = new Vector3(0, 0, 0);
        public float x;
        public float y;
        public float z;

        public Vector3() { }

        public Vector3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b) {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator *(Vector3 a, float b) {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3 operator /(Vector3 a, float b) {
            return new Vector3(a.x / b, a.y / b, a.z / b);
        }

        public override string ToString() {
            return string.Format("({0} {1} {2})", x, y, z);
        }
    }
}

#endif
