using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chigiri.BlendShapeCombiner
{

    [System.Serializable]
    public class NewKey
    {
        public string name;
        public SourceKey[] sourceKeys = new SourceKey[0];
    }

}
