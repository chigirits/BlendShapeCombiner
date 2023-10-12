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
        public bool forAnimation = true;
        public SourceKey[] sourceKeys = new SourceKey[0];

        public NewKey Clone(int oldVersion)
        {
            return new NewKey
            {
                name = this.name,
                forAnimation = this.forAnimation || oldVersion < 1005,
                sourceKeys = this.sourceKeys.Select(k => k.Clone(oldVersion)).ToArray(),
            };
        }
    }

}
