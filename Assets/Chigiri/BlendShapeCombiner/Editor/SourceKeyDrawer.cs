using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Chigiri.BlendShapeCombiner.Editor
{

    [CustomPropertyDrawer(typeof(SourceKey))]
    public class SourceKeyDrawer : PropertyDrawer
    {

        void DrawNameSelector(Rect position, SerializedProperty property)
        {
            var label = "";
            var tooltip = "合成元となるシェイプキーの名前";
            var name = property.FindPropertyRelative("name");
            var isSelected = property.FindPropertyRelative("_isSelected").boolValue;
            var useTextField = property.FindPropertyRelative("_useTextField").boolValue;
            if (!useTextField)
            {
                if (!isSelected)
                {
                    EditorGUI.LabelField(position, new GUIContent(name.stringValue), new GUIContent(label, tooltip));
                    return;
                }
                var selector = property.FindPropertyRelative("_nameSelector");
                if (selector.isArray && 0 < selector.arraySize)
                {
                    var options = new string[selector.arraySize];
                    var selected = -1;
                    for (var i = 0; i < selector.arraySize; i++)
                    {
                        options[i] = selector.GetArrayElementAtIndex(i).stringValue;
                        if (options[i] == name.stringValue) selected = i;
                    }
                    if (name.stringValue == "") selected = 0;
                    if (0 <= selected)
                    {
                        selected = EditorGUI.Popup(position, label, selected, options);
                        name.stringValue = options[selected];
                        return;
                    }
                }
            }
            EditorGUI.PropertyField(position, name, new GUIContent(label, tooltip));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var orgLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0;
            var rects = Helper.SplitRect(position, false, 16f, -1f, 40f, 16f, 40f);
            var r = 0;
            var style = new GUIStyle {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };
            var numStyle = EditorStyles.numberField;
            numStyle.margin = new RectOffset { };
            numStyle.padding = new RectOffset { };
            var index = property.FindPropertyRelative("_index");

            EditorGUI.LabelField(rects[r++], "", $"{index.intValue}", style);

            DrawNameSelector(rects[r++], property);

            var xSignBounds = property.FindPropertyRelative("xSignBounds");
            xSignBounds.intValue = EditorGUI.Popup(rects[r++], "", xSignBounds.intValue+1, new string[]{"L","LR","R"}) - 1;

            EditorGUI.LabelField(rects[r++], "", " x", style);

            var scale = property.FindPropertyRelative("scale");
            scale.floatValue = EditorGUI.FloatField(rects[r++], scale.floatValue, numStyle);

            EditorGUIUtility.labelWidth = orgLabelWidth;
        }

    }

}
