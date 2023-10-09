﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace Chigiri.BlendShapeCombiner.Editor
{

    [CustomEditor(typeof(BlendShapeCombiner))]
    public class BlendShapeCombinerEditor : UnityEditor.Editor
    {

        ReorderableList reorderableList;
        string[] blendShapes;
        string validationError;

        [MenuItem("Chigiri/Create BlendShapeCombiner")]
        public static void CreateBlendShapeCombiner()
        {
            var path = AssetDatabase.GUIDToAssetPath("12c469a9f19e32744a18d7e7eefef715");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.SetAsLastSibling();
            Selection.activeGameObject = instance;
            Undo.RegisterCreatedObjectUndo(instance, "Create BlendShapeCombiner");
        }

        private void OnEnable()
        {
        }

        BlendShapeCombiner self
        {
            get { return target as BlendShapeCombiner; }
        }

        SerializedProperty newKeys
        {
            get { return serializedObject.FindProperty("newKeys"); }
        }

        SerializedProperty targetRenderer
        {
            get { return serializedObject.FindProperty("targetRenderer"); }
        }

        SerializedProperty sourceMesh
        {
            get { return serializedObject.FindProperty("sourceMesh"); }
        }

        SerializedProperty overwriteExistingKeys
        {
            get { return serializedObject.FindProperty("overwriteExistingKeys"); }
        }

        SerializedProperty clearAllExistingKeys
        {
            get { return serializedObject.FindProperty("clearAllExistingKeys"); }
        }

        SerializedProperty clearNormal
        {
            get { return serializedObject.FindProperty("clearNormal"); }
        }

        SerializedProperty clearTangent
        {
            get { return serializedObject.FindProperty("clearTangent"); }
        }

        SerializedProperty useTextField
        {
            get { return serializedObject.FindProperty("useTextField"); }
        }

        // Revert Target ボタンを有効にするときtrue
        bool isRevertTargetEnable
        {
            get
            {
                return targetRenderer.objectReferenceValue != null &&
                    sourceMesh.objectReferenceValue != null &&
                    0 < newKeys.arraySize;
            }
        }

        static float lineHeight
        {
            get
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        static float linePitch
        {
            get
            {
                return lineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        // リスト要素の高さ
        float ElementHeightCallback(int i)
        {
            var newKey = newKeys.GetArrayElementAtIndex(i);
            var n = newKey.FindPropertyRelative("sourceKeys").arraySize;
            return linePitch * (2 + n);
        }

        // リスト要素を描画
        void DrawElementCallback(Rect rect, int i, bool isActive, bool isFocused)
        {
            var orgLabelWidth = EditorGUIUtility.labelWidth;

            var newKey = newKeys.GetArrayElementAtIndex(i);
            var sourceKeys = newKey.FindPropertyRelative("sourceKeys");
            var r = new Rect(rect.x, rect.y, rect.width, lineHeight);

            // 各フィールドを描画
            EditorGUIUtility.labelWidth = 50f;
            EditorGUI.PropertyField(r, newKey.FindPropertyRelative("name"), new GUIContent("Name", "新しく作成するシェイプキーの名前"));
            r.y += linePitch;
            for (int j = 0; j < sourceKeys.arraySize; j++)
            {
                var sourceKey = sourceKeys.GetArrayElementAtIndex(j);
                EditorGUI.PropertyField(r, sourceKey);
                r.y += linePitch;
            }
            r.width = lineHeight;

            // + が押されたら Source Key を追加
            if (GUI.Button(r, EditorGUIUtility.TrIconContent("Toolbar Plus"), "RL FooterButton"))
            {
                sourceKeys.InsertArrayElementAtIndex(sourceKeys.arraySize);
            }

            // 削除ボタンが押された Source Key を削除
            for (int j = sourceKeys.arraySize - 1; 0 <= j; j--)
            {
                var toBeDeleted = sourceKeys.GetArrayElementAtIndex(j).FindPropertyRelative("_toBeDeleted");
                if (toBeDeleted.boolValue) sourceKeys.DeleteArrayElementAtIndex(j);
            }

            EditorGUIUtility.labelWidth = orgLabelWidth;
        }

        // SourceKeyDrawer に引き渡すUI関連のパラメータを更新
        void UpdateSourceKeyUIParams()
        {
            for (int i = 0; i < self.newKeys.Length; i++)
            {
                var newKey = self.newKeys[i];
                for (int j = 0; j < newKey.sourceKeys.Length; j++)
                {
                    var key = newKey.sourceKeys[j];
                    key._index = j;
                    key._nameSelector = self.useTextField ? null : blendShapes;
                    key._isDeletable = 2 <= newKey.sourceKeys.Length;
                    key._toBeDeleted = false;
                    key._useTextField = self.useTextField;
                    key._isSelected = reorderableList != null && i == reorderableList.index;
                }
            }
        }

        // リスト描画の準備
        void PrepareReordableList()
        {
            if (reorderableList != null) return;

            reorderableList = new ReorderableList(
                elements: self.newKeys,
                elementType: typeof(NewKey),
                draggable: true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true
            );

            // reorderableList.drawElementBackgroundCallback = (Rect rect, int i, bool isActive, bool isFocused) => { };

            reorderableList.drawElementCallback = DrawElementCallback;

            // reorderableList.drawFooterCallback = rect => { };
            reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "New Keys");
            reorderableList.elementHeightCallback = ElementHeightCallback;
            reorderableList.onAddCallback = list =>
            {
                // New Key を追加
                var n = newKeys.arraySize;
                newKeys.InsertArrayElementAtIndex(n);
                var newKey = newKeys.GetArrayElementAtIndex(n);
                var name = newKey.FindPropertyRelative("name");
                if (name.stringValue == "") name.stringValue = "new_key";
                var sourceKeys = newKey.FindPropertyRelative("sourceKeys");
                if (sourceKeys.arraySize == 0)
                {
                    sourceKeys.InsertArrayElementAtIndex(0);
                    var sourceKey = sourceKeys.GetArrayElementAtIndex(0);
                    sourceKey.FindPropertyRelative("scale").floatValue = 1f;
                    sourceKeys.InsertArrayElementAtIndex(1);
                }
            };
            // reorderableList.onAddDropdownCallback = (rect, list) => Debug.Log("onAddDropdown");
            // reorderableList.onCanAddCallback = list => true;
            // reorderableList.onCanRemoveCallback = list => true;
            // reorderableList.onChangedCallback = list => Debug.Log("onChanged");
            // reorderableList.onMouseUpCallback = list => { };
            reorderableList.onRemoveCallback = list =>
            {
                newKeys.DeleteArrayElementAtIndex(list.index);
                if (newKeys.arraySize <= list.index) list.index--;
            };
            reorderableList.onReorderCallback = list => UpdateSourceKeyUIParams();
            reorderableList.onSelectCallback = list => UpdateSourceKeyUIParams();
        }

        public override void OnInspectorGUI()
        {
            // ブレンドシェイプの選択肢を収集
            if (blendShapes == null)
            {
                var bs = new List<string>();
                if (self.sourceMesh != null)
                {
                    for (var i = 0; i < self.sourceMesh.blendShapeCount; i++)
                    {
                        bs.Add(self.sourceMesh.GetBlendShapeName(i));
                    }
                }
                blendShapes = bs.ToArray();
                UpdateSourceKeyUIParams();
            }

            // 操作前の値を一部保持（比較用）
            var prevTargetRenderer = self.targetRenderer;
            var prevSourceMesh = self.sourceMesh;

            // 描画
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            {
                // リスト描画の準備
                PrepareReordableList();

                // UI描画

                EditorGUILayout.PropertyField(targetRenderer, new GUIContent("Target", "操作対象のSkinnedMeshRenderer"));
                EditorGUILayout.PropertyField(sourceMesh, new GUIContent("Source Mesh", "オリジナルのメッシュ"));
                EditorGUILayout.PropertyField(overwriteExistingKeys, new GUIContent("Overwrite Existing Keys", "既存の同名シェイプキーに上書きする"));
                EditorGUILayout.PropertyField(clearAllExistingKeys, new GUIContent("Clear All Existing Keys", "既存のシェイプキーをすべて削除する"));
                EditorGUILayout.PropertyField(clearNormal, new GUIContent("Clear Normal", "法線をクリアする"));
                EditorGUILayout.PropertyField(clearTangent, new GUIContent("Clear Tangent", "タンジェントをクリアする"));
                EditorGUILayout.PropertyField(useTextField, new GUIContent("Use Text Field", "シェイプキー選択UIをすべてテキスト入力欄で表示"));

                EditorGUI.BeginDisabledGroup(sourceMesh.objectReferenceValue == null);
                reorderableList.DoLayoutList();
                EditorGUI.EndDisabledGroup();

                // エラー表示
                if (validationError != "")
                {
                    EditorGUILayout.HelpBox(validationError, MessageType.Error, true);
                }

                // Revert Target に関する注意
                if (isRevertTargetEnable)
                {
                    EditorGUILayout.HelpBox("Undo 時にメッシュが消えた場合は Revert Target ボタンを押してください。", MessageType.Info, true);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    // Process And Save As... ボタン
                    EditorGUI.BeginDisabledGroup(validationError != "");
                    if (GUILayout.Button(new GUIContent("Process And Save As...", "新しいメッシュを生成し、保存ダイアログを表示します。")))
                    {
                        CombinerImpl.Process(self);
                    }
                    EditorGUI.EndDisabledGroup();

                    // Revert Target ボタン
                    EditorGUI.BeginDisabledGroup(!isRevertTargetEnable);
                    if (GUILayout.Button(new GUIContent("Revert Target", "Target の SkinnedMeshRenderer にアタッチされていたメッシュを元に戻します。"))) RevertTarget();
                    EditorGUI.EndDisabledGroup();

                    // Capture ボタン
                    if (GUILayout.Button(new GUIContent("Capture", "Target の SkinnedMeshRenderer に設定されているシェイプキー値を合成して、新しいシェイプキーを作成します。"))) Capture();

                    // Extract ボタン
                    EditorGUI.BeginDisabledGroup(reorderableList.index < 0);
                    if (GUILayout.Button(new GUIContent("Extract", "選択中の新しいシェイプキーが含む元のシェイプキーの値を、Target の SkinnedMeshRenderer に書き戻します。"))) Extract();
                    EditorGUI.EndDisabledGroup();

                    // Sort Sources ボタン
                    EditorGUI.BeginDisabledGroup(reorderableList.index < 0);
                    if (GUILayout.Button(new GUIContent("Sort Sources", "選択中の新しいシェイプキーの Source を名前順にソートします。"))) SortSources();
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                // 何らかの操作があったときに必要な処理
                UpdateSourceKeyUIParams();
                validationError = Helper.Chomp(Validate());
            }

            // Target を変更したときに Source Mesh が空なら自動設定
            if (prevTargetRenderer != self.targetRenderer && self.targetRenderer != null && self.sourceMesh == null)
            {
                self.sourceMesh = self.targetRenderer.sharedMesh;
            }

            // Source Mesh を変更したときはブレンドシェイプ一覧を次回更新する
            if (prevSourceMesh != self.sourceMesh)
            {
                blendShapes = null;
                if (self.newKeys.Length == 0)
                {
                    self.newKeys = self.newKeys.Append(new NewKey
                    {
                        name = "new_key",
                        sourceKeys = new SourceKey[]
                        {
                            new SourceKey{ scale = 1f },
                            new SourceKey{ scale = 1f }
                        }
                    }).ToArray();
                }
            }
        }

        public void Awake()
        {
            UpdateSourceKeyUIParams();
            validationError = Helper.Chomp(Validate());
        }

        string Validate()
        {
            if (targetRenderer.objectReferenceValue == null)
            {
                return "Target を指定してください";
            }
            if (sourceMesh.objectReferenceValue == null || self.sourceMesh == null)
            {
                return "Source Mesh を指定してください";
            }
            if (newKeys.arraySize < 1 && !(clearAllExistingKeys.boolValue || clearNormal.boolValue || clearTangent.boolValue))
            {
                return "New Keys を1つ以上指定してください";
            }
            var names = new Dictionary<string, bool>();
            for (var j = 0; j < newKeys.arraySize; j++)
            {
                var newKey = newKeys.GetArrayElementAtIndex(j);
                var name = newKey.FindPropertyRelative("name").stringValue;
                if (name == "")
                {
                    return $"New Keys [{j}] ({name}) > Name に新しく作成するシェイプキーの名前を指定してください";
                }
                if (0 <= self.sourceMesh.GetBlendShapeIndex(name) && !(self.overwriteExistingKeys || self.clearAllExistingKeys))
                {
                    return $"New Keys [{j}] ({name}) > Name に指定されたシェイプキーは定義済みです。新しい名前を指定してください";
                }
                if (names.ContainsKey(name))
                {
                    return $"New Keys [{j}] ({name}) > Name に指定された名前は重複しています。異なる名前を指定してください";
                }
                names[name] = true;
                var sourceKeys = newKey.FindPropertyRelative("sourceKeys");
                if (sourceKeys.arraySize < 1)
                {
                    return $"New Keys [{j}] ({name}) の + ボタンを押してください";
                }
                for (var i = 0; i < sourceKeys.arraySize; i++)
                {
                    var key = sourceKeys.GetArrayElementAtIndex(i);
                    var n = key.FindPropertyRelative("name").stringValue;
                    if (n == "")
                    {
                        return $"New Keys [{j}] ({name}) > Source Key [{i}] にコピー元シェイプキーを指定してください";
                    }
                    if (self.sourceMesh.GetBlendShapeIndex(n) < 0)
                    {
                        return $"New Keys [{j}] ({name}) > Source Key [{i}] に指定されたシェイプキー ({n}) は存在しません";
                    }
                }
            }
            return "";
        }

        void RevertTarget()
        {
            Undo.RecordObject(self.targetRenderer, "Revert Target (BlendShapeCombiner)");
            self.targetRenderer.sharedMesh = self.sourceMesh;
        }

        void Capture()
        {
            Undo.RecordObject(self, "Capture (BlendShapeCombiner)");
            var mesh = self.targetRenderer.sharedMesh;
            var n = mesh.blendShapeCount;
            var newKey = new NewKey{};
            newKey.name = "captured_key";
            var sourceKeys = new List<SourceKey>();
            for (var i=0; i<n; i++)
            {
                var weight = self.targetRenderer.GetBlendShapeWeight(i);
                if (weight == 0) continue;
                sourceKeys.Add(new SourceKey{
                    name = mesh.GetBlendShapeName(i),
                    scale = weight * 0.01f,
                });
            }
            newKey.sourceKeys = sourceKeys.ToArray();
            self.newKeys = self.newKeys.Append(newKey).ToArray();
            UpdateSourceKeyUIParams();
        }

        void Extract()
        {
            var i = reorderableList.index;
            if (i < 0) return;
            Undo.RecordObject(self.targetRenderer, "Extract (BlendShapeCombiner)");
            var mesh = self.targetRenderer.sharedMesh;
            var newKey = self.newKeys[i];
            foreach (var sourceKey in newKey.sourceKeys)
            {
                var j = mesh.GetBlendShapeIndex(sourceKey.name);
                self.targetRenderer.SetBlendShapeWeight(j, sourceKey.scale * 100f);
            }
        }

        void SortSources()
        {
            var i = reorderableList.index;
            if (i < 0) return;
            var newKey = self.newKeys[i];
            newKey.sourceKeys = newKey.sourceKeys.OrderBy(x => x.name).ToArray();
        }

    }
}
