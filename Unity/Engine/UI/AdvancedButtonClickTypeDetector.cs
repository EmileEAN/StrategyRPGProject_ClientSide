using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine.UI
{
    //[RequireComponent(typeof(AdvancedButton))] Commented out to avoid circular reference that prevents it from being deleted
    public class AdvancedButtonClickTypeDetector : MonoBehaviour
    {
        #region Private Fields
        private AdvancedButton m_advancedButton;
        #endregion

        // Awake is called before Update for the first frame
        void Awake()
        {
            m_advancedButton = this.GetComponent<AdvancedButton>();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_advancedButton.IsExecuting) // If an event is being invoked
                return; // Do nothing

            if (m_advancedButton.TimePressStarted == -1f) // If pressing has been canceled
                return; // Do nothing

            bool clicked = m_advancedButton.TimePressEnded > m_advancedButton.TimePressStarted; // The button has been released

            if (m_advancedButton.EnableLongPress && !clicked) // If long press is enabled and the button is still pressed
            {
                if (Time.realtimeSinceStartup - m_advancedButton.TimePressStarted >= m_advancedButton.SecondsForLongPress) // If the button has been pressed long enough
                    m_advancedButton.Execute(eClickType.LongPress); // Indicate advanced button to invoke long press event
            }
            else if (clicked)
            {
                if (!m_advancedButton.EnableDoubleClick
                    || (m_advancedButton.EnableDoubleClick && (Time.realtimeSinceStartup - m_advancedButton.TimePressEnded > m_advancedButton.MaxSecondsForDoubleClick))) // If double click is enabled and time since last click has exceeded max seconds for double click; which means that the next click has not ocurred
                {
                    m_advancedButton.Execute(eClickType.Single); // Indicate advanced button to invoke single click event
                }
            }
        }
    }

    public enum eClickType
    {
        Single,
        Double,
        LongPress
    }
}
