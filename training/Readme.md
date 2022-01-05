## 迷キューを学習する

mlagents (python package) を利用した学習は2つのやり方があります。
- Unity Editor と連動して学習を回す
  - シングルプロセスでエディタ自体の実行効率も高くないので学習が遅い
  - 設定が簡単、学習過程が眺められる
- 学習環境をバイナリにビルドして、バックグラウンドでバイナリを複数立ち上げて学習を回す
  - マルチプロセスでレンダリングも省けるので学習が早い

### Unity Editor で学習

1. Unityプロジェクトを開いて、学習用のシーン `Assests/meicu/Scenes/Train.unity` を開きます。

1. `training` フォルダの位置でターミナルを開きます。セットアップした python 環境に入ります。

1. ターミナルで以下のコマンドを実行します。<br>
今回は長さ（色マスの数）が4固定の問題のみを対象とする簡単な設定（quest4.yaml）で試してみます。<br>
「Start training by pressing the Play button in the Unity Editor.」が出力されるまでお待ち下さい。

> mlagents-learn [学習設定yaml] --run-id=[今回の学習のIDを設定]
```
mlagents-learn config\quest4.yaml --run-id=quest4
```

4. Unity Editor のプレイボタンを押して、学習を始めます。

1. 問題がなければ、Unity Editor 上迷キューが高速度で試行錯誤を始めて、ターミナルの方では学習の進捗が定期的に報告されます。

### バイナリで学習

> Windows での説明となります。

1. Unityプロジェクトを開いて、学習用のシーン `Assests/meicu/Scenes/Train.unity` を開きます。

1. `Train.unity` を Windows にビルドします。<br>
ビルドしたものは `training/env/` に置いておきます（exeファイルが `training/env/` の直下にあるように）。

1. `training` フォルダの位置でターミナルを開きます。セットアップした python 環境に入ります。

1. ターミナルで以下のコマンドを実行します。<br>
今回は長さ（色マスの数）が4固定の問題のみを対象とする簡単な設定（quest4.yaml）で試してみます。

> mlagents-learn [学習設定yaml] --run-id=[今回の学習のIDを設定] --env=[学習環境所在のフォルダ] --num-envs=[プロセス数] --no-graphics
```
mlagents-learn config\quest4.yaml --env=env --run-id=quest4 --num-envs=8 --no-graphics
```

5. 問題がなければ、学習が始まって、ターミナルの方では学習の進捗が定期的に報告されます。

<br>

## 学習済みモデルの利用

上記手順で学習したモデル（onnxファイル）は `training/results/` の下に `run-id` を名前としたフォルダに保存されます。

#### Train.unity で確認

1. onnx ファイルを Unity プロジェクトの Assets 下の任意の位置にコピーペーストします。

1. Unity Editor のヒエラルキーウィンドウでゲームオブジェクト `Script` を選択し、プロジェクトウィンドウから onnx ファイルを見つけて、インスペクタで表示された Behavior Parameters -> Model にドラッグ&ドロップします。

1. インスペクタで、Env Paras コンポネントの `Quest Max Scale` `Quest Min Scale` を学習設定と一致するように両方ともに 4 に設定します。

1. プレイボタンを押して、迷キューが問題を解けているかを確認します。

#### Game.unity に組み込む

ランタイムにモデルをロードするために、モデルファイルを Resources フォルダに移動します。
参考として現在アプリに使われているモデルは `Assets/meicu/Resources/meicu-models/` に置いてあります。

迷キューアプリは、レベルごとに異なるモデルを指定していますので、自作のモデルに入れ替えるには、[`Assets/meicu/Scripts/Config.cs`](https://github.com/morikatron/toio-AI-meicu/blob/develop/toio-AI-meicu/Assets/meicu/Scripts/Config.cs#L113) を開いて、レベルごとに設定されたモデルのパスを自作のモデルのパスに変更します。

モデルの構造（主にレイヤー数）が変更された場合、行動確率を正しく取得するには、対応のレイヤー名を設定し直す必要があります。
1. `Netron` という onnx モデルの構造を表示するツールをインストールします。[Barracuda ドキュメント](https://docs.unity3d.com/Packages/com.unity.barracuda@2.0/manual/VisualizingModel.html) を参照。
1. 自作のモデルをダブルクリックして Netron で表示します。
1. `Softmax` 層を見つけてクリックし、右側のプロパティで input の名前を確認します。（アプリ内蔵のモデルの場合は`40`になります）
1. Unity Editor のヒエラルキーで、Scripts -> Controller -> AI を選択し、インスペクタの Game Agent -> Additional Output Names の Element 0 に、名前を入れます。

> 注意：現在の実装では、全てのモデルのSoftmaxのinput名が統一しなければなりません。

<br>

## 学習設定の yaml ファイル

アルゴリズムのハイパーパラメータ設定は [ml-agents ドキュメント](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-Configuration-File.md) を参照してください。

`Train.unity` で定義された環境パラメータは3つとなります。
- `questMaxScale：`ランダムに生成されるお題の長さ（色マス数）の上限
- `questMinScale`：ランダムに生成されるお題の長さ（色マス数）の上限
- `colorReward`：お題を解けた報酬が1固定なのに対し、初期の学習を加速させるために、お題通りに色マスを1つ踏む度に追加の報酬を設定します。既定値が `0.1` です。
- `randomStart`（bool）：スタート位置がランダムか中央固定かを設定します。アプリで推論時は中央固定になりますが、ランダムで学習したほうが精度が良い場合もあります。既定値が `false` です。

例として、`config/quest4.yaml` では、以下の設定になっています。
```yaml
environment_parameters:
  questMaxScale: 4
  questMinScale: 4
```
