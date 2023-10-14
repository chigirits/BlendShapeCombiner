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
        public string groupByRegex;
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

        public Regex CreateGroupByRegex()
        {
            return SafeNewRegex(groupByRegex);
        }

        public Regex CreateDontClearRegex()
        {
            return SafeNewRegex(dontClearRegex);
        }

        public string[] CollectGroups()
        {
            var groups = new HashSet<string>();
            groups.Add("");
            var rx = CreateGroupByRegex();
            foreach (var newKey in newKeys)
            {
                if (newKey.bakeIntoBase || !newKey.forAnimation) continue;
                foreach (var sourceKey in newKey.sourceKeys)
                    groups.Add(GuessGroup(rx, sourceKey.name));
            }
            return groups.ToArray();
        }

        public Dictionary<string, string> CollectGroupMap()
        {
            var map = new Dictionary<string, string>();
            var rx = CreateGroupByRegex();
            foreach (var newKey in newKeys)
            {
                if (newKey.bakeIntoBase || !newKey.forAnimation) continue;
                foreach (var sourceKey in newKey.sourceKeys)
                {
                    if (map.ContainsKey(sourceKey.name)) continue;
                    map[sourceKey.name] = GuessGroup(rx, sourceKey.name);
                }
            }
            return map;
        }

        public string[] CollectAnimationKeys()
        {
            var keys = new HashSet<string>();
            foreach (var newKey in newKeys)
            {
                if (newKey.bakeIntoBase || !newKey.forAnimation) continue;
                keys.Add(newKey.name);
            }
            return keys.ToArray();
        }

        public string GuessGroup(Regex rx, string name)
        {
            if (rx == null) return "";
            var match = rx.Match(name);
            if (!match.Success) return "";
            for (var k = 0; k < match.Groups.Count; k++)
            {
                var group = match.Groups[k];
                int dummy;
                if (int.TryParse(group.Name, out dummy)) continue;
                return group.Name;
            }
            return "";
        }

    }

}
