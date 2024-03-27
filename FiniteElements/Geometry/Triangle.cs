using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace FiniteElements
{
    public class Triangle
    {
        private Point[] _points;
        private Edge[] _edges;

        public Point[] Points
        {
            get { return _points; }
        }

        public Edge[] Edges
        {
            get { return _edges; }
        }

        public Triangle(Point[] points)
        {
            _points = points;

            //if (IsClockwise())
                //ReversePoints();

            _edges = new Edge[]{
                new Edge(points[0], points[1]),
                new Edge(points[1], points[2]),
                new Edge(points[2], points[0])
            };
        }

        public Triangle(Point p1, Point p2, Point p3)
        {
            _points = new Point[]{ p1, p2, p3 };

            //if (IsClockwise())
                //ReversePoints();

            _edges = new Edge[]{
                new Edge(_points[0], _points[1]),
                new Edge(_points[1], _points[2]),
                new Edge(_points[2], _points[0])
            };
        }

        public Point this[int index]
        {
            get
            {
                if (index < 0 || index >= _points.Length)
                    throw new IndexOutOfRangeException("Invalid vertex index.");

                return _points[index];
            }
            set
            {
                if (index < 0 || index >= _points.Length)
                    throw new IndexOutOfRangeException("Invalid vertex index.");

                _points[index] = value;
            }
        }

        public void ReversePoints()
        {
            _points = _points.Reverse().ToArray();
        }

        public bool IsClockwise()
        {
            float area = 0;
            for (int i = 0; i < _points.Count(); i++)
            {
                int next = (i + 1) % _points.Count();
                area += (_points[i].X * _points[next].Y) - (_points[i].Y * _points[next].X);
            }

            return area < 0;
        }

        public static bool IsClockwise(Point[] points)
        {
            float area = 0;
            for (int i = 0; i < points.Count(); i++)
            {
                int next = (i + 1) % points.Count();
                area += (points[i].X * -points[next].Y) - (-points[i].Y * points[next].X);
            }
            // Y values is negative because in editor Y axis is flipped

            return area < 0;
        }

        public System.Drawing.Point[] GetDrawingArray()
        {
            System.Drawing.Point[] points = new System.Drawing.Point[]{
                this[0].ToDrawingPoint(),
                this[1].ToDrawingPoint(),
                this[2].ToDrawingPoint()
            };

            return points;
        }

        public List<System.Drawing.Point> GetDrawingList()
        {
            return GetDrawingArray().ToList();
        }

        public System.Drawing.Point[] GetDrawingDisplacedArray()
        {
            System.Drawing.Point[] pts = new System.Drawing.Point[]
            {
                this[0].ToDrawingDisplacedPoint(),
                this[1].ToDrawingDisplacedPoint(),
                this[2].ToDrawingDisplacedPoint()
            };

            return pts;
        }

        public double GetArea()
        {
            double a = Math.Sqrt(Math.Pow(this[0].X - this[1].X, 2) +
                Math.Pow(this[0].Y - this[1].Y, 2));
            double b = Math.Sqrt(Math.Pow(this[1].X - this[2].X, 2) +
                Math.Pow(this[1].Y - this[2].Y, 2));
            double c = Math.Sqrt(Math.Pow(this[2].X - this[0].X, 2) +
                Math.Pow(this[2].Y - this[0].Y, 2));

            double semiP = (a + b + c) / 2;
            double area = Math.Sqrt(semiP * (semiP - a) * (semiP - b) * (semiP - c));

            return area;
        }

        public static bool operator ==(Triangle t1, Triangle t2)
        {
            if (ReferenceEquals(t1, null) && ReferenceEquals(t2, null))
                return true;
            if (ReferenceEquals(t1, null) || ReferenceEquals(t2, null))
                return false;

            return (t1[0] == t2[0] && t1[1] == t2[1] && t1[2] == t2[2]) ||
                (t1[0] == t2[1] && t1[1] == t2[2] && t1[2] == t2[0]) ||
                (t1[0] == t2[2] && t1[1] == t2[0] && t1[2] == t2[1]);
        }

        public static bool operator !=(Triangle t1, Triangle t2)
        {
            return !(t1 == t2);
        }

        public override bool Equals(object t)
        {
            if (t is Triangle)
            {
                return (this[0] == ((Triangle)t)[0] && this[1] == ((Triangle)t)[1] && this[2] == ((Triangle)t)[2]) ||
                    (this[0] == ((Triangle)t)[1] && this[1] == ((Triangle)t)[2] && this[2] == ((Triangle)t)[0]) ||
                    (this[0] == ((Triangle)t)[2] && this[1] == ((Triangle)t)[0] && this[2] == ((Triangle)t)[1]);
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this[0].X * 31 * 31 + this[1].Y * 31 + this[2].X;
        }
    }
}
