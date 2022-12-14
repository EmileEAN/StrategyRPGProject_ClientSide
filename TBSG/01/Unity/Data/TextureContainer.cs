namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class TextureContainer
    {
        private static TextureContainer m_instance;

        public static TextureContainer Instance { get { return m_instance ?? (m_instance = new TextureContainer()); } }

        private TextureContainer() { }
    }
}
