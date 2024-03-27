using System;
using System.Drawing;

namespace FiniteElements.MathPhys
{
    public class FiniteElement
    {
        private const double maxDeform = 0.2;
        private const double maxStrain = 10_000;

        private Material _material;
        private Triangle _triangle;

        private double[][] _geometry;
        private double[][] _elastisy;
        private double[][] _stiffness;

        private double[][] _deformation;
        private double[][] _strain;

        public Material Material
        {
            get { return _material; }
        }

        public Triangle Triangle
        {
            get { return _triangle; }
        }

        public double[][] Stiffness
        {
            get { return _stiffness; }
        }

        public double[][] Geometry
        {
            get { return _geometry; }
        }

        public double[][] Elastisy
        {
            get { return _elastisy; }
        }

        public double[][] Deformation
        {
            get { return _deformation; }
        }

        public double[][] Strain
        {
            get { return _strain; }
        }

        public double VonMisesStress
        {
            get { return GetVonMissesStress(); }
        }

        public FiniteElement(Material material, Triangle triangle)
        {
            _material = material;
            _triangle = triangle;
            _stiffness = GetLocalStiffness();
        }

        public double[][] GetLocalStiffness()
        {
            double[][] stiffness;
            double area = _triangle.GetArea();

            //// elastisy matrix
            double num = _material.Young / (1 - Math.Pow(_material.Poisson, 2));
            double[][] matrE = new double[][]
            {
                new double[] { 1, _material.Poisson, 0 },
                new double[] { _material.Poisson, 1, 0 },
                new double[] { 0, 0, (1 - _material.Poisson) / 2 }
            };
            matrE = FEMMethod.MatrixMul(matrE, num);
            FEMMethod.CopyMatrix(matrE, ref _elastisy);

            // triangle sides legths
            int y12 = Triangle[1].Y - Triangle[2].Y;
            int y20 = Triangle[2].Y - Triangle[0].Y;
            int y01 = Triangle[0].Y - Triangle[1].Y;
            int x21 = Triangle[2].X - Triangle[1].X;
            int x02 = Triangle[0].X - Triangle[2].X;
            int x10 = Triangle[1].X - Triangle[0].X;

            // geometry matrix
            num = 1 / (2 * area);
            double[][] matrB = new double[][]
            {
                new double[] { y12, 0, y20, 0, y01, 0 },
                new double[] { 0,x21, 0, x02, 0, x10 },
                new double[] { x21, y12, x02, y20, x10, y01 }
            };

            matrB = FEMMethod.MatrixMul(matrB, num);
            FEMMethod.CopyMatrix(matrB, ref _geometry);

            // stiffness matrix
            num = _material.Thickness * area;
            stiffness = FEMMethod.MatrixMul(FEMMethod.TransposeMatrix(matrB), matrE);
            stiffness = FEMMethod.MatrixMul(stiffness, matrB);
            stiffness = FEMMethod.MatrixMul(stiffness, num);

            return stiffness;
        }

        public double[][] GetDisplacements()
        {
            double[][] disp = new double[6][];

            for (int i = 0; i < 3; i++)
            {
                if (_triangle[i].Displacement == null)
                    throw new Exception("Error. No displacements.");

                disp[i * 2] = new double[] { _triangle[i].Displacement[0] };
                disp[i * 2 + 1] = new double[] { _triangle[i].Displacement[1] };
            }

            return disp;
        }

        public double[][] GetDeformation()
        {
            double[][] disp = GetDisplacements();
            double[][] def = FEMMethod.MatrixMul(_geometry, disp);

            FEMMethod.CopyMatrix(def, ref _deformation);
            return def;
        }

        public double[][] GetStrain()
        {
            if (_deformation == null)
                throw new Exception("Error. No deformation.");

            double[][] strain = FEMMethod.MatrixMul(_elastisy, _deformation);
            FEMMethod.CopyMatrix(strain, ref _strain);

            return strain;
        }

        public double GetVonMissesStress()
        {
            if (_strain == null)
                throw new Exception("Error. No strain");

            return Math.Sqrt(Math.Pow(_strain[0][0], 2) - _strain[0][0] * _strain[1][0] + Math.Pow(_strain[1][0], 2) + 3 * Math.Pow(_strain[2][0], 2));
        }

        public Color GetDeformationColor(int mode = 0)
        {
            // modes: 0 - sum, 1 - one, 2 - two, 3 - three

            //minHSV = 250;
            //maxHSV = 0;

            double deform;
            switch (mode)
            {
                case 0:
                    deform = GetSumDeformation();
                    break;

                case 1:
                    deform = _deformation[0][0];
                    break;

                case 2:
                    deform = _deformation[1][0];
                    break;

                case 3:
                    deform = _deformation[2][0];
                    break;

                default:
                    deform = GetSumDeformation();
                    break;
            }

            if (deform <= 0) deform = 0.0;
            else if (deform > maxDeform) deform = maxDeform - 1e-3;

            float absoluteDeform = (float)(deform / maxDeform);
            absoluteDeform = 1 - absoluteDeform;
            absoluteDeform *= 250;

            return ColorFromHSV(absoluteDeform, 1, 1);
        }

        private double GetSumDeformation()
        {
            if (_strain == null)
                throw new Exception("Error. No deformations.");

            return _deformation[0][0] + _deformation[1][0] + _deformation[2][0];
        }

        public Color GetStrainColor(int mode = 0)
        {
            // modes: 0 - sum, 1 - one, 2 - two, 3 - three

            //minHSV = 250;
            //maxHSV = 0;

            double strain;
            switch (mode)
            {
                case 0:
                    strain = GetSumStrain();
                    break;

                case 1:
                    strain = _strain[0][0];
                    break;

                case 2:
                    strain = _strain[1][0];
                    break;

                case 3:
                    strain = _strain[2][0];
                    break;

                default:
                    strain = GetSumStrain();
                    break;
            }

            if (strain <= 0) strain = 0.01;
            else if (strain > maxStrain) strain = maxStrain - 0.01;

            float absoluteStrain = (float)(strain / maxStrain);
            absoluteStrain = 1 - absoluteStrain;
            absoluteStrain *= 250;

            return ColorFromHSV(absoluteStrain, 1, 1);
        }

        private double GetSumStrain()
        {
            if (_strain == null)
                throw new Exception("Error. No strains.");

            return _strain[0][0] + _strain[1][0] + _strain[2][0];
        }

        public Color GetVonMisesStressColor()
        {
            double maxStress = 10_000;
            double stress = GetVonMissesStress();

            if (stress < 0) stress = 0.01;
            else if (stress > maxStress) stress = maxStress - 0.01;

            float absoluteStress = (float)(stress / maxStress);
            absoluteStress = 1 - absoluteStress;
            absoluteStress *= 250;

            return ColorFromHSV(absoluteStress, 1, 1);
        }

        private Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = (int)(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }
}
