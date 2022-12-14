using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [RequireComponent(typeof(ParentBasedRectTransformAnchorSetter))]
    public class MaskInvertingImage : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material result = new Material(base.materialForRendering);
                result.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return result;
            }
        }
    }
}
