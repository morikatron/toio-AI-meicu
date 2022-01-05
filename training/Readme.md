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
今回は長さ（色マスの数）が4固定の問題のみを対象とする簡単な設定（quest4.yaml）で試してみます。

```
mlagents-learn config\quest4.yaml --run-id=quest4
```
> mlagents-learn [学習設定yaml] --run-id=[今回の学習のIDを設定]

「[INFO] Listening on port 5004. Start training by pressing the Play button in the Unity Editor.」が出力されるまでお待ち下さい。

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

```
mlagents-learn config\quest4.yaml --env=env --run-id=quest4 --num-envs=8 --no-graphics
```
> mlagents-learn [学習設定yaml] --run-id=[今回の学習のIDを設定] --env=[学習環境所在のフォルダ] --num-envs=[プロセス数] --no-graphics

5. 問題がなければ、学習が始まって、ターミナルの方では学習の進捗が定期的に報告されます。

### 学習済みモデルの利用

#### Train.unity で確認

#### Game.unity に組み込む

