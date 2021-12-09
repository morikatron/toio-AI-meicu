using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


namespace toio.AI.meicu
{
    public class PageLearn : MonoBehaviour
    {
        public GameObject ui;
        public UIBoard uiBoard;
        public UIQuest uiQuest;
        public Game game;
        public Text text;
        public Button btnNext;
        public Button btnBack;
        public Button btnHint;

        int phase = 0;
        MeiQuest quest = default;
        bool isHeatmapReceived = false;


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            if (active)
            {
                uiQuest.Reset();
                uiBoard.Reset();

                AIController.ins.heatmapCallback += OnHeatmap;
                AIController.ins.LoadBestModel();

                MeiPrefs.SetLearnCleared();
                phase = 0;
                Refresh();
            }
            else
            {
                StopAllCoroutines();
                AIController.ins.heatmapCallback -= OnHeatmap;

                PlayerController.ins.isPause = false;
                AIController.ins.isPause = false;
            }
        }

        internal void Pause()
        {

        }

        internal void Resume()
        {
            Refresh();
        }


        public void OnBtnHint()
        {

        }

        public void OnBtnNext()
        {
            if (phase == 26)
            {
                PageManager.OnBtnHome();
                return;
            }
            phase ++;

            Refresh();
        }

        public void OnBtnBack()
        {
            if (phase > 0)
                phase --;

            Refresh();
        }

        void OnHeatmap()
        {
            isHeatmapReceived = true;
        }


        internal void Refresh()
        {
            StopAllCoroutines();
            StartCoroutine(IE_Refresh());
        }

        IEnumerator IE_Refresh()
        {
            btnNext.interactable = false;
            btnBack.interactable = false;

            if (phase == 0)
            {
                text.text = "ここでは、僕が「どのように正しい道を見つけているのか」についてかいせつするよ。";
                btnHint.gameObject.SetActive(false);
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                ui.transform.Find("Illust").gameObject.SetActive(false);
                ui.transform.Find("Video").gameObject.SetActive(false);
                uiBoard.HideHeatmap();

                quest = new MeiQuest(4, 4, new Env.Space[]{Env.Space.R}, 3, 4);
                uiQuest.ShowQuest(quest);
                uiQuest.ShowA(0);
                uiBoard.ShowGoal(3, 4);
                uiBoard.ShowKomaA(4, 4);

            }
            else if (phase == 1)
            {
                text.text = "少し難しく言うと、「AIの考え方」「AIがどのように学習するか」についてだよ。";
            }
            else if (phase == 2)
            {
                text.text = "まず最初、僕は自分の位置と問題を見てみる。";
            }
            else if (phase == 3)
            {
                text.text = "でも、最初は「色」も「問題の意味」も分からないので、とりあえず上下左右のどこかに動いてみる。";
            }
            else if (phase == 4)
            {
                text.text = "つまり、最初は「てきとうに動いてみる」だけなんだ。";
            }
            else if (phase == 5)
            {
                text.text = "例えばこの問題の場合、キミはすぐに「上に動けばいい」ってわかるでしょ？";
            }
            else if (phase == 6)
            {
                text.text = "でも僕は、最初はそれすら分からない。\nだから、上下左右のどこにも動く可能性があるんだ。";

                uiBoard.HideHeatmap();
            }
            else if (phase == 7)
            {
                text.text = "明るさで表現すると、こんな感じ。\nどのマスも同じ明るさ、どこも同じだけ可能性があるってこと。";
                float[,] heatmap = new float[9, 9];
                heatmap[3, 4] = 0.25f; heatmap[5, 4] = 0.25f; heatmap[4, 3] = 0.25f; heatmap[4, 5] = 0.25f;
                uiBoard.ShowHeatmap(heatmap);
                ui.transform.Find("ImgThink").gameObject.SetActive(true);
            }
            else if (phase == 8)
            {
                text.text = "そして、ぐうぜん「上」に動いてゴールする事ができたら、「どうやら上が正しいみたいだぞ」って思うんだ。";
            }
            else if (phase == 9)
            {
                text.text = "そうすると、最初に「上」に動くことをどんどん選ぶようになってゆくんだ。";
            }

            else if (phase == 10)
            {
                text.text = "これを「試行錯誤（しこうさくご）」って言うんだ。\nじっさいに100回やってみるね！";
            }
            else if (phase == 11)
            {
                float[,] heatmap = new float[9, 9];
                float[] probs = new float[4];
                for (int i = 0; i < 4; i++) probs[i] = 0.25f;   // initial uniform prob dist.

                // Learning Iteration
                for (int t = 0; t < 100; t++)
                {
                    float interval = Mathf.Max( 1-(1f/15f)*Mathf.Min(15f, t) , 0.02f);

                    text.text = $"試行 {t+1} 回目\n\n\n\n";

                    // Restart
                    uiBoard.HideTrajA();
                    uiBoard.ShowKomaA(4, 4);
                    heatmap[3, 4] = probs[0]; heatmap[4, 5] = probs[1]; heatmap[5, 4] = probs[2]; heatmap[4, 3] = probs[3];
                    uiBoard.ShowHeatmap(heatmap);
                    yield return new WaitForSecondsRealtime(interval);

                    // Sample action
                    var action = SampleIdx(probs);
                    if (t == 0) action = 0;
                    if (t == 1) action = Random.Range(1, 3);

                    // Env step
                    (var r, var c) = Env.Translate((Env.Action)action, 4, 4);
                    var pos = new Vector2Int(r, c);

                    // Update GUI
                    uiBoard.ShowKomaA(pos);
                    uiBoard.ShowTrajA(new Vector2Int[]{pos});

                    // Learn
                    if (r == 3 && c == 4)   // Goal
                    {
                        uiQuest.ShowA(1);
                        for (int a = 0; a < 4; a++)
                        {
                            if (a == action) probs[a] += 0.009f;
                            else probs[a] -= 0.003f;
                        }

                        // Update text
                        if (t < 2)
                        {
                            text.text = $"試行 {t+1} 回目\n\n「上」に行ってみたらゴールに成功したね。この時にAIは「上に行けばゴールの可能性がアップする」と学習するんだ。";
                            interval += 3;
                        }
                        else
                        {
                            text.text = $"試行 {t+1} 回目\n\nゴール成功\n「上」の可能性  アップ\n";
                        }
                    }
                    else    // Fail
                    {
                        for (int a = 0; a < 4; a++)
                        {
                            if (a == action) probs[a] -= 0.009f;
                            else probs[a] += 0.003f;
                        }

                        // Update text
                        string actionStr = "";
                        if (action == 1) actionStr = "右";
                        if (action == 2) actionStr = "下";
                        if (action == 3) actionStr = "左";
                        if (t < 2)
                        {
                            text.text = $"試行 {t+1} 回目\n\n「{actionStr}」に行ってみたら今度はゴールに失敗したね。この時にAIは「左に行くとゴールの可能性がダウンする」と学習するんだ。";
                            interval += 3;
                        }
                        else
                        {
                            text.text = $"試行 {t+1} 回目\n\nゴール失敗\n「{actionStr}」の可能性 ダウン\n";
                        }
                    }

                    yield return new WaitForSecondsRealtime(interval);
                }
                text.text = "ほら「上」のマスが明るくなってきたでしょ？";
            }
            else if (phase == 12)
            {
                text.text = "これは、僕がまず「上のマスに移動するのが良さそうだ」って分かってきたって事なんだ。";
            }


            else if (phase == 13)
            {
                text.text = "今は最初の一歩だけ見せたけど、じっさいはこの先も一歩一歩、同じようにうまくいくかどうかをためしてるんだ。";

                // Back
                ui.transform.Find("Video").gameObject.SetActive(false);
                uiBoard.gameObject.SetActive(true);
                uiQuest.gameObject.SetActive(true);
            }
            else if (phase == 14)
            {
                // Back
                ui.transform.Find("Illust").gameObject.SetActive(false);

                // Hide Board etc.
                uiBoard.gameObject.SetActive(false);
                uiQuest.gameObject.SetActive(false);
                ui.transform.Find("ImgThink").gameObject.SetActive(false);

                // Show Video
                ui.transform.Find("Video").gameObject.SetActive(true);
                VideoPlayer vid = ui.transform.Find("Video").GetComponentInChildren<VideoPlayer>();
                vid.frame = 0;

                text.text = "長い問題で、正しい道を探す時はこんな感じ…";
                yield return new WaitForSecondsRealtime(1.5f);
                vid.Play();

                yield return new WaitUntil(() => vid.time > 3);
                text.text = "最初はぜんぜんゴールできない…";

                yield return new WaitUntil(() => vid.time > 7);
                text.text = "1000回くらいがんばってもまだまだ…";

                yield return new WaitUntil(() => vid.time > 14);
                text.text = "2000回くらいで、やっとゴールできはじめたよ！";

                yield return new WaitUntil(() => vid.time > 21);
                text.text = "3000回こえたら、ほとんどゴールできるようになってきたね！";

                yield return new WaitUntil(() => vid.time > 28);
                text.text = "ほぼカンペキにゴールできるようになるまで、僕は4000回くらいチャレンジしたんだよ！";

                yield return new WaitForSecondsRealtime(0.5f);
            }


            else if (phase == 15)
            {
                // Hide Video
                ui.transform.Find("Video").gameObject.SetActive(false);

                // Show Illust
                ui.transform.Find("Illust").gameObject.SetActive(true);

                text.text = "僕たちAIはコンピュータだから、最初から正しい道を知っているんじゃないかって思うかもしれないけど、";
            }
            else if (phase == 16)
            {
                text.text = "実は、こうやって何度も何度も失敗しながら、それでも繰り返しチャレンジして、やっと正解を見つけるんだ！";
            }
            else if (phase == 17)
            {
                text.text = "なんだかキミたち人間と似てるでしょ？";
            }
            else if (phase == 18)
            {
                text.text = "そして僕たちも、何度も何度も繰り返して学習するほど、強くかしこくなってゆくんだ！";
            }
            else if (phase == 19)
            {
                text.text = "でも…サボって学習しなければ弱いまま…";
            }
            else if (phase == 20)
            {
                // Back
                btnHint.gameObject.SetActive(false);
                ui.transform.Find("Illust").gameObject.SetActive(true);

                text.text = "これもキミたちと似ているね！";
            }

            else if (phase == 21)
            {
                // Hide Illust
                ui.transform.Find("Illust").gameObject.SetActive(false);

                // Show hint
                btnHint.gameObject.SetActive(true);

                text.text = "ちなみに、このボタンに気付いた？";
            }
            else if (phase == 22)
            {
                text.text = "このボタンは、僕の頭の中をこっそりのぞけるヒミツのボタン…";
            }
            else if (phase == 23)
            {
                text.text = "いま僕がどんな道を考えているか、見ることができるんだ。";
            }
            else if (phase == 24)
            {
                text.text = "もし、キミがどっちに進めばわからなくなった時は、こっそり見てみるといいかもね！";
            }
            else if (phase == 25)
            {
                text.text = "最初のころの僕はまだ弱いから…間違ったり、なかなか道が見えなかったりするけどね。";
            }
            else if (phase == 26)
            {
                text.text = "迷路バトルで僕に勝っていったら…\nどんどん強い、たくさん学習した僕と戦えるから、頑張ってみて！";
            }

            // else if (phase == 9)
            // {
            //     text.text = "こうして、少しづつ先の「選ぶマス」の可能性を考えてゆくと、だんだん道が浮かび上がってきたでしょ？";
            //     Env env = new Env();
            //     env.Reset(4, 4);
            //     uiBoard.ShowKomaA(4, 4);
            //     for (int i = 0; i < 10; i++)
            //     {
            //         // New Quest
            //         quest = env.GenerateQuest(Random.Range(4, 7));
            //         env.SetQuest(quest);

            //         uiQuest.ShowQuest(quest);
            //         uiBoard.ShowGoal(quest.goalRow, quest.goalCol);
            //         uiBoard.HideTrajA();
            //         uiBoard.ShowHeatmap();
            //         yield return new WaitForSecondsRealtime(0.1f);

            //         for (int step = 0; step < quest.Length*2-2; step++)
            //         {
            //             float t = Time.realtimeSinceStartup;

            //             // Get Heatmap for step
            //             isHeatmapReceived = false;
            //             yield return AIController.ins.PredictHeatmapOnce(env, step);
            //             yield return new WaitUntil(() => isHeatmapReceived);

            //             // Interval
            //             yield return new WaitForSecondsRealtime(Mathf.Max(0f, 1f - (Time.realtimeSinceStartup - t)));

            //             uiBoard.ShowHeatmap(AIController.ins.Heatmap);
            //         }
            //         // Interval
            //         yield return new WaitForSecondsRealtime(1f);

            //         if (i == 3)
            //         {
            //             btnNext.interactable = true;
            //             btnBack.interactable = true;
            //         }
            //     }
            // }

            yield return new WaitForSecondsRealtime(0.1f);
            btnNext.interactable = true;

            if (phase > 0)
                btnBack.interactable = true;
        }

        static int SampleIdx(float[] probs)
        {
            var p = Random.Range(0f, 1f);
            float cumu = 0;
            for (var i = 0; i < probs.Length; i++)
            {
                cumu += probs[i];
                if (p < cumu)
                {
                    return i;
                }
            }
            return probs.Length - 1;
        }
    }

}
