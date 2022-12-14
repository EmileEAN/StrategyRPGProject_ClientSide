using System;
using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public class UnitEvolutionRecipe
    {
        public UnitEvolutionRecipe(UnitData _unitAfterEvolution, List<EvolutionMaterial> _materials, int _cost)
        {
            UnitAfterEvolution = _unitAfterEvolution; // Getting a reference to an object stored within GameDataContainer

            m_materials = new EvolutionMaterial[CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE];
            if (_materials != null && _materials.Count == m_materials.Length)
            {
                for (int i = 0; i < CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE; i++)
                {
                    m_materials[i] = _materials[i];
                }
            }

            Cost = _cost;
        }

        #region Properties
        public UnitData UnitAfterEvolution { get; }

        public IList<EvolutionMaterial> Materials { get { return Array.AsReadOnly(m_materials); } }

        public int Cost { get; } // Golds
        #endregion

        #region Private Read-only Fields
        private readonly EvolutionMaterial[] m_materials;
        #endregion
    }

    public class WeaponRecipe
    {
        public WeaponRecipe(WeaponData _product, List<EquipmentMaterial> _materials, int _cost, WeaponData _weaponToUpgrade = null)
        {
            Product = _product; // Getting a reference to an object stored within GameDataContainer

            m_materials = new EquipmentMaterial[CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE];
            if (_materials != null && _materials.Count == m_materials.Length)
            {
                for (int i = 0; i < CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE; i++)
                {
                    m_materials[i] = _materials[i];
                }
            }

            Cost = _cost;

            WeaponToUpgrade = _weaponToUpgrade; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public WeaponData Product { get; }
        public WeaponData WeaponToUpgrade { get; }

        public IList<EquipmentMaterial> Materials { get { return Array.AsReadOnly(m_materials); } }

        public int Cost { get; } // Golds
        #endregion

        #region Private Read-only Fields
        private readonly EquipmentMaterial[] m_materials;
        #endregion
    }

    public class ArmourRecipe
    {
        public ArmourRecipe(ArmourData _product, List<EquipmentMaterial> _materials, int _cost, ArmourData _armourToUpgrade = null)
        {
            Product = _product; // Getting a reference to an object stored within GameDataContainer

            m_materials = new EquipmentMaterial[CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE];
            if (_materials != null && _materials.Count == m_materials.Length)
            {
                for (int i = 0; i < CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE; i++)
                {
                    m_materials[i] = _materials[i];
                }
            }

            Cost = _cost;

            ArmourToUpgrade = _armourToUpgrade; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public ArmourData Product { get; }
        public ArmourData ArmourToUpgrade { get; }

        public IList<EquipmentMaterial> Materials { get { return Array.AsReadOnly(m_materials); } }

        public int Cost { get; } // Golds
        #endregion

        #region Private Read-only Fields
        private readonly EquipmentMaterial[] m_materials;
        #endregion
    }

    public class AccessoryRecipe
    {
        public AccessoryRecipe(AccessoryData _product, List<EquipmentMaterial> _materials, int _cost, AccessoryData _accessoryToUpgrade = null)
        {
            Product = _product; // Getting a reference to an object stored within GameDataContainer

            m_materials = new EquipmentMaterial[CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE];
            if (_materials != null && _materials.Count == m_materials.Length)
            {
                for (int i = 0; i < CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE; i++)
                {
                    m_materials[i] = _materials[i];
                }
            }
            
            Cost = _cost;

            AccessoryToUpgrade = _accessoryToUpgrade; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public AccessoryData Product { get; }
        public AccessoryData AccessoryToUpgrade { get; }

        public IList<EquipmentMaterial> Materials { get { return Array.AsReadOnly(m_materials); } }

        public int Cost { get; } // Golds
        #endregion

        #region Private Read-only Fields
        private readonly EquipmentMaterial[] m_materials;
        #endregion
    }

    public class ItemRecipe
    {
        public ItemRecipe(Item _product, List<ItemMaterial> _materials, int _cost, Item _itemToUpgrade = null)
        {
            Product = _product; // Getting a reference to an object stored within GameDataContainer

            m_materials = new ItemMaterial[CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE];
            if (_materials != null && _materials.Count == m_materials.Length)
            {
                for (int i = 0; i < CoreValues.MAX_NUM_OF_ELEMENTS_IN_RECIPE; i++)
                {
                    m_materials[i] = _materials[i];
                }
            }

            Cost = _cost;

            ItemToUpgrade = _itemToUpgrade; // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public Item Product { get; }
        public Item ItemToUpgrade { get; }

        public IList<ItemMaterial> Materials { get { return Array.AsReadOnly(m_materials); } }

        public int Cost { get; } // Golds
        #endregion

        #region Private Read-only Fields
        private readonly ItemMaterial[] m_materials;
        #endregion
    }
}
