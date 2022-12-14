using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [AddComponentMenu("UI/Image Toggle Group", 36)]
    [DisallowMultipleComponent]
    public class ImageToggleGroup : UIBehaviour
    {
        [SerializeField] private bool m_allowSwitchOff = false;
        public bool AllowSwitchOff { get { return m_allowSwitchOff; } set { m_allowSwitchOff = value; } }

        private List<ImageToggle> m_toggles = new List<ImageToggle>();

        protected ImageToggleGroup()
        { }

        private void ValidateToggleIsInGroup(ImageToggle _toggle)
        {
            if (_toggle == null || !m_toggles.Contains(_toggle))
                throw new ArgumentException(string.Format("ImageToggle {0} is not part of ImageToggleGroup {1}", _toggle, this));
        }

        public void NotifyToggleOn(ImageToggle _toggle)
        {
            ValidateToggleIsInGroup(_toggle);

            // disable all toggles in the group
            for (int i = 0; i < m_toggles.Count; i++)
            {
                if (m_toggles[i] == _toggle)
                    continue;

                m_toggles[i].IsOn = false;
            }
        }

        public void UnregisterToggle(ImageToggle _toggle)
        {
            if (m_toggles.Contains(_toggle))
                m_toggles.Remove(_toggle);
        }

        public void RegisterToggle(ImageToggle _toggle)
        {
            if (!m_toggles.Contains(_toggle))
                m_toggles.Add(_toggle);
        }

        public bool AnyToggleOn()
        {
            return m_toggles.Find(x => x.IsOn) != null;
        }

        public IEnumerable<ImageToggle> ActiveToggles()
        {
            return m_toggles.Where(x => x.IsOn);
        }

        public void SetAllTogglesOff()
        {
            bool oldAllowSwitchOff = m_allowSwitchOff;
            m_allowSwitchOff = true;

            for (int i = 0; i < m_toggles.Count; i++)
                m_toggles[i].IsOn = false;

            m_allowSwitchOff = oldAllowSwitchOff;
        }
    }
}
