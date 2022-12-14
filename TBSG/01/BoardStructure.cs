using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public class Socket
    {
        public Socket(eTileType _tileType)
        {
            TileType = _tileType;
            Unit = null;
            TrapEffects = new List<Effect>();
        }
        /// <summary>
        /// Copy Ctor
        /// </summary>
        public Socket(Socket _socket)
        {
            TileType = _socket.TileType;
            Unit = _socket.Unit;
            TrapEffects = _socket.TrapEffects;
        }

        public eTileType TileType;
        public UnitInstance Unit;
        public List<Effect> TrapEffects;
    }

    public class Board
    {
        /// <summary>
        /// Ctor
        /// PreCondition: _tileSet.Count > 0;
        /// PostCondition: All eTileType properties of Sockets in the Board will have an eTileType value assigned. The eTileType value for each socket will be randomly selected from those in _tileSet.
        /// </summary>
        /// <param name="_tileSet"></param>
        public Board(List<eTileType> _tileSet)
        {
            Sockets = new Socket[CoreValues.SIZE_OF_A_SIDE_OF_BOARD, CoreValues.SIZE_OF_A_SIDE_OF_BOARD];

            List<eTileType> tileTypes = TileFunctions.GetRandomTileTypes(CoreValues.SIZE_OF_A_SIDE_OF_BOARD * CoreValues.SIZE_OF_A_SIDE_OF_BOARD, _tileSet);

            for (int x = 0; x < CoreValues.SIZE_OF_A_SIDE_OF_BOARD; x++)
            {
                for(int y = 0; y < CoreValues.SIZE_OF_A_SIDE_OF_BOARD; y++)
                {
                    Sockets[x, y] = new Socket(tileTypes[CoreValues.SIZE_OF_A_SIDE_OF_BOARD * x + y]);
                }
            }
        }
        /// <summary>
        /// Copy Ctor
        /// </summary>
        public Board(Board _board)
        {
            Sockets = new Socket[CoreValues.SIZE_OF_A_SIDE_OF_BOARD, CoreValues.SIZE_OF_A_SIDE_OF_BOARD];

            for (int x = 0; x < CoreValues.SIZE_OF_A_SIDE_OF_BOARD; x++)
            {
                for (int y = 0; y < CoreValues.SIZE_OF_A_SIDE_OF_BOARD; y++)
                {
                    Sockets[x, y] = new Socket(_board.Sockets[x, y]);
                }
            }
        }

        #region Properties
        public Socket[,] Sockets { get; set; }
        #endregion
    } 
}
