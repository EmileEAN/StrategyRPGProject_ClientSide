using System;
using System.Collections.Generic;
using System.Linq;

namespace EEANWorks.Games.TBSG._01
{
    public class Field
    {
        /// <summary>
        /// Ctor
        /// //PreCondition: _board, _player1 and _player2 have been initialized successfully
        /// //PostCondition: Field will be instantiated successfully
        /// </summary>
        private Field(Board _board, PlayerOnBoard _player1, PlayerOnBoard _player2)
        {
            Board = _board;

            m_players = new PlayerOnBoard[] { _player1, _player2 };

            //Set Initial position for each unit that Players[0] owns
            foreach (var unit in m_players[0].AlliedUnits.Select((v, i) => new { v, i }))
            {
                switch(unit.i)
                {
                    default: //case 0;
                        Board.Sockets[1, 0].Unit = unit.v;
                        break;
                    case 1:
                        Board.Sockets[2, 1].Unit = unit.v;
                        break;
                    case 2:
                        Board.Sockets[3, 0].Unit = unit.v;
                        break;
                    case 3:
                        Board.Sockets[4, 1].Unit = unit.v;
                        break;
                    case 4:
                        Board.Sockets[5, 0].Unit = unit.v;
                        break;
                }
            }

            int size = CoreValues.SIZE_OF_A_SIDE_OF_BOARD;
            //Set Initial position for each unit that Players[1] owns
            foreach (var unit in m_players[1].AlliedUnits.Select((v, i) => new { v, i }))
            {
                switch (unit.i)
                {
                    default: //case 0;
                        Board.Sockets[size - 2, size - 1].Unit = unit.v;
                        break;
                    case 1:
                        Board.Sockets[size - 3, size - 2].Unit = unit.v;
                        break;
                    case 2:
                        Board.Sockets[size - 4, size - 1].Unit = unit.v;
                        break;
                    case 3:
                        Board.Sockets[size - 5, size - 2].Unit = unit.v;
                        break;
                    case 4:
                        Board.Sockets[size - 6, size - 1].Unit = unit.v;
                        break;
                }
            }

            //Assign all existing units into list
            Units = new List<UnitInstance>();
            foreach (PlayerOnBoard pob in m_players)
            {
                foreach (UnitInstance unit in pob.AlliedUnits)
                {
                    Units.Add(unit);
                }
            }
        }
        /// <summary>
        /// Copy Ctor
        /// </summary>
        public Field(Field _field)
        {
            Board = new Board(_field.Board);

            m_players = new PlayerOnBoard[] { _field.Players[0], _field.Players[1] };

            Units = new List<UnitInstance>(_field.Units);
        }

        #region Properties
        public Board Board { get; }

        public IList<PlayerOnBoard> Players { get { return Array.AsReadOnly(m_players); } }

        /// <summary>
        ///List to reference all existing units, regardless of the owner player
        /// </summary>
        public List<UnitInstance> Units { get; private set; }
        #endregion

        #region Private Fields
        private PlayerOnBoard[] m_players;
        #endregion

        #region Public Functions
        /// <summary>
        /// PreCondition: _player1 and _player2 have been initialized successfully. _player1.Teams.Count > _player1TeamIndex; _player2.Teams.Count > _player2TeamIndex; _tileSet.Count > 0;
        /// PostCondition: An Instance of Field will be created and returned successfully.
        /// </summary>
        /// <returns></returns>
        public static Field NewField(Player _player1, int _player1TeamIndex, Player _player2, int _player2TeamIndex, List<eTileType> _tileSet)
        {
            Board board = new Board(_tileSet);

            PlayerOnBoard player1 = new PlayerOnBoard(_player1, _player1TeamIndex, true);
            PlayerOnBoard player2 = new PlayerOnBoard(_player2, _player2TeamIndex, false);

            if (player1.AlliedUnits.Count < 1 || player2.AlliedUnits.Count < 1)
                return null;

            return new Field(board, player1, player2);
        }

        /// <summary>
        /// PreCondition: _unit has been initialized successfully; _unit has been added to Units (list of UnitInstance) successfully.
        /// PostCondition: If _unit is registered in Units property, the corresponding index value will be returned. -1 will be returned if _unit was not found in Units.
        /// </summary>
        /// <param name="_unit"></param>
        /// <returns></returns>
        public int GetUnitIndex(UnitInstance _unit)
        {
            int unitIndex = 0;
            while (unitIndex < Units.Count)
            {
                if (Units[unitIndex] == _unit)
                    return unitIndex;

                unitIndex++;
            }

            return -1; // This will be returned when _unit was not found within Units
        }

        public List<UnitInstance> GetUnitsInCoords(List<_2DCoord> _targetCoords)
        {
            List<UnitInstance> result = new List<UnitInstance>();

            if (_targetCoords == null)
                return result;

            foreach (_2DCoord targetCoord in _targetCoords)
            {
                if (IsCoordWithinBoard(targetCoord))
                {
                    UnitInstance unit = Board.Sockets[targetCoord.X, targetCoord.Y].Unit;
                    if (unit != null)
                        result.Add(unit);
                }
            }

            return result;
        }

        public List<Socket> GetSocketsInCoords(List<_2DCoord> _targetCoords)
        {
            List<Socket> result = new List<Socket>();
            foreach (_2DCoord targetCoord in _targetCoords)
            {
                if (IsCoordWithinBoard(targetCoord))
                {
                    result.Add(Board.Sockets[targetCoord.X, targetCoord.Y]);
                }
            }

            return result;
        }

        /// <summary>
        /// PreCondition: _unit has been initialized successfully.
        /// PostCondition: If succeeded, will return a valid _2DCoord within Board. Will return _2DCoord(-1, -1) if failed.
        /// </summary>
        /// <returns></returns>
        public _2DCoord UnitLocation(UnitInstance _unit)
        {
            for (int x = 1; x <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD; x++)
            {
                for (int y = 1; y <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD; y++)
                {
                    if (Board.Sockets[x - 1, y - 1].Unit == _unit)
                        return new _2DCoord(x - 1, y - 1);
                }
            }

            return new _2DCoord(-1, -1); // invalid coordinate for a board
        }

        public _2DCoord SocketLocation(Socket _socket)
        {
            for (int x = 1; x <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD; x++)
            {
                for (int y = 1; y <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD; y++)
                {
                    if (Board.Sockets[x - 1, y - 1] == _socket)
                        return new _2DCoord(x - 1, y - 1);
                }
            }

            return new _2DCoord(-1, -1); // invalid coordinate for a board
        }

        /// <summary>
        /// PreCondition: None.
        /// PostCondition: All Units with isAlive property set to false, and that are assigned to a Socket on the Board, will be removed from the Socket.
        /// </summary>
        /// 
        public void RemoveNonAliveUnitFromBoard(UnitInstance _unit)
        {
            if (_unit == null)
                return;

            _2DCoord currentLocation = UnitLocation(_unit);
            if (!_unit.IsAlive
                && IsCoordWithinBoard(currentLocation))
                this.Board.Sockets[currentLocation.X, currentLocation.Y].Unit = null; // Remove unit assigned to the Socket
        }

        //public void RemoveNonAliveUnitsFromBoard()
        //{
        //    foreach (PlayerOnBoard p in Players)
        //    {
        //        foreach (UnitInstance u in p.AlliedUnits)
        //        {
        //            _2DCoord currentLocation = UnitLocation(u);

        //            if (!u.IsAlive
        //                && currentLocation.X != -1) // currentLocation.X == -1 means that UnitLocation(c) returned a coordinate out of the board (UnitBase not found)
        //                this.Board.Sockets[currentLocation.X, currentLocation.Y].Unit = null; // Remove unit assigned to the Socket
        //        }
        //    }
        //}

        /// <summary>
        /// PreCondition: _referencePoint is a valid coord on the Board; _relativePointWithCorrectDirection has been passed through function, RelativeCoordToCorrectDirection();
        /// PostCondition: Returns correct _2DCoord based on the _referencePoint (returns the sum of the _referencePoint and _relativePointWithCorrectDirection)
        /// </summary>
        /// <param name="_referencePoint"></param>
        /// <param name="_relativeCoordWithDirectionAdjusted"></param>
        /// <returns></returns>
        public _2DCoord ToRealCoord(_2DCoord _referencePoint, _2DCoord _relativePointWithCorrectDirection)
        {
            return _referencePoint + _relativePointWithCorrectDirection;
        }

        /// <summary>
        /// PreCondition: _ownerPlayer has been initialized successfully; _coord is a valid coord within the Board;
        /// PostCondition: 
        /// </summary>
        /// <param name="_ownerPlayer"></param>
        /// <param name="_coord"></param>
        /// <param name="_directionFromPlayerPerspective"></param>
        /// <returns></returns>
        public _2DCoord RelativeCoordToCorrectDirection(PlayerOnBoard _ownerPlayer, _2DCoord _coord)
        {

            eFieldDirection realDirection = eFieldDirection.POSITIVE_Y; //Set this as default value

            //Set the actual value of realDirection
            if (_ownerPlayer == m_players[0])
                realDirection = eFieldDirection.POSITIVE_Y;
            else //if (_ownerPlayer == Players[1]) --> the perspective of Players[1] is 180 degrees different from that of Players[0]
                realDirection = eFieldDirection.NEGATIVE_Y;

            //Rotate _coord based on realDirection
            switch (realDirection)
            {
                case eFieldDirection.POSITIVE_X:
                    _coord.Rotate90DegreesAnticlockwise(1);
                    break;
                case eFieldDirection.NEGATIVE_Y:
                    _coord.Rotate90DegreesAnticlockwise(2);
                    break;
                case eFieldDirection.NEGATIVE_X:
                    _coord.Rotate90DegreesAnticlockwise(3);
                    break;
                default:
                    break;
            }

            return _coord;
        }

        public bool IsCoordWithinBoard(_2DCoord _coord)
        {
            if (_coord.X >= 0 && _coord.X <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD - 1
                    && _coord.Y >= 0 && _coord.Y <= CoreValues.SIZE_OF_A_SIDE_OF_BOARD - 1)
            {
                return true;
            }

            return false;
        }
        #endregion
    }

    public enum eFieldDirection
    {
        POSITIVE_Y,
        POSITIVE_X,
        NEGATIVE_Y,
        NEGATIVE_X,
    }
}
