using FiniteElements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiniteElements
{
    public class Polygon
    {
        private List<Point> _boundary;
        private List<Point> _innerPoints;
        private List<Hole> _holes = new List<Hole>();

        public List<Point> Points
        {
            get
            {
                List<Point> allPoints = new List<Point>(_boundary);
                allPoints.AddRange(_innerPoints);

                foreach (Hole h in _holes)
                    allPoints.AddRange(h.Points);

                return allPoints;
            }
        }

        public List<Point> Boundary
        {
            get { return _boundary; }
        }

        public List<Point> InnerPoints
        {
            get { return _innerPoints; }
        }

        public List<Hole> Holes
        {
            get { return _holes; }
        }

        public List<Edge> Edges
        {
            get
            {
                List<Edge> edges = new List<Edge>();
                int j = Boundary.Count - 1;

                for (int i = 0; i < Boundary.Count; i++)
                {
                    edges.Add(new Edge(Boundary[i], Boundary[j]));
                    j = i;
                }

                return edges;
            }
        }

        public List<Edge> HolesEdges
        {
            get
            {
                List<Edge> edges = new List<Edge>();

                foreach (Hole h in _holes)
                    edges.AddRange(h.Boundary);

                return edges;
            }
        }

        public int FixedPointsCount
        {
            get
            {
                int count = 0;

                foreach (Point p in Points)
                {
                    if (p.IsFixed)
                        count++;
                }

                foreach (Hole h in _holes)
                {
                    foreach (Point p in h.Points)
                    {
                        if (p.IsFixed)
                            count++;
                    }
                }

                return count;
            }
        }

        public int HolesPointsCount
        {
            get
            {
                int count = 0;

                foreach (Hole h in _holes)
                    count += h.Points.Count;

                return count;
            }
        }

        public int PointsCount
        {
            get { return Boundary.Count + InnerPoints.Count + HolesPointsCount; }
        }

        public Polygon(List<Point> boundary)
        {
            _boundary = new List<Point>(boundary);
            _innerPoints = new List<Point>();
        }

        public Polygon()
        {
            _boundary = new List<Point>();
            _innerPoints = new List<Point>();
        }


        public bool IsSelfIntersects()
        {
            foreach (Edge e1 in Edges)
            {
                foreach (Edge e2 in Edges)
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

        public void AddHole(Hole hole)
        {
            if (!IsHoleInPolygon(hole))
                throw new Exception("Error. Hole lies out of polygon.");
            else if (IsHoleIntersectOtherHoles(hole))
                throw new Exception("Error. Hole intersect other holes.");

            _holes.Add(new Hole(hole));
        }

        public void RemoveLastHole()
        {
            if (_holes.Count > 0)
                _holes.RemoveAt(_holes.Count - 1);
        }

        public void AddBoundaryPoint(Point p) => _boundary.Add(p);

        public void RemoveBoundaryPoint(Point p) => _boundary.Remove(p);

        public bool IsClockwise()
        {
            float area = 0;
            int next;

            for (int i = 0; i < _boundary.Count; i++)
            {
                next = (i + 1) % _boundary.Count;
                area += (_boundary[i].X * -_boundary[next].Y) - (-_boundary[i].Y * _boundary[next].X);
            }
            // Y values is negative because in editor Y axis is flipped

            return area < 0;
        }

        public void ReversePoints()
        {
            _boundary.Reverse();
        }

        public void ClearPoints()
        {
            _boundary.Clear();
            _innerPoints.Clear();
        }

        public void ClearInnerPoints()
        {
            _innerPoints.Clear();
        }

        public void ClearHoles()
        {
            _holes.Clear();
        }

        public System.Drawing.Point[] GetDrawingPointsArray()
        {
            System.Drawing.Point[] pts = new System.Drawing.Point[Points.Count];

            for (int i = 0; i < Points.Count; i++)
                pts[i] = Points[i].ToDrawingPoint();

            return pts;
        }

        public System.Drawing.Point[] GetDrawingInnerPointsArray()
        {
            System.Drawing.Point[] pts = new System.Drawing.Point[InnerPoints.Count];

            for (int i = 0; i < InnerPoints.Count; i++)
                pts[i] = InnerPoints[i].ToDrawingPoint();

            return pts;
        }

        public System.Drawing.Point[] GetDrawingBoundaryArray()
        {
            System.Drawing.Point[] pts = new System.Drawing.Point[Boundary.Count];

            for (int i = 0; i < Boundary.Count; i++)
                pts[i] = Boundary[i].ToDrawingPoint();

            return pts;
        }

        public System.Drawing.Point[] GetDrawingDisplacedArray()
        {
            System.Drawing.Point[] pts = new System.Drawing.Point[Boundary.Count];

            for (int i = 0; i < Boundary.Count; i++)
                pts[i] = Boundary[i].ToDrawingDisplacedPoint();

            return pts;
        }

        public System.Drawing.Point[][] GetDrawingHolesArray()
        {
            System.Drawing.Point[][] holePoints = new System.Drawing.Point[Holes.Count][];

            for (int i = 0; i < _holes.Count; i++)
                holePoints[i] = _holes[i].GetDrawingArray();

            return holePoints;
        }

        public void InsertPoints(int numPoints, double minDistance)
        {
            Point newPt;
            double pointDst = minDistance;
            double edgeDst = minDistance;
            double holeDst = minDistance;
            int failsCounter = 0;
            Random rnd = new Random();

            int screenWidth = 1920;
            int screenHeight = 1080;

            for (int i = 0; i < numPoints; i++)
            {
                if (failsCounter > 1000)
                    break;

                newPt = new Point(rnd.Next(0, screenWidth), rnd.Next(0, screenHeight));

                if (!IsPointInPolygon(newPt))
                {
                    i -= 1;
                    failsCounter++;
                    continue;
                }
                else
                {
                    pointDst = CheckMinDistanceToPoints(newPt, minDistance);
                    edgeDst = CheckMinDistanceToEdges(newPt, minDistance);
                    holeDst = CheckMinDistanceToHoles(newPt, minDistance);

                    if (pointDst < minDistance || edgeDst < minDistance || holeDst < minDistance)
                    {
                        i -= 1;
                        failsCounter++;
                        continue;
                    }
                    else
                    {
                        _innerPoints.Add(newPt);
                        failsCounter = 0;
                    }
                }
            }
        }

        private double CheckMinDistanceToPoints(Point newPt, double minDistance)
        {
            double pointDst = minDistance;

            foreach (Point pt in Points)
            {
                pointDst = Math.Sqrt(Math.Pow(newPt.X - pt.X, 2) + Math.Pow(newPt.Y - pt.Y, 2));

                if (pointDst < minDistance)
                    break;
            }

            return pointDst;
        }

        private double CheckMinDistanceToEdges(Point newPt, double minDistance)
        {
            double edgeDst = minDistance;

            foreach (Edge e in Edges)
            {
                edgeDst = e.GetDistance(newPt);

                if (edgeDst < minDistance)
                    break;
            }

            return edgeDst;
        }

        private double CheckMinDistanceToHoles(Point newPt, double minDistance)
        {
            double holeDst = minDistance;

            foreach (Hole h in _holes)
            {
                foreach (Edge e in h.Boundary)
                {
                    holeDst = e.GetDistance(newPt);

                    if (holeDst < minDistance)
                        break;
                }

                if (holeDst < minDistance)
                    break;
            }

            return holeDst;
        }

        public bool IsHoleInPolygon(Hole hole)
        {
            foreach (Point p in hole.Points)
            {
                if (!IsPointInPolygon(p))
                    return false;
            }

            foreach (Edge holeEdge in hole.Boundary)
            {
                foreach (Edge polyEdge in Edges)
                {
                    if (polyEdge.IsIntersects(holeEdge))
                        return false;
                }
            }

            return true;
        }

        private bool IsHoleIntersectOtherHoles(Hole hole)
        {
            foreach (Hole oldHole in _holes)
            {
                foreach (Edge oldEdge in oldHole.Boundary)
                {
                    foreach (Edge newEdge in hole.Boundary)
                    {
                        if (newEdge.IsIntersects(oldEdge) && !newEdge.IsHaveCommonPoints(oldEdge))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool IsPointInPolygon(Point p)
        {
            int j = Boundary.Count - 1;
            bool c = false;

            for (int i = 0; i < Boundary.Count; i++)
            {
                if ((_boundary[i].Y > p.Y) != (_boundary[j].Y > p.Y))
                {
                    if (p.X < (_boundary[j].X - _boundary[i].X) * (p.Y - _boundary[i].Y) / (_boundary[j].Y - _boundary[i].Y) + _boundary[i].X)
                        c = !c;
                }

                j = i;
            }

            foreach (Hole hole in _holes)
            {
                if (hole.IsPointInHole(p))
                    return false;
            }

            return c;
        }
    }
}
