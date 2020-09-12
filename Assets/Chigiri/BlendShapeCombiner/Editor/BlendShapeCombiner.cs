using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Chigiri.BlendShapeCombiner.Editor
{
    public class BlendShapeCombiner : ScriptableWizard
    {

        public SkinnedMeshRenderer target;
        public NewKey[] newKeys = new NewKey[]{
            new NewKey{
                name = "",
                sourceKeys = new SourceKey[] {
                    new SourceKey{ name = "", scale = 1.0f },
                    new SourceKey{ name = "", scale = 1.0f }
                }
            }
        };

        [System.Serializable]
        public class NewKey
        {
            public string name;
            public SourceKey[] sourceKeys;
        }

        [System.Serializable]
        public class SourceKey
        {
            public string name;
            public float scale = 1.0f;
        }

        [MenuItem("Chigiri/BlendShapeCombiner")]
        static void Open()
        {
            DisplayWizard<BlendShapeCombiner>("BlendShapeCombiner", "Save As...");
        }

        private void OnWizardUpdate()
        {
            if (target == null)
            {
                errorString = "対象となるSkinnedMeshRendererをTargetに指定してください";
                isValid = false;
                return;
            }
            if (target.sharedMesh == null)
            {
                errorString = "対象となるSkinnedMeshRendererに正常なメッシュがアタッチされていません";
                isValid = false;
                return;
            }
            if (newKeys.Length < 1)
            {
                errorString = "New Keys を1つ以上指定してください";
                isValid = false;
                return;
            }
            for (var j = 0; j < newKeys.Length; j++)
            {
                var newKey = newKeys[j];
                if (newKey.name == "")
                {
                    errorString = $"New Keys > Element [{j}] > Name に新しく作成するシェイプキーの名前を指定してください";
                    isValid = false;
                    return;
                }
                if (0 <= target.sharedMesh.GetBlendShapeIndex(newKey.name))
                {
                    errorString = $"New Keys > Element [{j}] > Name に指定されたシェイプキーは定義済みです。新しい名前を指定してください";
                    isValid = false;
                    return;
                }
                if (newKey.sourceKeys.Length < 1)
                {
                    errorString = $"New Keys > Element [{j}] > Source Keys を1つ以上指定してください";
                    isValid = false;
                    return;
                }
                for (var i = 0; i < newKey.sourceKeys.Length; i++)
                {
                    var key = newKey.sourceKeys[i];
                    if (key.name == "")
                    {
                        errorString = $"New Keys > Element [{j}] > Source Keys > Element [{i}] > Name にコピー元シェイプキーの名前を指定してください";
                        isValid = false;
                        return;
                    }
                    if (target.sharedMesh.GetBlendShapeIndex(key.name) < 0)
                    {
                        errorString = $"New Keys > Element [{j}] > Source Keys > Element [{i}] > Name に指定されたシェイプキーは存在しません";
                        isValid = false;
                        return;
                    }
                }
            }
            errorString = "";
            isValid = true;
        }

        static string SanitizeFileName(string name)
        {
            var reg = new Regex("[\\/:\\*\\?<>\\|\\\"]");
            return reg.Replace(name, "_");
        }

        Mesh getBaseMesh()
        {
            if (target == null)
            {
                Debug.LogError("Target is not selected");
                return null;
            }
            var baseMesh = target.sharedMesh;
            if (baseMesh == null)
            {
                Debug.LogError("Target has no valid mesh");
                return null;
            }
            return baseMesh;
        }

        static Vector3[] AddVector3(Vector3[] src0, Vector3[] src1, float scale)
        {
            var result = new Vector3[src0.Length];
            for (int i=0; i<src0.Length; i++) result[i] = src0[i] + src1[i] * scale;
            return result;
        }

        void OnWizardCreate()
        {
            if (!isValid) return;
            var baseMesh = getBaseMesh();
            var result = MergeBlendShapes();
            string path = EditorUtility.SaveFilePanel("Save the new mesh as", "Assets", SanitizeFileName(baseMesh.name), "asset");
            if (path.Length > 0)
            {
                var dataPath = Application.dataPath;
                if (!path.StartsWith(dataPath))
                {
                    Debug.LogError("Invalid path: Path must be under " + dataPath);
                }
                else
                {
                    path = path.Replace(dataPath, "Assets");
                    AssetDatabase.CreateAsset(result, path);
                    Debug.Log("Asset exported: " + path);
                }
            }
        }

        Mesh MergeBlendShapes()
        {
            var baseMesh = getBaseMesh();
            if (baseMesh == null) return null;

            Mesh ret = Instantiate(baseMesh);
            ret.name = baseMesh.name;
            var src = Instantiate(ret);

            foreach (var newKey in newKeys)
            {
                var n = newKey.sourceKeys.Length;
                var newFrames = 0;
                for (var i = 0; i < n; i++)
                {
                    var key = newKey.sourceKeys[i];
                    int index = src.GetBlendShapeIndex(key.name);
                    int numFrames = src.GetBlendShapeFrameCount(index);
                    if (i == 0 || numFrames < newFrames) newFrames = numFrames;
                }

                var tempVertices = new Vector3[baseMesh.vertexCount];
                var tempNormals = new Vector3[baseMesh.vertexCount];
                var tempTangents = new Vector3[baseMesh.vertexCount];

                for (var frame=0; frame<newFrames; frame++)
                {
                    float weight = 0.0f;
                    var vertices = new Vector3[baseMesh.vertexCount];
                    var normals = new Vector3[baseMesh.vertexCount];
                    var tangents = new Vector3[baseMesh.vertexCount];
                    for (var i=0; i<n; i++)
                    {
                        var key = newKey.sourceKeys[i];
                        int index = src.GetBlendShapeIndex(key.name);
                        weight += src.GetBlendShapeFrameWeight(index, frame);
                        baseMesh.GetBlendShapeFrameVertices(index, frame, tempVertices, tempNormals, tempTangents);
                        vertices = AddVector3(vertices, tempVertices, key.scale);
                        normals = AddVector3(normals, tempNormals, key.scale);
                        tangents = AddVector3(tangents, tempTangents, key.scale);
                    }
                    ret.AddBlendShapeFrame(newKey.name, weight/n, vertices, normals, tangents);
                }
            }
            return ret;
        }

    }
}
