using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.UI
{
    public class TargetAreaDisplayer : MonoBehaviour
    {
        #region Private Fields
        private Image[,] m_images_tileIcon;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            Transform boardContainer = this.transform.Find("BoardContainer");
            Transform board = boardContainer.Find("Board");

            m_images_tileIcon = new Image[CoreValues.SIZE_OF_A_SIDE_OF_BOARD, CoreValues.SIZE_OF_A_SIDE_OF_BOARD];

            int tileIndex = 0;
            foreach (Transform child in board)
            {
                _2DCoord coord = tileIndex.To2DCoord();

                m_images_tileIcon[coord.X, coord.Y] = child.Find("Image@Icon").GetComponent<Image>();

                tileIndex++;
            }

            ClearAll();
        }

        public void DisplayTargetArea(eTargetRangeClassification _targetRangeClassification)
        {
            ClearAll();

            foreach (_2DCoord coord in TargetArea.GetTargetArea(_targetRangeClassification))
            {
                // The coord uses negative values as relative coordinate, such as (-1, 0)
                // Here we will be using a two-dimension array; and we need to translate those values so that no negative number is included
                int amountToMove = CoreValues.SIZE_OF_A_SIDE_OF_BOARD.Middle(false) - 1;
                int adjustedX = coord.X + amountToMove;
                int adjustedY = coord.Y + amountToMove;
                Image image_tileIcon = m_images_tileIcon[adjustedX, adjustedY];
                image_tileIcon.enabled = true;
            }
        }

        private void ClearAll()
        {
            int boardSideSize = CoreValues.SIZE_OF_A_SIDE_OF_BOARD;
            int centerIndex = boardSideSize.Middle() - 1;
            for (int y = 0; y < boardSideSize; y++)
            {
                for (int x = 0; x < boardSideSize; x++)
                {
                    if (x == centerIndex && y == centerIndex)
                        m_images_tileIcon[x, y].enabled = true;
                    else
                        m_images_tileIcon[x, y].enabled = false;
                }
            }
        }
    }
}