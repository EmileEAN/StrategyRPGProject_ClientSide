using System.Collections.Generic;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01.Unity.SceneSpecific
{
    [RequireComponent(typeof(Camera))]
    public class CustomCameraShifter : MonoBehaviour
    {
        private List<CAMERA_INFO> m_cameraInfos;
        private int m_currentIndex;

        void Start()
        {
            m_cameraInfos = new List<CAMERA_INFO>();

            m_cameraInfos.Add(new CAMERA_INFO { Position = this.transform.position, Rotation = this.transform.rotation }); //Default Position in the editor
            m_cameraInfos.Add(new CAMERA_INFO { Position = new Vector3(23, 174, 155), Rotation = Quaternion.Euler(new Vector3(62, 0, 0)) });
            m_cameraInfos.Add(new CAMERA_INFO { Position = new Vector3(480, 274, 218), Rotation = Quaternion.Euler(new Vector3(60, -45, -0.25f)) });

            m_currentIndex = 0;

            this.transform.position = m_cameraInfos[m_currentIndex].Position;
            this.transform.rotation = m_cameraInfos[m_currentIndex].Rotation;

        }

        public void ChangeCameraProperties()
        {
            if (m_currentIndex == m_cameraInfos.Count - 1) //If it is the last index of CamerasInfo
                m_currentIndex = 0;
            else
                m_currentIndex++;

            this.transform.position = m_cameraInfos[m_currentIndex].Position;
            this.transform.rotation = m_cameraInfos[m_currentIndex].Rotation;
        }
    }

    public struct CAMERA_INFO
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}