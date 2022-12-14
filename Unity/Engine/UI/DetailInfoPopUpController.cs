using UnityEngine;
using UnityEngine.EventSystems;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DetailInfoPopUpController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Properties
        public GameObject GO_detailInfoPopUp { get; private set; }
        #endregion

        #region Private Fields
        private bool m_isInitialized;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_isInitialized = false;

            GO_detailInfoPopUp = this.transform.FindChildWithTag("DetailInfoPopUp").gameObject;
            GO_detailInfoPopUp.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isInitialized)
                Initialize();

            DeactivatePopUpIfPointerExitsAll();
        }

        private void Initialize()
        {
            if (m_isInitialized)
                return;

            RectTransform rt_detailInfoPopUp = GO_detailInfoPopUp.GetComponent<RectTransform>();

            RectTransform rt_popUPParent = null;
            while (true)
            {
                if (rt_popUPParent == null)
                    rt_popUPParent = this.transform.GetComponent<RectTransform>();
                else
                    rt_popUPParent = rt_popUPParent.parent.GetComponent<RectTransform>();

                if (rt_popUPParent.tag == "Canvas")
                    break;

                Vector2 parentAnchorSize = rt_popUPParent.anchorMax - rt_popUPParent.anchorMin;
                rt_detailInfoPopUp.anchorMin = rt_popUPParent.anchorMin + parentAnchorSize * rt_detailInfoPopUp.anchorMin;
                rt_detailInfoPopUp.anchorMax = rt_popUPParent.anchorMax - parentAnchorSize * (Vector2.one - rt_detailInfoPopUp.anchorMax);
            }

            rt_detailInfoPopUp.parent = rt_popUPParent.transform;
            rt_detailInfoPopUp.SetPosition(0, 0, 0, 0);

            m_isInitialized = true;
        }

        public void OnPointerEnter(PointerEventData _eventData) { GO_detailInfoPopUp.SetActive(true); }

        public void OnPointerExit(PointerEventData _eventData) { } // Do not use the original behaviour of OnPointerExit
        private void DeactivatePopUpIfPointerExitsAll()
        {
            if (!this.gameObject.IsPointerOver())
            {
                if (!GO_detailInfoPopUp.IsPointerOver())
                {
                    if (!GO_detailInfoPopUp.IsPointOverAnyChild())
                        GO_detailInfoPopUp.SetActive(false);
                }
            }
        }
    }
}