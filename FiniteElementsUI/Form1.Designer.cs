using FiniteElements;
using FiniteElements.Geometry;
using FiniteElements.MathPhys;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FiniteElementsUI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private int pointSize = 4;
        private int lineSize = 3;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                Graphics g = e.Graphics;
                Pen pen = new Pen(Color.Gray);

                DrawGrid(g, pen);

                switch (_currentDrawMode)
                {
                    case DrawModes.Polygon:
                        DrawCurrentHole(g);
                        DrawHoles(g);
                        DrawShape(g);
                        break;

                    case DrawModes.Triangles:
                        DrawTriangles(g);
                        break;

                    case DrawModes.DisplacementsAndStrains:
                        DrawInitShape(g);
                        DrawStrainedTriangles(g);
                        DrawDisplacedShape(g);
                        DrawFixedPoints(g);
                        break;

                    case DrawModes.DisplacementsAndDeforms:
                        DrawInitShape(g);
                        DrawDeformedTriangles(g);
                        DrawDisplacedShape(g);
                        DrawFixedPoints(g);
                        break;

                    case DrawModes.DisplacementsAndVonMises:
                        DrawInitShape(g);
                        DrawVonMisesTriangles(g);
                        DrawDisplacedShape(g);
                        DrawFixedPoints(g);
                        break;
                }
                if (_forces.Count > 0) DrawForces(g);

                pen.Dispose();
                g.Dispose();
            }
            catch (Exception){}
        }

        private void DrawGrid(Graphics g,  Pen pen)
        {
            int step = 20;
            Size screenSize = Screen.FromControl(this).Bounds.Size;

            for (int x = 200; x <= screenSize.Width; x += step)
                g.DrawLine(pen, x, 0, x, screenSize.Height);

            for (int y = 0; y <= screenSize.Height; y += step)
                g.DrawLine(pen, 180, y, screenSize.Width, y);
        }

        private void DrawShape(Graphics g)
        {
            System.Drawing.Point triP1;
            System.Drawing.Point triP2;
            System.Drawing.Point triP3;

            if (_polygon.Boundary.Count > 1)
            {
                using (Pen p = new Pen(Color.CornflowerBlue, lineSize))
                {
                    g.DrawLines(p, _polygon.GetDrawingBoundaryArray());

                    System.Drawing.Point firstPoint = _polygon.Boundary[0].ToDrawingPoint();
                    System.Drawing.Point lastPoint = _polygon.Boundary[_polygon.Boundary.Count - 1].ToDrawingPoint();
                    g.DrawLine(p, firstPoint, lastPoint);
                }
            }

            foreach (FiniteElements.Point point in _polygon.Points)
            {
                if (point.IsFixed)
                {
                    triP1 = new System.Drawing.Point(point.X, point.Y - pointSize * 2);
                    triP2 = new System.Drawing.Point(point.X + pointSize * 2, point.Y + pointSize * 2);
                    triP3 = new System.Drawing.Point(point.X - pointSize * 2, point.Y + pointSize * 2);
                    g.FillPolygon(Brushes.IndianRed, new System.Drawing.Point[] { triP1, triP2, triP3 });
                }
                else
                {
                    g.FillEllipse(Brushes.MediumSeaGreen, point.X - pointSize, point.Y - pointSize, pointSize * 2, pointSize * 2);
                }
            }
        }

        private void DrawTriangles(Graphics g)
        {
            System.Drawing.Point triP1;
            System.Drawing.Point triP2;
            System.Drawing.Point triP3;

            using (Pen p = new Pen(Color.LimeGreen, lineSize))
            {
                foreach (Triangle tri in _triangles)
                {
                    g.DrawLines(p, tri.GetDrawingArray());
                    g.DrawLine(p, tri[0].ToDrawingPoint(), tri[2].ToDrawingPoint());

                    foreach (FiniteElements.Point point in tri.Points)
                    {
                        if (point.IsFixed)
                        {
                            triP1 = new System.Drawing.Point(point.X, point.Y - pointSize * 2);
                            triP2 = new System.Drawing.Point(point.X + pointSize * 2, point.Y + pointSize * 2);
                            triP3 = new System.Drawing.Point(point.X - pointSize * 2, point.Y + pointSize * 2);
                            g.FillPolygon(Brushes.IndianRed, new System.Drawing.Point[] { triP1, triP2, triP3 });
                        }
                        else
                        {
                            g.FillEllipse(Brushes.MediumSlateBlue, point.X - pointSize, point.Y - pointSize, pointSize * 2, pointSize * 2);
                        }
                    }
                }
            }
        }

        private void DrawHoles(Graphics g)
        {
            if (_polygon.Holes.Count == 0) return;

            System.Drawing.Point[][] holesPoints = _polygon.GetDrawingHolesArray();
            System.Drawing.Point firstP;
            System.Drawing.Point lastP;

            using (Pen pen = new Pen(Brushes.CornflowerBlue, lineSize))
            {
                for (int i = 0; i < holesPoints.Length; i++)
                {
                    g.DrawLines(pen, holesPoints[i]);

                    firstP = holesPoints[i].First();
                    lastP = holesPoints[i].Last();
                    g.DrawLine(pen, firstP, lastP);
                }
            }
        }

        private void DrawCurrentHole(Graphics g)
        {
            System.Drawing.Point firstP;
            System.Drawing.Point lastP;
            System.Drawing.Point triP1;
            System.Drawing.Point triP2;
            System.Drawing.Point triP3;

            if (_currentHole.Points.Count > 1)
            {
                using (Pen pen = new Pen(Brushes.LimeGreen, lineSize))
                {
                    g.DrawLines(pen, _currentHole.GetDrawingArray());

                    firstP = _currentHole.Points.First().ToDrawingPoint();
                    lastP = _currentHole.Points.Last().ToDrawingPoint();
                    g.DrawLine(pen, firstP, lastP);
                }
            }

            foreach (FiniteElements.Point p in _currentHole.Points)
            {
                if (p.IsFixed)
                {
                    triP1 = new System.Drawing.Point(p.X, p.Y - pointSize * 2);
                    triP2 = new System.Drawing.Point(p.X + pointSize * 2, p.Y + pointSize * 2);
                    triP3 = new System.Drawing.Point(p.X - pointSize * 2, p.Y + pointSize * 2);
                    g.FillPolygon(Brushes.IndianRed, new System.Drawing.Point[] { triP1, triP2, triP3 });
                }
                else
                {
                    g.FillEllipse(Brushes.CornflowerBlue, p.X - pointSize, p.Y - pointSize, pointSize * 2, pointSize * 2);
                }
            }
        }

        private void DrawForces(Graphics g)
        {
            System.Drawing.Point[] points;
            System.Drawing.Point center;
            double angle;
            int rotatedX;
            int rotatedY;

            using (Pen p = new Pen(Brushes.Orange, lineSize))
            {
                foreach (Force f in _forces)
                {
                    points = new System.Drawing.Point[]
                    {
                        new System.Drawing.Point(0, 0), // center
                        new System.Drawing.Point(10, -10),
                        new System.Drawing.Point(10, 10),
                        new System.Drawing.Point(40, 0)
                    };

                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].X += f.Position.X;
                        points[i].Y += f.Position.Y;
                    }

                    center = points[0];
                    angle = Math.Atan2(f.DirectionY, f.DirectionX) + Math.PI;

                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].X -= center.X;
                        points[i].Y -= center.Y;

                        rotatedX = Convert.ToInt32(points[i].X * Math.Cos(angle) - points[i].Y * Math.Sin(angle));
                        rotatedY = Convert.ToInt32(points[i].X * Math.Sin(angle) + points[i].Y * Math.Cos(angle));

                        points[i].X = rotatedX + center.X;
                        points[i].Y = rotatedY + center.Y;
                    }

                    g.DrawLine(p, points[0], points[1]);
                    g.DrawLine(p, points[0], points[2]);
                    g.DrawLine(p, points[0], points[3]);
                }
            }
        }

        private void DrawStrainedTriangles(Graphics g)
        {
            foreach (FiniteElement el in _elements)
                g.FillPolygon(new SolidBrush(el.GetStrainColor((int)_currentFEMDispMode)), el.Triangle.GetDrawingDisplacedArray());
        }

        private void DrawDeformedTriangles(Graphics g)
        {
            foreach (FiniteElement el in _elements)
                g.FillPolygon(new SolidBrush(el.GetDeformationColor((int)_currentFEMDispMode)), el.Triangle.GetDrawingDisplacedArray());
        }

        private void DrawVonMisesTriangles(Graphics g)
        {
            foreach (FiniteElement el in _elements)
                g.FillPolygon(new SolidBrush(el.GetVonMisesStressColor()), el.Triangle.GetDrawingDisplacedArray());
        }

        private void DrawDisplacedShape(Graphics g)
        {
            using (Pen p = new Pen(Color.BlueViolet, lineSize))
            {
                g.DrawLines(p, _polygon.GetDrawingDisplacedArray());

                System.Drawing.Point firstPoint = _polygon.Boundary[0].ToDrawingDisplacedPoint();
                System.Drawing.Point lastPoint = _polygon.Boundary[_polygon.Boundary.Count - 1].ToDrawingDisplacedPoint();
                g.DrawLine(p, firstPoint, lastPoint);

                foreach (Hole h in _polygon.Holes)
                {
                    g.DrawLines(p, h.GetDrawingDisplacedArray());

                    firstPoint = h.Points[0].ToDrawingDisplacedPoint();
                    lastPoint = h.Points[h.Points.Count - 1].ToDrawingDisplacedPoint();
                    g.DrawLine(p, firstPoint, lastPoint);
                }
            }
        }

        private void DrawInitShape(Graphics g)
        {
            using (Pen p = new Pen(Color.LightSlateGray, lineSize))
            {
                g.DrawLines(p, _polygon.GetDrawingBoundaryArray());

                System.Drawing.Point firstPoint = _polygon.Boundary[0].ToDrawingPoint();
                System.Drawing.Point lastPoint = _polygon.Boundary[_polygon.Boundary.Count - 1].ToDrawingPoint();
                g.DrawLine(p, firstPoint, lastPoint);

                foreach (Hole h in _polygon.Holes)
                {
                    g.DrawLines(p, h.GetDrawingArray());

                    firstPoint = h.Points[0].ToDrawingPoint();
                    lastPoint = h.Points[h.Points.Count - 1].ToDrawingPoint();
                    g.DrawLine(p, firstPoint, lastPoint);
                }
            }
        }

        private void DrawFixedPoints(Graphics g)
        {
            System.Drawing.Point triP1;
            System.Drawing.Point triP2;
            System.Drawing.Point triP3;

            foreach (FiniteElements.Point point in _polygon.Points)
            {
                if (point.IsFixed)
                {
                    triP1 = new System.Drawing.Point(point.X, point.Y - pointSize * 2);
                    triP2 = new System.Drawing.Point(point.X + pointSize * 2, point.Y + pointSize * 2);
                    triP3 = new System.Drawing.Point(point.X - pointSize * 2, point.Y + pointSize * 2);
                    g.FillPolygon(Brushes.IndianRed, new System.Drawing.Point[] { triP1, triP2, triP3 });
                }
            }
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.deformStrainCBox = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.inputModeCBox = new System.Windows.Forms.ComboBox();
            this.cursorCoordsLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.youngTextBox = new System.Windows.Forms.TextBox();
            this.poissonTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.thicknessTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.button10 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.forceYTextBox = new System.Windows.Forms.TextBox();
            this.forceXTextBox = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.button12 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.minDstTBox = new System.Windows.Forms.TextBox();
            this.button9 = new System.Windows.Forms.Button();
            this.panel6 = new System.Windows.Forms.Panel();
            this.button11 = new System.Windows.Forms.Button();
            this.panel7 = new System.Windows.Forms.Panel();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.deformStrainCBox);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.inputModeCBox);
            this.panel1.Location = new System.Drawing.Point(2, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(177, 100);
            this.panel1.TabIndex = 0;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label12.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label12.Location = new System.Drawing.Point(20, 51);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(120, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "Deform/Strain mode";
            // 
            // deformStrainCBox
            // 
            this.deformStrainCBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deformStrainCBox.FormattingEnabled = true;
            this.deformStrainCBox.Items.AddRange(new object[] {
            "Sum",
            "One",
            "Two",
            "Three"});
            this.deformStrainCBox.Location = new System.Drawing.Point(14, 67);
            this.deformStrainCBox.Name = "deformStrainCBox";
            this.deformStrainCBox.Size = new System.Drawing.Size(147, 21);
            this.deformStrainCBox.TabIndex = 13;
            this.deformStrainCBox.SelectedIndexChanged += new System.EventHandler(this.OnFEMDisplayModeChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label11.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label11.Location = new System.Drawing.Point(55, 9);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(70, 13);
            this.label11.TabIndex = 11;
            this.label11.Text = "Input mode";
            // 
            // inputModeCBox
            // 
            this.inputModeCBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inputModeCBox.FormattingEnabled = true;
            this.inputModeCBox.Items.AddRange(new object[] {
            "None",
            "Points",
            "Holes",
            "Forces"});
            this.inputModeCBox.Location = new System.Drawing.Point(14, 27);
            this.inputModeCBox.Name = "inputModeCBox";
            this.inputModeCBox.Size = new System.Drawing.Size(147, 21);
            this.inputModeCBox.TabIndex = 11;
            this.inputModeCBox.SelectedIndexChanged += new System.EventHandler(this.OnInputModeChanged);
            // 
            // cursorCoordsLabel
            // 
            this.cursorCoordsLabel.AutoSize = true;
            this.cursorCoordsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cursorCoordsLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.cursorCoordsLabel.Location = new System.Drawing.Point(7, 1023);
            this.cursorCoordsLabel.Name = "cursorCoordsLabel";
            this.cursorCoordsLabel.Size = new System.Drawing.Size(51, 18);
            this.cursorCoordsLabel.TabIndex = 1;
            this.cursorCoordsLabel.Text = "(X, Y)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(9, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Young\'s module";
            // 
            // youngTextBox
            // 
            this.youngTextBox.Location = new System.Drawing.Point(12, 52);
            this.youngTextBox.Name = "youngTextBox";
            this.youngTextBox.Size = new System.Drawing.Size(100, 20);
            this.youngTextBox.TabIndex = 2;
            this.youngTextBox.Text = "210";
            // 
            // poissonTextBox
            // 
            this.poissonTextBox.Location = new System.Drawing.Point(11, 91);
            this.poissonTextBox.Name = "poissonTextBox";
            this.poissonTextBox.Size = new System.Drawing.Size(100, 20);
            this.poissonTextBox.TabIndex = 5;
            this.poissonTextBox.Text = "0.25";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(8, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Poisson\'s ratio";
            // 
            // thicknessTextBox
            // 
            this.thicknessTextBox.Location = new System.Drawing.Point(10, 130);
            this.thicknessTextBox.Name = "thicknessTextBox";
            this.thicknessTextBox.Size = new System.Drawing.Size(100, 20);
            this.thicknessTextBox.TabIndex = 7;
            this.thicknessTextBox.Text = "0.1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(7, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Thickness";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(200)))), ((int)(((byte)(240)))));
            this.panel2.Controls.Add(this.label15);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.thicknessTextBox);
            this.panel2.Controls.Add(this.poissonTextBox);
            this.panel2.Controls.Add(this.youngTextBox);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Location = new System.Drawing.Point(2, 635);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(177, 164);
            this.panel2.TabIndex = 8;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label10.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label10.Location = new System.Drawing.Point(116, 133);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "(m)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label9.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label9.Location = new System.Drawing.Point(118, 55);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "(KPa)";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(170)))), ((int)(((byte)(240)))));
            this.panel3.Controls.Add(this.button10);
            this.panel3.Controls.Add(this.button4);
            this.panel3.Controls.Add(this.button5);
            this.panel3.Controls.Add(this.button3);
            this.panel3.Controls.Add(this.button1);
            this.panel3.Location = new System.Drawing.Point(2, 805);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(177, 179);
            this.panel3.TabIndex = 9;
            // 
            // button10
            // 
            this.button10.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button10.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button10.Location = new System.Drawing.Point(11, 141);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(150, 25);
            this.button10.TabIndex = 16;
            this.button10.Text = "Show Von Mises stress";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.OnShowVonMisesStressClick);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button4.Location = new System.Drawing.Point(11, 110);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(150, 25);
            this.button4.TabIndex = 15;
            this.button4.Text = "Show deformations";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnShowDeformationsClick);
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button5.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button5.Location = new System.Drawing.Point(11, 48);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(150, 25);
            this.button5.TabIndex = 14;
            this.button5.Text = "Triangulate";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.OnTriangulateButtonClick);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button3.Location = new System.Drawing.Point(11, 79);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(150, 25);
            this.button3.TabIndex = 12;
            this.button3.Text = "Show strains";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OnShowStrainsClick);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button1.Location = new System.Drawing.Point(11, 17);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 25);
            this.button1.TabIndex = 10;
            this.button1.Text = "Show Polygon";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnShowPolygonButtonClick);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button2.Location = new System.Drawing.Point(14, 65);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(150, 25);
            this.button2.TabIndex = 11;
            this.button2.Text = "Clear Grid";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClearButtonClick);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(230)))), ((int)(((byte)(170)))));
            this.panel4.Controls.Add(this.button8);
            this.panel4.Controls.Add(this.label8);
            this.panel4.Controls.Add(this.label7);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.label5);
            this.panel4.Controls.Add(this.label6);
            this.panel4.Controls.Add(this.forceYTextBox);
            this.panel4.Controls.Add(this.forceXTextBox);
            this.panel4.Location = new System.Drawing.Point(2, 217);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(177, 128);
            this.panel4.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label8.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label8.Location = new System.Drawing.Point(143, 68);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(24, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "(N)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label7.Location = new System.Drawing.Point(143, 42);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "(N)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label4.Location = new System.Drawing.Point(8, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Y: ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label5.Location = new System.Drawing.Point(3, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(167, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "--------------- Force ---------------";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label6.Location = new System.Drawing.Point(8, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(23, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "X: ";
            // 
            // forceYTextBox
            // 
            this.forceYTextBox.Location = new System.Drawing.Point(37, 65);
            this.forceYTextBox.Name = "forceYTextBox";
            this.forceYTextBox.Size = new System.Drawing.Size(100, 20);
            this.forceYTextBox.TabIndex = 5;
            this.forceYTextBox.Text = "-5000";
            // 
            // forceXTextBox
            // 
            this.forceXTextBox.Location = new System.Drawing.Point(37, 39);
            this.forceXTextBox.Name = "forceXTextBox";
            this.forceXTextBox.Size = new System.Drawing.Size(100, 20);
            this.forceXTextBox.TabIndex = 2;
            this.forceXTextBox.Text = "5000";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(14, 11);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(83, 17);
            this.checkBox1.TabIndex = 10;
            this.checkBox1.Text = "Snap to grid";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.OnSnapToGridChanged);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(210)))), ((int)(((byte)(170)))));
            this.panel5.Controls.Add(this.button7);
            this.panel5.Controls.Add(this.checkBox1);
            this.panel5.Controls.Add(this.button2);
            this.panel5.Location = new System.Drawing.Point(2, 109);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(177, 102);
            this.panel5.TabIndex = 11;
            // 
            // button12
            // 
            this.button12.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button12.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button12.Location = new System.Drawing.Point(14, 40);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(150, 25);
            this.button12.TabIndex = 19;
            this.button12.Text = "Add new hole";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.OnAddHoleClick);
            // 
            // button8
            // 
            this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button8.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button8.Location = new System.Drawing.Point(14, 91);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(150, 25);
            this.button8.TabIndex = 18;
            this.button8.Text = "Clear forces";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.OnClearForcesClick);
            // 
            // button7
            // 
            this.button7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button7.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button7.Location = new System.Drawing.Point(14, 34);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(150, 25);
            this.button7.TabIndex = 17;
            this.button7.Text = "Remove last point";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.OnRemoveLastPointClick);
            // 
            // button6
            // 
            this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button6.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button6.Location = new System.Drawing.Point(14, 62);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(150, 25);
            this.button6.TabIndex = 16;
            this.button6.Text = "Insert inner points";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.OnInsertInnerPointsClick);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label14.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label14.Location = new System.Drawing.Point(92, 39);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(79, 13);
            this.label14.TabIndex = 19;
            this.label14.Text = "(1/k density)";
            // 
            // minDstTBox
            // 
            this.minDstTBox.Location = new System.Drawing.Point(17, 36);
            this.minDstTBox.Name = "minDstTBox";
            this.minDstTBox.Size = new System.Drawing.Size(72, 20);
            this.minDstTBox.TabIndex = 20;
            this.minDstTBox.Text = "20";
            // 
            // button9
            // 
            this.button9.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button9.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button9.Location = new System.Drawing.Point(15, 93);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(150, 25);
            this.button9.TabIndex = 21;
            this.button9.Text = "Clear inner points";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.OnClearInnerPointsClick);
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.panel6.Controls.Add(this.label16);
            this.panel6.Controls.Add(this.button9);
            this.panel6.Controls.Add(this.button6);
            this.panel6.Controls.Add(this.label14);
            this.panel6.Controls.Add(this.minDstTBox);
            this.panel6.Location = new System.Drawing.Point(2, 498);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(177, 131);
            this.panel6.TabIndex = 22;
            // 
            // button11
            // 
            this.button11.BackColor = System.Drawing.Color.LightGray;
            this.button11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button11.Location = new System.Drawing.Point(1866, 1001);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(40, 40);
            this.button11.TabIndex = 23;
            this.button11.Text = "?";
            this.button11.UseVisualStyleBackColor = false;
            this.button11.Click += new System.EventHandler(this.OnQuestionmarkClick);
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(240)))), ((int)(((byte)(170)))));
            this.panel7.Controls.Add(this.label13);
            this.panel7.Controls.Add(this.button14);
            this.panel7.Controls.Add(this.button13);
            this.panel7.Controls.Add(this.button12);
            this.panel7.Location = new System.Drawing.Point(2, 351);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(177, 141);
            this.panel7.TabIndex = 24;
            // 
            // button13
            // 
            this.button13.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button13.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button13.Location = new System.Drawing.Point(14, 71);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(150, 25);
            this.button13.TabIndex = 20;
            this.button13.Text = "Remove last hole";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.OnRemoveLastHoleClick);
            // 
            // button14
            // 
            this.button14.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button14.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button14.Location = new System.Drawing.Point(14, 102);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(150, 25);
            this.button14.TabIndex = 21;
            this.button14.Text = "Clear holes";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.OnClearHolesClick);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label15.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label15.Location = new System.Drawing.Point(1, 11);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(172, 13);
            this.label15.TabIndex = 19;
            this.label15.Text = "-------------- Material --------------";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label13.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label13.Location = new System.Drawing.Point(7, 12);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(167, 13);
            this.label13.TabIndex = 19;
            this.label13.Text = "--------------- Holes ---------------";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.471698F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label16.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label16.Location = new System.Drawing.Point(3, 14);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(171, 13);
            this.label16.TabIndex = 22;
            this.label16.Text = "----------- Inner Points -----------";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1039);
            this.Controls.Add(this.panel7);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.cursorCoordsLabel);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFormMouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnFormMouseMove);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private Label cursorCoordsLabel;
        private Label label1;
        private TextBox youngTextBox;
        private TextBox poissonTextBox;
        private Label label2;
        private TextBox thicknessTextBox;
        private Label label3;
        private Panel panel2;
        private Panel panel3;
        private Button button1;
        private Button button3;
        private Button button2;
        private Button button5;
        private Panel panel4;
        private Label label5;
        private Label label6;
        private TextBox forceYTextBox;
        private TextBox forceXTextBox;
        private Label label8;
        private Label label7;
        private Label label4;
        private Label label9;
        private Label label10;
        private Button button4;
        private ComboBox inputModeCBox;
        private Label label12;
        private ComboBox deformStrainCBox;
        private Label label11;
        private CheckBox checkBox1;
        private Panel panel5;
        private Button button6;
        private Button button8;
        private Button button7;
        private Label label14;
        private TextBox minDstTBox;
        private Button button9;
        private Panel panel6;
        private Button button10;
        private Button button11;
        private Button button12;
        private Panel panel7;
        private Button button14;
        private Button button13;
        private Label label15;
        private Label label13;
        private Label label16;
    }
}

