using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


namespace toio.AI.meicu
{
    public class PageLearn : MonoBehaviour
    {
        public Game game;
        public GameObject ui;
        public UIBoard uiBoard;
        public UIQuest uiQuest;
        public Text text;
        public Button btnNext;
        public Button btnBack;
        public UISwitch swHint;

        public VideoPlayer videoPlayer;


        int phase = 0;
        MeiQuest quest = default;
        float[,] heatmap;
        private bool requestBtnNext = false; // btn operation during one phase


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            if (active)
            {
                uiQuest.Reset();
                uiBoard.Reset();
                swHint.GetComponent<Button>().interactable = false;
                swHint.isOn = false;

                MeiPrefs.SetLearnCleared(); // TODO

                phase = 0;
                requestBtnNext = false;
                Refresh();
            }
            else
            {
                StopAllCoroutines();

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
            if (swHint.isOn)
            {
                ui.transform.Find("ImgThink").gameObject.SetActive(true);
                uiBoard.ShowHeatmap(heatmap);
            }
            else
            {
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                uiBoard.HideHeatmap();
            }
        }

        public void OnBtnNext()
        {
            // BtnNext used as interaction during one phase
            if (requestBtnNext)
            {
                requestBtnNext = false;
                return;
            }

            if (phase == 25)
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
                text.text = "ここでは、ボクが\n「どのように正しい道を見つけるのか」\nについてかいせつするよ。";
                ui.transform.Find("Indicators").Find("Arrow").gameObject.SetActive(false);
                ui.transform.Find("Indicators").Find("Hint").gameObject.SetActive(false);
                ui.transform.Find("Indicators").Find("Trials").gameObject.SetActive(false);
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                ui.transform.Find("Illust").gameObject.SetActive(false);
                ui.transform.Find("Illust2").gameObject.SetActive(false);
                ui.transform.Find("Illust3").gameObject.SetActive(false);
                ui.transform.Find("Video").gameObject.SetActive(false);
                uiBoard.gameObject.SetActive(true);
                uiQuest.gameObject.SetActive(true);
                uiBoard.HideHeatmap();

                quest = new MeiQuest(4, 4, new Env.Space[]{Env.Space.R}, 3, 4);
                uiQuest.ShowQuest(quest);
                uiQuest.ShowA(0);
                uiBoard.ShowGoal(3, 4);
                uiBoard.ShowKomaA(4, 4);

            }
            else if (phase == 1)
            {
                text.text = "少し難しく言うと、\n「AIの考え方」\n「AIがどのように学習するか」\nについてだよ。";
            }
            else if (phase == 2)
            {
                text.text = "まず最初、ボクは自分の位置と問題を見てみる。";
            }
            else if (phase == 3)
            {
                text.text = "でも、最初は「色」も「問題の意味」も分からないので、とりあえず上下左右のどこかに動いてみる。";
            }
            else if (phase == 4)
            {
                // Back
                ui.transform.Find("Indicators").Find("Arrow").gameObject.SetActive(false);

                text.text = "つまり、最初は\n「てきとうに動いてみる」\nだけなんだ。";
            }
            else if (phase == 5)
            {
                text.text = "例えばこの問題の場合、キミはすぐに\n「上に動けばいい」ってわかるでしょ？";
                btnNext.interactable = true;

                // Blink Arrow
                for (int i=0; i < 5; i++)
                {
                    ui.transform.Find("Indicators").Find("Arrow").gameObject.SetActive(true);
                    yield return new WaitForSecondsRealtime(0.5f);
                    ui.transform.Find("Indicators").Find("Arrow").gameObject.SetActive(false);
                    yield return new WaitForSecondsRealtime(0.5f);
                }
            }
            else if (phase == 6)
            {
                // Back
                uiBoard.HideHeatmap();
                ui.transform.Find("ImgThink").gameObject.SetActive(false);

                // Hide Arrow
                ui.transform.Find("Indicators").Find("Arrow").gameObject.SetActive(false);

                text.text = "でもボクは、最初はそれすら分からないんだ。\nだから、上下左右のどこにも動く可能性があるんだよ。";
                btnNext.gameObject.SetActive(true);
            }
            else if (phase == 7)
            {
                text.text = "明るさで表現すると、こんな感じ。\nどのマスも同じ明るさ、どこも同じだけ可能性があるってこと。";

                // Show heatmap
                heatmap = new float[9, 9];
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
                // Back
                ui.transform.Find("Indicators").Find("Trials").gameObject.SetActive(false);

                text.text = "これを「試行錯誤（しこうさくご）」って言うんだ。\nじっさいに100回やってみるね！";
            }
            else if (phase == 11)
            {
                heatmap = new float[9, 9];
                float[] probs = new float[4];
                for (int i = 0; i < 4; i++) probs[i] = 0.25f;   // initial uniform prob dist.

                // Learning Iteration
                for (int t = 0; t < 100; t++)
                {
                    float interval = Mathf.Max( 1-(1f/15f)*Mathf.Min(15f, t) , 0.02f);

                    text.text = $"　　　　　　試行 {t+1} 回目\n\n\n\n";

                    // Restart
                    ui.transform.Find("Indicators").Find("Trials").gameObject.SetActive(false);
                    uiBoard.HideTrajA();
                    uiBoard.ShowKomaA(4, 4);
                    heatmap[3, 4] = probs[0]; heatmap[4, 5] = probs[1]; heatmap[5, 4] = probs[2]; heatmap[4, 3] = probs[3];
                    uiBoard.ShowHeatmap(heatmap);
                    yield return new WaitForSecondsRealtime(interval);

                    // Sample action
                    var action = SampleIdx(probs);
                    if (t == 0 || t == 99) action = 0;
                    if (t == 1) action = Random.Range(1, 3);

                    // Env step
                    (var r, var c) = Env.Translate((Env.Action)action, 4, 4);
                    var pos = new Vector2Int(r, c);

                    // Learn
                    if (action == 0)   // Goal
                    {
                        for (int a = 0; a < 4; a++)
                        {
                            if (a == action) probs[a] += 0.009f;
                            else probs[a] -= 0.003f;
                        }
                    }
                    else    // Fail
                    {
                        for (int a = 0; a < 4; a++)
                        {
                            if (a == action) probs[a] -= 0.009f;
                            else probs[a] += 0.003f;
                        }
                    }

                    // Update GUI
                    uiBoard.ShowKomaA(pos);
                    uiBoard.ShowTrajA(new Vector2Int[]{pos});
                    ui.transform.Find("Indicators").Find("Trials").gameObject.SetActive(true);
                    ui.transform.Find("Indicators").Find("Trials").Find("a0").gameObject.SetActive(action==0);
                    ui.transform.Find("Indicators").Find("Trials").Find("a1").gameObject.SetActive(action==1);
                    ui.transform.Find("Indicators").Find("Trials").Find("a2").gameObject.SetActive(action==2);
                    ui.transform.Find("Indicators").Find("Trials").Find("a3").gameObject.SetActive(action==3);

                    if (action == 0)   // Goal
                    {
                        uiQuest.ShowA(1);
                        // Update text
                        if (t < 2)
                        {
                            text.text = $"　　　　　　試行 {t+1} 回目\n\n「上」に動いてみたらゴールに成功！\nこの時AIは\n「上に行けばゴールの可能性がアップ」\nと学習するんだ。";
                        }
                        else
                        {
                            text.text = $"　　　　　　試行 {t+1} 回目\n\nゴール成功\n「上」の可能性  アップ\n";
                        }
                    }
                    else    // Fail
                    {
                        // Update text
                        string actionStr = "";
                        if (action == 1) actionStr = "右";
                        if (action == 2) actionStr = "下";
                        if (action == 3) actionStr = "左";
                        if (t < 2)
                        {
                            text.text = $"　　　　　　試行 {t+1} 回目\n\n「{actionStr}」に行ってみたら、今度はゴールに\n失敗しちゃった！\nこの時AIは\n「{actionStr}に行けばゴールの可能性がダウン」\nと学習するんだ。";
                        }
                        else
                        {
                            text.text = $"　　　　　　試行 {t+1} 回目\n\nゴール失敗\n「{actionStr}」の可能性 ダウン\n";
                        }
                    }

                    yield return new WaitForSecondsRealtime(interval);

                    // Request button interaction
                    if (t < 2)
                    {
                        requestBtnNext = true;
                        btnNext.interactable = true;
                        yield return new WaitUntil(()=>!requestBtnNext);
                        btnNext.interactable = false;
                    }
                }
                text.text = "ほら「上」のマスが明るくなってきたでしょ？";
            }
            else if (phase == 12)
            {
                // Hide Indicator
                ui.transform.Find("Indicators").Find("Trials").gameObject.SetActive(false);

                text.text = "これは、ボクがまず「上のマスに移動するのが良さそうだ」って分かってきたって事なんだ。";
            }


            else if (phase == 13)
            {
                text.text = "今は最初の一歩だけ見せたけど、じっさいはこの先も一歩一歩、同じようにうまくいくかどうかをためしてるんだ。";

                // Back
                ui.transform.Find("Video").gameObject.SetActive(false);
                uiBoard.gameObject.SetActive(true);
                uiQuest.gameObject.SetActive(true);
                ui.transform.Find("ImgThink").gameObject.SetActive(true);
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
                videoPlayer.frame = 1;
                videoPlayer.Pause();

                text.text = "長い問題で、正しい道を探す時はこんな感じ…";
                // yield return new WaitForSecondsRealtime(1.5f);
                yield return new WaitForSecondsRealtime(1f);
                videoPlayer.Play();

                if (videoPlayer.isPlaying) yield return new WaitUntil(() => videoPlayer.time > 3);
                else yield return new WaitForSecondsRealtime(2f);
                videoPlayer.Pause();
                yield return new WaitForSecondsRealtime(0.2f);
                text.text = "最初はぜんぜんゴールできない…";
                yield return new WaitForSecondsRealtime(0.2f);
                videoPlayer.Play();

                if (videoPlayer.isPlaying) yield return new WaitUntil(() => videoPlayer.time > 7);
                else yield return new WaitForSecondsRealtime(2f);
                videoPlayer.Pause();
                yield return new WaitForSecondsRealtime(0.2f);
                text.text = "1000回くらいがんばってもまだまだ…";
                yield return new WaitForSecondsRealtime(0.2f);
                videoPlayer.Play();

                if (videoPlayer.isPlaying) yield return new WaitUntil(() => videoPlayer.time > 13);
                else yield return new WaitForSecondsRealtime(2f);
                videoPlayer.Pause();
                yield return new WaitForSecondsRealtime(0.2f);
                text.text = "2000回くらいで、やっとゴールできはじめたよ！";
                yield return new WaitForSecondsRealtime(0.2f);
                videoPlayer.Play();

                if (videoPlayer.isPlaying) yield return new WaitUntil(() => videoPlayer.time > 20);
                else yield return new WaitForSecondsRealtime(2f);
                videoPlayer.Pause();
                yield return new WaitForSecondsRealtime(0.2f);
                text.text = "3000回こえたら、ほとんどゴールできるようになってきたね！";
                yield return new WaitForSecondsRealtime(0.2f);
                videoPlayer.Play();

                if (videoPlayer.isPlaying) yield return new WaitUntil(() => videoPlayer.time > 27);
                else yield return new WaitForSecondsRealtime(2f);
                videoPlayer.Pause();
                yield return new WaitForSecondsRealtime(0.2f);
                text.text = "ほぼカンペキにゴールできるようになるまで、ボクは4000回くらいチャレンジしたんだよ！";
                yield return new WaitForSecondsRealtime(0.2f);
                videoPlayer.Play();

                yield return new WaitForSecondsRealtime(0.5f);
            }


            else if (phase == 15)
            {
                // Hide Video
                ui.transform.Find("Video").gameObject.SetActive(false);

                text.text = "ボクたちAIが学習していく\n\n流れをまとめると...";

                // Show Illust
                var illustTr = ui.transform.Find("Illust");
                illustTr.gameObject.SetActive(true);
                for (int i = 0; i < illustTr.childCount; i++)
                    illustTr.GetChild(i).gameObject.SetActive(false);

                illustTr.Find("Step1").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(3f);
                illustTr.Find("Step2").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                illustTr.Find("Step3").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                illustTr.Find("Step4").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                illustTr.Find("Step5").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                illustTr.Find("Step6").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);

                text.text = "わかったかな？\n\nボクたちAIはコンピュータだから、最初から正しい道を知っているんじゃないかって思うかもしれないけど、";
            }
            else if (phase == 16)
            {
                // Back
                ui.transform.Find("Illust").gameObject.SetActive(true);
                ui.transform.Find("Illust2").gameObject.SetActive(false);

                text.text = "実は、こうやって何度も失敗しながら、それでも繰り返しチャレンジして、\nやっと答えを見つけるんだ！";
            }
            else if (phase == 17)
            {
                // Hide illust
                ui.transform.Find("Illust").gameObject.SetActive(false);

                text.text = "ほら見てごらん...\n\n何度もチャレンジして\n答えを見つけるところが\nどこかキミたち人間と似てるよね";

                // Show illust2
                var illustTr = ui.transform.Find("Illust2");
                ui.transform.Find("Illust2").gameObject.SetActive(true);
                illustTr.gameObject.SetActive(true);
                for (int i = 0; i < illustTr.childCount; i++)
                    illustTr.GetChild(i).gameObject.SetActive(false);

                illustTr.Find("Step1").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(3f);
                illustTr.Find("Step2").gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(2f);
                illustTr.Find("Step3").gameObject.SetActive(true);
            }
            else if (phase == 18)
            {
                // Back
                swHint.GetComponent<Button>().interactable = false;
                uiBoard.gameObject.SetActive(false);
                uiQuest.gameObject.SetActive(false);
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                ui.transform.Find("Indicators").Find("Hint").gameObject.SetActive(false);

                // Hide illust2
                ui.transform.Find("Illust2").gameObject.SetActive(false);

                // Show illust3
                ui.transform.Find("Illust3").gameObject.SetActive(true);

                text.text = "だからキミたち人間と\nボクたちAIは\nいい友だちになれると思う";
            }

            else if (phase == 19)
            {
                // Hide Illust
                ui.transform.Find("Illust3").gameObject.SetActive(false);

                // Show Board
                uiBoard.gameObject.SetActive(true);
                uiQuest.gameObject.SetActive(true);

                // Show hint
                swHint.GetComponent<Button>().interactable = true;
                swHint.isOn = false;
                OnBtnHint();
                var hintTr = ui.transform.Find("Indicators").Find("Hint");

                text.text = "ちなみに、このボタンに気付いた？";

                hintTr.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.75f);
                btnNext.interactable = true;
                btnBack.interactable = true;
                hintTr.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.75f);
                hintTr.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.75f);
                hintTr.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.75f);
                hintTr.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(0.75f);
                hintTr.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.75f);
                hintTr.gameObject.SetActive(true);
            }
            else if (phase == 20)
            {
                text.text = "このボタンは、ボクの頭の中をこっそりのぞけるヒミツのボタン…";
            }
            else if (phase == 21)
            {
                text.text = "いまボクがどんな道を考えているか、\n見ることができるんだ。\n\n押してみてごらん。";
                var ishint = swHint.isOn;
                yield return new WaitUntil(()=>swHint.isOn!=ishint);
            }
            else if (phase == 22)
            {
                text.text = "もし、キミがどっちに進めばわからなくなった時は、こっそり見てみるといいかもね！";
            }
            else if (phase == 23)
            {
                text.text = "最初のころのボクはまだ弱いから…間違ったり、なかなか道が見えなかったりするけどね。";
            }
            else if (phase == 24)
            {
                text.text = "迷路バトルでボクに勝っていったら…\nどんどん強い、たくさん学習したボクと戦えるから、頑張ってみて！";
            }
            else if (phase == 25)
            {
                MeiPrefs.SetLearnCleared();
                text.text = "それではあらためて、\nボクとバトルで勝負だ！";
            }

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
