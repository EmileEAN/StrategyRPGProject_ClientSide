using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    [RequireComponent(typeof(InfoPanelManager))]
    [RequireComponent(typeof(InfoPanelManager_UnitActionRanges))]
    [RequireComponent(typeof(InfoPanelManager_UnitEquipments))]
    [RequireComponent(typeof(InfoPanelManager_UnitSkills))]
    public class InfoPanelManager_Unit : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_unitInfoPanelPrefab;
        private GameObject m_unitLabelPrefab;

        private InfoPanelManager m_mainInfoPanelManager;
        private InfoPanelManager_UnitActionRanges m_infoPanelManager_unitActionRanges;
        private InfoPanelManager_UnitEquipments m_infoPanelManager_unitEquipments;
        private InfoPanelManager_UnitSkills m_infoPanelManager_unitSkills;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;

        private Image m_image_unit;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_unitInfoPanelPrefab = InfoPanelPrefabContainer.Instance.UnitInfoPanelPrefab;
            m_unitLabelPrefab = InfoPanelPrefabContainer.Instance.InfoPanelUnitLabelPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
            m_infoPanelManager_unitActionRanges = this.GetComponent<InfoPanelManager_UnitActionRanges>();
            m_infoPanelManager_unitEquipments = this.GetComponent<InfoPanelManager_UnitEquipments>();
            m_infoPanelManager_unitSkills = this.GetComponent<InfoPanelManager_UnitSkills>();
        }

        public void InstantiateInfoPanel(int _unitUniqueId, bool _disableChanges = false)
        {
            Unit unit = GameDataContainer.Instance.Player.UnitsOwned.First(x => x.UniqueId == _unitUniqueId);

            InstantiateInfoPanel(unit, _disableChanges);
        }
        public void InstantiateInfoPanel(Unit _unit, bool _disableChanges = false)
        {
            GameObject go_infoPanel = Instantiate(m_unitInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => Request_RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _unit, _disableChanges);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Unit _unit, bool _disableChanges)
        {
            //-----------------------------------------------------------------------------
            // Initialize Left Section
            //-----------------------------------------------------------------------------
            Transform transform_leftSection = _go_infoPanel.transform.Find("LeftSection");

            transform_leftSection.Find("Panel@Nickname").Find("Text@Nickname").GetComponent<Text>().text = _unit.Nickname ?? _unit.BaseInfo.Name;
            transform_leftSection.Find("Name").Find("Text@Name").GetComponent<Text>().text = _unit.BaseInfo.Name;

            Transform transform_images_unit = transform_leftSection.Find("UnitImagesContainer").Find("UnitImages");
            m_image_unit = transform_images_unit.Find("Image@Unit").GetComponent<Image>();
            m_image_unit.sprite = SpriteContainer.Instance.UnitIcons[_unit.BaseInfo];
            m_image_unit.sprite.texture.filterMode = FilterMode.Point;
            transform_images_unit.Find("Image@RarityFrame").Find("Image@Texture").GetComponent<Image>().sprite = SpriteContainer.Instance.GetRaritySprite(_unit);

            //-----------------------------------------------------------------------------
            // Initialize Middle Section
            //-----------------------------------------------------------------------------
            Transform transform_middleSection = _go_infoPanel.transform.Find("MiddleSection");

            eElement element1 = _unit.BaseInfo.Elements[0];
            eElement element2 = _unit.BaseInfo.Elements[1];
            Transform transform_elementsAndGender = transform_middleSection.Find("Panel@ElementsAndGender");
            Image image_element1 = transform_elementsAndGender.Find("Image@Element1").GetComponent<Image>();
            Image image_element2 = transform_elementsAndGender.Find("Image@Element2").GetComponent<Image>();
            Image image_gender = transform_elementsAndGender.Find("Image@Gender").GetComponent<Image>();

            if (element1 == eElement.None)
                image_element1.enabled = false;
            else
            {
                image_element1.enabled = true;
                image_element1.sprite = SpriteContainer.Instance.GetElementIcon(element1);
            }

            if (element2 == eElement.None)
                image_element2.enabled = false;
            else
            {
                image_element2.enabled = true;
                image_element2.sprite = SpriteContainer.Instance.GetElementIcon(element2);
            }

            image_gender.sprite = SpriteContainer.Instance.GetGenderIcon(_unit);

            ImageToggle imageToggle_lock = transform_middleSection.Find("ImageToggle@Lock").GetComponent<ImageToggle>();
            if (!_disableChanges)
            {
                imageToggle_lock.IsOn = _unit.IsLocked;
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                imageToggle_lock.onValueChanged.AddListener(delegate { this.StartCoroutineRepetitionUntilTrue(ChangeUnitLockStatus(looperAndCoroutineLinker, GameDataContainer.Instance.SessionId, _unit, imageToggle_lock), looperAndCoroutineLinker); });
            }
            else
                imageToggle_lock.gameObject.SetActive(false);

            Transform transform_baseStatus = transform_middleSection.Find("BaseStatus");
            transform_baseStatus.Find("Level").Find("Text@Value").GetComponent<Text>().text = Calculator.Level(_unit).ToString();
            transform_baseStatus.Find("MaxHP").Find("Text@Value").GetComponent<Text>().text = Calculator.MaxHP(_unit).ToString();
            transform_baseStatus.Find("PhyStr").Find("Text@Value").GetComponent<Text>().text = Calculator.PhysicalStrength(_unit).ToString();
            transform_baseStatus.Find("PhyRes").Find("Text@Value").GetComponent<Text>().text = Calculator.PhysicalResistance(_unit).ToString();
            transform_baseStatus.Find("MagStr").Find("Text@Value").GetComponent<Text>().text = Calculator.MagicalStrength(_unit).ToString();
            transform_baseStatus.Find("MagRes").Find("Text@Value").GetComponent<Text>().text = Calculator.MagicalResistance(_unit).ToString();
            transform_baseStatus.Find("Vit").Find("Text@Value").GetComponent<Text>().text = Calculator.Vitality(_unit).ToString();

            //-----------------------------------------------------------------------------
            // Initialize Right Section
            //-----------------------------------------------------------------------------
            Transform transform_rightSection = _go_infoPanel.transform.Find("RightSection");

            Transform transform_labelsPanel = transform_rightSection.Find("Labels").Find("Panel@Labels");
            foreach (string label in _unit.BaseInfo.Labels)
            {
                GameObject go_label = Instantiate(m_unitLabelPrefab, transform_labelsPanel);
                go_label.transform.Find("Text").GetComponent<Text>().text = label;
            }

            //-----------------------------------------------------------------------------
            // Initialize Bottom Section
            //-----------------------------------------------------------------------------
            Transform transform_bottomSection = _go_infoPanel.transform.Find("BottomSection");
            transform_bottomSection.Find("Button@ActionRanges").GetComponent<Button>().onClick.AddListener(() => m_infoPanelManager_unitActionRanges.InstantiateInfoPanel(_unit));
            transform_bottomSection.Find("Button@Equipments").GetComponent<Button>().onClick.AddListener(() => m_infoPanelManager_unitEquipments.InstantiateInfoPanel(_unit, _disableChanges));
            transform_bottomSection.Find("Button@Skills").GetComponent<Button>().onClick.AddListener(() => m_infoPanelManager_unitSkills.InstantiateInfoPanel(_unit));
        }

        //Remove newest info panel
        private void Request_RemoveInfoPanel(GameObject _go_infoPanel) { StartCoroutine(RemoveInfoPanel(_go_infoPanel)); }
        IEnumerator RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            yield return StartCoroutine(m_mainInfoPanelManager.RemoveInfoPanel(_go_infoPanel));
            m_image_unit.sprite.texture.filterMode = FilterMode.Bilinear;
        }

        public IEnumerator ChangeUnitLockStatus(LooperAndCoroutineLinker _looperAndCoroutineLinker, string _sessionId, Unit _unit, ImageToggle _imageToggle)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "ChangeUnitLockStatus"},
                    {"sessionId", _sessionId},
                    {"uniqueId", _unit.UniqueId.ToString()},
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

                if (response == "sessionExpired")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Session Error!", CoreValues.SESSION_ERROR_MESSAGE, "Return To Title", () => SceneConnector.GoToScene("scn_Title"));
                else if (response == "error")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error", "Someting went wrong!\nPlease try again.", "OK");
                else if (response == "locked")
                    _unit.IsLocked = true;
                else if (response == "unlocked")
                    _unit.IsLocked = false;

                _imageToggle.IsOn = _unit.IsLocked;

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }
    }
}