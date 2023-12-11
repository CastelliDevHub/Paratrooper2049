using System;
using System.Windows;
using System.Windows.Media;

namespace Paratrooper2049
{
    internal class BaseObject
    {
        public enum gameObject
        {
            Bullet, Laser, Cluster, Heli, Para, Airplane, Gift
        }

        protected PointCollection _vertices = new PointCollection();
        protected PointCollection _battleField = new PointCollection();

        protected double _direction, _speed;
        protected bool _lastFrame = false;
        protected gameObject _description;

        public PointCollection Vertices { get => _vertices; }

        public double Speed { get => _speed; set => _speed = value; }

        public bool LastFrame { get => _lastFrame; set => _lastFrame = value; }

        public gameObject Description { get => _description; }

        protected static Point RotateObject(Point point, Point center, double angle)
        {
            Point temp = new Point(point.X - center.X, point.Y - center.Y);

            // Rotate the point by the given angle
            double newX = temp.X * Math.Cos(angle) - temp.Y * Math.Sin(angle);
            double newY = temp.X * Math.Sin(angle) + temp.Y * Math.Cos(angle);

            // Translate the point back to its original position
            newX += center.X;
            newY += center.Y;

            return new Point(newX, newY);
        }

        public static bool IntersectObject(PointCollection verticesA, PointCollection verticesB)
        {
            for (int i = 0; i < verticesA.Count; i++)
            {
                Point va = verticesA[i];
                Point vb = verticesA[(i + 1) % verticesA.Count];

                Point axis = new Point(va.Y - vb.Y, vb.X - va.X);

                axis = Normalize(axis);

                ProjectVertices(verticesA, axis, out double minA, out double maxA);
                ProjectVertices(verticesB, axis, out double minB, out double maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
            }

            for (int i = 0; i < verticesB.Count; i++)
            {
                Point va = verticesB[i];
                Point vb = verticesB[(i + 1) % verticesB.Count];

                Point axis = new Point(va.Y - vb.Y, vb.X - va.X);

                axis = Normalize(axis);

                ProjectVertices(verticesA, axis, out double minA, out double maxA);
                ProjectVertices(verticesB, axis, out double minB, out double maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ProjectVertices(PointCollection vertices, Point axis, out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;

            for (int i = 0; i < vertices.Count; i++)
            {
                Point p = vertices[i];
                double proj = DotProduct(p, axis);

                if (proj < min) { min = proj; }
                if (proj > max) { max = proj; }
            }
        }

        private static double DotProduct(Point a, Point b)
        {
            // a · b = ax * bx + ay * by
            return a.X * b.X + a.Y * b.Y;
        }

        private static double Magnitude(Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        private static Point Normalize(Point p)
        {
            double mag = Magnitude(p);
            return new Point(p.X / mag, p.Y / mag);
        }
    }
}