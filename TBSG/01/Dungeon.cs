using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public class Dungeon
    {
        public Dungeon(string _name, List<Floor> _floors)
        {
            Name = _name.CoalesceNullAndReturnCopyOptionally(true);

            m_floors = _floors.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer
        }

        #region Properties
        public string Name { get; }

        public IList<Floor> Floors { get { return m_floors.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<Floor> m_floors;
        #endregion
    }

    public class Floor
    {
        public Floor(int _tileSetId, List<DungeonUnitInfo> _dungeonUnitInfos)
        {
            TileSetId = _tileSetId; 

            m_dungeonUnitInfos = _dungeonUnitInfos.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer
        }

        #region Properties
        public int TileSetId { get; }

        public IList<DungeonUnitInfo> DungeonUnitInfos { get { return m_dungeonUnitInfos.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<DungeonUnitInfo> m_dungeonUnitInfos;
        #endregion
    }

    public class DungeonUnitInfo
    {
        public DungeonUnitInfo(UnitData _unitData, int _minLevel, int _maxLevel, decimal _dropRate, List<DropItemInfo> _dropItemInfos)
        {
            UnitData = _unitData; // Getting a reference to an object stored within GameDataContainer

            int maxLevelForRarity = Calculator.MaxLevelForRarity(UnitData);
            MinLevel = (_minLevel > 0) ? ((_minLevel <= maxLevelForRarity) ? _minLevel : maxLevelForRarity ) : 1;
            MaxLevel = (_maxLevel >= MinLevel) ? ((_maxLevel <= maxLevelForRarity) ? _maxLevel : maxLevelForRarity) : MinLevel;

            DropRate = (_dropRate >= 0) ? ((_dropRate <= 1) ? _dropRate : 1 ) : 0;

            m_dropItemInfos = _dropItemInfos.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer
        }

        #region Properties
        public UnitData UnitData { get; }

        public int MinLevel { get; }
        public int MaxLevel { get; }

        public decimal DropRate { get; }

        public IList<DropItemInfo> DropItemInfos { get { return m_dropItemInfos.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<DropItemInfo> m_dropItemInfos;
        #endregion
    }

    public struct DropItemInfo
    {
        public DropItemInfo(Item _item, decimal _dropRate)
        {
            Item = _item;
            DropRate = (_dropRate >= 0) ? ((_dropRate <= 1) ? _dropRate : 1) : 0;
        }

        public Item Item { get; }
        public decimal DropRate { get; }
    }

    public struct FloorInstanceMemberInfo
    {
        public FloorInstanceMemberInfo(UnitData _unitData, int _unitLevel, bool _drops, int _numberOfDroppingItems)
        {
            UnitData = _unitData; // Getting a reference to an object stored within GameDataContainer
            UnitLevel = _unitLevel;
            Drops = _drops;
            NumberOfDroppingItems = _numberOfDroppingItems;
        }

        #region Properties
        public UnitData UnitData { get; }
        public int UnitLevel { get; }
        public bool Drops { get; }
        public int NumberOfDroppingItems { get; }
        #endregion
    }
}
