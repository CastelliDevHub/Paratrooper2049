using System;
using System.Windows;

namespace Paratrooper2049
{
    internal class Weapon : BaseObject
    {
        //Bullet
        public Weapon(Point center, double direction, double formW, double formH)
        {
            _direction = direction;
            _speed = 10;
            _description = gameObject.Bullet;

            _vertices.Add(RotateObject(new Point(center.X, center.Y - 35), center, direction));
            _vertices.Add(RotateObject(new Point(center.X + 1, center.Y - 35 - 1), center, direction));
            _vertices.Add(RotateObject(new Point(center.X, center.Y - 35 - 4), center, direction));
            _vertices.Add(RotateObject(new Point(center.X - 1, center.Y - 35 - 1), center, direction));

            _battleField.Add(new Point(-5, -5));
            _battleField.Add(new Point(formW + 5, -5));
            _battleField.Add(new Point(formW + 5, formH + 5));
            _battleField.Add(new Point(-5, formH + 5));
        }

        //Laser Beam
        public Weapon(Point center, double direction)
        {
            _direction = direction;
            _speed = 255;

            _description = gameObject.Laser;

            _vertices.Add(RotateObject(new Point(center.X + 1, center.Y - 35), center, direction));
            _vertices.Add(RotateObject(new Point(center.X + 1, center.Y - 35 - 10000), center, direction));
            _vertices.Add(RotateObject(new Point(center.X - 1, center.Y - 35 - 10000), center, direction));
            _vertices.Add(RotateObject(new Point(center.X - 1, center.Y - 35), center, direction));
        }

        //cluster bomb
        public Weapon(Point center, double direction, double formW, double formH, bool missile)
        {
            _direction = direction;
            _speed = 5;
            _description = gameObject.Cluster;

            _vertices.Add(RotateObject(new Point(center.X, center.Y - 35), center, direction));
            _vertices.Add(RotateObject(new Point(center.X + 5, center.Y - 35 - 4), center, direction));
            _vertices.Add(RotateObject(new Point(center.X, center.Y - 35 - 10), center, direction));
            _vertices.Add(RotateObject(new Point(center.X - 5, center.Y - 35 - 4), center, direction));

            _battleField.Add(new Point(formW / 4, formH / 4));
            _battleField.Add(new Point(formW / 4 * 3, formH / 4));
            _battleField.Add(new Point(formW / 4 * 3, formH + 5));
            _battleField.Add(new Point(formW / 4, formH + 5));
        }

        public void UpdatePosition()
        {
            if (_description == gameObject.Bullet)
            {
                for (int i = 0; i < _vertices.Count; i++)
                {
                    _vertices[i] = new Point(_vertices[i].X + _speed * Math.Sin(_direction), _vertices[i].Y - _speed * Math.Cos(_direction));
                }

                if (!IntersectObject(_vertices, _battleField)) _lastFrame = true;
            }
            else if (_description == gameObject.Laser)
            {
                _speed -= 15;
                if (_speed < 0) _lastFrame = true;
            }
            else if (_description == gameObject.Cluster)
            {
                for (int i = 0; i < _vertices.Count; i++)
                {
                    _vertices[i] = new Point(_vertices[i].X + _speed * Math.Sin(_direction), _vertices[i].Y - _speed * Math.Cos(_direction));
                }

                if (!IntersectObject(_vertices, _battleField))
                {
                    _lastFrame = true;
                }
            }
        }
    }
}