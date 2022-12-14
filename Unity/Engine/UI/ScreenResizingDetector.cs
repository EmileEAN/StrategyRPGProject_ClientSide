using EEANWorks.Games.Unity.Engine.EventSystems;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [RequireComponent(typeof(CustomStandaloneInputModule))]
    public class ScreenResizingDetector : MonoBehaviour
    {

        public float LatestScreenWidth { get; private set; }
        public float LatestScreenHeight { get; private set; }

        private CustomStandaloneInputModule m_customStandaloneInputModule;

        // Awake is called before Update for the first frame
        void Awake()
        {
            UpdateScreenSizeInfo();

            m_customStandaloneInputModule = this.GetComponent<CustomStandaloneInputModule>();
        }

        // Update is called once per frame
        void Update()
        {
            if (WasScreenResized())
            {
                UpdateScreenSizeInfo();
                m_customStandaloneInputModule.UpdateReferenceTextures();
            }
        }

        private bool WasScreenResized()
        {
            if (LatestScreenWidth != Screen.width)
                return true;

            if (LatestScreenHeight != Screen.height)
                return true;

            return false;
        }

        private void UpdateScreenSizeInfo()
        {
            LatestScreenWidth = Screen.width;
            LatestScreenHeight = Screen.height;
            //Debug.Log("Screen size has been updated.");
        }
    }
}