# BlendShapeCombiner

複数のブレンドシェイプを合成した新しいシェイプキーを追加し、新しいメッシュとして保存するUnityエディタ拡張です。

## 動作環境

Unity 2018.4 以降

## インポート手順

### unitypackageをインポートする方法

Release ページより `BlendShapeCombiner-vX.X.X.unitypackage` をダウンロードし、Unityにインポートする

### パッケージマネージャを用いる方法

1. インポート先プロジェクトの `Packages/manifest.json` をテキストエディタ等で開く
2. `dependencies` の配列内に以下の行を追加
   
   ```
   "com.github.chigirits.blendshapecombiner": "https://github.com/chigirits/BlendShapeCombiner.git",
   ```

## 使い方

1. メニュー/Chigiri/BlendShapeCombiner を選択<br>
   ![usage-01](https://user-images.githubusercontent.com/61717977/89705645-fe6cfa80-d999-11ea-9cb2-faeecdff5b43.png)
2. 操作対象のSkinnedMeshRender、合成元の複数シェイプキーの名前、合成結果となる新しいキー名を入力して Save As...<br>
   ![usage-02](https://user-images.githubusercontent.com/61717977/89705647-ff9e2780-d999-11ea-813a-8f23d24fbdc9.png)
3. 新しいメッシュのファイル名を指定して保存<br>
   ![usage-03](https://user-images.githubusercontent.com/61717977/89705648-00cf5480-d99a-11ea-9991-fdd23459b053.png)
4. SkinnedMeshRendererのメッシュを新しいものに差し替え、キーが増えていることを確認<br>
   ![usage-04](https://user-images.githubusercontent.com/61717977/89705650-0167eb00-d99a-11ea-9226-d0caa7bca331.png)
