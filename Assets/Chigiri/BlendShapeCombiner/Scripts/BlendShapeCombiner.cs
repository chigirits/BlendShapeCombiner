using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public bool showExpertOptions;
        public bool overwriteExistingKeys;
        public bool clearAllExistingKeys;
        public bool clearNormal;
        public bool clearTangent;
        public bool useTextField;
        public bool usePercentage;
        public string dontClearRegex;
        public NewKey[] newKeys = new NewKey[0];

        [NonSerialized] public string[] _shapeKeys;

        public void ReplaceWithClone(int oldVersion)
        {
            newKeys = newKeys.Select(k => k.Clone(oldVersion)).ToArray();
        }

        static Regex SafeNewRegex(string pattern)
        {
            Regex rx = null;
            try
            {
                rx = new Regex(pattern);
            }
            catch (ArgumentException e)
            {
                rx = null;
            }
            return rx;
        }

        public Regex CreateDontClearRegex()
        {
            return SafeNewRegex(dontClearRegex);
        }

    }

}
