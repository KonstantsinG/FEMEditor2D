using System;

namespace FiniteElements
{
    public class Edge
    {
        private Point _start;
        private Point _end;

        public Point Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public Point End
        {
            get { return _end; }
            set { _end = value; }
        }

        public double Length
        {
            get { return Math.Sqrt(Math.Pow(Start.X - End.X, 2) + Math.Pow(Start.Y - End.Y, 2)); }
        }

        public double LengthSqr
        {
            get { return Math.Pow(Start.X - End.X, 2) + Math.Pow(Start.Y - End.Y, 2); }
        }


        public Edge(Point start, Point end)
        {
            Start = start;
            End = end;
        }

        public bool IsHaveCommonPoints(Edge other)
        {
            return !(Start != other.Start && Start != other.End && End != other.Start && End != other.End);
        }

        public bool IsIntersects(Edge other)
        {
            double down = (other.End.X - other.Start.X) * (End.Y - Start.Y) - (other.End.Y - other.Start.Y) * (End.X - Start.X);

            // parallel
            if (down == 0)
                return false;

            double alphaUp = (other.End.X - other.Start.X) * (other.Start.Y - Start.Y) - (other.End.Y - other.Start.Y) * (other.Start.X - Start.X);
            double alpha = alphaUp / down;

            if (alpha < 0 || alpha > 1)
                return false;

            double betaUp = (End.X - Start.X) * (other.Start.Y - Start.Y) - (End.Y - Start.Y) * (other.Start.X - Start.X);
            double beta = betaUp / down;

            if (beta < 0 || beta > 1)
                return false;

            // ovelap
            if (alphaUp == betaUp && betaUp == 0)
                return false;

            return true;
        }

        public double GetDistance(Point p)
        {
            double l2 = LengthSqr;

            if (l2 == 0)
                return Math.Pow(Start.X - p.X, 2) + Math.Pow(Start.Y - p.Y, 2);

            double t = ((p.X -  Start.X) * (End.X - Start.X) + (p.Y - Start.Y) * (End.Y - Start.Y)) / l2;
            t = Math.Max(0, Math.Min(1, t));

            int x2 = Convert.ToInt32(Start.X + t * (End.X - Start.X));
            int y2 = Convert.ToInt32(Start.Y + t * (End.Y - Start.Y));
            Point p2 = new Point(x2, y2);

            return Math.Sqrt(Math.Pow(p.X - p2.X, 2) + Math.Pow(p.Y - p2.Y, 2));
        }

        public override bool Equals(object other)
        {
            if (other is Edge)
            {
                return (Start == ((Edge)other).Start && End == ((Edge)other).End) ||
                    (Start == ((Edge)other).End && End == ((Edge)other).Start);
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Start.X * 31 + End.Y;
        }

        public static bool operator ==(Edge e1, Edge e2)
        {
            if (ReferenceEquals(e1, null) && ReferenceEquals(e2, null))
                return true;
            if (ReferenceEquals(e1, null) || ReferenceEquals(e2, null))
                return false;

            return (e1.Start == e2.Start && e1.End == e2.End) ||
                (e1.Start == e2.End && e1.End == e2.Start);
        }

        public static bool operator !=(Edge e1, Edge e2)
        {
            return !(e1 == e2);
        }
    }
}
