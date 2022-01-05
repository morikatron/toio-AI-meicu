# toio-AI-meicu



## 動作環境

### システム環境

> toio SDK for Unity 1.4.0 と一致しています。

- Unity（2020.3.17f1 LTS 推奨）
- Mac（macOS ver.10.14以上）
- Windows 10（64 ビット版のみ）
- iOS端末（iOS ver.12以上）
- Android端末（Android OS 9.0以上）

### ハードウェア

- toio™コア キューブ 2 台
- [「トイオ・コレクション」](https://toio.io/titles/toio-collection.html)付属プレイマット

### 依存パッケージ

- [toio SDK for Unity 1.4.0](https://github.com/morikatron/toio-sdk-for-unity)
- [ml-agents (Morikatron's Fork)](https://github.com/morikatron/ml-agents/tree/r18_additional_outputs)

<br>

## セットアップ

### 本レポジトリをクローン
### toio SDK for Unity 1.4.0 を導入
### ml-agents (Morikatron's Fork) を導入

元の ml-agents は Barracuda を利用し、Actor モデルの出力だけを取得しています。そして mlagents (python pacakge)で作られた Actor モデルの最終出力は、行動確率からサンプリングされた行動のインデックスになっています。行動確率を取得し迷キュー「思考」を可視化するために、ml-agents を簡単に改造しました。



<br>

## ビルド

ビルドの方法や注意事項は toio SDK for Unity の [【ドキュメント】](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.3.0/docs#-3-ビルド) に詳しく記載してありますので、そちらを参照してください。

<br>

## Unity プロジェクト構成

`Assests/meicu/Scenes/` の下に、２つのシーンがあります。`Game.unity` はゲームアプリ用のシーンで、`Train.unity` は迷キューを学習する用のシーンになります。

`Game.unity` のヒエラルキーの概要が以下になります。
- Simulator：ビルドしたアプリでは使われないが、エディタ上で動作確認する際に使えます
- Scripts：スクリプトがまとめて格納されています
  - UI：各画面を制御するスクリプトが格納されています
  - Controller
    - PlayerController：プレイヤーのキューブの状態に応じて、ゲーム操作とキューブ制御を行うスクリプト
    - AIController：迷キューのAIがキューブ状態とゲーム状態に応じて、ゲーム操作とキューブ制御を行うスクリプト
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