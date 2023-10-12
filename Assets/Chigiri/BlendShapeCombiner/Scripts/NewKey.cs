using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chigiri.BlendShapeCombiner
{

    [System.Serializable]
    public class NewKey
    {
        public string name;
        public SourceKey[] sourceKeys = new SourceKey[0];

        public NewKey Clone()
        {
            return new NewKey
            {
                name = this.name,
                sourceKeys = this.sourceKeys.Select(k => k.Clone()).ToArray(),
            };
        }
    }

}
