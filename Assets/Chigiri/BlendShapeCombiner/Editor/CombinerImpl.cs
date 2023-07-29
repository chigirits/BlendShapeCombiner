﻿using UnityEngine;
using UnityEditor;
using System.IO;

namespace Chigiri.BlendShapeCombiner.Editor
{
    public class CombinerImpl
    {

        public static void Process(BlendShapeCombiner p)
        {
            var resultMesh = MergeBlendShapes(p);
            if (resultMesh == null) return;
            resultMesh.name = p.sourceMesh.name + ".BlendShapeAdded";

            // 保存ダイアログを表示
            string dir = AssetDatabase.GetAssetPath(p.sourceMesh);
            dir = dir == "" ? "Assets" : Path.GetDirectoryName(dir);
            string path = EditorUtility.SaveFilePanel("Save the new mesh as", dir, Helper.SanitizeFileName(resultMesh.name), "asset");
            if (path.Length == 0) return;

            // 保存
            if (!path.StartsWith(Application.dataPath))
            {
                Debug.LogError("Invalid path: Path must be under " + Application.dataPath);
                return;
            }
            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.CreateAsset(resultMesh, path);
            Debug.Log("Asset exported: " + path);

            // Targetのメッシュを差し替えてシェイプキーのウェイトを設定
            Undo.RecordObject(p.targetRenderer, "Process (BlendShapeCombiner)");
            p.targetRenderer.sharedMesh = resultMesh;
            // Selection.activeGameObject = self.targetRenderer.gameObject;
        }

        static Mesh MergeBlendShapes(BlendShapeCombiner p)
        {
            var source = p.sourceMesh;
            var nVertex = source.vertexCount;
            var nSubMesh = source.subMeshCount;
            Mesh ret = Object.Instantiate(source);
            ret.name = source.name;
            var src = Object.Instantiate(ret);
            if (p.clearAllExistingKeys)
            {
                ret.ClearBlendShapes();
            }
            else
            if (p.overwriteExistingKeys)
            {
                ret.ClearBlendShapes();
                var n = source.blendShapeCount;
                for (var i = 0; i < n; i++)
                {
                    var key = source.GetBlendShapeName(i);
                    var skip = false;
                    foreach (var newKey in p.newKeys)
                    {
                        if (newKey.name == key) skip = true;
                    }
                    if (skip) continue;
                    int numFrames = source.GetBlendShapeFrameCount(i);
                    var vertices = new Vector3[nVertex];
                    var normals = new Vector3[nVertex];
                    var normalsSetTo = p.clearNormal ? new Vector3[nVertex] : normals;
                    var tangents = new Vector3[nVertex];
                    var tangentsSetTo = p.clearTangent ? new Vector3[nVertex] : tangents;
                    for (var frame = 0; frame < numFrames; frame++)
                    {
                        source.GetBlendShapeFrameVertices(i, frame, vertices, normalsSetTo, tangentsSetTo);
                        var weight = source.GetBlendShapeFrameWeight(i, frame);
                        ret.AddBlendShapeFrame(key, weight, vertices, normals, tangents);
                    }
                }
            }

            var sourceVertices = Helper.GetPosedVertices(p.targetRenderer, source);

            foreach (var newKey in p.newKeys)
            {
                var n = newKey.sourceKeys.Count;
                var newFrames = 0;
                for (var i = 0; i < n; i++)
                {
                    var key = newKey.sourceKeys[i];
                    int index = src.GetBlendShapeIndex(key.name);
                    int numFrames = src.GetBlendShapeFrameCount(index);
                    if (i == 0 || numFrames < newFrames) newFrames = numFrames;
                }

                var tempVertices = new Vector3[nVertex];
                var tempNormals = new Vector3[nVertex];
                var tempTangents = new Vector3[nVertex];

                for (var frame = 0; frame < newFrames; frame++)
                {
                    float weight = 0.0f;
                    var vertices = new Vector3[nVertex];
                    var normals = new Vector3[nVertex];
                    var tangents = new Vector3[nVertex];
                    for (var i = 0; i < n; i++)
                    {
                        var key = newKey.sourceKeys[i];
                        int index = src.GetBlendShapeIndex(key.name);

                        var scale = new float[nVertex];
                        for (var j = 0; j < nVertex; j++)
                        {
                            var v = sourceVertices[j];
                            var xb = key.xSignBounds;
                            var includes = xb==0 || xb<0 && v.x<0 || 0<xb && 0<v.x;
                            scale[j] = includes ? key.scale : 0f;
                        }

                        weight += src.GetBlendShapeFrameWeight(index, frame);
                        source.GetBlendShapeFrameVertices(index, frame, tempVertices, tempNormals, tempTangents);
                        vertices = Helper.AddVector3(vertices, tempVertices, scale);
                        if (!p.clearNormal) normals = Helper.AddVector3(normals, tempNormals, scale);
                        if (!p.clearTangent) tangents = Helper.AddVector3(tangents, tempTangents, scale);
                    }
                    ret.AddBlendShapeFrame(newKey.name, weight/n, vertices, normals, tangents);
                }
            }
            return ret;
        }

    }
}
