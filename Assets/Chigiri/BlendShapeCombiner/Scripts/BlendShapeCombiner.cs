using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chigiri.BlendShapeCombiner
{

	[DisallowMultipleComponent]
	[HelpURL("https://github.com/chigirits/BlendShapeCombiner")]
    public class BlendShapeCombiner : MonoBehaviour
    {

        public SkinnedMeshRenderer targetRenderer;
        public Mesh sourceMesh;
        public List<NewKey> newKeys = new List<NewKey>{};

    }

}
