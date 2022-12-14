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
    [RequireComponent(typeof(InfoPanelManager_Skill))]
    public class InfoPanelManager_Weapon : MonoBehaviour
    {
        #region Private Fields
        private GameObject m_weaponInfoPanelPrefab;

        private InfoPanelManager m_mainInfoPanelManager;
        private InfoPanelManager_Skill m_infoPanelManager_skill;

        private List<GameObject> m_gos_infoPanel;
        private Transform m_transform_canvas;

        private Image m_image_weapon;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_gos_infoPanel = new List<GameObject>();
            m_transform_canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            m_weaponInfoPanelPrefab = InfoPanelPrefabContainer.Instance.WeaponInfoPanelPrefab;
            m_mainInfoPanelManager = this.GetComponent<InfoPanelManager>();
            m_infoPanelManager_skill = this.GetComponent<InfoPanelManager_Skill>();
        }

        public void InstantiateInfoPanel(Weapon _weapon)
        {
            GameObject go_infoPanel = Instantiate(m_weaponInfoPanelPrefab, m_transform_canvas, false);
            go_infoPanel.transform.Find("Button@Close").GetComponent<Button>().onClick.AddListener(() => Request_RemoveInfoPanel(go_infoPanel));
            m_gos_infoPanel.Add(go_infoPanel);
            m_mainInfoPanelManager.Request_AddInfoPanel(go_infoPanel);

            InitializeInfoPanel(go_infoPanel, _weapon);
        }

        private void InitializeInfoPanel(GameObject _go_infoPanel, Weapon _weapon)
        {
            //-----------------------------------------------------------------------------
            // Initialize Left Section
            //-----------------------------------------------------------------------------
            Transform transform_leftSection = _go_infoPanel.transform.Find("LeftSection");

            transform_leftSection.Find("Panel@Name").Find("Text@Name").GetComponent<Text>().text = _weapon.BaseInfo.Name;

            Transform transform_images_weapon = transform_leftSection.Find("WeaponImagesContainer").Find("WeaponImages");
            m_image_weapon = transform_images_weapon.Find("Image@Weapon").GetComponent<Image>();
            m_image_weapon.sprite = SpriteContainer.Instance.WeaponIcons[_weapon.BaseInfo];
            m_image_weapon.sprite.texture.filterMode = FilterMode.Point;
            transform_images_weapon.Find("Image@RarityFrame").Find("Image@Texture").GetComponent<Image>().sprite = SpriteContainer.Instance.GetRaritySprite(_weapon);

            ImageToggle imageToggle_lock = transform_leftSection.Find("ImageToggle@Lock").GetComponent<ImageToggle>();
            imageToggle_lock.IsOn = _weapon.IsLocked;
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            imageToggle_lock.onValueChanged.AddListener(delegate { this.StartCoroutineRepetitionUntilTrue(ChangeWeaponLockStatus(looperAndCoroutineLinker, GameDataContainer.Instance.SessionId, _weapon, imageToggle_lock), looperAndCoroutineLinker); });

            //-----------------------------------------------------------------------------
            // Initialize Right Section
            //-----------------------------------------------------------------------------
            Transform transform_rightSection = _go_infoPanel.transform.Find("RightSection");

            string typeText = string.Empty;
            {
                if (_weapon is OrdinaryWeapon) typeText = "Ordinary";
                else if (_weapon is LevelableWeapon) typeText = "Levelable";
                else if (_weapon is TransformableWeapon) typeText = "Transformable";
                else /*if (_weapon is LevelableTransformableWeapon)*/ typeText = "Levelable & Transformable";
            }
            transform_rightSection.Find("Type").Find("Text@Value").GetComponent<Text>().text = typeText;

            Transform transform_level = transform_rightSection.Find("Level");
            int level = Calculator.Level(_weapon);
            if (level != 0) // It means that the weapon is levelable
                transform_level.Find("Text@Value").GetComponent<Text>().text = level.ToString();
            else
                transform_level.gameObject.SetActive(false);

            Transform transform_mainWeaponSkill = transform_rightSection.Find("MainWeaponSkill");
            Skill mainWeaponSkill = _weapon.BaseInfo.MainWeaponSkill;
            transform_mainWeaponSkill.Find("Button@SkillInfo").GetComponent<GameObjectFormatter_SkillInfoButton>().Format(mainWeaponSkill, () => m_infoPanelManager_skill.InstantiateInfoPanel(mainWeaponSkill));

            Transform transform_classificationIcons = transform_rightSection.Find("Classifications").Find("ClassificationIcons");
            int numOfClassifications = _weapon.BaseInfo.WeaponClassifications.Count;
            for (int i = 0; i < transform_classificationIcons.childCount; i++)
            {
                Transform transform_classificationIcon = transform_classificationIcons.GetChild(i);
                if (i < numOfClassifications)
                {
                    eWeaponClassification classification = _weapon.BaseInfo.WeaponClassifications[i];

                    transform_classificationIcon.GetComponent<DetailInfoPopUpController>().GO_detailInfoPopUp.transform.Find("Text").GetComponent<Text>().text = classification.ToString();

                    Image image_classification = transform_classificationIcon.GetComponent<Image>();
                    image_classification.sprite = SpriteContainer.Instance.GetWeaponClassificationIcon(classification);
                }
                else
                    transform_classificationIcon.gameObject.SetActive(false);
            }

            //-----------------------------------------------------------------------------
            // Initialize Bottom Section
            //-----------------------------------------------------------------------------
            Transform transform_bottomSection = _go_infoPanel.transform.Find("BottomSection");
            //transform_bottomSection.Find("ScrollMenu@Details").Find("Contents").Find("Text").GetComponent<Text>().text = _weapon.BaseInfo.Explanation;
            transform_bottomSection.Find("ScrollMenu@Details").Find("Contents").Find("Text").GetComponent<Text>().text = "[Equipment Effects] Explanation is to be implemented...";
        }

        //Remove newest info panel
        private void Request_RemoveInfoPanel(GameObject _go_infoPanel) { StartCoroutine(RemoveInfoPanel(_go_infoPanel)); }
        IEnumerator RemoveInfoPanel(GameObject _go_infoPanel)
        {
            m_gos_infoPanel.Remove(_go_infoPanel);
            yield return StartCoroutine(m_mainInfoPanelManager.RemoveInfoPanel(_go_infoPanel));
            m_image_weapon.sprite.texture.filterMode = FilterMode.Bilinear;
        }

        public IEnumerator ChangeWeaponLockStatus(LooperAndCoroutineLinker _looperAndCoroutineLinker, string _sessionId, Weapon _weapon, ImageToggle _imageToggle)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "ChangeWeaponLockStatus"},
                    {"sessionId", _sessionId},
                    {"uniqueId", _weapon.UniqueId.ToString()},
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
                    _weapon.IsLocked = true;
                else if (response == "unlocked")
                    _weapon.IsLocked = false;

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();

                _imageToggle.IsOn = _weapon.IsLocked;
            }
        }
    }
}