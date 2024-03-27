using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiniteElements
{
    public class Point
    {
        private int _x;
        private int _y;
        private bool _isFixed;
        private double[] _displacement;

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public bool IsFixed
        {
            get { return _isFixed; }
            set { _isFixed = value; }
        }

        public double[] Displacement
        {
            get { return _displacement; }
            set { _displacement = value; }
        }

        public Point(int x, int y, bool isFixed = false)
        {
            _x = x;
            _y = y;
            _isFixed = isFixed;
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public System.Drawing.Point ToDrawingPoint()
        {
            return new System.Drawing.Point(X, Y);
        }

        public System.Drawing.Point ToDrawingDisplacedPoint()
        {
            if (_displacement == null)
                throw new Exception("Error. No such displacements");

            int dispX = Convert.ToInt32(Math.Ceiling(_displacement[0]));
            int dispY = Convert.ToInt32(Math.Ceiling(_displacement[1]));

            return new System.Drawing.Point(X + dispX, Y + dispY);
        }

        public static bool operator ==(Point p1, Point p2)
        {
            if (ReferenceEquals(p1, null) && ReferenceEquals(p2, null))
                return true;
            if (ReferenceEquals(p1, null) || ReferenceEquals(p2, null))
                return false;

            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Point)
                return X == ((Point)obj).X && Y == ((Point)obj).Y;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return X * 31 + Y;
        }
    }
}
