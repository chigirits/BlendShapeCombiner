using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chigiri.BlendShapeCombiner
{

	[DisallowMultipleComponent]
	[HelpURL("https://github.com/chigirits/BlendShapeCombiner")]
    public class BlendShapeCombiner : MonoBehaviour
    {

        public int version;
        public SkinnedMeshRenderer targetRenderer;
        public Mesh sourceMesh;
        public bool overwriteExistingKeys;
        public bool clearAllExistingKeys;
        public bool clearNormal;
        public bool clearTangent;
        public bool useTextField;
        public bool usePercentage;
        public NewKey[] newKeys = new NewKey[0];

        [NonSerialized] public string[] _shapeKeys;

        public void ReplaceWithClone()
        {
            newKeys = newKeys.Select(k => k.Clone()).ToArray();
        }
    }

}
