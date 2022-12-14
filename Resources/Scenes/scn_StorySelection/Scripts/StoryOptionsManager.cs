using EEANWorks.Games.TBSG._01.Data;
using EEANWorks.Games.TBSG._01.Unity.Data;
using EEANWorks.Games.Unity.Engine;
using EEANWorks.Games.Unity.Engine.UI;
using EEANWorks.Games.Unity.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    public class StoryOptionsManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject m_storyOptionButtonPrefab;
        [SerializeField]
        private GameObject m_episodeLocationObjectPrefab;

        [SerializeField]
        private GameObject m_storySelectionPanelGO;
        [SerializeField]
        private GameObject m_arcSelectionPanelGO;
        [SerializeField]
        private GameObject m_chapterSelectionPanelGO;
        [SerializeField]
        private GameObject m_episodeSelectionPanelGO;
        #endregion

        #region Private Fields
        private static bool m_firstInitialization = true;

        private bool m_isInitialized;

        private static bool m_wasPreviousStoryTypeMain;
        private IEnumerable<Story> m_stories;

        private bool m_hasStoryTypeChanged;

        private static eSelectionPhase m_selectionPhase; // Retain selection info through scene shifting

        private static Story m_selectedStory; // Retain selection info through scene shifting
        private static StoryArc m_selectedArc; // Retain selection info through scene shifting
        private static StoryChapter m_selectedChapter; // Retain selection info through scene shifting

        private string m_title_storyType;

        private Text[] m_texts_title;
        private Transform[] m_transforms_optionButtonsContainer;
        #endregion

        // Awake is called before the first frame update
        void Awake()
        {
            if (m_firstInitialization)
                m_hasStoryTypeChanged = true;

            m_texts_title = new Text[4]; // For story, arc, chapter, and episode
            {
                m_texts_title[0] = m_storySelectionPanelGO.transform.Find("Text@Title").GetComponent<Text>();
                m_texts_title[1] = m_arcSelectionPanelGO.transform.Find("Text@Title").GetComponent<Text>();
                m_texts_title[2] = m_chapterSelectionPanelGO.transform.Find("Text@Title").GetComponent<Text>();
                m_texts_title[3] = m_chapterSelectionPanelGO.transform.Find("Text@Title").GetComponent<Text>();
            }

            m_transforms_optionButtonsContainer = new Transform[3]; // For story, arc, and chapter
            {
                m_transforms_optionButtonsContainer[0] = m_storySelectionPanelGO.transform.Find("OptionsContainer").GetChild(0).Find("Contents");
                m_transforms_optionButtonsContainer[1] = m_arcSelectionPanelGO.transform.Find("OptionsContainer").GetChild(0).Find("Contents");
                m_transforms_optionButtonsContainer[2] = m_chapterSelectionPanelGO.transform.Find("OptionsContainer").GetChild(0).Find("Contents");
            }

            if (GameDataContainer.Instance.ShowMainStories)
            {
                m_hasStoryTypeChanged = m_wasPreviousStoryTypeMain == false;

                m_stories = GameDataContainer.Instance.MainStories?.Cast<Story>();
                m_title_storyType = "Main Stories";
            }
            else
            {
                m_hasStoryTypeChanged = m_wasPreviousStoryTypeMain == true;

                m_stories = GameDataContainer.Instance.EventStories?.Cast<Story>();
                m_title_storyType = "Event Stories";
            }

            if (m_hasStoryTypeChanged)
            {
                m_selectionPhase = eSelectionPhase.Story;
                m_selectedStory = null;
                m_selectedChapter = null;
                m_selectedArc = null;
            }

            m_wasPreviousStoryTypeMain = GameDataContainer.Instance.ShowMainStories;
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();
        }

        private void Initialize()
        {
            if (m_isInitialized)
                return;

            if (m_stories == null)
                PopUpWindowManager.Instance.CreateSimplePopUp("Error", "Something went wrong!", "Return", () => SceneConnector.GoToPreviousScene(true));
            else
                UpdateUI();

            m_isInitialized = true;
            m_firstInitialization = false;
        }

        public void ToPreviousPhase()
        {
            int currentPhaseIndex = Convert.ToInt32(m_selectionPhase);
            if (currentPhaseIndex != 0) // If it is not the first phase
            {
                m_selectionPhase = (eSelectionPhase)(currentPhaseIndex - 1);
                UpdateUI(false);
            }
        }

        private void UpdateUI(bool _redrawAll = true)
        {
            HideAllPanels();

            switch (m_selectionPhase)
            {
                default: // case eSelectionPhase.Story
                    {
                        m_storySelectionPanelGO.SetActive(true);
                        if (_redrawAll)
                        {
                            m_texts_title[0].text = m_title_storyType;
                            m_transforms_optionButtonsContainer[0].ClearChildren();
                            foreach (Story story in m_stories)
                            {
                                Button button = Instantiate(m_storyOptionButtonPrefab, m_transforms_optionButtonsContainer[0]).GetComponent<Button>(); // Instantiate a button to open the arcs panel for the story
                                button.GetComponent<Image>().sprite = ImageConverter.ByteArrayToSprite(story.BannerImageAsBytes, FilterMode.Point);
                                button.transform.Find("Text").GetComponent<Text>().text = story.Title;
                                Story tmp_story = story; // Wrapper to avoid same object from being eventually passed to the below delegate
                                button.onClick.AddListener(delegate { m_selectedStory = tmp_story; m_selectionPhase = eSelectionPhase.Arc; UpdateUI(); });
                            }
                        }
                    }
                    break;

                case eSelectionPhase.Arc:
                    {
                        m_arcSelectionPanelGO.SetActive(true);
                        if (_redrawAll)
                        {
                            m_texts_title[1].text = m_title_storyType + " > " + m_selectedStory.Title;
                            m_transforms_optionButtonsContainer[1].ClearChildren();
                            foreach (StoryArc arc in m_selectedStory.Arcs)
                            {
                                Button button = Instantiate(m_storyOptionButtonPrefab, m_transforms_optionButtonsContainer[1]).GetComponent<Button>(); // Instantiate a button to open the chapters panel for the arc
                                button.GetComponent<Image>().sprite = ImageConverter.ByteArrayToSprite(arc.BannerImageAsBytes, FilterMode.Point);
                                button.transform.Find("Text").GetComponent<Text>().text = arc.Title;
                                StoryArc tmp_arc = arc; // Wrapper to avoid same object from being eventually passed to the below delegate
                                button.onClick.AddListener(delegate { m_selectedArc = tmp_arc; m_selectionPhase = eSelectionPhase.Chapter; UpdateUI(); });
                            }
                        }
                    }
                    break;

                case eSelectionPhase.Chapter:
                    {
                        m_chapterSelectionPanelGO.SetActive(true);
                        if (_redrawAll)
                        {
                            m_texts_title[2].text = m_title_storyType + " > " + m_selectedStory.Title + " > " + m_selectedArc.Title;
                            m_transforms_optionButtonsContainer[2].ClearChildren();
                            foreach (StoryChapter chapter in m_selectedArc.Chapters)
                            {
                                Button button = Instantiate(m_storyOptionButtonPrefab, m_transforms_optionButtonsContainer[2]).GetComponent<Button>(); // Instantiate a button to open the episodes panel for the chapter
                                button.GetComponent<Image>().sprite = ImageConverter.ByteArrayToSprite(chapter.BannerImageAsBytes, FilterMode.Point);
                                button.transform.Find("Text").GetComponent<Text>().text = chapter.Title;
                                StoryChapter tmp_chapter = chapter; // Wrapper to avoid same object from being eventually passed to the below delegate
                                button.onClick.AddListener(delegate { m_selectedChapter = tmp_chapter; m_selectionPhase = eSelectionPhase.Episode; UpdateUI(); });
                            }
                        }
                    }
                    break;

                case eSelectionPhase.Episode:
                    {
                        m_episodeSelectionPanelGO.SetActive(true);
                        m_texts_title[3].text = m_title_storyType + " > " + m_selectedStory.Title + " > " + m_selectedArc.Title + " > " + m_selectedChapter.Title;
                        Transform transform_chapterMapPanel = m_episodeSelectionPanelGO.transform.Find("Panel@ChapterMap");
                        Image image_chapterMapPanel = transform_chapterMapPanel.GetComponent<Image>(); // Get the image component for the chapter map
                        Sprite sprite_chapterMap = SpriteContainer.Instance.GetChapterMapImage(m_selectedChapter); // Get the chapter map sprite that corresponds to the selected chapter
                        image_chapterMapPanel.sprite = sprite_chapterMap; // Set the sprite into the image component

                        int mapChipSize = SpriteContainer.Instance.GetChapterMapChipSize(m_selectedChapter);
                        int numOfGrids_horizontal = sprite_chapterMap.texture.width / mapChipSize;
                        int numOfGrids_vertical = sprite_chapterMap.texture.height / mapChipSize;
                        decimal gridSize_horizontal = ((decimal)numOfGrids_horizontal).Reciprocal();
                        decimal gridSize_vertical = ((decimal)numOfGrids_vertical).Reciprocal();

                        for (int i = 0; i < m_selectedChapter.Episodes.Count; i++)
                        {
                            GameObject episodeLocationObject = Instantiate(m_episodeLocationObjectPrefab, transform_chapterMapPanel); // Instantiate an object containing a button to move to the pre-battle novel scene of the episode
                            StoryEpisode tmp_episode = m_selectedChapter.Episodes[0];
                            episodeLocationObject.transform.Find("Text@EpisodeIdAndLocationName").GetComponent<Text>().text = (i + 1).ToString() + ". " + tmp_episode.LocationName;
                            Button button = episodeLocationObject.transform.Find("Button@LocationIcon").GetComponent<Button>();
                            button.image.sprite = SpriteContainer.Instance.GetEpisodeLocationIcon(tmp_episode);

                            // Calculate button anchors and place it on the map
                            float anchorMin_x = (float)(gridSize_horizontal * tmp_episode.ChapterMapCoord.X);
                            float anchorMax_y = 1 - (float)(gridSize_vertical * tmp_episode.ChapterMapCoord.Y);
                            float width = (float)(gridSize_horizontal * tmp_episode.LocationIconSize);
                            float height = (float)(gridSize_vertical * tmp_episode.LocationIconSize);
                            float anchorMax_x = anchorMin_x + width;
                            float anchorMin_y = anchorMax_y - height;
                            RectTransform rt_button = episodeLocationObject.GetComponent<RectTransform>();
                            rt_button.anchorMin = new Vector2(anchorMin_x, anchorMin_y);
                            rt_button.anchorMax = new Vector2(anchorMax_x, anchorMax_y);

                            button.onClick.AddListener(delegate { GameDataContainer.Instance.EpisodeToPlay = tmp_episode; GameDataContainer.Instance.EpisodePhase = eEpisodePhase.PreBattleScene; SceneConnector.GoToScene("scn_Novel"); });
                        }
                    }
                    break;
            }
        }

        private void HideAllPanels()
        {
            m_storySelectionPanelGO.SetActive(false);
            m_arcSelectionPanelGO.SetActive(false);
            m_chapterSelectionPanelGO.SetActive(false);
            m_episodeSelectionPanelGO.SetActive(false);
        }

        private enum eSelectionPhase
        {
            Story,
            Arc,
            Chapter,
            Episode
        }
    }
}