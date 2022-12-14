using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    [RequireComponent(typeof(InfoPanelManager))]
    public class InfoPanelManager_Armour : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_armourInfoPanelPrefab;

        private InfoPanelManager m_mainInfoPanelManager;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;

        private Image m_image_equipment;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_armourInfoPanelPrefab = InfoPanelPrefabContainer.Instance.ArmourInfoPanelPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
        }

        public void InstantiateInfoPanel(Armour _armour)
        {
            GameObject go_infoPanel = Instantiate(m_armourInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => Request_RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _armour);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Armour _armour)
        {
            //-----------------------------------------------------------------------------
            // Initialize Left Section
            //-----------------------------------------------------------------------------
            Transform transform_leftSection = _go_infoPanel.transform.Find("LeftSection");

            transform_leftSection.Find("Panel@Name").Find("Text@Name").GetComponent<Text>().text = _armour.BaseInfo.Name;

            Transform transform_images_armour = transform_leftSection.Find("EquipmentImagesContainer").Find("EquipmentImages");
            m_image_equipment = transform_images_armour.Find("Image@Equipment").GetComponent<Image>();
            m_image_equipment.sprite = SpriteContainer.Instance.ArmourIcons[_armour.BaseInfo];
            m_image_equipment.sprite.texture.filterMode = FilterMode.Point;
            transform_images_armour.Find("Image@RarityFrame").Find("Image@Texture").GetComponent<Image>().sprite = SpriteContainer.Instance.GetRaritySprite(_armour);

            ImageToggle imageToggle_lock = transform_leftSection.Find("ImageToggle@Lock").GetComponent<ImageToggle>();
            imageToggle_lock.IsOn = _armour.IsLocked;
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            imageToggle_lock.onValueChanged.AddListener(delegate { this.StartCoroutineRepetitionUntilTrue(ChangeArmourLockStatus(looperAndCoroutineLinker, GameDataContainer.Instance.SessionId, _armour, imageToggle_lock), looperAndCoroutineLinker); });

            Transform transform_classificationIcon = transform_leftSection.Find("Classification").Find("Image@Classification");
            eArmourClassification classification = _armour.BaseInfo.ArmourClassification;
            transform_classificationIcon.GetComponent<DetailInfoPopUpController>().GO_detailInfoPopUp.transform.Find("Text").GetComponent<Text>().text = classification.ToString();
            Image image_classification = transform_classificationIcon.GetComponent<Image>();
            image_classification.sprite = SpriteContainer.Instance.GetArmourClassificationIcon(classification);

            Image image_gender = transform_leftSection.Find("TargetGender").Find("Image@Gender").GetComponent<Image>();
            image_gender.sprite = SpriteContainer.Instance.GetGenderIcon(_armour.BaseInfo.TargetGender);

            //-----------------------------------------------------------------------------
            // Initialize Right Section
            //-----------------------------------------------------------------------------
            Transform transform_rightSection = _go_infoPanel.transform.Find("RightSection");
            //transform_rightSection.Find("ScrollMenu@Details").Find("Contents").Find("Text").GetComponent<Text>().text = _armour.BaseInfo.Explanation;
            transform_rightSection.Find("ScrollMenu@Details").Find("Contents").Find("Text").GetComponent<Text>().text = "[Equipment Effects] Explanation is to be implemented...";
        }

        //Remove newest info panel
        private void Request_RemoveInfoPanel(GameObject _go_infoPanel) { StartCoroutine(RemoveInfoPanel(_go_infoPanel)); }
        IEnumerator RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            yield return StartCoroutine(m_mainInfoPanelManager.RemoveInfoPanel(_go_infoPanel));
            m_image_equipment.sprite.texture.filterMode = FilterMode.Bilinear;
        }

        private IEnumerator ChangeArmourLockStatus(LooperAndCoroutineLinker _looperAndCoroutineLinker, string _sessionId, Armour _armour, ImageToggle _imageToggle)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "ChangeArmourLockStatus"},
                    {"sessionId", _sessionId},
                    {"uniqueId", _armour.UniqueId.ToString()},
                    {"lock", _imageToggle.IsOn.ToString()}
                };

            UnityWebRequest uwr = UnityWebRequest.Post(CoreValues.SERVER_URL, values);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                if (_looperAndCoroutineLinker.IsEndOfLoop)
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Connection Error: " + uwr.error + "\nPlease check your Internet connection.", "OK");
            }
            else
            {
                string response = uwr.downloadHandler.text;

                if (response == "locked")
                    _armour.IsLocked = true;
                else if (response == "unlocked")
                    _armour.IsLocked = false;

                _imageToggle.IsOn = _armour.IsLocked;

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }
    }
}