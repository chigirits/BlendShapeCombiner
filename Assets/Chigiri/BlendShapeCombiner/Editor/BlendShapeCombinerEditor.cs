using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Chigiri.BlendShapeCombiner.Editor
{

    [CustomEditor(typeof(BlendShapeCombiner))]
    public class BlendShapeCombinerEditor : UnityEditor.Editor
    {

        const int SCHEMA_VERSION = 1005;

        ReorderableList newKeysList;
        ReorderableList sourceKeysList;
        int newKeyIndexOfCurrentSourceKeysList = -1;
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

        SerializedProperty version
        {
            get { return serializedObject.FindProperty("version"); }
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

        SerializedProperty showExpertOptions
        {
            get { return serializedObject.FindProperty("showExpertOptions"); }
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

        SerializedProperty usePercentage
        {
            get { return serializedObject.FindProperty("usePercentage"); }
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

        float NewKeyHeightCallback(int i)
        {
            return linePitch;
        }

        void DrawNewKeyCallback(Rect rect, int i, bool isActive, bool isFocused)
        {
            var newKey = newKeys.GetArrayElementAtIndex(i);
            EditorGUI.LabelField(rect, new GUIContent(newKey.FindPropertyRelative("name").stringValue));
        }

        // SourceKeyDrawer に引き渡すUI関連のパラメータを更新
        void UpdateSourceKeyUIParams()
        {
            // for (int i = 0; i < self.newKeys.Length; i++)
            // {
            //     var newKey = self.newKeys[i];
            //     for (int j = 0; j < newKey.sourceKeys.Length; j++)
            //     {
            //         var key = newKey.sourceKeys[j];
            //     }
            // }
            // serializedObject.Update();
        }

        void PrepareNewKeysList()
        {
            if (newKeysList != null) return;

            newKeysList = new ReorderableList(serializedObject, newKeys);

            // reorderableList.drawElementBackgroundCallback = (Rect rect, int i, bool isActive, bool isFocused) => { };

            newKeysList.drawElementCallback = DrawNewKeyCallback;

            // reorderableList.drawFooterCallback = rect => { };
            newKeysList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "New Keys");
            newKeysList.elementHeightCallback = NewKeyHeightCallback;
            newKeysList.onAddCallback = list =>
            {
                // New Key を追加
                var n = 0 <= list.index ? list.index : newKeys.arraySize;
                newKeys.InsertArrayElementAtIndex(n);
                if (0 <= list.index) return;
                var newKey = newKeys.GetArrayElementAtIndex(n);
                var name = newKey.FindPropertyRelative("name");
                if (name.stringValue == "") name.stringValue = "new_key";
                var sourceKeys = newKey.FindPropertyRelative("sourceKeys");
                if (sourceKeys.arraySize == 0)
                {
                    sourceKeys.InsertArrayElementAtIndex(0);
                    var sourceKey = sourceKeys.GetArrayElementAtIndex(0);
                    sourceKey.FindPropertyRelative("scale").doubleValue = 1.0;
                    sourceKeys.InsertArrayElementAtIndex(1);
                }
            };
            // newKeysList.onAddDropdownCallback = (rect, list) => Debug.Log("onAddDropdown");
            // newKeysList.onCanAddCallback = list => true;
            // newKeysList.onCanRemoveCallback = list => 1 <= newKeys.arraySize;
            // newKeysList.onChangedCallback = list => Debug.Log("onChanged");
            // newKeysList.onMouseUpCallback = list => { };
            newKeysList.onRemoveCallback = list =>
            {
                newKeys.DeleteArrayElementAtIndex(list.index);
                if (newKeys.arraySize <= list.index) list.index--;
            };
            newKeysList.onReorderCallback = list => UpdateSourceKeyUIParams();
            newKeysList.onSelectCallback = list => UpdateSourceKeyUIParams();
        }

        float SourceKeyHeightCallback(int i)
        {
            return linePitch;
        }

        void DrawSourceKeyCallback(Rect rect, int i, bool isActive, bool isFocused)
        {
            var newKey = newKeys.GetArrayElementAtIndex(newKeyIndexOfCurrentSourceKeysList);
            var sourceKeys = newKey.FindPropertyRelative("sourceKeys");
            var sourceKey = sourceKeys.GetArrayElementAtIndex(i);
            EditorGUI.PropertyField(rect, sourceKey);
        }

        void PrepareSourceKeysList(int newKeyIndex)
        {
            if (newKeyIndex != newKeyIndexOfCurrentSourceKeysList)
            {
                sourceKeysList = null;
                newKeyIndexOfCurrentSourceKeysList = newKeyIndex;
            }
            if (sourceKeysList != null) return;

            var newKey = newKeys.GetArrayElementAtIndex(newKeyIndex);
            var sourceKeys = newKey.FindPropertyRelative("sourceKeys");
            sourceKeysList = new ReorderableList(sourceKeys.serializedObject, sourceKeys);

            // reorderableList.drawElementBackgroundCallback = (Rect rect, int i, bool isActive, bool isFocused) => { };

            sourceKeysList.drawElementCallback = DrawSourceKeyCallback;

            // reorderableList.drawFooterCallback = rect => { };
            sourceKeysList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Source Keys");
            sourceKeysList.elementHeightCallback = SourceKeyHeightCallback;
            sourceKeysList.onAddCallback = list =>
            {
                // Source Key を追加
                var n = 0 <= list.index ? list.index : sourceKeys.arraySize;
                sourceKeys.InsertArrayElementAtIndex(n);
                // var sourceKey = sourceKeys.GetArrayElementAtIndex(n);
                // var name = sourceKey.FindPropertyRelative("name");
                // if (name.stringValue == "") name.stringValue = "source_key";
                // sourceKey.FindPropertyRelative("scale").doubleValue = 1.0;
                // sourceKey.FindPropertyRelative("xSignBounds").intValue = 0;
            };
            // sourceKeysList.onAddDropdownCallback = (rect, list) => Debug.Log("onAddDropdown");
            // sourceKeysList.onCanAddCallback = list => true;
            // sourceKeysList.onCanRemoveCallback = list => 1 <= sourceKeys.arraySize;
            // sourceKeysList.onChangedCallback = list => Debug.Log("onChanged");
            // sourceKeysList.onMouseUpCallback = list => { };
            sourceKeysList.onRemoveCallback = list =>
            {
                sourceKeys.DeleteArrayElementAtIndex(list.index);
                if (sourceKeys.arraySize <= list.index) list.index--;
            };
            sourceKeysList.onReorderCallback = list => UpdateSourceKeyUIParams();
            sourceKeysList.onSelectCallback = list => UpdateSourceKeyUIParams();
        }

        void RecollectShapeKeys()
        {
            if (self.sourceMesh == null)
            {
                self._shapeKeys = new string[0];
                serializedObject.Update();
                return;
            }
            var n = self.sourceMesh.blendShapeCount;
            self._shapeKeys = new string[n];
            for (var i = 0; i < n; i++) self._shapeKeys[i] = self.sourceMesh.GetBlendShapeName(i);
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            // ブレンドシェイプの選択肢を収集
            if (self._shapeKeys == null)
            {
                RecollectShapeKeys();
                UpdateSourceKeyUIParams();
            }

            // 操作前の値を一部保持（比較用）
            var prevTargetRenderer = self.targetRenderer;
            var prevSourceMesh = self.sourceMesh;

            // 描画
            serializedObject.Update();
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                // リスト描画の準備
                PrepareNewKeysList();

                // UI描画

                EditorGUILayout.PropertyField(targetRenderer, new GUIContent("Target", "操作対象のSkinnedMeshRenderer"));
                EditorGUILayout.PropertyField(sourceMesh, new GUIContent("Source Mesh", "オリジナルのメッシュ"));
                EditorGUILayout.PropertyField(overwriteExistingKeys, new GUIContent("Overwrite Existing Keys", "既存の同名シェイプキーに上書きする"));
                EditorGUILayout.PropertyField(clearAllExistingKeys, new GUIContent("Clear All Existing Keys", "既存のシェイプキーをすべて削除する"));
                EditorGUILayout.PropertyField(useTextField, new GUIContent("Use Text Field", "シェイプキー選択UIをすべてテキスト入力欄で表示"));
                EditorGUILayout.PropertyField(usePercentage, new GUIContent("Use Percentage", "スケール係数を百分率で指定"));
                if (showExpertOptions.boolValue = EditorGUILayout.Foldout(showExpertOptions.boolValue, "Expert Options"))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(clearNormal, new GUIContent("Clear Normal", "法線をクリアする"));
                        EditorGUILayout.PropertyField(clearTangent, new GUIContent("Clear Tangent", "タンジェントをクリアする"));
                    }
                }

                using (new EditorGUI.DisabledGroupScope(sourceMesh.objectReferenceValue == null))
                {
                    if (newKeysList != null) newKeysList.DoLayoutList();
                }

                // 選択中の NewKey
                if (newKeysList != null && 0 <= newKeysList.index)
                {
                    EditorGUILayout.LabelField("Detail of selected New Key:");
                    var box = new GUIStyle(GUI.skin.window);
                    box.padding = new RectOffset(15, 15, 10, 10);
                    using (new EditorGUILayout.VerticalScope(box))
                    {
                        var i = newKeysList.index;
                        var newKey = newKeys.GetArrayElementAtIndex(i);
                        var sourceKeys = newKey.FindPropertyRelative("sourceKeys");
                        PrepareSourceKeysList(i);

                        EditorGUILayout.PropertyField(newKey.FindPropertyRelative("name"), new GUIContent("Name", "新しく作成するシェイプキーの名前"));
                        if (showExpertOptions.boolValue = EditorGUILayout.Foldout(showExpertOptions.boolValue, "Expert Options"))
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.PropertyField(newKey.FindPropertyRelative("forAnimation"), new GUIContent("For Animation", "アニメーション出力対象"));
                            }
                        }
                        if (sourceKeysList != null) sourceKeysList.DoLayoutList();
                    }
                }

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

                using (new EditorGUILayout.HorizontalScope())
                {
                    // Process And Save As... ボタン
                    using (new EditorGUI.DisabledGroupScope(validationError != ""))
                    {
                        if (GUILayout.Button(new GUIContent("Process And Save As...", "新しいメッシュを生成し、保存ダイアログを表示します。")))
                        {
                            CombinerImpl.Process(self);
                        }
                    }

                    // Revert Target ボタン
                    using (new EditorGUI.DisabledGroupScope(!isRevertTargetEnable))
                    {
                        if (GUILayout.Button(new GUIContent("Revert Target", "Target の SkinnedMeshRenderer にアタッチされていたメッシュを元に戻します。"))) RevertTarget();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    // Capture ボタン
                    using (new EditorGUI.DisabledGroupScope(targetRenderer.objectReferenceValue == null))
                    {
                        if (GUILayout.Button(new GUIContent("Capture", "Target の SkinnedMeshRenderer に設定されているシェイプキー値を合成して、新しいシェイプキーを作成します。"))) Capture();
                    }

                    // Extract ボタン
                    using (new EditorGUI.DisabledGroupScope(targetRenderer.objectReferenceValue == null || newKeysList.index < 0))
                    {
                        if (GUILayout.Button(new GUIContent("Extract", "選択中の新しいシェイプキーが含む元のシェイプキーの値を、Target の SkinnedMeshRenderer に書き戻します。"))) Extract();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    // Sort Sources ボタン
                    using (new EditorGUI.DisabledGroupScope(newKeysList.index < 0))
                    {
                        if (GUILayout.Button(new GUIContent("Sort Sources", "選択中の新しいシェイプキーの Source を名前順にソートします。"))) SortSources();
                    }

                    // Merge Down ボタン
                    using (new EditorGUI.DisabledGroupScope(newKeysList.index < 0 || newKeys.arraySize - 1 <= newKeysList.index))
                    {
                        if (GUILayout.Button(new GUIContent("Merge Down", "選択中の新しいシェイプキーを、その下の項目にマージします。"))) MergeDown();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(newKeysList.index < 0))
                    {
                        var p = usePercentage.boolValue;
                        var p3 = p ? "12.3%" : "0.123";
                        var p2 = p ? "12%" : "0.12";
                        if (GUILayout.Button(new GUIContent($"Round Scale (like {p3})", "選択中の新しいシェイプキーのスケール係数を、小数第3位（百分率で第1位）までになるよう四捨五入します。"))) RoundScale(1000.0);
                        if (GUILayout.Button(new GUIContent($"Round Scale (like {p2})", "選択中の新しいシェイプキーのスケール係数を、小数第2位（百分率で整数）までになるよう四捨五入します。"))) RoundScale(100.0);
                    }
                }

                if (showExpertOptions.boolValue = EditorGUILayout.Foldout(showExpertOptions.boolValue, "Expert Options"))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // Create Animation Clips ボタン
                            using (new EditorGUI.DisabledGroupScope(validationError != ""))
                            {
                                if (GUILayout.Button(new GUIContent("Create Animation Clips", "指定したフォルダ内に、各新規シェイプキーに対応するアニメーションクリップを生成します。"))) CreateAnimationClips();
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                serializedObject.ApplyModifiedProperties();
                if (check.changed)
                {
                    // 何らかの操作があったときに必要な処理
                    UpdateSourceKeyUIParams();
                    validationError = Helper.Chomp(Validate());
                }
            }

            // Target を変更したときに Source Mesh が空なら自動設定
            if (prevTargetRenderer != self.targetRenderer && self.targetRenderer != null && self.sourceMesh == null)
            {
                self.sourceMesh = self.targetRenderer.sharedMesh;
            }

            // Source Mesh を変更したときはブレンドシェイプ一覧を次回更新する
            if (prevSourceMesh != self.sourceMesh)
            {
                self._shapeKeys = null;
                if (self.newKeys.Length == 0)
                {
                    self.newKeys = new NewKey[1];
                    self.newKeys[0] = new NewKey
                    {
                        name = "new_key",
                        sourceKeys = new SourceKey[]
                        {
                            new SourceKey{ scale = 1.0 },
                            new SourceKey{ scale = 1.0 },
                        },
                    };
                }
            }
        }

        void Migrate()
        {
            if (SCHEMA_VERSION <= self.version) return;
            Debug.Log($"Migrate: version {self.version} -> {SCHEMA_VERSION}");
            self.ReplaceWithClone(self.version);
            self.version = SCHEMA_VERSION;
            serializedObject.Update();
            var j = JsonUtility.ToJson(self);
            Debug.Log($"JSON: {j}");
        }

        public void Awake()
        {
            Migrate();
            RecollectShapeKeys();
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
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); 
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
                    scale = (double)weight * 0.01,
                });
            }
            newKey.sourceKeys = sourceKeys.ToArray();
            self.newKeys = self.newKeys.Append(newKey).ToArray();
            UpdateSourceKeyUIParams();
            serializedObject.Update();
            EditorUtility.SetDirty(self);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); 
        }

        void Extract()
        {
            var i = newKeysList.index;
            if (i < 0) return;
            Undo.RecordObject(self.targetRenderer, "Extract (BlendShapeCombiner)");
            var mesh = self.targetRenderer.sharedMesh;
            var newKey = self.newKeys[i];
            foreach (var sourceKey in newKey.sourceKeys)
            {
                var j = mesh.GetBlendShapeIndex(sourceKey.name);
                self.targetRenderer.SetBlendShapeWeight(j, (float)(sourceKey.scale * 100.0));
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); 
        }

        void SortSources()
        {
            var i = newKeysList.index;
            if (i < 0) return;
            Undo.RecordObject(self, "Sort Sources (BlendShapeCombiner)");
            var newKey = self.newKeys[i];
            newKey.sourceKeys = newKey.sourceKeys.OrderBy(x => x.name).ToArray();
            serializedObject.Update();
            EditorUtility.SetDirty(self);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); 
        }

        void MergeDown()
        {
            var i = newKeysList.index;
            if (i < 0 || self.newKeys.Length - 1 <= i) return;
            Undo.RecordObject(self, "Merge Down (BlendShapeCombiner)");
            var src = self.newKeys[i];
            var dst = self.newKeys[i+1];
            dst.sourceKeys = src.sourceKeys.Concat(dst.sourceKeys).ToArray();
            dst.name = src.name + " + " + dst.name;
            var l = self.newKeys.ToList();
            l.RemoveAt(i);
            self.newKeys = l.ToArray();
            serializedObject.Update();
            EditorUtility.SetDirty(self);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); 
        }

        void RoundScale(double precision)
        {
            var i = newKeysList.index;
            if (i < 0) return;
            Undo.RecordObject(self, "Round Scale (BlendShapeCombiner)");
            var newKey = self.newKeys[i];
            foreach (var key in newKey.sourceKeys)
            {
                key.scale = Math.Round(key.scale * precision) / precision;
            }
            serializedObject.Update();
            EditorUtility.SetDirty(self);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); 
        }

        void CreateAnimationClips()
        {
            string dir = AssetDatabase.GetAssetPath(self.sourceMesh);
            dir = dir == "" ? "Assets" : Path.GetDirectoryName(dir);
            string path = EditorUtility.SaveFolderPanel("Save the animations into", dir, "");
            if (path.Length == 0) return;

            if (!path.StartsWith(Application.dataPath))
            {
                Debug.LogError("Invalid path: Path must be under " + Application.dataPath);
                return;
            }
            path = path.Replace(Application.dataPath, "Assets");

            var keys = new HashSet<string>();
            foreach (var newKey in self.newKeys)
            {
                if (!newKey.forAnimation) continue;
                keys.Add(newKey.name);
            }

            var objectPath = self.targetRenderer.gameObject.name;
            foreach (var newKey in self.newKeys)
            {
                if (!newKey.forAnimation) continue;
                var clip = new AnimationClip();
                foreach (var key in keys)
                {
                    var v = key == newKey.name ? 1f : 0f;
                    var curve = AnimationCurve.Linear(0f, v, 1f/60f, v);
                    clip.SetCurve(objectPath, typeof(SkinnedMeshRenderer), "blendShape." + key, curve);
                }
                AssetDatabase.CreateAsset(clip, Path.Combine(path, newKey.name + ".anim"));
            }

        }

    }
}
