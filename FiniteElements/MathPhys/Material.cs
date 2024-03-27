using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiniteElements.MathPhys
{
    public class Material
    {
        private double _young;
        private double _poisson;
        private double _thickness;

        public double Young
        {
            get { return _young; }
            set { _young = value; }
        }

        public double Poisson
        {
            get { return _poisson; }
            set { _poisson = value; }
        }

        public double Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        public Material(double young, double poisson, double thickness)
        {
            _young = young;
            _poisson = poisson;
            _thickness = thickness;

            if (young <= 0)
                throw new Exception("Young must be greather than zero.");
            else if (poisson <= 0 || poisson >= 0.5)
                throw new Exception("Poisson must be between 0 and 0.5.");
            else if (_thickness <= 0)
                throw new Exception("Thickness must be greather than zero.");
        }
    }
}
