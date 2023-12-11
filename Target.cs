using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paratrooper2049
{
    internal class Target : BaseObject
    {
        private double heliWidth = 60, heliHeight = 30;
        private double paraWidth = 10, paraHeight = 20;

        private PointCollection _ground = new PointCollection();
        private PointCollection _cannonBase = new PointCollection();
        private PointCollection _paraArea1 = new PointCollection();
        private PointCollection _paraArea2 = new PointCollection();

        public BitmapImage _heli01 = new BitmapImage(new Uri("heli01.png", UriKind.Relative));
        public BitmapImage _heli02 = new BitmapImage(new Uri("heli02.png", UriKind.Relative));

        public BitmapImage ObjectBitmap;
        private Random rand = new Random();

        public bool ReleaseHit = false;

        //heli
        public Target(double direction, double formW, double formH)
        {
            _direction = direction;
            _speed = 5;
            _description = gameObject.Heli;

            if (_direction > 0)
            {
                _vertices.Add(new Point(-heliWidth, heliHeight));
                _vertices.Add(new Point(0, heliHeight));
                _vertices.Add(new Point(0, 2 * heliHeight));
                _vertices.Add(new Point(-heliWidth, 2 * heliHeight));
                ObjectBitmap = _heli01.Clone();
            }
            else
            {
                _vertices.Add(new Point(formW, heliHeight));
                _vertices.Add(new Point(formW + heliWidth, heliHeight));
                _vertices.Add(new Point(formW + heliWidth, 2 * heliHeight));
                _vertices.Add(new Point(formW, 2 * heliHeight));
                ObjectBitmap = _heli02.Clone();
            }

            _battleField.Add(new Point(-5, -5));
            _battleField.Add(new Point(formW + 5, -5));
            _battleField.Add(new Point(formW + 5, formH + 5));
            _battleField.Add(new Point(-5, formH + 5));

            _paraArea1.Add(new Point(heliWidth, heliHeight));
            _paraArea1.Add(new Point(formW / 2 - 2 * heliWidth, heliHeight));
            _paraArea1.Add(new Point(formW / 2 - 2 * heliWidth, 2 * heliHeight));
            _paraArea1.Add(new Point(heliWidth, 2 * heliHeight));

            _paraArea2.Add(new Point(formW - heliWidth, heliHeight));
            _paraArea2.Add(new Point(formW / 2 + 2 * heliWidth, heliHeight));
            _paraArea2.Add(new Point(formW / 2 + 2 * heliWidth, 2 * heliHeight));
            _paraArea2.Add(new Point(formW - heliWidth, 2 * heliHeight));
        }
        
        //paratrooper
        public Target(PointCollection heli, double formW, double formH)
        {
            _direction = Math.PI;
            _speed = 2;
            _description = gameObject.Para;

            _vertices.Add(new Point(heli[0].X + heliWidth / 2, 2 * heliHeight));
            _vertices.Add(new Point(heli[0].X + heliWidth / 2 + paraWidth, 2 * heliHeight));
            _vertices.Add(new Point(heli[0].X + heliWidth / 2 + paraWidth, 2 * heliHeight + paraHeight));
            _vertices.Add(new Point(heli[0].X + heliWidth / 2, 2 * heliHeight + paraHeight));

            _ground.Add(new Point(-5, formH - 42));
            _ground.Add(new Point(formW + 5, formH - 42));
            _ground.Add(new Point(formW + 5, formH));
            _ground.Add(new Point(-5, formH));

            _cannonBase.Add(new Point(formW / 2 - 30, formH));
            _cannonBase.Add(new Point(formW / 2 + 30, formH));
            _cannonBase.Add(new Point(formW / 2 + 30, formH / 2));
            _cannonBase.Add(new Point(formW / 2 - 30, formH / 2));
        }
        
        //gift
        public Target(double formW, double formH)
        {
            int casual = rand.Next(1, 6);

            _direction = Math.PI;
            _speed = 2;
            _description = gameObject.Gift;

            _vertices.Add(new Point(formW / 6 * casual, -10));
            _vertices.Add(new Point(formW / 6 * casual + 10, -10));
            _vertices.Add(new Point(formW / 6 * casual + 10, 0));
            _vertices.Add(new Point(formW / 6 * casual, 0));

            _ground.Add(new Point(-5, formH - 42));
            _ground.Add(new Point(formW + 5, formH - 42));
            _ground.Add(new Point(formW + 5, formH));
            _ground.Add(new Point(-5, formH));
        }

        public void UpdatePosition()
        {
            if (_description == gameObject.Heli)
            {
                for (int i = 0; i < _vertices.Count; i++)
                {
                    _vertices[i] = new Point(_vertices[i].X + _speed * Math.Sin(_direction), _vertices[i].Y - _speed * Math.Cos(_direction));
                }

                if (!IntersectObject(_vertices, _battleField)) _lastFrame = true;

                ReleaseHit = IntersectObject(_vertices, _paraArea1) || IntersectObject(_vertices, _paraArea2);
            }
            else if (_description == gameObject.Para)
            {
                _speed = _speed * 1.01;
                for (int i = 0; i < _vertices.Count; i++)
                {
                    _vertices[i] = new Point(_vertices[i].X + _speed * Math.Sin(_direction), _vertices[i].Y - _speed * Math.Cos(_direction));
                }

                if (IntersectObject(_vertices, _ground))
                {
                    if (paraHeight == 30)
                    {
                        paraHeight = 40;
                        _vertices[0] = new Point(_vertices[0].X, _vertices[0].Y + 20);
                        _vertices[1] = new Point(_vertices[1].X, _vertices[1].Y + 20);
                    }
                    _direction = -Math.Sign(_vertices[1].X - _ground[1].X / 2) * Math.PI / 2;
                    _speed = 2;
                }

                if (IntersectObject(_vertices, _cannonBase))
                {
                    _lastFrame = true;
                    ReleaseHit = true;
                }

                if (_vertices[0].Y > _cannonBase[0].Y / 4 * 3 && paraHeight == 20)
                {
                    _speed = 2;
                    _vertices[0] = new Point(_vertices[0].X, _vertices[0].Y - paraHeight);
                    _vertices[1] = new Point(_vertices[1].X, _vertices[1].Y - paraHeight);
                    paraHeight = 30;
                }
            }
            else if (_description == gameObject.Gift)
            {
                for (int i = 0; i < _vertices.Count; i++)
                {
                    _vertices[i] = new Point(_vertices[i].X + _speed * Math.Sin(_direction), _vertices[i].Y - _speed * Math.Cos(_direction));
                }

                if (IntersectObject(_vertices, _ground)) _lastFrame = true;
            }
        }
    }
}