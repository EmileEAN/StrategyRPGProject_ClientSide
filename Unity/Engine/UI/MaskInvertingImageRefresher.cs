using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EEANWorks.Games.Unity.Engine.UI
{
    /// <summary>
    /// Used to avoid the issue with the mask inverting images not being rendered on scene load
    /// </summary>
    public class MaskInvertingImageRefresher : MonoBehaviour
    {
        #region Private Fields
        private int m_refresh;

        private MaskInvertingImage[] m_images;
        #endregion

        // Called before the first Update()
        void Awake()
        {
            m_refresh = 0;

            m_images = this.transform.root.GetComponentsInChildren<MaskInvertingImage>();
        }

        // Called every frame
        void Update()
        {
            if (m_refresh > 1)
                return;

            if (m_refresh == 0)
            {
                foreach (MaskInvertingImage image in m_images)
                {
                    image.enabled = false;
                }
                m_refresh++;
            }
            else if (m_refresh == 1)
            {
                foreach (MaskInvertingImage image in m_images)
                {
                    image.enabled = true;
                }
                m_refresh++;
            }
        }
    }
}
