using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
            if (phase == 14)
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
                text.text = "僕が迷路をどう解いているのか、「AIの考え方」と「AIがどう学習したか」を教えるね。";
                btnBack.gameObject.SetActive(false);
                btnHint.gameObject.SetActive(false);
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 1)
            {
                text.text = "僕は自分の位置と問題を見て、まずは上下左右のどこかに移動してみるんだ。";
                btnBack.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 2)
            {
                text.text = "でも僕は、最初は「色」も「問題の意味」も分からないので、てきとうに動いてみるだけなんだ。";
                uiQuest.Reset();
                uiBoard.Reset();
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 3)
            {
                text.text = "例えばこの問題の場合、キミはひとめ見ただけで、上の赤色に行けばいいかなと分かるでしょ？だから「まずはコッチかな？」って考えて動かしてみることができる。";
                quest = new MeiQuest(4, 4, new Env.Space[]{Env.Space.R}, 3, 4);
                uiQuest.ShowQuest(quest);
                uiQuest.ShowA(0);
                uiBoard.ShowGoal(3, 4);
                uiBoard.ShowKomaA(4, 4);
                uiBoard.HideHeatmap();
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 4)
            {
                text.text = "でも僕はわからないので、上下左右のどこにも動く可能性がある。僕の動く可能性があるところは、明るさで表すとこんな感じ。どこもいっしょだね。";
                float[,] heatmap = new float[9, 9];
                heatmap[3, 4] = 0.25f; heatmap[5, 4] = 0.25f; heatmap[4, 3] = 0.25f; heatmap[4, 5] = 0.25f;
                uiBoard.ShowHeatmap(heatmap);
                ui.transform.Find("ImgThink").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }

            else if (phase == 5)
            {
                text.text = "そして、ぐうぜん「上」に動いた結果ゴールする事ができたら、「どうやら上が正しいみたいだぞ」って思うんだ。そうすると、最初に「上」に動くことをどんどん選ぶようになっていくんだ。";
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 6)
            {
                text.text = "これを「試行錯誤（しこうさくご）」って言うんだ。じっさいに100回やってみるね！";
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 7)
            {
                float[,] heatmap = new float[9, 9];
                float[] probs = new float[4];
                for (int i = 0; i < 4; i++) probs[i] = 0.25f;   // initial uniform prob dist.

                // Learning Iteration
                for (int t = 0; t < 100; t++)
                {
                    float interval = Mathf.Max( 1-(1f/15f)*Mathf.Min(15f, t) , 0.05f);

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
                            interval += 2;
                        }
                        else
                        {
                            text.text = $"試行 {t+1} 回目\n\nゴール成功\n「上」の可能性  アップ";
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
                            interval += 2;
                        }
                        else
                        {
                            text.text = $"試行 {t+1} 回目\n\nゴール失敗\n「{actionStr}」の可能性 DOWN";
                        }
                    }

                    yield return new WaitForSecondsRealtime(interval);
                }
                text.text = "ほら「上」のマスが明るくなってきたでしょ？これは僕がまず「上のマスに移動するのが良さそうだ」ってわかってきたって事なんだ。";
                yield return new WaitForSecondsRealtime(1f);
            }
            else if (phase == 8)
            {
                text.text = "今は最初の一歩だけ見せたけど、じっさいはこの先も一歩一歩、同じようにうまくいくかどうかをためしてるんだ。";
            }
            else if (phase == 9)
            {
                text.text = "こうして、少しづつ先の「選ぶマス」の可能性を考えてゆくと、だんだん道が浮かび上がってきたでしょ？";
                Env env = new Env();
                env.Reset(4, 4);
                uiBoard.ShowKomaA(4, 4);
                for (int i = 0; i < 10; i++)
                {
                    // New Quest
                    quest = env.GenerateQuest(Random.Range(4, 7));
                    env.SetQuest(quest);

                    uiQuest.ShowQuest(quest);
                    uiBoard.ShowGoal(quest.goalRow, quest.goalCol);
                    uiBoard.HideTrajA();
                    uiBoard.ShowHeatmap();
                    yield return new WaitForSecondsRealtime(0.1f);

                    for (int step = 0; step < quest.Length*2-2; step++)
                    {
                        float t = Time.realtimeSinceStartup;

                        // Get Heatmap for step
                        isHeatmapReceived = false;
                        yield return AIController.ins.PredictHeatmapOnce(env, step);
                        yield return new WaitUntil(() => isHeatmapReceived);

                        // Interval
                        yield return new WaitForSecondsRealtime(Mathf.Max(0f, 1f - (Time.realtimeSinceStartup - t)));

                        uiBoard.ShowHeatmap(AIController.ins.Heatmap);
                    }
                    // Interval
                    yield return new WaitForSecondsRealtime(1f);

                    if (i == 3)
                    {
                        btnNext.interactable = true;
                        btnBack.interactable = true;
                    }
                }
            }
            else if (phase == 10)
            {
                text.text = "僕たちAI（エーアイ）はコンピュータだから、最初からゴールまでの道を知っているんじゃないかって思うかもしれないけど、実は一歩一歩コツコツと、何度も失敗して繰り返して、そしてやっと正解を見つけるんだ。なんだかキミたち人間と似てるでしょ？";
                btnHint.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 11)
            {
                text.text = "僕たちAIは、何度も繰り返して学習すればするほど強くなっていくんだ。でもサボって学習しなければ弱いまま…これもキミたちと似ているね！";
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 12)
            {
                text.text = "ちなみに、僕が今どんな道を考えているか、「ヒント」ボタンを押すとこっそり見ることができるよ！";
                btnHint.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 13)
            {
                text.text = "最初のころの僕はまだ弱いから…間違ってたり、道がなかなか見えなかったりするけどね！";
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 14)
            {
                text.text = "迷路バトルで僕に勝っていったら…どんどん強い、たくさん学習した僕と戦えるから、頑張ってみて！";
                yield return new WaitForSecondsRealtime(0.5f);
                btnNext.interactable = true;
                yield break;
            }

            btnNext.interactable = true;
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
