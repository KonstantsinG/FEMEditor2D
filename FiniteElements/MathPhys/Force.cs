using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiniteElements.MathPhys
{
    public class Force
    {
        private Point _position;
        private double _directionX;
        private double _directionY;

        public Point Position
        {
            get { return _position; }
        }

        public double DirectionX
        {
            get { return _directionX; }
        }

        public double DirectionY
        {
            get { return _directionY; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(Math.Pow(_directionX, 2) + Math.Pow(_directionY, 2)); }
        }

        public Force(Point position, double directionX, double directionY)
        {
            _position = position;
            _directionX = directionX;
            _directionY = directionY;
        }
    }
}
