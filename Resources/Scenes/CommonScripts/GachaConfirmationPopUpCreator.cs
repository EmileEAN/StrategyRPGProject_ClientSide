using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.Unity.Engine.UI;
using System.Linq;
using UnityEngine.Events;

namespace EEANWorks.Games.TBSG._01.Unity
{
    public static class GachaConfirmationPopUpCreator
    {
        #region Private Fields
        private const string POSITIVE_COLOR_OPENING_TAG = "<color=blue>";
        private const string NEGATIVE_COLOR_OPENING_TAG = "<color=red>";
        private const string COLOR_CLOSING_TAG = "</color>";
        private const string POSITIVE_YES_LABEL = "Roll";
        private const string NO_BUTTON_LABEL = "Cancel";
        #endregion

        public static void CreatePopUp(Gacha _gacha, GachaRollRequester _gachaRollRequester, DispensationOption _dispensationOption)
        {
            string popUpTitle = _gacha.Title;

            eCostType costType = _dispensationOption.CostType;
            int playerPossesion = GetPlayerPossesion(costType);
            int costValue = _dispensationOption.CostValue;

            bool isPossesionEnough = playerPossesion >= costValue;

            string colorOpeningTag = isPossesionEnough ? POSITIVE_COLOR_OPENING_TAG : NEGATIVE_COLOR_OPENING_TAG;
            string popUpMessage = GeneratePopUpMessage(_dispensationOption, playerPossesion, colorOpeningTag);

            string yesButtonLabel = isPossesionEnough ? POSITIVE_YES_LABEL : GenerateNegativeYesButtonLabel(costType);

            UnityAction positiveYesMethod = () => _gachaRollRequester.Request_RollGacha(_gacha, _dispensationOption);
            //UnityAction negativeYesMethod = () => SceneConnector.GoToScene("scn_Shop");
            //UnityAction yesMethod = isPossesionEnough ? positiveYesMethod : negativeYesMethod;
            UnityAction yesMethod = isPossesionEnough ? positiveYesMethod : null;

            PopUpWindowManager.Instance.CreateYesNoPopUp(popUpTitle, popUpMessage, yesButtonLabel, NO_BUTTON_LABEL, yesMethod);
        }

        private static int GetPlayerPossesion(eCostType _costType) { return _costType == eCostType.Gem ? GameDataContainer.Instance.Player.GemsOwned : GameDataContainer.Instance.Player.GoldOwned; }

        private static string GeneratePopUpMessage(DispensationOption _dispensationOption, int _playerPossesion, string _colorOpeningTag)
        {
            string costTypeString;
            eCostType costType = _dispensationOption.CostType;
            switch (costType)
            {
                default: // case eCostType.Gem || eCostType.Gold
                    costTypeString = costType.ToString();
                    break;
                case eCostType.Item:
                    {
                        Item costItem = GameDataContainer.Instance.ItemEncyclopedia.First(x => x.Id == _dispensationOption.CostItemId);
                        costTypeString = costItem.Name;
                    }
                    break;
            }
            costTypeString += "(s)";

            int costValue = _dispensationOption.CostValue;

            return "Would you like to roll this gacha by spending " + costValue.ToString() + " [" + costTypeString + "]?"
                                        + "\n\n"
                                        + "[" +costTypeString + "] owned: <color=green>" + _playerPossesion.ToString() + COLOR_CLOSING_TAG + " > " + _colorOpeningTag + (_playerPossesion - costValue).ToString() + COLOR_CLOSING_TAG;
        }

        private static string GenerateNegativeYesButtonLabel(eCostType _costType) { return "Buy " + _costType.ToString(); }
    }
}
