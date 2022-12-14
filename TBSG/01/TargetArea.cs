using System;
using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public static class TargetArea
    {
        //Ctor
        static TargetArea()
        {
            TargetAreaSets = new List<List<_2DCoord>>();

            #region 4 Coordinates
            #region Cross I
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 1),
                new _2DCoord(1, 0),
                new _2DCoord(0, -1),
                new _2DCoord(-1, 0)
            });
            #endregion

            #region Diagonal Cross I
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(1, 1),
                new _2DCoord(1, -1),
                new _2DCoord(-1, -1),
                new _2DCoord(-1, 1)
            });
            #endregion
            #endregion

            #region 8 Coordinates
            #region Cross II
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 1),
                new _2DCoord(1, 0),
                new _2DCoord(0, -1),
                new _2DCoord(-1, 0),
                new _2DCoord(0, 2),
                new _2DCoord(2, 0),
                new _2DCoord(0, -2),
                new _2DCoord(-2, 0)
            });
            #endregion

            #region Diagonal Cross II
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(1, 1),
                new _2DCoord(1, -1),
                new _2DCoord(-1, -1),
                new _2DCoord(-1, 1),
                new _2DCoord(2, 2),
                new _2DCoord(2, -2),
                new _2DCoord(-2, -2),
                new _2DCoord(-2, 2)
            });
            #endregion

            #region Square
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 1),
                new _2DCoord(1, 1),
                new _2DCoord(1, 0),
                new _2DCoord(1, -1),
                new _2DCoord(0, -1),
                new _2DCoord(-1, -1),
                new _2DCoord(-1, 0),
                new _2DCoord(-1, 1)
            });
            #endregion

            #region Knight
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(1, 2),
                new _2DCoord(2, 1),
                new _2DCoord(2, -1),
                new _2DCoord(1, -2),
                new _2DCoord(-1, -2),
                new _2DCoord(-2, -1),
                new _2DCoord(-2, 1),
                new _2DCoord(-1, 2),
            });
            #endregion

            #region Fixed Distance II
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 2),
                new _2DCoord(1, 1),
                new _2DCoord(2, 0),
                new _2DCoord(1, -1),
                new _2DCoord(0, -2),
                new _2DCoord(-1, -1),
                new _2DCoord(-2, 0),
                new _2DCoord(-1, 1)
            });
            #endregion
            #endregion

            #region 12 Coordinates
            #region Cross III
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 1),
                new _2DCoord(1, 0),
                new _2DCoord(0, -1),
                new _2DCoord(-1, 0),
                new _2DCoord(0, 2),
                new _2DCoord(2, 0),
                new _2DCoord(0, -2),
                new _2DCoord(-2, 0),
                new _2DCoord(0, 3),
                new _2DCoord(3, 0),
                new _2DCoord(0, -3),
                new _2DCoord(-3, 0)
            });
            #endregion

            #region Diagonal Cross III
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(1, 1),
                new _2DCoord(1, -1),
                new _2DCoord(-1, -1),
                new _2DCoord(-1, 1),
                new _2DCoord(2, 2),
                new _2DCoord(2, -2),
                new _2DCoord(-2, -2),
                new _2DCoord(-2, 2),
                new _2DCoord(3, 3),
                new _2DCoord(3, -3),
                new _2DCoord(-3, -3),
                new _2DCoord(-3, 3)
            });
            #endregion

            #region Cross Alter
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 1),
                new _2DCoord(1, 1),
                new _2DCoord(1, 0),
                new _2DCoord(1, -1),
                new _2DCoord(0, -1),
                new _2DCoord(-1, -1),
                new _2DCoord(-1, 0),
                new _2DCoord(-1, 1),
                new _2DCoord(0, 2),
                new _2DCoord(2, 0),
                new _2DCoord(0, -2),
                new _2DCoord(-2, 0)
            });
            #endregion

            #region Diagonal Cross Alter
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 1),
                new _2DCoord(1, 1),
                new _2DCoord(1, 0),
                new _2DCoord(1, -1),
                new _2DCoord(0, -1),
                new _2DCoord(-1, -1),
                new _2DCoord(-1, 0),
                new _2DCoord(-1, 1),
                new _2DCoord(2, 2),
                new _2DCoord(2, -2),
                new _2DCoord(-2, -2),
                new _2DCoord(-2, 2)
            });
            #endregion

            #region Cross Knight
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 1),
                new _2DCoord(1, 0),
                new _2DCoord(0, -1),
                new _2DCoord(-1, 0),
                new _2DCoord(1, 2),
                new _2DCoord(2, 1),
                new _2DCoord(2, -1),
                new _2DCoord(1, -2),
                new _2DCoord(-1, -2),
                new _2DCoord(-2, -1),
                new _2DCoord(-2, 1),
                new _2DCoord(-1, 2),
            });
            #endregion

            #region Diagonal Cross Knight
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(1, 1),
                new _2DCoord(1, -1),
                new _2DCoord(-1, -1),
                new _2DCoord(-1, 1),
                new _2DCoord(1, 2),
                new _2DCoord(2, 1),
                new _2DCoord(2, -1),
                new _2DCoord(1, -2),
                new _2DCoord(-1, -2),
                new _2DCoord(-2, -1),
                new _2DCoord(-2, 1),
                new _2DCoord(-1, 2),
            });
            #endregion

            #region Fixed Distance III
            TargetAreaSets.Add(new List<_2DCoord>
            {
                new _2DCoord(0, 3),
                new _2DCoord(1, 2),
                new _2DCoord(2, 1),
                new _2DCoord(3, 0),
                new _2DCoord(2, -1),
                new _2DCoord(1, -2),
                new _2DCoord(0, -3),
                new _2DCoord(-1, -2),
                new _2DCoord(-2, -1),
                new _2DCoord(-3, 0),
                new _2DCoord(-2, 1),
                new _2DCoord(-1, 2),
            });
            #endregion
            #endregion
        }

        #region Properties
        private static List<List<_2DCoord>> TargetAreaSets { get; }
        #endregion

        #region Public Methods
        public static List<_2DCoord> GetTargetArea(eTargetRangeClassification _targetAreaType)
        {
            int targetAreaTypeIndex = Convert.ToInt32(_targetAreaType);
            return TargetAreaSets[targetAreaTypeIndex].DeepCopy();
        }
        #endregion
    }
}
