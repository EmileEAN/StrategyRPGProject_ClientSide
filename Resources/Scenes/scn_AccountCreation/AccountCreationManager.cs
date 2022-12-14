using static EEANWorks.Games.Unity.Engine.MonoBehaviourExtension;
using EEANWorks.Games.Unity.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using EEANWorks.Games.Unity.Graphics;
using System.Linq;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class AccountCreationManager : MonoBehaviour
    {
        #region Serialized Fields
        public GameObject ImageTogglePrefab;

        public InputField PlayerNameInputField;
        public InputField UserNameInputField;
        public InputField PasswordInputField;
        public InputField PasswordConfirmationInputField;
        public InputField SecurityQuestionInputField;
        public InputField AnswerInputField;

        public Text PlayerNameStatusText;
        public Text UserNameStatusText;
        public Text PasswordStatusText;
        public Text PasswordConfirmationStatusText;
        public Text SecurityQuestionStatusText;
        public Text AnswerStatusText;
        public Text PartnerSelectionStatusText;

        public Text ValidityMessageText;
        #endregion

        #region Private Fields
        private bool m_loadedRequiredData;

        private bool m_isUserNameValid;

        private bool m_areAllInputValuesValid;

        private Transform m_transform_partnerImageToggleContainer;
        private ImageToggleGroup m_imageToggleGroup;
        private DynamicGridLayoutGroup m_dynamicGridLayoutGroup;
        #endregion


        //Use this for initialization
        void Awake()
        {
            m_loadedRequiredData = false;

            m_isUserNameValid = false;

            m_areAllInputValuesValid = false;

            m_transform_partnerImageToggleContainer = GameObject.Find("PartnerImageToggleContainer").transform;
            m_imageToggleGroup = m_transform_partnerImageToggleContainer.GetComponent<ImageToggleGroup>();
            m_dynamicGridLayoutGroup = m_transform_partnerImageToggleContainer.GetComponent<DynamicGridLayoutGroup>();

            StartCoroutine(LoadRequiredData());
        }

        IEnumerator LoadRequiredData()
        {
            LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
            yield return this.StartCoroutineRepetitionUntilTrue(LoadInitiallyAvailableUnits(looperAndCoroutineLinker), looperAndCoroutineLinker);

            if (!m_loadedRequiredData)
                SceneConnector.GoToPreviousScene();
        }

        IEnumerator LoadInitiallyAvailableUnits(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "LoadInitiallyAvailableUnits"}
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
                string[] responseValues = response.Split(';');

                m_dynamicGridLayoutGroup.ElementsPerRow = responseValues.Length;

                foreach (string responseValue in responseValues)
                {
                    string[] unitInfo = responseValue.Split(',');
                    string unitId = unitInfo[0];
                    string iconBytesString = unitInfo[1];
                    byte[] iconAsBytes = Convert.FromBase64String(iconBytesString);

                    GameObject go_imageToggle = Instantiate(ImageTogglePrefab, m_transform_partnerImageToggleContainer);
                    go_imageToggle.name = unitId; // Set the unitId as the name of go_imageToggle (it will be used afterwards to determine the selected unit)
                    ImageToggle imageToggle = go_imageToggle.GetComponent<ImageToggle>();
                    imageToggle.Group = m_imageToggleGroup; // Apply toggle group to imageToggle
                    Image image_on = imageToggle.transform.Find("Image@On").GetComponent<Image>();
                    Image image_off = imageToggle.transform.Find("Image@Off").GetComponent<Image>();
                    image_on.sprite = image_off.sprite = ImageConverter.ByteArrayToSprite(iconAsBytes, FilterMode.Point); // Load image into imageToggle's on and off sprites
                    image_off.color = new Color(0.25f, 0.25f, 0.25f, 1f); // Make the off sprite be darker
                    imageToggle.IsOn = false; // All toggles except the first one, which will be automatically turned on by the toggle group, will be initially off
                }

                m_loadedRequiredData = true;

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        public void Request_CreateAccount()
        {
            StartCoroutine(ValidateInputsAndCreateAccount());
        }

        IEnumerator ValidateInputsAndCreateAccount()
        {
            yield return StartCoroutine(ValidateInputs());

            if (m_areAllInputValuesValid)
            {
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                yield return this.StartCoroutineRepetitionUntilTrue(CreateAccount(looperAndCoroutineLinker), looperAndCoroutineLinker);
            }
        }

        IEnumerator CreateAccount(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "CreateAccount"},
                    {"playerName", PlayerNameInputField.text},
                    {"userName", UserNameInputField.text},
                    {"password", PasswordInputField.text},
                    {"securityQuestion", SecurityQuestionInputField.text},
                    {"answer", AnswerInputField.text},
                    {"firstUnitId", m_imageToggleGroup.ActiveToggles().First().name}
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
                string[] responseValues = response.Split('\n');

                if (responseValues[0] == "success")
                    PopUpWindowManager.Instance.CreateSimplePopUp("Success!", "A new account has been created for you.", "Return to Title Menu", () => SceneConnector.GoToScene("scn_Title"));
                else
                {
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Something went wrong!", "OK");
                    m_areAllInputValuesValid = false;
                }

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        IEnumerator CheckUserNameValidity(LooperAndCoroutineLinker _looperAndCoroutineLinker)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    {"subject", "CheckUserNameValidity"},
                    {"userName", UserNameInputField.text}
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
                string[] responseValues = response.Split('\n');

                if (responseValues[0] == "valid")
                    m_isUserNameValid = true;
                else if (responseValues[0] == "invalid")
                    m_isUserNameValid = false;
                else
                    PopUpWindowManager.Instance.CreateSimplePopUp("Error!", "Something went wrong!", "OK");

                _looperAndCoroutineLinker.SetTerminateLoopToTrue();
            }
        }

        private IEnumerator ValidateInputs()
        {
            bool isPlayerNameValid = false;
            if (PlayerNameInputField.text.Length < 2)
                PlayerNameStatusText.text = "<color=red>Player Name must contain at least 2 characters!</color>";
            else
            {
                isPlayerNameValid = true;
                PlayerNameStatusText.text = "<color=green>OK!</color>";
            }

            if (UserNameInputField.text.Length < 8)
                UserNameStatusText.text = "<color=red>User Name must contain at least 8 characters!</color>";
            else
            {
                LooperAndCoroutineLinker looperAndCoroutineLinker = new LooperAndCoroutineLinker();
                yield return this.StartCoroutineRepetitionUntilTrue(CheckUserNameValidity(looperAndCoroutineLinker), looperAndCoroutineLinker);
                if (!m_isUserNameValid)
                    UserNameStatusText.text = "<color=red>User Name is already used!</color>";
                else
                    UserNameStatusText.text = "<color=green>OK!</color>";
            }

            bool isPasswordValid = false;
            if (PasswordInputField.text.Length < 8)
                PasswordStatusText.text = "<color=red>Password must contain at least 8 characters!</color>";
            else
            {
                isPasswordValid = true;
                PasswordStatusText.text = "<color=green>OK!</color>";
            }

            bool doesConfirmationMatchPassword = false;
            if (PasswordConfirmationInputField.text != PasswordInputField.text)
                PasswordConfirmationStatusText.text = "<color=red>It must match the above password!</color>";
            else
            {
                doesConfirmationMatchPassword = true;
                PasswordConfirmationStatusText.text = "<color=green>OK!</color>";
            }

            bool isFirstPartnerSelected = m_imageToggleGroup.AnyToggleOn();
            PartnerSelectionStatusText.text = isFirstPartnerSelected ? "<color=green>OK!</color>" : "<color=red>You have not selected your partner!</color>";

            if (isPlayerNameValid && m_isUserNameValid && isPasswordValid && doesConfirmationMatchPassword && isFirstPartnerSelected)
            {
                m_areAllInputValuesValid = true;
                ValidityMessageText.text = "";
            }
            else
                ValidityMessageText.text = "<color=red>There are some invalid values! Please check your input again.</color>";
        }
    }
}