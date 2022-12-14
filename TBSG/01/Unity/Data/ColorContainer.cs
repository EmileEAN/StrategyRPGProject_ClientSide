using System;
using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.Data
{
    public sealed class ColorContainer
    {
        private static ColorContainer m_instance;

        public static ColorContainer Instance { get { return m_instance ?? (m_instance = new ColorContainer()); } }

        private ColorContainer() { }

        #region Private Fields
        //private List<Color> m_elementFrameColors;
        #endregion

        #region Public Functions
        public void Initialize(List<Color> _elementFrameColors)
        {
            //m_elementFrameColors = _elementFrameColors;
        }

        /*
        public List<Color> ElementFrameColorsForUnit(UnitData _unit)
        {
            int elementIndex1 = Convert.ToInt32(_unit.Elements[0]);
            Color color_element1 = m_elementFrameColors[elementIndex1];

            int elementIndex2 = Convert.ToInt32(_unit.Elements[1]);
            Color color_element2;
            if (elementIndex2 == 0 && elementIndex1 != 0) // index == 0 means eElement.None
                color_element2 = m_elementFrameColors[elementIndex1];
            else
                color_element2 = m_elementFrameColors[elementIndex2];

            return new List<Color> { color_element1, color_element2 };
        }
        */
        #endregion
    }
}