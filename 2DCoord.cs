using System;
using System.Collections.Generic;

namespace EEANWorks
{
    public sealed class _2DCoord : IComparable, IComparable<_2DCoord>, IEquatable<_2DCoord>, IDeepCopyable<_2DCoord>
    {
        //ctor
        public _2DCoord(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }

        #region Properties
        public int X { get; private set; }
        public int Y { get; private set; }
        #endregion

        #region Public Methods
        public int CompareTo(_2DCoord _targetCoord)
        {
            if (_targetCoord == null) return -1;

            if (X == _targetCoord.X)
                return Y.CompareTo(_targetCoord.Y);
            else
                return X.CompareTo(_targetCoord.X);
        }

        public int CompareTo(object _object)
        {
            if (ReferenceEquals(_object, null)) return -1;
            if (ReferenceEquals(this, _object)) return 0;
            if (_object.GetType() != GetType())
                return -1;

            return CompareTo(_object as _2DCoord);
        }

        public bool Equals(_2DCoord _targetCoord)
        {
            if (ReferenceEquals(_targetCoord, null)) return false;

            return X == _targetCoord.X && Y == _targetCoord.Y;
        }

        public override bool Equals(object _object)
        {
            if (ReferenceEquals(_object, null)) return false;
            if (ReferenceEquals(this, _object)) return true;
            if (_object.GetType() != GetType()) return false;

            return Equals(_object as _2DCoord);
        }

        public override int GetHashCode() { return X ^ Y; }

        public static bool operator ==(_2DCoord _a, _2DCoord _b)
        {
            if (ReferenceEquals(_a, null))
                return ReferenceEquals(_b, null);

            return _a.Equals(_b);
        }

        public static bool operator !=(_2DCoord _a, _2DCoord _b)
        {
            if (ReferenceEquals(_a, null))
                return !ReferenceEquals(_b, null);

            return !_a.Equals(_b);
        }

        public static _2DCoord operator +(_2DCoord _a, _2DCoord _b)
        {
            int x = _a.X + _b.X;
            int y = _a.Y + _b.Y;

            return new _2DCoord(x, y);
        }

        /// <summary>
        /// A string representing an array of _2DCoords will be returned
        /// </summary>
        public static string CoordsToString(List<_2DCoord> _coords)
        {
            string result = "";

            if (_coords != null)
            {
                foreach (_2DCoord coord in _coords)
                {
                    result += coord.X.ToString() + "," + coord.Y.ToString() + ";";
                }
            }

            return result;
        }

        public static List<_2DCoord> StringToCoords(string _coordsString)
        {
            List<_2DCoord> result = new List<_2DCoord>();

            try
            {
                string[] separators = { ",", ";" };
                string[] coordStrings = _coordsString.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < coordStrings.Length; i += 2)
                {
                    result.Add(new _2DCoord(Convert.ToInt32(coordStrings[i]), Convert.ToInt32(coordStrings[i + 1])));
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// _2DCoord will be rotated _timesToRotate times towards the positive X
        /// </summary>
        public void Rotate90DegreesAnticlockwise(int _timesToRotate)
        {
            while (_timesToRotate > 0)
            {
                double tmp = X;
                double multiplier = CoreValues.MULTIPLIER_FOR_DEGREE_TO_RADIAN;
                X = Convert.ToInt32(X * Math.Cos(90.0D * multiplier) + Y * Math.Sin(90.0D * multiplier));
                Y = Convert.ToInt32(Y * Math.Cos(90.0D * multiplier) - tmp * Math.Sin(90.0D * multiplier));

                _timesToRotate--;
            }
        }

        /// <summary>
        /// Values of X and Y will be swapped
        /// </summary>
        public void InvertXY()
        {
            int tmp = X;
            X = Y;
            Y = tmp;
        }

        public _2DCoord DeepCopy()
        {
            return (_2DCoord)this.MemberwiseClone();
        }
        #endregion
    }
}
