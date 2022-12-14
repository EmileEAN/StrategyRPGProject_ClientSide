using System;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01
{
    public static class TileFunctions
    {
        /// <summary>
        /// PreCondition: _numberOfTiles > 0; _tileSet.Count > 0;
        /// PostCondition: Returns a List of eTileType with random eTileType values from _tileSet. Returns null if failed.
        /// </summary>
        /// <param name="_numberOfTiles"></param>
        /// <param name="_tileSet"></param>
        public static List<eTileType> GetRandomTileTypes(int _numberOfTiles, List<eTileType> _tileSet)
        {
            try
            {
                if (_numberOfTiles < 1)
                {
                    Debug.Log("The number of tiles to be created must be greater than 0.");
                    return null;
                }

                if(_tileSet.Count < 1)
                {
                    Debug.Log("_tileSet must have at least one element.");
                    _tileSet.Add(eTileType.Normal);
                }

                List<eTileType> tmp = new List<eTileType>();

                MTRandom.RandInit();

                for (int i = 1; i <= _numberOfTiles; i++)
                {
                    int index = MTRandom.GetRandInt(0, _tileSet.Count - 1);
                    tmp.Add(_tileSet[index]);
                }

                return tmp;
            }
            catch(Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }
    }


}
