using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine.UI;
using EEANWorks.Games.Unity.Graphics;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class NovelSceneManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private Sprite m_transparentSprite;
        [SerializeField]
        private int m_lettersPerFrame;
        [SerializeField]
        private int m_framesBetweenTextboxEndIconVisibilityFlipping;

        [SerializeField]
        private Image m_image_Background;

        // Left-side Images
        [SerializeField]
        private Image m_image_Left_EmotionIcon;
        [SerializeField]
        private Image m_image_Left_Tears;
        [SerializeField]
        private Image m_image_Left_WaterfallTears;
        [SerializeField]
        private Image m_image_Left_Shy;
        [SerializeField]
        private Image m_image_Left_Embarrassment;
        [SerializeField]
        private Image m_image_Left_RedNose;
        [SerializeField]
        private Image m_image_Left_BlackFaceMask;
        [SerializeField]
        private Image m_image_Left_BaseFront;
        [SerializeField]
        private Image m_image_Left_Mouth;
        [SerializeField]
        private Image m_image_Left_Face;
        [SerializeField]
        private Image m_image_Left_BlueFaceMask;
        [SerializeField]
        private Image m_image_Left_RedFaceMask;
        [SerializeField]
        private Image m_image_Left_BaseBack;
        // Center-side Images
        [SerializeField]
        private Image m_image_Center_EmotionIcon;
        [SerializeField]
        private Image m_image_Center_Tears;
        [SerializeField]
        private Image m_image_Center_WaterfallTears;
        [SerializeField]
        private Image m_image_Center_Shy;
        [SerializeField]
        private Image m_image_Center_Embarrassment;
        [SerializeField]
        private Image m_image_Center_RedNose;
        [SerializeField]
        private Image m_image_Center_BlackFaceMask;
        [SerializeField]
        private Image m_image_Center_BaseFront;
        [SerializeField]
        private Image m_image_Center_Mouth;
        [SerializeField]
        private Image m_image_Center_Face;
        [SerializeField]
        private Image m_image_Center_BlueFaceMask;
        [SerializeField]
        private Image m_image_Center_RedFaceMask;
        [SerializeField]
        private Image m_image_Center_BaseBack;
        // Right-side Images
        [SerializeField]
        private Image m_image_Right_EmotionIcon;
        [SerializeField]
        private Image m_image_Right_Tears;
        [SerializeField]
        private Image m_image_Right_WaterfallTears;
        [SerializeField]
        private Image m_image_Right_Shy;
        [SerializeField]
        private Image m_image_Right_Embarrassment;
        [SerializeField]
        private Image m_image_Right_RedNose;
        [SerializeField]
        private Image m_image_Right_BlackFaceMask;
        [SerializeField]
        private Image m_image_Right_BaseFront;
        [SerializeField]
        private Image m_image_Right_Mouth;
        [SerializeField]
        private Image m_image_Right_Face;
        [SerializeField]
        private Image m_image_Right_BlueFaceMask;
        [SerializeField]
        private Image m_image_Right_RedFaceMask;
        [SerializeField]
        private Image m_image_Right_BaseBack;

        [SerializeField]
        private DynamicGridLayoutGroup m_textboxTitleContainerLayoutGroup;
        [SerializeField]
        private Text m_text_Title;
        [SerializeField]
        private Text m_text_Textbox;
        #endregion

        #region Private Constant Fields
        private const string TEXTBOX_END_ICON = "<color=lightblue>▼</color>";
        #endregion

        #region Private Fields
        private bool m_isInitialized;

        private NovelScene m_novelScene;
        private int m_subsceneIndex;
        private int m_lineIndex;
        private bool m_showingText;
        private bool m_skipShowingText;
        private bool m_allTextsShown;

        private GameObject m_go_textArea;
        private bool m_isTextAreaHidden;

        private int m_framesUntilNextTextboxEndIconVisibilityFlip;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            m_isInitialized = false;

            m_subsceneIndex = 0;
            m_lineIndex = 0;
            m_showingText = false;
            m_skipShowingText = false;
            m_allTextsShown = false;

            m_go_textArea = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Panel@NovelMenu").Find("TextArea").gameObject;
            m_isTextAreaHidden = false;

            m_framesUntilNextTextboxEndIconVisibilityFlip = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();
            else if (m_lineIndex == 0 && !m_showingText) // If it has not started showing the lines
                StartCoroutine(ShowText()); // Show the first line
            else
            {
                if (Input.GetMouseButton(2) && !m_isTextAreaHidden) // If middle button is pressed
                {
                    m_go_textArea.SetActive(false);
                    m_isTextAreaHidden = true;
                }
                else if (!Input.GetMouseButton(2) && m_isTextAreaHidden)
                {
                    m_go_textArea.SetActive(true);
                    m_isTextAreaHidden = false;
                }

                if (!m_isTextAreaHidden)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (m_allTextsShown) // If no more animation is left for the subscene
                        {
                            if (m_subsceneIndex < m_novelScene.Subscenes.Count - 1) // If there is any other scene to show
                                ShiftSubscene();
                            else // If all subscenes have ended
                                EndScene();
                        }
                        else
                        {
                            if (m_showingText && !m_skipShowingText)
                                m_skipShowingText = true;
                            else if (!m_showingText)
                                StartCoroutine(ShowText());
                        }
                    }

                    if (m_text_Textbox.text != "" && !m_showingText) // If the textbox has been filled in with text
                    {
                        if (m_framesUntilNextTextboxEndIconVisibilityFlip == 0) // If this is the frame on which visibility needs to be flipped
                        {
                            if (m_text_Textbox.text.Contains(TEXTBOX_END_ICON))
                                m_text_Textbox.text = m_text_Textbox.text.Remove(TEXTBOX_END_ICON);
                            else
                                m_text_Textbox.text += TEXTBOX_END_ICON;

                            m_framesUntilNextTextboxEndIconVisibilityFlip = m_framesBetweenTextboxEndIconVisibilityFlipping; // Update the number of frames to wait until next visibility flip
                        }
                        else
                            m_framesUntilNextTextboxEndIconVisibilityFlip--;
                    }
                }
            }
        }

        private void Initialize()
        {
            if (m_isInitialized)
                return;

            try
            {
                switch (GameDataContainer.Instance.EpisodePhase)
                {
                    case eEpisodePhase.PreBattleScene:
                        m_novelScene = GameDataContainer.Instance.EpisodeToPlay.NovelSceneBeforeBattle; // Load novel scene data
                        break;
                         
                    case eEpisodePhase.PostBattleScene:
                        NovelScene? postBattleScene = GameDataContainer.Instance.EpisodeToPlay.NovelSceneAfterBattle;
                        if (postBattleScene == null) // If the episode does not have a post-battle scene
                            SceneConnector.GoToScene("scn_StorySelection"); // Return to the episode selection scene
                        m_novelScene = postBattleScene ?? default; // Load novel scene data. It will not be null.
                        break;
                }

                m_image_Background.sprite = ImageConverter.ByteArrayToSprite(m_novelScene.Subscenes[0].BackgroundImageAsBytes, FilterMode.Point); // Set the background image for the first subscene

                ClearImages(); // Apply TransparentSprite to all character image parts

                m_isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.Log("At NovelAnimationManager.Initialize(): " + ex.Message);
            }
        }

        IEnumerator ShowText()
        {
            if (m_showingText || m_allTextsShown)
                yield break;

            m_showingText = true;

            NovelLine currentLine = m_novelScene.Subscenes[m_subsceneIndex].Lines[m_lineIndex];

            // Change the position of the title container and set the image of the corresponding position if available
            DynamicGridLayoutGroup.eHorizontalAlignment textboxTitleContainerLayoutGroupHorizontalAlignment;
            NovelCharacterSpritePartsIndication? spriteSheetPartsIndication = currentLine.CharacterSpritePartsIndication;
            string characterName = spriteSheetPartsIndication?.CharacterName;
            NovelCharacterSpriteSet characterSpriteSet = null;
            if (characterName != null)
            {
                characterSpriteSet = SpriteContainer.Instance.CharacterName_NovelCharacterSpriteSet[characterName];
            }
            SetImages(currentLine.CharacterPosition, out textboxTitleContainerLayoutGroupHorizontalAlignment, spriteSheetPartsIndication, characterSpriteSet);
            m_textboxTitleContainerLayoutGroup.HorizontalAlignment = textboxTitleContainerLayoutGroupHorizontalAlignment;

            // Update the text that goes into the title container
            m_text_Title.text = characterName ?? "";

            // Clear the text within the textbox
            m_text_Textbox.text = "";

            // Add letter to the textbox
            int letterIndex = 0;
            int textLength = currentLine.Line.Length;
            while (letterIndex < textLength)
            {
                if (m_skipShowingText)
                {
                    m_text_Textbox.text = currentLine.Line;
                    break;
                }
                else
                {
                    for (int i = 0; i < m_lettersPerFrame; i++)
                    {
                        if (letterIndex == textLength)
                            break;

                        m_text_Textbox.text += currentLine.Line[letterIndex];
                        letterIndex++;
                    }
                }

                yield return null;
            }

            m_showingText = false;
            m_skipShowingText = false;

            if (m_lineIndex < m_novelScene.Subscenes[m_subsceneIndex].Lines.Count - 1)
                m_lineIndex++;
            else if (!m_allTextsShown)
                m_allTextsShown = true;
        }

        private void ClearImages()
        {
            DynamicGridLayoutGroup.eHorizontalAlignment mock_alignment;
            SetImages(eNovelCharacterPosition.Left, out mock_alignment, null, null);
            SetImages(eNovelCharacterPosition.Center, out mock_alignment, null, null);
            SetImages(eNovelCharacterPosition.Right, out mock_alignment, null, null);
        }
        private void SetImages(eNovelCharacterPosition _characterPosition, out DynamicGridLayoutGroup.eHorizontalAlignment _textboxTitleContainerLayoutGroupHorizontalAlignment, NovelCharacterSpritePartsIndication? _spriteSheetPartsIndication, NovelCharacterSpriteSet _characterSpriteSet)
        {
            switch (_characterPosition)
            {
                default: // case eNovelCharacterPosition.Left
                    {
                        _textboxTitleContainerLayoutGroupHorizontalAlignment = DynamicGridLayoutGroup.eHorizontalAlignment.Left;

                        m_image_Left_BaseBack.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.BaseBackSprite : m_transparentSprite;
                        m_image_Left_RedFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowRedFaceMaskSprite ?? default) ? _characterSpriteSet.RedFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_BlueFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowBlueFaceMaskSprite ?? default) ? _characterSpriteSet.BlueFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_Face.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.FaceSprites.GetSprite(_spriteSheetPartsIndication?.FaceType ?? default) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct face type instead of the default value.
                        m_image_Left_Mouth.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.MouthSprites.GetSprite(_spriteSheetPartsIndication?.MouthType ?? default) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct mouth type instead of the default value.
                        m_image_Left_BaseFront.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.BaseFrontSprite : m_transparentSprite;
                        m_image_Left_BlackFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowBlackFaceMaskSprite ?? default) ? _characterSpriteSet.BlackFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_RedNose.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowRedNoseSprite ?? default) ? _characterSpriteSet.RedNoseSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_Embarrassment.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowEmbarrassmentSprite ?? default) ? _characterSpriteSet.EmbarrassmentSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_Shy.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowShySprite ?? default) ? _characterSpriteSet.ShySprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_WaterfallTears.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowWaterfallTearsSprite ?? default) ? _characterSpriteSet.WaterfallTearsSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_Tears.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowTearsSprite ?? default) ? _characterSpriteSet.TearsSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Left_EmotionIcon.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.EmotionIconType != null) ? SpriteContainer.Instance.GetNovelEmotionIcon(_spriteSheetPartsIndication?.EmotionIconType ?? default) : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct value of the nullable emotion icon type. If the emotion icon type is null, the sprite will be transparent.
                    }
                    break;
                case eNovelCharacterPosition.Center:
                    {
                        _textboxTitleContainerLayoutGroupHorizontalAlignment = DynamicGridLayoutGroup.eHorizontalAlignment.Center;

                        m_image_Center_BaseBack.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.BaseBackSprite : m_transparentSprite;
                        m_image_Center_RedFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowRedFaceMaskSprite ?? default) ? _characterSpriteSet.RedFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_BlueFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowBlueFaceMaskSprite ?? default) ? _characterSpriteSet.BlueFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_Face.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.FaceSprites.GetSprite(_spriteSheetPartsIndication?.FaceType ?? default) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct face type instead of the default value.
                        m_image_Center_Mouth.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.MouthSprites.GetSprite(_spriteSheetPartsIndication?.MouthType ?? default) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct mouth type instead of the default value.
                        m_image_Center_BaseFront.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.BaseFrontSprite : m_transparentSprite;
                        m_image_Center_BlackFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowBlackFaceMaskSprite ?? default) ? _characterSpriteSet.BlackFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_RedNose.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowRedNoseSprite ?? default) ? _characterSpriteSet.RedNoseSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_Embarrassment.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowEmbarrassmentSprite ?? default) ? _characterSpriteSet.EmbarrassmentSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_Shy.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowShySprite ?? default) ? _characterSpriteSet.ShySprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_WaterfallTears.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowWaterfallTearsSprite ?? default) ? _characterSpriteSet.WaterfallTearsSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_Tears.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowTearsSprite ?? default) ? _characterSpriteSet.TearsSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Center_EmotionIcon.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.EmotionIconType != null) ? SpriteContainer.Instance.GetNovelEmotionIcon(_spriteSheetPartsIndication?.EmotionIconType ?? default) : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct value of the nullable emotion icon type. If the emotion icon type is null, the sprite will be transparent.
                    }
                    break;
                case eNovelCharacterPosition.Right:
                    {
                        _textboxTitleContainerLayoutGroupHorizontalAlignment = DynamicGridLayoutGroup.eHorizontalAlignment.Right;

                        m_image_Right_BaseBack.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.BaseBackSprite : m_transparentSprite;
                        m_image_Right_RedFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowRedFaceMaskSprite ?? default) ? _characterSpriteSet.RedFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_BlueFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowBlueFaceMaskSprite ?? default) ? _characterSpriteSet.BlueFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_Face.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.FaceSprites.GetSprite(_spriteSheetPartsIndication?.FaceType ?? default) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct face type instead of the default value.
                        m_image_Right_Mouth.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.MouthSprites.GetSprite(_spriteSheetPartsIndication?.MouthType ?? default) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct mouth type instead of the default value.
                        m_image_Right_BaseFront.sprite = (_characterSpriteSet != null) ? _characterSpriteSet.BaseFrontSprite : m_transparentSprite;
                        m_image_Right_BlackFaceMask.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowBlackFaceMaskSprite ?? default) ? _characterSpriteSet.BlackFaceMaskSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_RedNose.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowRedNoseSprite ?? default) ? _characterSpriteSet.RedNoseSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_Embarrassment.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowEmbarrassmentSprite ?? default) ? _characterSpriteSet.EmbarrassmentSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_Shy.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowShySprite ?? default) ? _characterSpriteSet.ShySprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_WaterfallTears.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowWaterfallTearsSprite ?? default) ? _characterSpriteSet.WaterfallTearsSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_Tears.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.ShowTearsSprite ?? default) ? _characterSpriteSet.TearsSprite : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct bool value instead of the default value.
                        m_image_Right_EmotionIcon.sprite = (_characterSpriteSet != null) ? ((_spriteSheetPartsIndication?.EmotionIconType != null) ? SpriteContainer.Instance.GetNovelEmotionIcon(_spriteSheetPartsIndication?.EmotionIconType ?? default) : m_transparentSprite) : m_transparentSprite; // Because spriteSheetPartsIndication is actually not null, it will always get the correct value of the nullable emotion icon type. If the emotion icon type is null, the sprite will be transparent.
                    }
                    break;
            }
        }

        private void ShiftSubscene()
        {
            m_subsceneIndex++;

            m_lineIndex = 0;
            m_allTextsShown = false;
        }

        private void EndScene()
        {
            eEpisodePhase episodePhase = GameDataContainer.Instance.EpisodePhase;
            if (episodePhase == eEpisodePhase.PreBattleScene)
            {
                Dungeon dungeon = GameDataContainer.Instance.EpisodeToPlay.Dungeon;
                if (dungeon == null)
                    SceneConnector.GoToScene("scn_StorySelection");
                else
                {
                    GameDataContainer.Instance.DungeonToPlay = dungeon;
                    SceneConnector.GoToScene("scn_DungeonLobby");
                }
            }
            else if (episodePhase == eEpisodePhase.PostBattleScene)
                SceneConnector.GoToScene("scn_StorySelection");
        }
    }
}