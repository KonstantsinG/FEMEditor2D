using System.Collections.Generic;

namespace FiniteElements.Geometry
{
    public class Hole
    {
        private List<Point> _points;

        public List<Point> Points
        {
            get { return _points; }
        }

        public List<Edge> Boundary
        {
            get
            {
                List<Edge> boundary = new List<Edge>();
                int j = _points.Count - 1;

                for (int i = 0; i < _points.Count; i++)
                {
                    boundary.Add(new Edge(_points[i], _points[j]));
                    j = i;
                }

                return boundary;
            }
        }

        public Hole(List<Point> points)
        {
            _points = points;
        }

        public Hole()
        {
            _points = new List<Point>();
        }

        public Hole(Hole hole)
        {
            List<Point> points = new List<Point>();

            foreach (Point p in hole.Points)
            {
                if (p.IsFixed)
                    points.Add(new Point(p.X, p.Y, true));
                else
                    points.Add(new Point(p.X, p.Y));
            }

            _points = points;
        }

        public bool IsSelfIntersects()
        {
            foreach (Edge e1 in Boundary)
            {
                foreach (Edge e2 in Boundary)
                {
                    if (e1 != e2)
                    {
                        if (e1.IsIntersects(e2) && !e1.IsHaveCommonPoints(e2))
                            return true;
                    }
                }
            }

            return false;
        }

        public void AddPoint(Point point) => _points.Add(point);

        public void ReversePoints() => _points.Reverse();

        public void ClearPoints() => _points.Clear();

        public bool IsClockwise()
        {
            float area = 0;
            int next;

            for (int i = 0; i < _points.Count; i++)
            {
                next = (i + 1) % _points.Count;
                area += (_points[i].X * -_points[next].Y) - (-_points[i].Y * _points[next].X);
            }
            // Y values is negative because in editor Y axis is flipped

            return area < 0;
        }

        public System.Drawing.Point[] GetDrawingArray()
        {
            System.Drawing.Point[] points = new System.Drawing.Point[_points.Count];

            for (int i = 0; i < _points.Count; i++)
                points[i] = _points[i].ToDrawingPoint();

            return points;
        }

        public System.Drawing.Point[] GetDrawingDisplacedArray()
        {
            System.Drawing.Point[] points = new System.Drawing.Point[_points.Count];

            for (int i = 0; i < _points.Count; i++)
                points[i] = _points[i].ToDrawingDisplacedPoint();

            return points;
        }

        public bool IsPointInHole(Point p)
        {
            int j = Points.Count - 1;
            bool c = false;

            for (int i = 0; i < Points.Count; i++)
            {
                if ((_points[i].Y > p.Y) != (_points[j].Y > p.Y))
                {
                    if (p.X < (_points[j].X - _points[i].X) * (p.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) + _points[i].X)
                        c = !c;
                }

                j = i;
            }

            return c;
        }
    }
}
