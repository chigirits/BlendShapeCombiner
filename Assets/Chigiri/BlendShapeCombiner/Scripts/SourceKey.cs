using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chigiri.BlendShapeCombiner
{

    [System.Serializable]
    public class SourceKey
    {
        public string name;
        public float scale = 1.0f;
        public int xSignBounds = 0;

        public int _index;
        public string[] _nameSelector = new string[0];
        public bool _useTextField;
        public bool _isSelected;
    }

}
