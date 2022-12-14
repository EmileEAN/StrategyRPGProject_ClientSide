using EEANWorks.Games.TBSG._01.Unity.Data;
using System;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class HPBarController_OpponentUnit : MonoBehaviour
    {
        #region Private Fields
        private UnityBattleSystem_Multiplayer m_mainScript;

        private OpponentUnitController m_unitController;
        private bool m_isInitialized;
        private int m_remainingHP; // Actual hp remaining
        private float m_remainingHP_rate; // Value between 0 and 1

        private Transform m_transform_remainingAmount;
        private SpriteRenderer m_spriteRenderer_RemainingAmount;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_isInitialized = false;
            m_unitController = this.transform.parent.GetComponent<OpponentUnitController>();
            m_transform_remainingAmount = this.transform.Find("RemainingAmount");
            m_spriteRenderer_RemainingAmount = m_transform_remainingAmount.GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (m_mainScript == null)
                    m_mainScript = this.transform.root.GetComponent<UnityBattleSystem_Multiplayer>();
                if (m_mainScript == null)
                    return;

                if (m_mainScript.IsInitialized && m_unitController.IsInitialized)
                {
                    if (m_unitController.OwnerId != m_mainScript.PlayerController.PlayerId)
                        this.transform.Rotate(new Vector3(0, 180f, 0));

                    m_remainingHP = m_unitController.RemainingHP;

                    if (m_remainingHP > 0)
                        m_remainingHP_rate = (float)m_remainingHP / (float)m_unitController.MaxHP;
                    else
                        m_remainingHP_rate = -1f;

                    m_isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("HPBarController: at Initialize() " + ex.Message);
            }
        }

        public void TryUpdateHP(int _amountModified, bool _didHit, bool _isCritical, int _remainingHP)
        {
            if (!m_isInitialized)
                return;

            HPModificationAnimation(_amountModified, _didHit, _isCritical); // Display the result of the modification made to the HP

            m_remainingHP = _remainingHP; // Update m_remainingHP to match the actual HP remaining

            m_remainingHP_rate = m_remainingHP / (float)m_unitController.MaxHP; //Get the ratio of remaining HP over the maximum HP

            m_transform_remainingAmount.localScale = new Vector3(m_remainingHP_rate, 1f, 1f); // Adjust size of HP bar based on m_remainingHP_rate

            Vector3 currentScale = m_transform_remainingAmount.localScale;
            Vector3 currentPos = m_transform_remainingAmount.localPosition;
            m_transform_remainingAmount.localPosition = new Vector3(-5.12f * (1 - currentScale.x), currentPos.y, currentPos.z); // Align the position of the [HP bar for remaining HP] to one of the extremes of the [HP bar for maximum HP.] By default the position of the [HP bar for remaining HP] is set to be on the exact center of the [HP bar for maximum HP] and that is not what we want. -5.12f adjusted the position the best
        }

        private void HPModificationAnimation(int _amountModified, bool _didHit, bool _isCritical)
        {
            GameObject go_damageText = Instantiate(BattleSceneAssetContainer.Instance.DamageTextPrefab, this.transform);
            go_damageText.transform.localPosition = new Vector3(0, 0, 0);
            go_damageText.transform.localScale = new Vector3(1, 8, 1);

            string textColorTag = "";

            if (_didHit)
            {
                if (_amountModified > 0)
                {
                    if (_isCritical)
                        textColorTag = "<color=pink>";
                    else
                        textColorTag = "<color=green>";
                }
                else if (_amountModified < 0)
                {
                    _amountModified *= -1;

                    if (_isCritical)
                        textColorTag = "<color=yellow>";
                    else
                        textColorTag = "<color=red>";
                }
                else
                    textColorTag = "<color=gray>";

                go_damageText.GetComponent<HPTextManager_SinglePlayer>().MyTextMesh.text = textColorTag + _amountModified.ToString() + "</color>";
            }
            else
                go_damageText.GetComponent<HPTextManager_SinglePlayer>().MyTextMesh.text = "<color=gray>Miss!!</color>";
        }
    }
}