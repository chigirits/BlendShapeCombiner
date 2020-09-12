# BlendShapeCombiner

複数のブレンドシェイプを合成した新しいシェイプキーを追加し、新しいメッシュとして保存するUnityエディタ拡張です。

開発経緯としては、VRChat Avatars 3.0 のまばたき用シェイプキーが1つしか指定できない問題への対策が発端ですが、他の用途にも汎用的に用いることができます。

## 動作環境

Unity 2018.4 以降

## インポート手順

### unitypackageをインポートする方法

[Releasesページ](https://github.com/chigirits/BlendShapeCombiner/releases) より最新版の `BlendShapeCombiner-vX.X.X.unitypackage` をダウンロードし、Unityにインポートする

### パッケージマネージャを用いる方法

1. インポート先プロジェクトの `Packages/manifest.json` をテキストエディタ等で開く
2. `dependencies` の配列内に以下の行を追加
   
   ```
   "com.github.chigirits.blendshapecombiner": "https://github.com/chigirits/BlendShapeCombiner.git",
   ```

## 使い方

1. メニュー/Chigiri/BlendShapeCombiner を選択<br>
   ![usage-01](https://user-images.githubusercontent.com/61717977/89705645-fe6cfa80-d999-11ea-9cb2-faeecdff5b43.png)
2. 操作対象のSkinnedMeshRender、合成結果を登録する新しいキー名、合成元の複数シェイプキーの名前を入力して Save As...<br>
   ![usage-02](https://user-images.githubusercontent.com/61717977/92988188-1fc68680-f504-11ea-9142-ce4bbaaf41ed.png)
3. 新しいメッシュのファイル名を指定して保存<br>
   ![usage-03](https://user-images.githubusercontent.com/61717977/92988599-fe679980-f507-11ea-9df8-a3ab7c168848.png)
4. SkinnedMeshRendererのメッシュを新しいものに差し替え、キーが増えていることを確認<br>
   ![usage-04](https://user-images.githubusercontent.com/61717977/92988190-205f1d00-f504-11ea-8943-dac1a4711b3f.png)

## Tips

`scale` に `1` 以外を指定することで、シェイプキーのかかり具合を極端にしたり、逆方向にすることができます。

たとえば、瞳を小さくするシェイプキーの `scale` に `-1` を指定することで、瞳を大きくするシェイプキーを作成することができます。

![usage-05](https://user-images.githubusercontent.com/61717977/92988185-1b9a6900-f504-11ea-84c4-aa234108caa5.png)

![usage-06](https://user-images.githubusercontent.com/61717977/92988187-1e955980-f504-11ea-8262-2dfc5dea82d2.png)

他にも、たとえば「A:目を閉じる」と「B:目を閉じて口を開く」しかシェイプキーがないモデルに対して、Aの -1 スケール化シェイプキーをBに重ねがけすることで、口だけを開いた表情を作ることができるようになります。

## 備考

- [unity3d-jp/BlendShapeBuilder](https://github.com/unity3d-jp/BlendShapeBuilder) から一部コードを借用しています（[MIT License](https://github.com/unity3d-jp/BlendShapeBuilder/blob/master/LICENSE.txt)）。
- 本パッケージのライセンスも [MIT License](./LICENSE) です。
