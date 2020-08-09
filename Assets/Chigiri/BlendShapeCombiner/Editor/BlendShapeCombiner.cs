using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Chigiri.BlendShapeCombiner.Editor
{
    public class BlendShapeCombiner : ScriptableWizard
    {

        public SkinnedMeshRenderer target;
        public string[] sourceKeys = new string[] { "", "" };
        public string newKey;

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
            if (sourceKeys.Length < 2)
            {
                errorString = "Source Keys を2つ以上指定してください";
                isValid = false;
                return;
            }
            for (var i = 0; i < sourceKeys.Length; i++)
            {
                if (sourceKeys[i] == "")
                {
                    errorString = $"Source Keys の Element [{i}] を指定してください";
                    isValid = false;
                    return;
                }
                if (target.sharedMesh.GetBlendShapeIndex(sourceKeys[i]) < 0)
                {
                    errorString = $"Source Keys の Element [{i}] に指定されたシェイプキーは存在しません";
                    isValid = false;
                    return;
                }
            }
            if (newKey == "")
            {
                errorString = "New Key を指定してください";
                isValid = false;
                return;
            }
            if (0 <= target.sharedMesh.GetBlendShapeIndex(newKey))
            {
                errorString = "New Key に指定されたシェイプキーは定義済みです。新しい名前を指定してください";
                isValid = false;
                return;
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

        static Vector3[] AddVector3(Vector3[] src0, Vector3[] src1)
        {
            var result = new Vector3[src0.Length];
            for (int i=0; i<src0.Length; i++) result[i] = src0[i] + src1[i];
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

            var n = sourceKeys.Length;
            var newFrames = 0;
            for (var i = 0; i < n; i++)
            {
                int index = src.GetBlendShapeIndex(sourceKeys[i]);
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
                    int index = src.GetBlendShapeIndex(sourceKeys[i]);
                    weight += src.GetBlendShapeFrameWeight(index, frame);
                    baseMesh.GetBlendShapeFrameVertices(index, frame, tempVertices, tempNormals, tempTangents);
                    vertices = AddVector3(vertices, tempVertices);
                    normals = AddVector3(normals, tempNormals);
                    tangents = AddVector3(tangents, tempTangents);
                }
                ret.AddBlendShapeFrame(newKey, weight/n, vertices, normals, tangents);
            }
            return ret;
        }

    }
}
