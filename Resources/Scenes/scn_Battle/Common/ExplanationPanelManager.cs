using UnityEngine;

public class ExplanationPanelManager : MonoBehaviour
{
    #region
    private GameObject m_go_explanationPanel;
    #endregion

    // Awake is called before Update for the first frame
    void Awake()
    {
        m_go_explanationPanel = this.transform.Find("Panel@Explanation").gameObject;
    }

    public void ShowMenu() { m_go_explanationPanel.SetActive(true); }

    public void HideMenu() { m_go_explanationPanel.SetActive(false); }
}
