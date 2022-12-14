using EEANWorks.Games.Unity.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Engine.UI
{
    public class Anim_ModifyImageBrightness : StateMachineBehaviour
    {

        public float RelativeBrightness;

        private Image m_image;

        private Sprite m_originalSprite;
        private Sprite m_targetSprite;
        private bool m_isInitialized = false;

        private void Initialize(Animator _animator)
        {
            if (!m_isInitialized)
            {
                m_image = _animator.GetComponent<Image>();
                if (m_image == null)
                    return;

                m_isInitialized = true;
            }
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            if (!m_isInitialized)
                Initialize(_animator);
            if (!m_isInitialized)
                return;

            m_originalSprite = m_image.sprite;
            Texture2D originalTexture = m_originalSprite.texture;

            // Create a new texture from the original texture
            Texture2D targetTexture = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, false);
            targetTexture.LoadRawTextureData(originalTexture.GetRawTextureData());

            //Debug.Log("Format: " + targetTexture.format.ToString());

            // Modify the new texture based on RelativeBrightness value
            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r *= RelativeBrightness;
                pixels[i].g *= RelativeBrightness;
                pixels[i].b *= RelativeBrightness;
            }
            targetTexture.SetPixels(pixels);

            //Apply all modifications to m_targetTexture
            targetTexture.Apply();

            m_targetSprite = ImageConverter.TextureToSprite(targetTexture);

            m_image.sprite = m_targetSprite;
            //Debug.Log("Entered State");
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex)
        {
            if (!m_isInitialized)
                Initialize(_animator);
            if (!m_isInitialized)
                return;

            m_image.sprite = m_originalSprite;
            //Debug.Log("Exited State");
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}