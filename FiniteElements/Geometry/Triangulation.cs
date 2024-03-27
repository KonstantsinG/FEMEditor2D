using FiniteElements.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace FiniteElements
{
    public class Triangulation
    {
        private List<Point> _points;
        private List<Point> _boundary;
        private List<Hole> _holes;

        public Triangulation(List<Point> points, List<Point> boundary, List<Hole> holes)
        {
            _points = points;
            _boundary = boundary;
            _holes = holes;
        }

        /// <summary>
        /// Delaunay triangulation
        /// </summary>
        /// <returns> Triangles set </returns>
        public List<Triangle> Triangulate()
        {
            List<Triangle> triangles;

            // First step -> Bowyer-Watson method to get convex hull triangulation
            triangles = TriangulateConvexHull();

            // Second step -> Insert boundary into triangulation (if it's not there)
            triangles = InsertPolygonBoundary(triangles);

            // Last step -> Trim triangles outside the boundary
            triangles = TrimOutsideTriangles(triangles);

            return triangles;
        }

        /// <summary>
        /// Bowyer-Watson method to get convex hull
        /// </summary>
        /// <returns> Convex hull triangulation </returns>
        private List<Triangle> TriangulateConvexHull()
        {
            List<Triangle> triangles = new List<Triangle>();

            // Big triangle and general points list
            int bignum = 10000;
            Point rightDown = new Point(bignum, -bignum);
            Point leftDown = new Point(-bignum, -bignum);
            Point centerTop = new Point(0, bignum);
            
            Triangle superTri = new Triangle(centerTop, leftDown, rightDown);
            triangles.Add(superTri);

            List<Triangle> corrTris;
            List<Edge> corrEdges;
            Point p;
            Triangle newTringle;

            // Adding points one by one
            for (int i = 0; i < _points.Count; i++)
            {
                // Find triangles which became corrupted by new point
                p = _points[i];
                corrTris = GetCorruptedTriangles(p, triangles);

                // Get corrupted triangles boundary
                corrEdges = GetBoundary(corrTris);

                // Remove corrupted triangles from triangulation
                foreach (Triangle corrT in corrTris)
                    triangles.Remove(corrT);
                
                // Re-triangulation from boundary and new point
                foreach (Edge corrE in corrEdges)
                {
                    newTringle = new Triangle(corrE.Start, corrE.End, p);

                    // If triangle has incorrect (clockwise) points order we must reverse them
                    if (newTringle.IsClockwise())
                        newTringle.ReversePoints();

                    triangles.Add(newTringle);
                }
            }

            List<Triangle> result = new List<Triangle>();
            List<Point> triPoints;

            // Remove Super triangle parts from triangulation
            foreach (Triangle tri in triangles)
            {
                triPoints = tri.Points.ToList();

                if (!triPoints.Contains(rightDown) && !triPoints.Contains(leftDown) && !triPoints.Contains(centerTop))
                    result.Add(tri);
            }

            return result;
        }

        /// <summary>
        /// Get triangles for which the Delaunay condition does not hold
        /// </summary>
        /// <param name="p"> Point </param>
        /// <param name="triangles"> Triangles </param>
        /// <returns> Corrupted triangles set </returns>
        private List<Triangle> GetCorruptedTriangles(Point p, List<Triangle> triangles)
        {
            List<Triangle> corrTriangles = new List<Triangle>();

            foreach (Triangle tri in triangles)
            {
                if (IsInCircumcircle(p, tri))
                    corrTriangles.Add(tri);
            }

            return corrTriangles;
        }

        /// <summary>
        /// Check is point in circumcircle of triangle
        /// </summary>
        /// <param name="p"> Point </param>
        /// <param name="tri"> Triangle </param>
        /// <returns> Is point in circumcircle of triangle </returns>
        private bool IsInCircumcircle(Point p, Triangle tri)
        {
            Point pa = tri[0] - p;
            Point pb = tri[1] - p;
            Point pc = tri[2] - p;

            double[,] matrix = new double[,]
            {
                { pa.X, pa.Y, pa.X * pa.X + pa.Y * pa.Y},
                { pb.X, pb.Y, pb.X * pb.X + pb.Y * pb.Y},
                { pc.X, pc.Y, pc.X * pc.X + pc.Y * pc.Y}
            };

            return Det(matrix) > 0.0;
        }

        /// <summary>
        /// Get matrix determinant
        /// </summary>
        /// <param name="matr"> Matrix </param>
        /// <returns> Matrix determinant </returns>
        private double Det(double[,] matr)
        {
            double a = matr[0, 0] * (matr[1, 1] * matr[2, 2] - matr[1, 2] * matr[2, 1]);
            double b = matr[0, 1] * (matr[1, 0] * matr[2, 2] - matr[1, 2] * matr[2, 0]);
            double c = matr[0, 2] * (matr[1, 0] * matr[2, 1] - matr[1, 1] * matr[2, 0]);

            return a - b + c;
        }

        /// <summary>
        /// Get triangulation boundary
        /// </summary>
        /// <param name="corrTris"> Corrupted triangles set </param>
        /// <returns> Edges set </returns>
        private List<Edge> GetBoundary(List<Triangle> corrTris)
        {
            List<Edge> boundary = new List<Edge>();
            bool isOnBoundary = true;

            foreach (Triangle t in corrTris)
            {
                foreach (Edge e in t.Edges)
                {
                    isOnBoundary = true;

                    foreach (Triangle corrT in corrTris)
                    {
                        if (t != corrT && corrT.Edges.Any(x => x == e))
                        {
                            isOnBoundary = false;
                            break;
                        }
                    }

                    if (isOnBoundary)
                        boundary.Add(e);
                }
            }

            return boundary;
        }

        /// <summary>
        /// Insert all boundaries into the triangulation if it is not there
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <returns> Triangulation with all boundaries </returns>
        public List<Triangle> InsertPolygonBoundary(List<Triangle> triangles)
        {
            List<Edge> boundaryEdges = new List<Edge>();
            int j = _boundary.Count - 1;

            // Get boundary edges
            for (int i = 0; i < _boundary.Count; i++)
            {
                boundaryEdges.Add(new Edge(_boundary[i], _boundary[j]));
                j = i;
            }

            // Insert boundary edges
            triangles = InsertBoundary(triangles, boundaryEdges);

            // Insert holes edges
            foreach (Hole h in _holes)
                triangles = InsertBoundary(triangles, new List<Edge>(h.Boundary));

            return triangles;
        }

        /// <summary>
        /// Insert a given boundary into the triangulation if it is not there
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <param name="boundaryEdges"> Boundary edges </param>
        /// <returns> Triangulation with boundary </returns>
        private List<Triangle> InsertBoundary(List<Triangle> triangles,  List<Edge> boundaryEdges)
        {
            // Get missing edges in boundary
            List<Edge> missing = GetMissingEdges(triangles, boundaryEdges);

            // Add each missing edge
            foreach (Edge e in missing)
                triangles = AddEdge(triangles, e);

            // Trim out triangles that intersects boundary
            triangles = TrimIntersectingTriangles(triangles, boundaryEdges);

            return triangles;
        }

        /// <summary>
        /// Add missing edge into triangulation
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <param name="edge"> Boundary edges </param>
        /// <returns> Triangulation with new edge </returns>
        private List<Triangle> AddEdge(List<Triangle> triangles, Edge edge)
        {
            List<Edge> intersectings = GetIntersectingEdges(triangles, edge);
            Triangle[] trisPair;
            Triangle[] newTrisPair;

            foreach (Edge e in intersectings)
            {
                // Get triangles quadrilateral
                trisPair = GetTrianglePair(triangles, e);

                if (trisPair.Any(x => x == null))
                    continue;

                // Quadriliteral with opposite diagonal
                newTrisPair = new Triangle[]
                {
                    new Triangle(edge.Start, edge.End, e.Start),
                    new Triangle(edge.Start, edge.End, e.End)
                };
                foreach (Triangle newT in newTrisPair)
                {
                    if (newT.IsClockwise())
                        newT.ReversePoints();
                }

                // Remove old quadrilateral and add new quadrilateral
                triangles.Remove(trisPair[0]);
                triangles.Remove(trisPair[1]);
                triangles.Add(newTrisPair[0]);
                triangles.Add(newTrisPair[1]);
            }

            return triangles;
        }


        /// <summary>
        /// Get a pair of triangles (quadrilateral) in which a given side is the common edge of the triangles
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <param name="commonEdge"> Triangles common edge </param>
        /// <returns> Pair of triangles (quadrilateral) </returns>
        private Triangle[] GetTrianglePair(List<Triangle> triangles, Edge commonEdge)
        {
            Triangle[] pair = new Triangle[2];
            int pairIdx = 0;

            foreach (Triangle t in triangles)
            {
                foreach (Edge e in t.Edges)
                {
                    if (e == commonEdge)
                    {
                        pair[pairIdx] = t;
                        pairIdx++;

                        if (pairIdx == 2) break;
                    }
                }

                if (pairIdx == 2) break;
            }

            return pair;
        }

        /// <summary>
        /// Get edges that intersects with given one
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <param name="newEdge"> Edge </param>
        /// <returns> Intersecting edges </returns>
        private List<Edge> GetIntersectingEdges(List<Triangle> triangles, Edge newEdge)
        {
            List<Edge> intersectingEdges = new List<Edge>();

            foreach (Triangle t in triangles)
            {
                foreach (Edge e in t.Edges)
                {
                    if (newEdge.IsIntersects(e) && !intersectingEdges.Any(x => x == e))
                    {
                        if (!newEdge.IsHaveCommonPoints(e))
                            intersectingEdges.Add(e);
                    }    
                }
            }

            return intersectingEdges;
        }

        /// <summary>
        /// Get missing edges in triangulation
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <param name="edges"> All necessary edges </param>
        /// <returns> Missing edges </returns>
        private List<Edge> GetMissingEdges(List<Triangle> triangles, List<Edge> edges)
        {
            // Get all necessary edges
            List<Edge> missingEdges = new List<Edge>(edges);

            foreach (Triangle t in triangles)
            {
                for (int i = 0; i < missingEdges.Count; i++)
                {
                    // If a triangulation has such an edge, it is not missing.
                    if (t.Edges.Any(x => x == missingEdges[i]))
                        missingEdges.RemoveAt(i);
                }
            }

            return missingEdges;
        }

        /// <summary>
        /// Remove triangles from triangulation that intersects triangulation boundary
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <param name="edges"> Triangulation boundary </param>
        /// <returns></returns>
        private List<Triangle> TrimIntersectingTriangles(List<Triangle> triangles, List<Edge> edges)
        {
            List<Triangle> corpses = new List<Triangle>();

            foreach (Edge e in edges)
            {
                foreach (Triangle t in triangles)
                {
                    if (t.Edges.All(x => x != e))
                    {
                        foreach (Edge triE in t.Edges)
                        {
                            if (triE.IsIntersects(e) && !triE.IsHaveCommonPoints(e))
                            {
                                corpses.Add(t);
                                break;
                            }
                        }
                    }
                }
            }

            foreach (Triangle corp in corpses)
                triangles.Remove(corp);

            return triangles;
        }

        /// <summary>
        /// Trim triangles outside all boundaries
        /// </summary>
        /// <param name="triangles"> Convex hull triangulation set </param>
        /// <returns> Non-convex hull triangulation set </returns>
        private List<Triangle> TrimOutsideTriangles(List<Triangle> triangles)
        {
            triangles = TrimOutsideBoundary(triangles);
            triangles = TrimOutsideHoles(triangles);

            return triangles;
        }

        /// <summary>
        /// Trim triangles outside the polygon boundary
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <returns> Non-convex hull triangulation </returns>
        private List<Triangle> TrimOutsideBoundary(List<Triangle> triangles)
        {
            List<Point> triangPoints = new List<Point>();
            List<Triangle> corpses = new List<Triangle>();

            foreach (Triangle t in triangles)
            {
                foreach (Point p in _boundary)
                {
                    if (p == t[0] || p == t[1] || p == t[2])
                        triangPoints.Add(p);
                    if (triangPoints.Count == 3)
                        break;
                }

                // If the order of the points in a triangle is clockwise (the opposite of most), it lies outside the boundary.
                if (triangPoints.Count > 0)
                {
                    if (Triangle.IsClockwise(triangPoints.ToArray()))
                        corpses.Add(t);

                    triangPoints.Clear();
                }
            }

            foreach (Triangle corp in corpses)
                triangles.Remove(corp);

            return triangles;
        }

        /// <summary>
        /// Trim triangles outside a holes boundary
        /// </summary>
        /// <param name="triangles"> Triangulation </param>
        /// <returns> Non-convex hull triangulation </returns>
        private List<Triangle> TrimOutsideHoles(List<Triangle> triangles)
        {
            List<Point> triangPoints = new List<Point>();
            List<Triangle> corpses = new List<Triangle>();

            foreach (Triangle t in triangles)
            {
                foreach (Hole hole in _holes)
                {
                    foreach (Point p in hole.Points)
                    {
                        if (p == t[0] || p == t[1] || p == t[2])
                            triangPoints.Add(p);
                        if (triangPoints.Count == 3)
                            break;
                    }

                    // If the order of the points in a triangle is clockwise (the opposite of most), it lies inside the hole.
                    if (triangPoints.Count > 0)
                    {
                        if (Triangle.IsClockwise(triangPoints.ToArray()))
                            corpses.Add(t);

                        triangPoints.Clear();
                    }
                }
            }

            foreach (Triangle corp in corpses)
                triangles.Remove(corp);

            return triangles;
        }
    }
}
