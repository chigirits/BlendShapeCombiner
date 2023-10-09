using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Chigiri.BlendShapeCombiner.Editor
{

    [CustomPropertyDrawer(typeof(SourceKey))]
    public class SourceKeyDrawer : PropertyDrawer
    {

        void DrawNameSelector(Rect position, SerializedProperty property, BlendShapeCombiner root)
        {
            var label = "";
            var tooltip = "合成元となるシェイプキーの名前";
            var name = property.FindPropertyRelative("name");
            if (!root.useTextField)
            {
                var shapeKeys = root._shapeKeys;
                if (shapeKeys != null && 0 < shapeKeys.Length)
                {
                    var selected = -1;
                    for (var i = 0; i < shapeKeys.Length; i++)
                    {
                        if (shapeKeys[i] != name.stringValue) continue;
                        selected = i;
                        break;
                    }
                    if (name.stringValue == "") selected = 0;
                    if (0 <= selected)
                    {
                        selected = EditorGUI.Popup(position, label, selected, shapeKeys);
                        name.stringValue = shapeKeys[selected];
                        return;
                    }
                }
            }
            EditorGUI.PropertyField(position, name, new GUIContent(label, tooltip));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var root = property.serializedObject.targetObject as BlendShapeCombiner;
            var orgLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0;
            var rects = Helper.SplitRect(position, false, -1f, 40f, 16f, 40f);
            var r = 0;
            var style = new GUIStyle {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };
            var numStyle = EditorStyles.numberField;
            numStyle.margin = new RectOffset { };
            numStyle.padding = new RectOffset { };

            DrawNameSelector(rects[r++], property, root);

            var xSignBounds = property.FindPropertyRelative("xSignBounds");
            xSignBounds.intValue = EditorGUI.Popup(rects[r++], "", xSignBounds.intValue+1, new string[]{"L","LR","R"}) - 1;

            EditorGUI.LabelField(rects[r++], "", " x", style);

            var p = root.usePercentage ? 100f : 1f;
            var scale = property.FindPropertyRelative("scale");
            scale.floatValue = EditorGUI.FloatField(rects[r++], scale.floatValue * p, numStyle) / p;

            EditorGUIUtility.labelWidth = orgLabelWidth;
        }

    }

}
