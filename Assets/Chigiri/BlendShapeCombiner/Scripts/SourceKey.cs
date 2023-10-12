using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chigiri.BlendShapeCombiner
{

    [System.Serializable]
    public class SourceKey
    {
        public string name;
        public double scale = 1.0;
        public int xSignBounds = 0;

        public SourceKey Clone()
        {
            return new SourceKey
            {
                name = this.name,
                scale = this.scale,
                xSignBounds = this.xSignBounds,
            };
        }
    }

}
