using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public class Team
    {
        public Team(List<Unit> _members, ItemSet _itemSet)
        {
            // Assign set of units
            Members = new Unit[CoreValues.MAX_MEMBERS_PER_TEAM];
            int numOfGivenMembers = (_members == null) ? 0 : _members.Count;
            for (int i = 1; i <= Members.Length; i++)
            {
                if (i > numOfGivenMembers)
                    Members[i - 1] = null;
                else
                    Members[i - 1] = _members[i - 1];
            }

            // Assign set of items
            if (_itemSet == null)
                ItemSet = new ItemSet(default, new Dictionary<BattleItem, int>());
            else
                ItemSet = new ItemSet(_itemSet.Id, _itemSet.QuantityPerItem);
        }

        #region Properties
        public Unit[] Members { get; set; }
        public ItemSet ItemSet { get; }
        #endregion

    }

    public class ItemSet
    {
        public ItemSet(int _id, Dictionary<BattleItem, int> _quantityPerItem)
        {
            Id = _id;
            QuantityPerItem = _quantityPerItem.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        public int Id { get; }
        public Dictionary<BattleItem, int> QuantityPerItem { get; }
    }
}
