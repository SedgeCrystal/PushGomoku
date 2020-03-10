using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleDirector : MonoBehaviour
{
    //一人用か
    //trueのとき後述のonePlayerPanelを有効にする
    public bool isOnePlayer;

    //一人用のときの難易度と順番の選択用のパネル
    GameObject onePlayerPanel;

    //一人用のときの難易度
    //0 easy,1 nomal, 2 hard
    public int difficulty;

    //一人用のとき、プレイヤーの順番
    //0のとき先攻、1のとき後攻
    public int playerOrder;

    //選択された項目の色を変更するために必要
    Image[] difficultyButtonImages;
    Image[] orderButtonImages;

    //ルールパネルの切り替え用
    //初期値は-1
    //-1 表示しない、0~2　rulePanelsの添え字と一致するものを表示
    int ruleIndex;

    //ルール表示用のパネル配列
    GameObject[] rulePanels;
    



    void Start()
    {
        //画面サイズの指定
        Screen.SetResolution(1024, 768, false, 60);
        //最初はonePlayerPanelを無効にするためfalse
        this.isOnePlayer = false;

        //onePlayerPanelの初期化
        //他のゲームオブジェクトを取得してから無効化する
        this.onePlayerPanel = GameObject.Find("OnePlayerPanel");

        //difficultyが対応する難易度のボタンのImageの添え字となるよう初期化
        this.difficultyButtonImages = new Image[3];
        this.difficultyButtonImages[0] = GameObject.Find("EasyButton").GetComponent<Image>();
        this.difficultyButtonImages[1] = GameObject.Find("NormalButton").GetComponent<Image>();
        this.difficultyButtonImages[2] = GameObject.Find("HardButton").GetComponent<Image>();

        //初期値はeasy
        this.difficulty = 0;

        //playerOrderが対応する順番のボタンのImageの添え字となるよう初期化
        this.orderButtonImages = new Image[2];
        this.orderButtonImages[0] = GameObject.Find("FirstButton").GetComponent<Image>();
        this.orderButtonImages[1] = GameObject.Find("SecondButton").GetComponent<Image>();

        //初期値はプレイヤーが先攻とする
        this.playerOrder = 0;


        //onePlayerPanelの無効化
        this.onePlayerPanel.SetActive(false);

        //rulePanelの初期化
        this.rulePanels = new GameObject[3];
        this.rulePanels[0] = GameObject.Find("RulePanel1");
        this.rulePanels[1] = GameObject.Find("RulePanel2");
        this.rulePanels[2] = GameObject.Find("RulePanel3");

        //ruleIndexの初期化
        this.ruleIndex = -1;
        //rulePanelの無効化
        this.RenderRulePanel();





        //次のシーンにisOnePlayerを渡す必要があるためシーン遷移で破壊されないようにする
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        //一人用モードの処理
        this.RenderOnePlayerPanel();
        //ルール説明の処理
        this.RenderRulePanel();
    }

    //onePlayerPanelの描画における処理
    void RenderOnePlayerPanel()
    {
        if (this.isOnePlayer)
        {
            //isOnePlayerがtureのとき、パネルを有効にして表示する
            this.onePlayerPanel.SetActive(true);

            //選択した難易度の色のみgrayにする
            for(int i = 0; i < 3; i++)
            {

                Color c = Color.white;
                if(i == this.difficulty)
                {
                    c = Color.gray;
                }
                this.difficultyButtonImages[i].color = c;
            }

            //選択した順番をgrayにする
            for(int i = 0;i < 2; i++)
            {
                Color c = Color.white;
                if (i == this.playerOrder)
                {
                    c = Color.gray;
                }
                this.orderButtonImages[i].color = c;
            }
        }
        //isOnePlayerがfalseのとき
        else
        {
            //isOnePlayerがfalseのとき、パネルを無効にする
            this.onePlayerPanel.SetActive(false);
        }
    }

    //rulePanelの有効、無効の切り替え
    void RenderRulePanel()
    {
        for(int i = 0; i < 3; i++)
        {
            //ruleIndexのみ有効化して表示
            if(i == this.ruleIndex)
            {
                this.rulePanels[i].SetActive(true);
            }
            //ruleIndexでないものは無効化
            else
            {
                this.rulePanels[i].SetActive(false);
            }
        }

    }

    //OnePlayerButtonが押されたときの処理
    public void OnClick_onePlayer()
    {
        //一人用モードにする
        this.isOnePlayer = true;
        
    }

    //TwoPlayersButtonが押されたときの処理
    public void OnClick_twoPlayers()
    {
        //二人用モードにする
        this.isOnePlayer = false;
        //一人用と違い、他の設定をする必要がないため、ゲーム開始
        SceneManager.LoadScene("GameScene");
    }

    //EasyButtonが押されたときの処理
    public void OnClick_easy()
    {
        this.difficulty = 0;
    }

    //NormalButtonが押されたときの処理
    public void OnClick_normal()
    {
        this.difficulty = 1;
    }

    //HardButtonが押されたときの処理
    public void OnClick_hard()
    {
        this.difficulty = 2;
    }

    //FirstButtonが押されたときの処理
    public void OnClick_first()
    {
        this.playerOrder = 0;
    }

    //SecondButtonが押されたときの処理
    public void OnClick_second()
    {
        this.playerOrder = 1;
    }

    //StartButtonが押されたときの処理
    public void OnClick_start()
    {
        //ゲーム開始
        SceneManager.LoadScene("GameScene");
    }

    //TitleButtonが押されたときの処理
    public void OnClick_title()
    {
        //一人用モードの設定を終了する
        this.isOnePlayer = false;
    }

    //最初のルールを表示する
    public void OnClick_rule()
    {
        //ruleIndexを0にする
        this.ruleIndex = 0;
    }

    //次のルールを表示する
    //最後まで行ったらルールを非表示にする
    public void OnClick_next()
    {
        //ruleIndexを1増やす
        this.ruleIndex++;

        //3以上となった場合、ルール説明が終わったので-1にする
        if (ruleIndex >= 3)
        {
            this.ruleIndex = -1;
        }
    }

    //前のルールを表示する
    //最初まで行ったらルールを非表示にする
    public void OnClick_back()
    {
        //ruleIndexを1減らす
        this.ruleIndex--;
        //最初まで行った場合、勝手に-1になるので処理は不要
        
    }

    //ルールを非表示にする
    public void OnClick_end()
    {
        //ruleIndexを-1にする
        this.ruleIndex = -1;
    }
}
