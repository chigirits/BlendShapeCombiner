# BlendShapeCombiner

複数のブレンドシェイプを合成した新しいシェイプキーを追加し、新しいメッシュとして保存するUnityエディタ拡張です。

係数を指定できるため、たとえば「瞳を小さくする」シェイプキーから「瞳を大きくする」シェイプキーを作ることも可能です。

開発経緯としては、VRChat Avatars 3.0 のまばたき用シェイプキーが1つしか指定できない問題への対策が発端ですが、他の用途にも汎用的に用いることができます。

## 動作環境

Unity 2018.4 以降

## インポート手順

### unitypackageをインポートする方法

[Releasesページ](https://github.com/chigirits/BlendShapeCombiner/releases) より最新版の `BlendShapeCombiner-vX.X.X.unitypackage` をダウンロードし、Unityにインポートする

### パッケージマネージャを用いる方法

1. インポート先プロジェクトの `Packages/manifest.json` をテキストエディタ等で開く
2. `dependencies` オブジェクト内に以下の要素を追加
   
   ```
   "com.github.chigirits.blendshapecombiner": "https://github.com/chigirits/BlendShapeCombiner.git",
   ```

## 使い方

1. シーンにモデルを配置してください。
2. 「メニュー/Chigiri/Create BlendShapeCombiner」を選択すると、ヒエラルキーのトップレベルに BlendShapeCombiner が配置されます。
3. BlendShapeCombiner の `Target` に、操作対象となる SkinnedMeshRenderer（アバターの表情に適用する場合、一般的には Body オブジェクト）を指定してください。<br>
   このとき、対象にアタッチされているメッシュが `Source Mesh` に自動的にセットされます。
   
   ![usage-01](https://user-images.githubusercontent.com/61717977/93739727-edafd580-fc23-11ea-983d-fd2c7836ebad.png)
4. `Name` に作成するシェイプキーの名前を入力し、`Source [X]` から合成元のシェイプキーを選択してください。
   
   - スケール係数（プルダウンの右にあるテキストボックスの数値）を `1` 以外にすることで、効果のかかり具合を調整することができます（後述の[スケール係数について](#スケール係数について)を参照）。
   - 合成元となるシェイプキーの数を増減するには、各 `Source [X]` の左にあるゴミ箱アイコンや左最下部にある `+` ボタンを押してください。
   - 作成するシェイプキー自体を増減するには、`New Keys` リストタブの右下にある `+` `-` を押してください（削除する際は事前に対象行を選択してください）。
   
   ![usage-02](https://user-images.githubusercontent.com/61717977/93739733-f2748980-fc23-11ea-9ff2-d3abe91057e0.png)
5. `Process And Save As...` ボタンを押して、生成された新しいメッシュを保存してください。
   
   ![usage-03](https://user-images.githubusercontent.com/61717977/93739737-f4d6e380-fc23-11ea-9118-d287db6226ec.png)

   保存が完了すると、`Target` の SkinnedMeshRenderer に新しいメッシュがアタッチされます。
   この差し替えられたメッシュに追加されている新しいシェイプキーの値を変更してみて、期待どおりの効果がかかることを確認してください。
   
   ![usage-04](https://user-images.githubusercontent.com/61717977/93739742-f7393d80-fc23-11ea-8686-1493f870986c.png)

## スケール係数について

スケール係数に `1` 以外を指定することで、シェイプキーのかかり具合を極端にしたり、逆方向にすることができます。

たとえば、瞳を小さくするシェイプキーの `scale` に `-1` を指定することで、瞳を大きくするシェイプキーを作成することができます。

![usage-05](https://user-images.githubusercontent.com/61717977/92988185-1b9a6900-f504-11ea-84c4-aa234108caa5.png)

![usage-06](https://user-images.githubusercontent.com/61717977/92988187-1e955980-f504-11ea-8262-2dfc5dea82d2.png)

他にも、たとえば「A:目を閉じる」と「B:目を閉じて口を開く」しかシェイプキーがないモデルに対して、Aの -1 スケール化シェイプキーをBに重ねがけすることで、口だけを開いた表情を作ることができるようになります。

## ライセンス

[MIT License](./LICENSE)

## 更新履歴

- v1.2.0
  - UIの改善
    - カスタムUI実装
    - Source Mesh を Target とは独立して保持し、リトライ時の手間を軽減
    - 保存ダイアログの初期ディレクトリを Source Mesh と同一に
    - Revert Target ボタンを追加
    - ツールチップ表示
- v1.1.0
  - スケール係数プロパティを追加
  - 複数シェイプキーの一括追加に対応
- v1.0.0
  - 初回リリース
