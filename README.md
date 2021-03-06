# toio-AI-meicu

toio SDK for Unityで開発したAI体験コンテンツ『toioのAIジム　AIロボ「迷キュー」に挑戦（ベータ版）』です。

ビルド済みのWebアプリおよびコンテンツ説明については以下のURLにアクセスしてください。
- [『toioのAIジム AIロボ「迷キュー」に挑戦』](https://morikatron.github.io/meicu/)
- [コンテンツ説明ページ](https://toio.io/blog/detail/20220511_meicu_release.html)


## 動作環境

当コンテンツをビルドするために必要となるものは以下の通りです。

### システム環境

> toio SDK for Unity v1.5.0 と一致しています。

- Unity（2021.3.0f1 LTS 推奨）
- Mac（macOS ver.10.14以上）
- Windows 10（64 ビット版のみ）

### ハードウェア

- toio™コア キューブ 2 台
- [「トイオ・コレクション」](https://toio.io/titles/toio-collection.html)付属プレイマット

### 依存パッケージ

- [toio SDK for Unity v1.5.0](https://github.com/morikatron/toio-sdk-for-unity)
- [ml-agents (Morikatron's fork)](https://github.com/morikatron/ml-agents/tree/r18_additional_outputs) （release 18 を改造したものとなります。）


## セットアップ

#### 本レポジトリをクローン

本レポジトリをクローンまたはダウンロードして、サブフォルダの `toio-AI-meicu` を Unity で開きます。

#### toio SDK for Unity v1.5.0 を導入

toio SDK for Unityの[【ドキュメント】](https://github.com/morikatron/toio-sdk-for-unity/blob/main/docs/download_sdk.md)を参考にしてください。

#### ml-agents (Morikatron's fork) を導入

> 元の ml-agents は Barracuda を利用し、Actor モデルの出力だけを取得しています。
そして mlagents (python pacakge)で作られた Actor モデルの最終出力は、
行動確率からサンプリングされた行動のインデックスになっています。
行動確率を取得し迷キューの「思考」を可視化するために、ml-agents を簡単に改造しました。

ml-agents (Morikatron's fork) の [r18_additional_outputs ブランチ](https://github.com/morikatron/ml-agents/tree/r18_additional_outputs) をダウンロードして、中の `com.unity.ml-agents` フォルダを丸ごと `toio-AI-meicu/Packages/` にコピー＆ペーストします。

学習を行う場合には、`mlagents (python package)` もインストールしてください。詳しくは ml-agents の [【ドキュメント】](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Installation.md#install-the-mlagents-python-package) を参照してください。


## ビルド

ビルドの方法や注意事項は toio SDK for Unity の [【ドキュメント】](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.3.0/docs#-3-ビルド) に詳しく記載してありますので、そちらを参照してください。


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
  - Video：かいせつ用の動画
  - Audio：BGMとSEを管理するスクリプト
- Canvas：各画面とUIの要素が格納されています、`Script/UI`と名称が対応しています。

`Train.unity` のヒエラルキーはシンプルです。
- Canvas：学習過程を表示するUI
- Script：学習用スクリプト


## 迷キューを学習させる

mlagents (python package) を利用してAIロボ「迷キュー」に学習をさせることができます。
詳しくは[【コチラ】](training/Readme.md) を参照してください。


## ライセンス

- [toio SDK for Unity](https://github.com/morikatron/toio-sdk-for-unity) (MIT License)
- [ml-agents](https://github.com/Unity-Technologies/ml-agents) (Apache License 2.0)
- [Noto Sans Japanese](https://fonts.google.com/noto/specimen/Noto+Sans+JP/about) (Open Font License)