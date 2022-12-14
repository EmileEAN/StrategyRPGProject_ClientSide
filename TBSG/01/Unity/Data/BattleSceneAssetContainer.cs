using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class BattleSceneAssetContainer
    {
        private static BattleSceneAssetContainer m_instance;

        public static BattleSceneAssetContainer Instance { get { return m_instance ?? (m_instance = new BattleSceneAssetContainer()); } }

        private BattleSceneAssetContainer() { }

        #region Properties
        public GameObject UnitStatusPrefab { get; private set; }
        public GameObject StatusEffectIconPrefab { get; private set; }

        public GameObject MoveButtonPrefab { get; private set; }
        public GameObject AttackButtonPrefab { get; private set; }
        public GameObject ItemButtonPrefab { get; private set; }
        public GameObject SkillButtonPrefab { get; private set; }

        public GameObject DamageTextPrefab { get; private set; }

        public Material Material_SelectedUnitChipFrame { get; private set; }

        public Material Material_ActiveStone { get; private set; }
        public Material Material_InactiveStone { get; private set; }

        public Material Material_NormalTile { get; private set; }
        public Material Material_BlueTile { get; private set; }
        public Material Material_RedTile { get; private set; }
        public Material Material_GreenTile { get; private set; }
        public Material Material_OcherTile { get; private set; }
        public Material Material_PurpleTile { get; private set; }
        public Material Material_YellowTile { get; private set; }
        public Material Material_HealTile { get; private set; }

        public Material Material_TargetingTile_Default { get; private set; }
        public Material Material_TargetingTile_NonSelectable { get; private set; }
        public Material Material_TargetingTile_Movement_Selectable { get; private set; }
        public Material Material_TargetingTile_Attack_Selectable { get; private set; }
        public Material Material_TargetingTile_Skill_Selectable { get; private set; }
        public Material Material_TargetingTile_Movement_Selected { get; private set; }
        public Material Material_TargetingTile_Attack_Selected { get; private set; }
        public Material Material_TargetingTile_Skill_Selected { get; private set; }

        public Shader Shader_Default { get; private set; }
        #endregion

        #region Public Functions
        public void Initialize(GameObject _unitStatusPrefab, GameObject _statusEffectIconPrefab, GameObject _moveButtonPrefab, GameObject _attackButtonPrefab, GameObject _itemButtonPrefab, GameObject _skillButtonPrefab, GameObject _damageTextPrefab, Material _material_selectedUnitChipFrame, Material _material_activeStone, Material _material_inactiveStone, Material _material_normalTile, Material _material_blueTile, Material _material_redTile, Material _material_greenTile, Material _material_ocherTile, Material _material_purpleTile, Material _material_yellowTile, Material _material_healTile, Material _material_targetingTile_default, Material _material_targetingTile_nonSelectable, Material _material_targetingTile_movement_selectable, Material _material_targetingTile_attack_selectable, Material _material_targetingTile_skill_selectable, Material _material_targetingTile_movement_selected, Material _material_targetingTile_attack_selected, Material _material_targetingTile_skill_selected)
        {
            UnitStatusPrefab = _unitStatusPrefab;
            StatusEffectIconPrefab = _statusEffectIconPrefab;

            MoveButtonPrefab = _moveButtonPrefab;
            AttackButtonPrefab = _attackButtonPrefab;
            ItemButtonPrefab = _itemButtonPrefab;
            SkillButtonPrefab = _skillButtonPrefab;

            DamageTextPrefab = _damageTextPrefab;

            Material_SelectedUnitChipFrame = _material_selectedUnitChipFrame;

            Material_ActiveStone = _material_activeStone;
            Material_InactiveStone = _material_inactiveStone;

            Material_NormalTile = _material_normalTile;
            Material_BlueTile = _material_blueTile;
            Material_RedTile = _material_redTile;
            Material_GreenTile = _material_greenTile;
            Material_OcherTile = _material_ocherTile;
            Material_PurpleTile = _material_purpleTile;
            Material_YellowTile = _material_yellowTile;
            Material_HealTile = _material_healTile;

            Material_TargetingTile_Default = _material_targetingTile_default;
            Material_TargetingTile_NonSelectable = _material_targetingTile_nonSelectable;
            Material_TargetingTile_Movement_Selectable = _material_targetingTile_movement_selectable;
            Material_TargetingTile_Attack_Selectable = _material_targetingTile_attack_selectable;
            Material_TargetingTile_Skill_Selectable = _material_targetingTile_skill_selectable;
            Material_TargetingTile_Movement_Selected = _material_targetingTile_movement_selected;
            Material_TargetingTile_Attack_Selected = _material_targetingTile_attack_selected;
            Material_TargetingTile_Skill_Selected = _material_targetingTile_skill_selected;

            Shader_Default = Shader.Find("Sprites/Default");
        }

        public Material GetTileMaterialForType(eTileType _tileType)
        {
            switch (_tileType)
            {
                default: //eTileType.NORMAL
                    return Material_NormalTile;
                case eTileType.Blue:
                    return Material_BlueTile;
                case eTileType.Red:
                    return Material_RedTile;
                case eTileType.Green:
                    return Material_GreenTile;
                case eTileType.Ocher:
                    return Material_OcherTile;
                case eTileType.Purple:
                    return Material_PurpleTile;
                case eTileType.Yellow:
                    return Material_YellowTile;
                case eTileType.Heal:
                    return Material_HealTile;
            }
        }
        #endregion
    }
}
