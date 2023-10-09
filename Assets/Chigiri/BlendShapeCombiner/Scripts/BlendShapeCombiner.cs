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
        public bool overwriteExistingKeys;
        public bool clearAllExistingKeys;
        public bool clearNormal;
        public bool clearTangent;
        public bool useTextField;
        public NewKey[] newKeys = new NewKey[0];

    }

}
