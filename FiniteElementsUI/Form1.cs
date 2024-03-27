using FiniteElements;
using FiniteElements.Geometry;
using FiniteElements.MathPhys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FiniteElementsUI
{
    public partial class Form1 : Form
    {
        private enum EnterModes
        {
            None,
            Points,
            Holes,
            Forces
        }

        private enum DrawModes
        {
            OnlyGrid,
            Polygon,
            Triangles,
            DisplacementsAndStrains,
            DisplacementsAndDeforms,
            DisplacementsAndVonMises
        }

        private enum FEMDisplayModes
        {
            Sum = 0,
            One = 1,
            Two = 2,
            Three = 3
        }

        private EnterModes _currentInputMode = EnterModes.None;
        private DrawModes _currentDrawMode = DrawModes.OnlyGrid;
        private FEMDisplayModes _currentFEMDispMode = FEMDisplayModes.Sum;
        private bool _isSnappedToGrid = false;

        private Polygon _polygon = new Polygon();
        private List<Triangle> _triangles = new List<Triangle>();
        private List<FiniteElement> _elements = new List<FiniteElement>();
        private List<Force> _forces = new List<Force>();
        private List<Hole> _holes = new List<Hole>();
        private Hole _currentHole = new Hole();

        public Form1()
        {
            InitializeComponent();
            //FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            inputModeCBox.SelectedIndex = 0;
            deformStrainCBox.SelectedIndex = 0;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleparam = base.CreateParams;
                handleparam.ExStyle |= 0x02000000;
                return handleparam;
            }
        }

        private void OnFormMouseMove(object sender, MouseEventArgs e)
        {
            cursorCoordsLabel.Text = e.Location.ToString();
        }

        private void OnFormMouseClick(object sender, MouseEventArgs e)
        {
            if (_currentInputMode == EnterModes.Points)
                EnterPolygonPoint(e);
            else if (_currentInputMode == EnterModes.Forces)
                EnterForcePoint(e);
            else if (_currentInputMode == EnterModes.Holes)
                EnterHolePoint(e);
        }

        private void EnterPolygonPoint(MouseEventArgs e)
        {
            Point newPoint = (_isSnappedToGrid) ? GetNearestSnappedPoint(e.X, e.Y) : new Point(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
                _polygon.AddBoundaryPoint(newPoint);
            else if (e.Button == MouseButtons.Right)
            {
                newPoint.IsFixed = true;
                _polygon.AddBoundaryPoint(newPoint);
            }

            if (_currentDrawMode != DrawModes.Polygon)
                _currentDrawMode = DrawModes.Polygon;

            Invalidate();
        }

        private void EnterHolePoint(MouseEventArgs e)
        {
            Point newPoint = (_isSnappedToGrid) ? GetNearestSnappedPoint(e.X, e.Y) : new Point(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
                _currentHole.AddPoint(newPoint);
            else if (e.Button == MouseButtons.Right)
            {
                newPoint.IsFixed = true;
                _currentHole.AddPoint(newPoint);
            }

            Invalidate();
        }

        private void EnterForcePoint(MouseEventArgs e)
        {
            try
            {
                double dirX = double.Parse(forceXTextBox.Text);
                double dirY = double.Parse(forceYTextBox.Text);
                Point forceP = (_isSnappedToGrid) ? GetNearestSnappedPoint(e.X, e.Y) : new Point(e.X, e.Y);
                Force f = new Force(forceP, dirX, -dirY);

                _forces.Add(f);
                Invalidate();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Forces Entering Error", MessageBoxIcon.Stop);
            }
        }

        private Point GetNearestSnappedPoint(int x, int y, int step = 20)
        {
            int borderX = 0;
            int borderY = 0;
            int newX;
            int newY;

            while (borderX < x)
                borderX += step;

            if (borderX - x <= x - (borderX - step))
                newX = borderX;
            else
                newX = borderX - step;

            while (borderY < y)
                borderY += step;

            if (borderY - y <= y - (borderY - step))
                newY = borderY;
            else
                newY = borderY - step;

            return new Point(newX, newY);
        }

        private void OnClearButtonClick(object sender, EventArgs e)
        {
            _currentDrawMode = DrawModes.OnlyGrid;

            _polygon.ClearPoints();
            _triangles.Clear();
            _forces.Clear();
            _elements.Clear();

            _polygon.ClearHoles();
            _holes.Clear();
            _currentHole.ClearPoints();

            Invalidate();
        }

        private void OnTriangulateButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_polygon.Boundary.Count < 3)
                    throw new Exception("Error. You need at least three boundary points");
                else if (_polygon.IsSelfIntersects())
                    throw new Exception("Error. Polygon edges are self-intersecting.");

                if (_polygon.IsClockwise())
                {
                    _polygon.ReversePoints();

                    string message = "The points of the polygon are ordered clockwise. They will be upside down.";
                    string caption = "Check Polygon points order";
                    ShowMessage(message, caption, MessageBoxIcon.Warning);
                }

                Triangulation triang = new Triangulation(_polygon.Points, _polygon.Boundary, _polygon.Holes);
                _triangles = triang.Triangulate();

                _currentDrawMode = DrawModes.Triangles;
                Invalidate();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Triangulation Error", MessageBoxIcon.Stop);
            }
        }

        private void ShowMessage(string message, string caption, MessageBoxIcon icon)
        {
            MessageBoxButtons buttons = MessageBoxButtons.OK;

            MessageBox.Show(message, caption, buttons, icon);
        }

        private void OnShowPolygonButtonClick(object sender, EventArgs e)
        {
            _currentDrawMode = DrawModes.Polygon;
            Invalidate();
        }

        private void OnShowStrainsClick(object sender, EventArgs e)
        {
            try
            {
                UseFEM();

                _currentDrawMode = DrawModes.DisplacementsAndStrains;
                Invalidate();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Calculation Error", MessageBoxIcon.Stop);
            }
        }

        private void UseFEM()
        {
            if (_polygon.FixedPointsCount < 2)
            {
                string message = "Not enough fixed points. You need at least two.";
                throw new Exception(message);
            }
            else if (_triangles.Count == 0)
            {
                string message = "Please, triangulate polygon.";
                throw new Exception(message);
            }
            else if (_forces.Count == 0)
            {
                string message = "Not enough forces. You need at least one.";
                throw new Exception(message);
            }

            double young = double.Parse(youngTextBox.Text) * 1e3;
            double poisson = double.Parse(poissonTextBox.Text);
            double thickness = double.Parse(thicknessTextBox.Text);
            Material material = new Material(young, poisson, thickness);

            _elements.Clear();
            foreach (Triangle triangle in _triangles)
                _elements.Add(new FiniteElement(material, triangle));

            FEMMethod fem = new FEMMethod(_elements, _polygon.Points, _forces);
            fem.CalculateFEM();

            for (int i = 0; i < _polygon.Points.Count; i++)
                _polygon.Points[i].Displacement = fem.Displacements[i];

            foreach (FiniteElement el in _elements)
            {
                el.GetDeformation();
                el.GetStrain();
            }
        }

        private void OnShowDeformationsClick(object sender, EventArgs e)
        {
            try
            {
                UseFEM();

                _currentDrawMode = DrawModes.DisplacementsAndDeforms;
                Invalidate();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Calculation Error", MessageBoxIcon.Stop);
            }
        }

        private void OnInputModeChanged(object sender, EventArgs e)
        {
            _currentInputMode = (EnterModes)Enum.Parse(typeof(EnterModes), inputModeCBox.SelectedItem.ToString());
        }

        private void OnFEMDisplayModeChanged(object sender, EventArgs e)
        {
            _currentFEMDispMode = (FEMDisplayModes)Enum.Parse(typeof(FEMDisplayModes), deformStrainCBox.SelectedItem.ToString());
        }

        private void OnSnapToGridChanged(object sender, EventArgs e)
        {
            _isSnappedToGrid = !_isSnappedToGrid;
        }

        private void OnInsertInnerPointsClick(object sender, EventArgs e)
        {
            try
            {
                if (_polygon != null)
                {
                    double minDst = double.Parse(minDstTBox.Text);

                    if (minDst < 20)
                    {
                        string message = "Warning. If the density value is too high, unpredictable problems may occur. It also takes much more time for calculations.";
                        ShowMessage(message, "Density warning", MessageBoxIcon.Warning);
                    }
                    
                    _polygon.InsertPoints(1000, minDst);

                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Inserting points error", MessageBoxIcon.Stop);
            }
        }

        private void OnRemoveLastPointClick(object sender, EventArgs e)
        {
            if (_polygon.Boundary.Count > 0)
            {
                Point lastPoint = _polygon.Boundary.Last();
                _polygon.RemoveBoundaryPoint(lastPoint);

                Invalidate();
            }
        }

        private void OnClearForcesClick(object sender, EventArgs e)
        {
            _forces.Clear();

            Invalidate();
        }

        private void OnClearInnerPointsClick(object sender, EventArgs e)
        {
            if ( _polygon != null)
                _polygon.ClearInnerPoints();

            Invalidate();
        }

        private void OnShowVonMisesStressClick(object sender, EventArgs e)
        {
            try
            {
                UseFEM();

                _currentDrawMode = DrawModes.DisplacementsAndVonMises;
                Invalidate();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Calculation Error", MessageBoxIcon.Stop);
            }
        }

        private void OnQuestionmarkClick(object sender, EventArgs e)
        {
            string guide = " - Controls - \n\n";
            guide += "Red panel\nInupt modes:\n1. None - does nothing.\n2. Points - LMC to add normal point, RMC to add fixed point.\n";
            guide += "3. Holes - LMC to add normal point, RMC to add fixed point. When you finish entering current hole click on 'Add new hole' (Green panel).\n";
            guide += "4. Forces - LMC to add new force. Input force parameters in yellow panel.\nDeform/Strain modes:\n1. Sum - use to show sum of deforms/strains.\n";
            guide += "2. One - use to show first deforms/strains.\n3. Two - use to show second deforms/strains.\nThree - use to show third deforms/strains.\n";
            guide += "Before showing deforms/strains you must calculate it. Use violet panel.\n\n";

            guide += "Orange panel\nAdditional interface functions:\n1. Snap to grid - use to snap every placed point (applies on boundary points, forces and holes).\n";
            guide += "2. Remove last point - use to remove last entered boundary point.\n3. Clear Grid - use to clear everything on screen.\n\n";

            guide += "Yellow panel\nForces settings:\n1. X - force magnitude in newtons on X axe.\n2. Y - force magnitude in newtons on Y axe.\n";
            guide += "3. Clear forces - use to clear all forces.\n\n";

            guide += "Green panel\nHoles settings:\n1. Add new hole - use to add new entered hole. When hole is not entered it's green, when entered it's blue.\n";
            guide += "2. Remove last hole - use to remove last hole (entered or not entered).\n3. Clear holes - use to clear all holes (entered or not entered).\n\n";

            guide += "Light blue panel\nInner points settings:\n1. Density field - enter necessary density value in format: '1 point / k pixels' where k is entered value.\n";
            guide += "2. Insert inner points - use to insert points into the polygon boundary. It placees from 1 to 1000 points by density value as much as possible.\n";
            guide += "3. Clear inner points - use to clear all points inside the polygon boundary (except holes).\n\n";

            guide += "Blue panel\nMaterial settings:\n1. Young's module - value characterizing the ability of a material to resist stretching and compression during ";
            guide += "elastic deformation. Specified in kilopascals\n2. Poisson's ratio - value characterizing the ratio of transverse to longitudinal deformation\n";
            guide += "3. Thickness - value characterizing the tickness of a flat plate. Specified in millimeters\n\n";

            guide += "Violet panel\nMain functions:\n1. Show polygon - use to switch view back to polygon.\n2. Triangulate - use to triangulate current polygon.\n";
            guide += "3. Show strains - use to calculate and show polygon displacements and strains.\n4. Show deformations - use to calculate and show polygon ";
            guide += "displacements and deformations.\n5. Show Von Mises stress - use to calculate and show displacements and Von Mises stress.";

            ShowMessage(guide, "Controls", MessageBoxIcon.Question);
        }

        private void OnAddHoleClick(object sender, EventArgs e)
        {
            try
            {
                if (_currentHole.Points.Count < 3)
                    throw new Exception("Error. Not enough points, you need at least three.");
                else if (_currentHole.IsSelfIntersects())
                    throw new Exception("Error. Hole is self-intersecting.");

                if (!_currentHole.IsClockwise())
                {
                    _currentHole.ReversePoints();

                    string message = "The points of the hole are ordered contrclockwise. They will be upside down.";
                    string caption = "Check Hole points order";
                    ShowMessage(message, caption, MessageBoxIcon.Warning);
                }

                _polygon.AddHole(_currentHole);
                _currentHole.ClearPoints();

                Invalidate();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Hole Entering Error", MessageBoxIcon.Stop);
            }
        }

        private void OnRemoveLastHoleClick(object sender, EventArgs e)
        {
            if (_currentHole.Points.Count > 0)
                _currentHole.ClearPoints();
            else
                _polygon.RemoveLastHole();

            Invalidate();
        }

        private void OnClearHolesClick(object sender, EventArgs e)
        {
            _currentHole.ClearPoints();
            _polygon.ClearHoles();

            Invalidate();
        }
    }
}
