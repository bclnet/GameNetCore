﻿namespace UnityEngine
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Plane
    {
        internal const int size = 0x10;
        private Vector3 m_Normal;
        private float m_Distance;
        public Plane(Vector3 inNormal, Vector3 inPoint)
        {
            this.m_Normal = Vector3.Normalize(inNormal);
            this.m_Distance = -Vector3.Dot(this.m_Normal, inPoint);
        }

        public Plane(Vector3 inNormal, float d)
        {
            this.m_Normal = Vector3.Normalize(inNormal);
            this.m_Distance = d;
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.m_Normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            this.m_Distance = -Vector3.Dot(this.m_Normal, a);
        }

        public Vector3 normal
        {
            get
            {
                return this.m_Normal;
            }
            set
            {
                this.m_Normal = value;
            }
        }
        public float distance
        {
            get
            {
                return this.m_Distance;
            }
            set
            {
                this.m_Distance = value;
            }
        }
        public void SetNormalAndPosition(Vector3 inNormal, Vector3 inPoint)
        {
            this.m_Normal = Vector3.Normalize(inNormal);
            this.m_Distance = -Vector3.Dot(inNormal, inPoint);
        }

        public void Set3Points(Vector3 a, Vector3 b, Vector3 c)
        {
            this.m_Normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            this.m_Distance = -Vector3.Dot(this.m_Normal, a);
        }

        public void Flip()
        {
            this.m_Normal = -this.m_Normal;
            this.m_Distance = -this.m_Distance;
        }

        public Plane flipped
        {
            get
            {
                return new Plane(-this.m_Normal, -this.m_Distance);
            }
        }
        public void Translate(Vector3 translation)
        {
            this.m_Distance += Vector3.Dot(this.m_Normal, translation);
        }

        public static Plane Translate(Plane plane, Vector3 translation)
        {
            return new Plane(plane.m_Normal, plane.m_Distance += Vector3.Dot(plane.m_Normal, translation));
        }

        public Vector3 ClosestPointOnPlane(Vector3 point)
        {
            float num = Vector3.Dot(this.m_Normal, point) + this.m_Distance;
            return (point - ((Vector3)(this.m_Normal * num)));
        }

        public float GetDistanceToPoint(Vector3 point)
        {
            return (Vector3.Dot(this.m_Normal, point) + this.m_Distance);
        }

        public bool GetSide(Vector3 point)
        {
            return ((Vector3.Dot(this.m_Normal, point) + this.m_Distance) > 0f);
        }

        public bool SameSide(Vector3 inPt0, Vector3 inPt1)
        {
            float distanceToPoint = this.GetDistanceToPoint(inPt0);
            float num2 = this.GetDistanceToPoint(inPt1);
            return (((distanceToPoint > 0f) && (num2 > 0f)) || ((distanceToPoint <= 0f) && (num2 <= 0f)));
        }

        public bool Raycast(Ray ray, out float enter)
        {
            float a = Vector3.Dot(ray.direction, this.m_Normal);
            float num2 = -Vector3.Dot(ray.origin, this.m_Normal) - this.m_Distance;
            if (Mathf.Approximately(a, 0f))
            {
                enter = 0f;
                return false;
            }
            enter = num2 / a;
            return (enter > 0f);
        }

        public override string ToString()
        {
            object[] args = new object[] { this.m_Normal.x, this.m_Normal.y, this.m_Normal.z, this.m_Distance };
            return UnityString.Format("(normal:({0:F1}, {1:F1}, {2:F1}), distance:{3:F1})", args);
        }

        public string ToString(string format)
        {
            object[] args = new object[] { this.m_Normal.x.ToString(format), this.m_Normal.y.ToString(format), this.m_Normal.z.ToString(format), this.m_Distance.ToString(format) };
            return UnityString.Format("(normal:({0}, {1}, {2}), distance:{3})", args);
        }
    }
}

