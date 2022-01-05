# toio-AI-meicu



## 動作環境

### システム環境

> toio SDK for Unity v1.4.0 と一致しています。

- Unity（2020.3.17f1 LTS 推奨）
- Mac（macOS ver.10.14以上）
- Windows 10（64 ビット版のみ）
- iOS端末（iOS ver.12以上）
- Android端末（Android OS 9.0以上）

### ハードウェア

- toio™コア キューブ 2 台
- [「トイオ・コレクション」](https://toio.io/titles/toio-collection.html)付属プレイマット

### 依存パッケージ

- [toio SDK for Unity v1.4.0](https://github.com/morikatron/toio-sdk-for-unity)
- [ml-agents (Morikatron's fork) release 18](https://github.com/morikatron/ml-agents/tree/r18_additional_outputs)

<br>

## セットアップ

#### 本レポジトリをクローン

本レポジトリをクローンまたはダウンロードして、サブフォルダの `toio-AI-meicu` を Unity で開きます。

#### toio SDK for Unity v1.4.0 を導入

[toio SDK for Unity v1.4.0](https://github.com/morikatron/toio-sdk-for-unity/releases/tag/v1.4.0) の unitypackage をダウンロードして、Unity にドラッグ&ドロップします。

#### ml-agents (Morikatron's fork) を導入

> 元の ml-agents は Barracuda を利用し、Actor モデルの出力だけを取得しています。
そして mlagents (python pacakge)で作られた Actor モデルの最終出力は、
行動確率からサンプリングされた行動のインデックスになっています。
行動確率を取得し迷キューの「思考」を可視化するために、ml-agents を簡単に改造しました。

ml-agents (Morikatron's fork) の [r18_additional_outputs ブランチ](https://github.com/morikatron/ml-agents/tree/r18_additional_outputs) をダウンロードして、中の `com.unity.ml-agents` フォルダを丸ごと `toio-AI-meicu/Packages/` にコピーペーストします。

学習を行う場合には、`mlagents (python package)` をもインストールしてください。詳しくは ml-agents の [【ドキュメント】](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Installation.md#install-the-mlagents-python-package) を参照してください。

<br>

## ビルド

ビルドの方法や注意事項は toio SDK for Unity の [【ドキュメント】](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.3.0/docs#-3-ビルド) に詳しく記載してありますので、そちらを参照してください。

<b

## Unity プロジェクト構成

`Assests/meicu/Scenes/` の下に、２つのシーンがあります。`Game.unity` はゲームアプリ用のシーンで、`Train.unity` は迷キューを学習する用のシーンになります。

`Game.unity` のヒエラルキーの概要が以下になります。
- Simulator：ビルドしたアプリでは使われないが、エディタ上で動作確認する際に使えます
- Scripts：スクリプトがまとめて格納されています
  - UI：各画面を制御するスクリプトが格納されています
  - Controller
    - Player：プレイヤーのキューブの状態に応じて、ゲーム操作とキューブ制御を行うスクリプト
    - AI：迷キューのAIがキューブ状態とゲーム状態に応じて、ゲーム操作とキューブ制御を行うスクリプト
  - Game：ゲームコード
  - Video：かいせつ用の動画が格納されています
  - Audio：BGMとSEを管理するスクリプト
- Canvas：各画面とUIの要素が格納されています、`Script/UI`と名称が対応しています。

`Train.unity` のヒエラルキーはシンプルです。
- Canvas：学習過程を表示するUI
- Script：学習用スクリプト


<br>

## 迷キューを学習

[【コチラ】](training/Readme.md) を参照してください。

<br>

## ライセンス


### Third Party Notices

- [ml-agents](https://github.com/Unity-Technologies/ml-agents) (Apache License 2.0)
- [Noto Sans Japanese](https://fonts.google.com/noto/specimen/Noto+Sans+JP/about) (Open Font License)