using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chigiri.BlendShapeCombiner
{

    [System.Serializable]
    public class NewKey
    {
        public string name;
        public List<SourceKey> sourceKeys = new List<SourceKey> {};
    }

}
