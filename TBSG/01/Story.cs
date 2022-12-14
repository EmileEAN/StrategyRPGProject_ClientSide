using System.Collections.Generic;

namespace EEANWorks.Games.TBSG._01
{
    public abstract class Story
    {
        public Story(int _id, string _title, byte[] _bannerImageAsBytes, List<StoryArc> _arcs)
        {
            Id = _id;
            Title = _title.CoalesceNullAndReturnCopyOptionally(true);

            BannerImageAsBytes = _bannerImageAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            m_arcs = _arcs.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer
        }

        #region Properties
        public int Id { get; }
        public string Title { get; }

        public byte[] BannerImageAsBytes { get; }

        public IList<StoryArc> Arcs { get { return m_arcs.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<StoryArc> m_arcs;
        #endregion
    }

    public class MainStory : Story
    {
        public MainStory(int _id, string _title, byte[] _bannerImageAsBytes, List<StoryArc> _arcs) : base(_id, _title, _bannerImageAsBytes, _arcs)
        {
        }
    }

    public class EventStory : Story
    {
        public EventStory(int _id, string _title, byte[] _bannerImageAsBytes, List<StoryArc> _arcs) : base(_id, _title, _bannerImageAsBytes, _arcs)
        {
        }
    }

    public class StoryArc
    {
        public StoryArc(string _title, byte[] _bannerImageAsBytes, List<StoryChapter> _chapters)
        {
            Title = _title.CoalesceNullAndReturnCopyOptionally(true);

            BannerImageAsBytes = _bannerImageAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            m_chapters = _chapters.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer
        }

        #region Properties
        public string Title { get; }

        public byte[] BannerImageAsBytes { get; }

        public IList<StoryChapter> Chapters { get { return m_chapters.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<StoryChapter> m_chapters;
        #endregion
    }

    public class StoryChapter
    {
        public StoryChapter(string _title, byte[] _bannerImageAsBytes, byte[] _mapImageAsBytes, List<StoryEpisode> _episodes)
        {
            Title = _title.CoalesceNullAndReturnCopyOptionally(true);

            BannerImageAsBytes = _bannerImageAsBytes.CoalesceNullAndReturnCopyOptionally(eCopyType.Deep);

            MapImageAsBytes = _mapImageAsBytes; // Getting a reference to a byte[] stored within GameDataContainer

            m_episodes = _episodes.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting references to objects stored within GameDataContainer
        }

        #region Properties
        public string Title { get; }

        public byte[] BannerImageAsBytes { get; }

        public byte[] MapImageAsBytes { get; }

        public IList<StoryEpisode> Episodes { get { return m_episodes.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<StoryEpisode> m_episodes;
        #endregion
    }

    public class StoryEpisode
    {
        public StoryEpisode(string _title, string _locationName, byte[] _locationIconAsBytes, _2DCoord _chapterMapCoord, int _locationIconSize, NovelScene _novelSceneBeforeBattle, Dungeon _dungeon, NovelScene? _novelSceneAfterBattle)
        {
            Title = _title.CoalesceNullAndReturnCopyOptionally(true);

            LocationIconAsBytes = _locationIconAsBytes; // Getting a reference to a byte[] stored within GameDataContainer

            ChapterMapCoord = _chapterMapCoord;

            LocationIconSize = _locationIconSize;

            NovelSceneBeforeBattle = _novelSceneBeforeBattle;

            Dungeon = _dungeon;

            NovelSceneAfterBattle = (Dungeon != null) ? _novelSceneAfterBattle : null;
        }

        #region Properties
        public string Title { get; }

        public string LocationName { get; }
        public byte[] LocationIconAsBytes { get; }
        public _2DCoord ChapterMapCoord { get; }
        public int LocationIconSize { get; }

        public NovelScene NovelSceneBeforeBattle { get; }

        public Dungeon Dungeon { get; } // Nullable. If null, the episode will not have the battle part.

        public NovelScene? NovelSceneAfterBattle { get; } // Nullable. If this is null, the episode will not have a novel scene after the battle part. If Dungeon is null, this will also be null. 
        #endregion
    }

    public struct NovelScene
    {
        public NovelScene(List<NovelSubscene> _subscenes)
        {
            m_subscenes = _subscenes.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow);
        }

        #region Properties
        public IList<NovelSubscene> Subscenes { get { return m_subscenes.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<NovelSubscene> m_subscenes;
        #endregion
    }

    public struct NovelSubscene
    {
        public NovelSubscene(byte[] _backgroundImageAsBytes, List<NovelLine> _lines)
        {
            BackgroundImageAsBytes = _backgroundImageAsBytes; // Getting a reference to a byte[] stored within GameDataContainer

            m_lines = _lines.CoalesceNullAndReturnCopyOptionally(eCopyType.Shallow); // Getting a reference to an object stored within GameDataContainer
        }

        #region Properties
        public byte[] BackgroundImageAsBytes { get; }
        public IList<NovelLine> Lines { get { return m_lines.AsReadOnly(); } }
        #endregion

        #region Private Read-only Fields
        private readonly List<NovelLine> m_lines;
        #endregion
    }

    public struct NovelLine
    {
        public NovelLine(string _line, eNovelCharacterPosition _characterPosition, NovelCharacterSpritePartsIndication? _characterSpritePartsIndication)
        {
            Line = _line.CoalesceNullAndReturnCopyOptionally(true);
            CharacterPosition = _characterPosition;
            CharacterSpritePartsIndication = _characterSpritePartsIndication;
        }

        public string Line { get; }
        public eNovelCharacterPosition CharacterPosition { get; }
        public NovelCharacterSpritePartsIndication? CharacterSpritePartsIndication { get; } // Nullable
    }

    public struct NovelCharacterSpritePartsIndication
    {
        public NovelCharacterSpritePartsIndication(string _characterName, bool _showTearsSprite, bool _showWaterfallTearsSprite, bool _showShySprite, bool _showEmbarrassmentSprite, bool _showRedNoseSprite, bool _showBlackFaceMaskSprite, bool _showBlueFaceMaskSprite, bool _showRedFaceMaskSprite,  eNovelCharacterFaceType _faceType, eNovelCharacterMouthType _mouthType, eNovelEmotionIconType? _emotionIconType)
        {
            CharacterName = _characterName.CoalesceNullAndReturnCopyOptionally(true);

            ShowTearsSprite = _showTearsSprite;
            ShowWaterfallTearsSprite = _showWaterfallTearsSprite;
            ShowShySprite = _showShySprite;
            ShowEmbarrassmentSprite = _showEmbarrassmentSprite;
            ShowRedNoseSprite = _showRedNoseSprite;

            ShowBlackFaceMaskSprite = _showBlackFaceMaskSprite;
            ShowBlueFaceMaskSprite = _showBlueFaceMaskSprite;
            ShowRedFaceMaskSprite = _showRedFaceMaskSprite;

            FaceType = _faceType;
            MouthType = _mouthType;
            EmotionIconType = _emotionIconType;
        }

        public string CharacterName { get; }

        public bool ShowTearsSprite { get; }
        public bool ShowWaterfallTearsSprite { get; }
        public bool ShowShySprite { get; }
        public bool ShowEmbarrassmentSprite { get; }
        public bool ShowRedNoseSprite { get; }

        public bool ShowBlackFaceMaskSprite { get; }
        public bool ShowBlueFaceMaskSprite { get; }
        public bool ShowRedFaceMaskSprite { get; }

        public eNovelCharacterFaceType FaceType { get; }
        public eNovelCharacterMouthType MouthType { get; }
        public eNovelEmotionIconType? EmotionIconType { get; } // Nullable
    }

    public enum eNovelCharacterFaceType
    {
        LookingStraight,
        Narrowing1,
        Narrowing2,
        Closed,
        HalfAsleep,
        LookingUp,
        LookingAway,
        NarrowingAndLookingAway,
        LookingDown,
        Winking,
        Smiling,
        Surprised_EyesOpen,
        Surpriesd_EyesClosed,
        Suffering,
        NoLight,
        Love,
        Expecting,
        Dazed,
        Money,
        Extra
    }

    public enum eNovelCharacterMouthType
    {
        Expressionless,
        Smile1,
        Smile2,
        Smile3,
        Grin,
        Smug,
        Smirk,
        DevilishSmile,
        Disgust1,
        Disgust2,
        Sour,
        Bite1,
        Bite2,
        Bite3,
        Bite4,
        Bite5,
        Downturned1,
        Downturned2,
        Sigh,
        PuffedCheeks,
        MouthOpen_Speak,
        MouthOpen_Horizontally,
        MouthOpen_Shout,
        Surprise1,
        Surprise2,
        Surprise3,
        TongueOut1,
        TongueOut2,
        TongueOut3,
        Drool
    }

    public enum eNovelEmotionIconType
    {
        Amused,
        SpacedOut,
        Sleeping,
        Noticed,
        Shocked,
        Fun,
        Glitter,
        Flowers,
        Furious,
        Irritated,
        Uneasy,
        Silence,
        Idea,
        Yes,
        No,
        Heart,
        BrokenHeart,
        Exclamation,
        Question,
        ExclamationAndQuestion,
        Injured,
        Gloomy,
        Angry,
        Sweat,
        Droplets,
        Trembling
    }

    public enum eNovelCharacterPosition
    {
        Left,
        Center,
        Right
    }
}
