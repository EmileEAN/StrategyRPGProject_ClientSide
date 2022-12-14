using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

namespace EEANWorks.Games.TBSG._01
{
    public class Player
    {
        public Player(int _id, string _name, List<Unit> _unitsOwned, List<Weapon> _weaponsOwned, List<Armour> _armoursOwned, List<Accessory> _accessoriesOwned, Dictionary<Item, int> _itemsOwned, List<ItemSet> _itemSets, List<Team> _teams, int _gemsOwned, int _goldOwned)
        {
            // Assign Id
            Id = _id;

            // Assign Name
            Name = _name.CoalesceNullAndReturnCopyOptionally(true);

            // Assign Playable Units Owned
            UnitsOwned = _unitsOwned.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            // Assign Weapons Owned
            WeaponsOwned = _weaponsOwned.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            // Assign Armours Owned
            ArmoursOwned = _armoursOwned.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            // Assign Accessories Owned
            AccessoriesOwned = _accessoriesOwned.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            // Assign Items Owned
            ItemsOwned = _itemsOwned.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            // Assign ItemSets Owned
            ItemSets = _itemSets.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            // Assign Teams Owned
            Teams = _teams.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);

            GemsOwned = _gemsOwned;
            GoldOwned = _goldOwned;
         }

        /*--------------------------------------------
        Properties
        --------------------------------------------*/

        // Base Properties
        public int Id { get; }
        public string Name { get; }

        // Playable Units Owned
        public List<Unit> UnitsOwned { get; set; } // Will Store References

        // Equipments Owned
        public List<Weapon> WeaponsOwned { get; set; } // Will Store References
        public List<Armour> ArmoursOwned { get; set; } // Will Store References
        public List<Accessory> AccessoriesOwned { get; set; } // Will Store References

        // Items Owned
        public Dictionary<Item, int> ItemsOwned { get; set; } // Will Store References

        // Sets of Playable Units Owned
        public List<ItemSet> ItemSets { get; set; } // Will Store References
        public List<Team> Teams { get; set; } // Will Store References

        public int GemsOwned { get; set; }
        public int GoldOwned { get;  set; }
    }

    public sealed class PlayerOnBoard
    {
        public PlayerOnBoard(string _name, Team _team, bool _isPlayer1)
        {
            try
            {
                Name = _name.CoalesceNullAndReturnCopyOptionally(true);

                IsPlayer1 = _isPlayer1;

                Moved = false;
                Attacked = false;

                UsedUltimateSkill = false;

                // Assign Allied Units
                AlliedUnits = new List<UnitInstance>();

                if (_team != null)
                {
                    bool isTeamEmpty = true;
                    foreach (Unit member in _team.Members)
                    {
                        if (member != null)
                        {
                            isTeamEmpty = false;
                            break;
                        }
                    }

                    if (!isTeamEmpty)
                    {
                        foreach (Unit member in _team.Members)
                        {
                            if (member != null)
                                AlliedUnits.Add(new UnitInstance(member, this));
                        }
                    }
                    else
                        Debug.Log("No units in team!");

                    Items = _team.ItemSet.QuantityPerItem.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
                }
                else
                    Debug.Log("Null Team object!");

                // ID of Unit Currently Selected
                SelectedUnitIndex = -1; // Meaning none of the characters is selected

                MaxSP = 0;
                RemainingSP = 0;
            }
            catch (Exception ex)
            {
                Debug.Log("PlayerOnBoard: at Ctor1() " + ex.Message);
            }
        }

        public PlayerOnBoard(Player _player, int _teamIndex, bool _isPlayer1)
        {
            try
            {
                Name = string.Empty;

                IsPlayer1 = _isPlayer1;

                Moved = false;
                Attacked = false;

                UsedUltimateSkill = false;

                // Assign Allied Units
                AlliedUnits = new List<UnitInstance>();
                // Assign Items
                Items = new Dictionary<BattleItem, int>();

                if (_player != null)
                {
                    Name = string.Copy(_player.Name);

                    IList<Team> tmp_teams = _player.Teams.AsReadOnly();

                    if (tmp_teams.Count > _teamIndex && _teamIndex >= 0)
                    {
                        bool isTeamEmpty = true;
                        foreach (Unit member in tmp_teams[_teamIndex].Members)
                        {
                            if (member != null)
                            {
                                isTeamEmpty = false;
                                break;
                            }
                        }

                        if (isTeamEmpty)
                            Debug.Log("No units in team!");
                        else
                        {
                            foreach (Unit member in tmp_teams[_teamIndex].Members)
                            {
                                if (member != null)
                                    AlliedUnits.Add(new UnitInstance(member, this));
                            }
                        }

                        if (tmp_teams[_teamIndex].ItemSet != null)
                        {
                            foreach (var itemQuantity in tmp_teams[_teamIndex].ItemSet.QuantityPerItem)
                            {
                                BattleItem targetItem = itemQuantity.Key;
                                int quantityRequired = itemQuantity.Value;

                                _player.ItemsOwned.TryGetValue(targetItem, out int quantityOwned);
                                int quantity = (quantityOwned > -quantityRequired) ? quantityRequired : quantityOwned;
                                Items.Add(targetItem, quantity);
                            }
                        }
                    }
                    else if (tmp_teams.Count > 0)
                    {
                        bool isTeamEmpty = true;
                        foreach (Unit member in tmp_teams[0].Members)
                        {
                            if (member != null)
                            {
                                isTeamEmpty = false;
                                break;
                            }
                        }

                        if (isTeamEmpty)
                            Debug.Log("No units in team!");
                        else
                        {
                            foreach (Unit member in tmp_teams[0].Members)
                            {
                                if (member != null)
                                    AlliedUnits.Add(new UnitInstance(member, this));
                            }
                        }

                        foreach (var itemQuantity in tmp_teams[0].ItemSet.QuantityPerItem)
                        {
                            BattleItem targetItem = itemQuantity.Key;
                            int quantityRequired = itemQuantity.Value;

                            _player.ItemsOwned.TryGetValue(targetItem, out int quantityOwned);
                            int quantity = (quantityOwned > -quantityRequired) ? quantityRequired : quantityOwned;
                            Items.Add(targetItem, quantity);
                        }
                    }
                    else
                        Debug.Log("No teams found!");
                }

                // ID of Unit Currently Selected
                SelectedUnitIndex = -1; // Meaning none of the characters is selected

                MaxSP = 0;
                RemainingSP = 0;
            }
            catch (Exception ex)
            {
                Debug.Log("PlayerOnBoard: at Ctor2() " + ex.Message);
            }
        }

        #region Properties
        public string Name { get; }

        public bool IsPlayer1 { get; set; }

        public bool Moved { get; set; }
        public bool Attacked { get; set; }
        public bool UsedUltimateSkill { get; private set; }

        public List<UnitInstance> AlliedUnits { get; }

        public Dictionary<BattleItem, int> Items { get; }

        public int SelectedUnitIndex { get; set; } // Id of the unit in AlliedUnits 

        public int MaxSP { get; set; }
        public int RemainingSP { get; set; }
        #endregion

        #region Public Methods
        public bool HasRequiredItems(ReadOnlyDictionary<SkillMaterial, int> _itemCosts)
        {
            foreach (var itemCost in _itemCosts)
            {
                SkillMaterial targetItem = itemCost.Key;
                int quantityRequired = itemCost.Value;

                int quantityOwned = Items[targetItem];
            }

            return true;
        }

        public void SetUsedUltimateSkillToTrue() { UsedUltimateSkill = true; }
        #endregion
    }
}
