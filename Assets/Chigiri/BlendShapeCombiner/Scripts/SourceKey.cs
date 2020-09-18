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

        public int _index;
        public List<string> _nameSelector = new List<string>();
        public bool _isDeletable;
        public bool _toBeDeleted;
    }

}
