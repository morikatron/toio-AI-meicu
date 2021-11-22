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
                AIController.ins.heatmapCallback -= OnHeatmap;
            }
        }

        public void OnBtnHome()
        {
            StopAllCoroutines();
            PageManager.SetPage(PageManager.EPage.Title);
        }

        public void OnBtnHint()
        {

        }

        public void OnBtnNext()
        {
            phase ++;

            if (phase > 12)
            {
                OnBtnHome();
                return;
            }

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

            if (phase == 0)
            {
                text.text = "私の「考え方」とそれをどう「学習」したかを教えるね。";
                btnBack.gameObject.SetActive(false);
                btnHint.gameObject.SetActive(false);
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 1)
            {
                text.text = "私はお題と自分の位置などを見て、上下左右のどちらかに移動する。";
                btnBack.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 2)
            {
                text.text = "しかし私は人間が言う「お題」の意味も「色」の意味もわからないので、最初は適当に動くだけ。";
                uiQuest.Reset();
                uiBoard.Reset();
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 3)
            {
                text.text = "例えばこの場合、人間は上の色がお題に合っているのがわかるが、私にはわからないので、上下左右にどちらにも動く可能性がある。";
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
                text.text = "移動するマスの可能性を明るさで表すと、こんな感じ。";
                float[,] heatmap = new float[9, 9];
                heatmap[3, 4] = 0.25f; heatmap[5, 4] = 0.25f; heatmap[4, 3] = 0.25f; heatmap[4, 5] = 0.25f;
                uiBoard.ShowHeatmap(heatmap);
                ui.transform.Find("ImgThink").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }

            else if (phase == 5)
            {
                text.text = "たまたま上に動いてゴールした場合、ゲームから報酬を得て「上」が正しいとわかり、「上」をもっと選ぶようにするのだ。";
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 6)
            {
                text.text = "この過程は「試行錯誤」といい、何回も繰り返される。\n\n仮に100回動かしてみよう。";
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 7)
            {
                float[,] heatmap = new float[9, 9];
                float[] probs = new float[4];
                for (int i = 0; i < 4; i++) probs[i] = 0.25f;
                for (int t = 0; t < 100; t++)
                {
                    // float interval = 1;
                    float interval = Mathf.Max( 1-(1f/15f)*Mathf.Min(15f, t) , 0.05f);

                    text.text = $"試行 {t+1} 回目\n\n\n";

                    // Restart
                    uiBoard.HideTrajA();
                    uiBoard.ShowKomaA(4, 4);
                    heatmap[3, 4] = probs[0]; heatmap[4, 5] = probs[1]; heatmap[5, 4] = probs[2]; heatmap[4, 3] = probs[3];
                    uiBoard.ShowHeatmap(heatmap);
                    yield return new WaitForSecondsRealtime(interval);

                    // Step
                    var action = SampleIdx(probs);
                    (var r, var c) = Env.Translate((Env.Action)action, 4, 4);
                    var pos = new Vector2Int(r, c);
                    uiBoard.ShowKomaA(pos);
                    uiBoard.ShowTrajA(new Vector2Int[]{pos});

                    // goal
                    if (r == 3 && c == 4)
                    {
                        uiQuest.ShowA(1);
                        for (int a = 0; a < 4; a++)
                        {
                            if (a == action) probs[a] += 0.009f;
                            else probs[a] -= 0.003f;
                        }
                        text.text = $"試行 {t+1} 回目\n\nゴール成功\n「上」の可能性  UP ";
                    }
                    else
                    {
                        for (int a = 0; a < 4; a++)
                        {
                            if (a == action) probs[a] -= 0.009f;
                            else probs[a] += 0.003f;
                        }
                        string actionStr = "";
                        if (action == 1) actionStr = "右";
                        if (action == 2) actionStr = "下";
                        if (action == 3) actionStr = "左";
                        text.text = $"試行 {t+1} 回目\n\nゴール失敗\n「{actionStr}」の可能性 DOWN";
                    }
                    yield return new WaitForSecondsRealtime(interval);
                }
                text.text = "「上」のマスが明るくなってきたね。\n（「上」を選ぶ可能性が高くなってきた）";
            }
            else if (phase == 8)
            {
                text.text = "今は一つだけのお題の正しい動きを学習したが、実際は任意の問題に学習していくのだ。";
                for (int i = 0; i < 10; i++)
                {
                    quest = Env.GenerateQuest(Random.Range(2, 5));
                    uiBoard.ShowGoal(quest.goalRow, quest.goalCol);
                    uiBoard.ShowKomaA(4, 4);
                    uiBoard.HideTrajA();
                    uiBoard.HideHeatmap();
                    uiQuest.ShowQuest(quest);
                    yield return new WaitForSecondsRealtime(0.5f);

                    if (i == 3)
                        btnNext.interactable = true;
                }
            }
            else if (phase == 9)
            {
                text.text = "次の１手の可能性は上下左右の４つしかないが、数手先の可能性考えていくと、路線が浮かぶね。";
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
                        btnNext.interactable = true;
                }
            }
            else if (phase == 10)
            {
                text.text = "これが私たち（ＡＩ)の学習の仕方です。みんな(人間)の考え方と似てるところあるでしょ？";
                btnHint.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 11)
            {
                text.text = "ちなみにゲーム中は右上の「ヒント」ボタンを押すと、私の考えを覗けるよ。";
                btnHint.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (phase == 12)
            {
                text.text = "解説は以上。\n\n一緒にバトルしよ。";
                yield return new WaitForSecondsRealtime(0.5f);
            }

            btnNext.interactable = true;
            yield break;
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
