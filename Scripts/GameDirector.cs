using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameDirector : MonoBehaviour
{
    //一人でプレイするかか二人でプレイするか
    //ArrowButtonControllerからアクセスするためpublic
    public bool isOnePlayer;

    //ボードの状態を表す。1が白、-1が黒のコマが置かれていることを表す。
    //0はコマがないことを表す。
    //第一添え字は左からいくつか、第二添え字は下からいくつかを表す。0始まり。
    public int[,] board;

    //表示用のゲームオブジェクトのSpriteRender
    //表示させる色がboardの状態に対応している。
    //1 白,-1 黒,0　無色 
    //第一添え字は左からいくつか、第二添え字は下からいくつかを表す。0始まり。
    SpriteRenderer[,] pieceSR;

    
    //どの場所からコマを挿入するかを決めるボタン
    //第一添え字は砲口、第二添え字は左または下からいくつかを表す。0始まり。
    //insertDir：0 右から,1 上から,2 左から,3 下から
    GameObject[,] arrowButtons;


    //nextPiece：次のコマ。1が白。-1が黒。
    public int nextPiece;
    public int playerPiece;

    GameObject canvas;

    //次のコマの色を表す
    Image nextPieceImage;

    //ポーズ中のボタンを出力するためのパネル
    GameObject pausePanel;
    //ポーズ中かを表す
    public bool isPause;

    //ゲームの決着がついたか
    bool isGameEnd;
    //ゲーム結果を出力するためのパネル
    GameObject resultPanel;
    //ゲーム結果を表すテキスト
    Text resultText;

    public GameObject piecePrefab;
    public GameObject arrowButtonPrefab;

    //OnePlayerのときの対戦相手
    Opponent opponent;
    
    //対戦相手の思考時間
    //すぐに対戦相手に打たれると相手が打ったのか分からない
    float thinkTime;
    //対戦相手のターンであることを示すパネル
    GameObject thinkPanel;

    //thinkTimeの最大値
    const float MAX_THINK_TIME = 1;
    //ボードの一辺あたりのマスの数
    public const int GRID_NUM = 5;
    //無限大を表す定数
    const int INFTY = 1000000000;
    

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyになっているTileDirectorという名前のゲームオブジェクトの取得
        GameObject tdGameObject = GameObject.Find("TitleDirector");
        //tdGameObjectのTitleDirectorコンポーネント取得
        TitleDirector titleDirector = tdGameObject.GetComponent<TitleDirector>();
        //TItleDirectorからisOnePlayer,difficulty,playerOrderを取得
        this.isOnePlayer = titleDirector.isOnePlayer;
        int difficulty = titleDirector.difficulty;
        int playerOrder = titleDirector.playerOrder;
        
        //DontDestroyのため手動で破棄
        Destroy(tdGameObject);

        //playerOrderからplayerPieceを設定
        this.playerPiece = (playerOrder == 0 ? 1 : -1);

        //ボードの初期化
        this.board = new int[GRID_NUM, GRID_NUM];



        //表示用オブジェクトの初期化
        this.pieceSR = new SpriteRenderer[GRID_NUM, GRID_NUM];
        //コマの位置を調整する
        //中心が(2,2)となるようにするためのoffSet
        int offSet = -GRID_NUM / 2;
        //distance:コマ間の距離
        float pieceDis = 1.5f;
        for (int x = 0; x < GRID_NUM; x++)
        {
            for (int y = 0; y < GRID_NUM; y++)
            {
                GameObject piece = Instantiate(piecePrefab) as GameObject;
                piece.transform.position = new Vector3(x + offSet, y + offSet, 0) * pieceDis;

                pieceSR[x, y] = piece.GetComponent<SpriteRenderer>();
            }
        }


        //白先攻
        this.nextPiece = 1;


        this.canvas = GameObject.Find("Canvas");

        //矢印ボタン配列の生成
        this.arrowButtons = new GameObject[4, GRID_NUM];
        
        //矢印ボタンのゲームオブジェクトの生成し配列に保持
        for (int dir = 0; dir < 4; dir++)
        {
            for (int pos = 0; pos < GRID_NUM; pos++)
            {

                GameObject arrowButton = Instantiate(arrowButtonPrefab) as GameObject;
                //canvasの子とすることでbuttonとして機能するようにする
                arrowButton.transform.SetParent(this.canvas.transform);
                //canvas内での描画順を最初にする
                arrowButton.transform.SetAsFirstSibling();
                //ArrowButtonControlerにおけるinsertDirとinsertPosを対応させる。
                ArrowButtonController abc = arrowButton.GetComponent<ArrowButtonController>();
                abc.insertDir = dir;
                abc.insertPos = pos;
                this.arrowButtons[dir, pos] = arrowButton;
            }
        }

        //矢印ボタンの位置と向きの調整
        this.AdjustArrowButtons();

        //次のコマを表すImageの初期化
        this.nextPieceImage = GameObject.Find("NextPieceImage").GetComponent<Image>();
        //先攻は白
        this.nextPieceImage.color = Color.white;

        //pausePanelを初期化し、Inactiveにしておく
        this.pausePanel = GameObject.Find("PausePanel");
        this.pausePanel.SetActive(false);
        //最初はポーズ中ではない
        this.isPause = false;


        //最初は決着はついていない
        this.isGameEnd = false;
        //resultPanelとresultTextを初期化し、Inactiveにしておく
        this.resultPanel = GameObject.Find("ResultPanel");
        this.resultText = GameObject.Find("ResultText").GetComponent<Text>();
        this.resultPanel.SetActive(false);

        //一人用のとき、difficultyとplayerPieceを用いて対戦相手を生成
      
        if (isOnePlayer)
        {
            //対戦相手のコマは-this.playerPieceで表される
            this.opponent = new Opponent(difficulty,-this.playerPiece);
        }
        //対戦相手の思考時間の初期化
        this.thinkTime = 0;

        //thinkPanelの初期化
        this.thinkPanel = GameObject.Find("ThinkPanel");
        this.thinkPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //駒の描画
        this.RenderPieces();
        //画面サイズ変更で位置が変わる可能性があるため、矢印ボタンの位置調整
        this.AdjustArrowButtons();
        //次の番のがどちらかを示す駒の色変更
        this.SetNextPieceImageColor();
        //矢印ボタンの有効、無効化
        this.JudgeActive_ArrBut();
        //ゲームの終了判定
        this.JudgeGame();
        //対戦相手の行動
        this.OppAct();


    }

    public static void Insert(int[,] board,int insertPos, int insertDir,int gridNum,int nextPiece)
    {
        
        //insertDirによる場合分け
        //可能ならそれぞれの処理を関数化したいが...
        //insertDirによってboardの行と列のどちらにループを回すかと、終了条件の符号が変わるため関数化は難しい？
        switch (insertDir)
        {
            case 0:

                //コマを挿入しずらしていく
                for (int x = gridNum - 1; x >= 0; x--)
                {
                    int tmp = nextPiece;
                    nextPiece = board[x, insertPos];
                    board[x, insertPos] = tmp;

                    //次のコマが何もない場合、ループを抜ける
                    if (nextPiece == 0)
                    {
                        break;
                    }
                }
                break;

            case 1:
                //コマを挿入しずらしていく
                for (int y = gridNum - 1; y >= 0; y--)
                {
                    int tmp = nextPiece;
                    nextPiece = board[insertPos, y];
                    board[insertPos, y] = tmp;

                    //次のコマが何もない場合、ループを抜ける
                    if (nextPiece == 0)
                    {
                        break;
                    }
                }
                break;
            case 2:
                //コマを挿入しずらしていく
                for (int x = 0; x < gridNum; x++)
                {
                    int tmp = nextPiece;
                    nextPiece = board[x, insertPos];
                    board[x, insertPos] = tmp;

                    //次のコマが何もない場合、ループを抜ける
                    if (nextPiece == 0)
                    {
                        break;
                    }
                }
                break;
            case 3:
                //コマを挿入しずらしていく
                for (int y = 0; y < gridNum; y++)
                {
                    int tmp = nextPiece;
                    nextPiece = board[insertPos, y];
                    board[insertPos, y] = tmp;

                    //次のコマが何もない場合、ループを抜ける
                    if (nextPiece == 0)
                    {
                        break;
                    }
                }
                break;
        }

    }

    //ボード上のコマの描画
    void RenderPieces()
    {
        for (int x = 0; x < GRID_NUM; x++)
        {
            for (int y = 0; y < GRID_NUM; y++)
            {
                Color c;
                if (board[x, y] == -1)
                {
                    c = Color.black;
                }
                else if (board[x, y] == 0)
                {
                    c = Color.clear;
                }
                else
                {
                    c = Color.white;
                }

                pieceSR[x, y].color = c;
            }
        }
    }

    //矢印ボタンの位置の調整
    //スクリーンサイズが変わるとボタンの位置を変える必要がある
    void AdjustArrowButtons()
    {
        //ボードのワールド座標における大きさ
        Vector3 boardSize = GameObject.Find("Board").transform.localScale;
        //スクリーン座標におけるボードの一辺の大きさ
        float boardLen = RectTransformUtility.WorldToScreenPoint(Camera.main, boardSize).x;
        //矢印ボタンの間隔
        float arrowInterval = boardLen * 4 / 25;
        //矢印ボタンをボードの外側に置くための距離
        float arrowDis = boardLen / 2;

        //中心を合わせるためのoffset
        int offSet = -GRID_NUM / 2;

        for (int dir = 0; dir < 4; dir++)
        {
            for (int pos = 0; pos < GRID_NUM; pos++)
            {

                //位置と向きの変更
                switch (dir)
                {
                    case 0:

                        arrowButtons[dir, pos].transform.localPosition = new Vector3(arrowDis, arrowInterval * (pos + offSet), 0);
                        arrowButtons[dir, pos].transform.localRotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case 1:
                        arrowButtons[dir, pos].transform.localPosition = new Vector3(arrowInterval * (pos + offSet), arrowDis, 0);
                        arrowButtons[dir, pos].transform.localRotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case 2:
                        arrowButtons[dir, pos].transform.localPosition = new Vector3(-arrowDis, arrowInterval * (pos + offSet), 0);
                        arrowButtons[dir, pos].transform.localRotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case 3:
                        arrowButtons[dir, pos].transform.localPosition = new Vector3(arrowInterval * (pos + offSet), -arrowDis, 0);
                        break;
                }

                
            }
        }

    }

    //矢印ボタンをActiveとInactiveのどちらにするかを決める。
    void JudgeActive_ArrBut()
    {
        for(int dir = 0; dir < 4; dir++)
        {
            for(int pos = 0; pos < GRID_NUM; pos++)
            {
                bool canActivate = CanActivate_ArrBut(this.board, pos, dir,GRID_NUM, this.nextPiece);
                this.arrowButtons[dir, pos].SetActive(canActivate);
            }
        }
        
    }

    void SetNextPieceImageColor()
    {
        if(this.nextPiece == 1)
        {
            this.nextPieceImage.color = Color.white;
        }
        else
        {
            this.nextPieceImage.color = Color.black;
        }

    }

    //pieceColの色で5目並んでいる行や列、斜めが存在するか
    //存在する場合true、しない場合false
    bool IsAligned(int piece)
    {
        int countLine = CountLine_pieceNum(this.board, piece, GRID_NUM, GRID_NUM);

        //gridNum以上並んでいる列が存在する場合true、そうでない場合false
        return (countLine > 0);
    }

    //ゲームが終了したかを判定し、結果に応じてresultPanelを有効にする
    //両方が同時にそろった場合、揃えた方の負けとする。
    void JudgeGame()
    {
        //白黒そろっているかの確認
        bool isAlignedW = this.IsAligned(1);
        bool isAlignedB = this.IsAligned(-1);

        //白黒どちらもそろっていない場合、何もせず終了
        if(!isAlignedW && !isAlignedB)
        {
            return;
        }

        //どちらかがそろっているのでresultPanelをActiveにし、isGameEndをtureにする
        this.resultPanel.SetActive(true);
        this.isGameEnd = true;

        //これ以上コマが挿入されないようarrowButtonをInactiveにする。
        for(int dir = 0; dir < 4; dir++)
        {
            for(int pos = 0; pos < GRID_NUM; pos++)
            {
                arrowButtons[dir, pos].SetActive(false);
            }
        }

        //一人用の場合
        if (this.isOnePlayer)
        {

            bool canPlayerWin = this.IsAligned(this.playerPiece);

            //両方が同時にそろった場合
            if(isAlignedW && isAlignedB)
            {
                //プレイヤーのコマと次のコマの色が一致していない場合、プレイヤーが同時にそろえたことになる
               if(this.playerPiece != this.nextPiece)
                {
                    canPlayerWin = false;
                }
            }

            if (canPlayerWin)
            {
                this.resultText.text = "You Win!";
            }
            else
            {
                this.resultText.text = "You Lose...";
            }
        }
        //二人用の場合
        else
        {
            //白のみそろった場合
            if (!isAlignedB)
            {
                this.resultText.text = "White Win!";
            }
            //黒のみそろった場合
            else if (!isAlignedW)
            {
                this.resultText.text = "Black Win!";
            }
            //両方そろった場合
            else
            {
                //次のコマが白ならばそろえたのは黒なので白の勝ち
                if(this.nextPiece == 1)
                {
                    this.resultText.text = "White Win!";
                }
                //次のコマが黒ならばそろえたのは白なので黒の勝ち
                else
                {
                    this.resultText.text = "Black Win!";
                }
            }

            
        }

    }

    //対戦相手の行動
    void OppAct()
    {

        //二人用またはプレイヤーのターンまたは勝敗がついたときは何もしない
        if (!this.isOnePlayer || this.nextPiece == this.playerPiece || this.isGameEnd)
        {
            return;
        }

        //一人用かつプレイヤーのターンではなく、勝敗がついていない場合の処理
        //対戦相手の番であることを示す
        this.thinkPanel.SetActive(true);
        //MAX_THINK_TIMEになるまで待機
        if (this.thinkTime < MAX_THINK_TIME)
        {
            this.thinkTime += Time.deltaTime;
        }
        //
        else
        {

            //thinkTimeを0に戻す
            this.thinkTime = 0;

            //解の探索
            //返り値は順に挿入位置insertPos、挿入方向insertDir
            int[] nextHand = this.opponent.NextHand(this.board);
            //探索した解の位置にコマを挿入する
            Insert(this.board, nextHand[0], nextHand[1], GRID_NUM, this.nextPiece);

            this.nextPiece *= -1;
            //対戦相手の番が終わったことを反映する
            this.thinkPanel.SetActive(false);
        }


    }
    //ゲームを新しく始める
    //resultとpauseで同じ関数
    public void OnClick_restart()
    {
        //ボードの初期化
        for(int x = 0; x < GRID_NUM; x++)
        {
            for(int y = 0; y < GRID_NUM; y++)
            {
                this.board[x, y] = 0;
            }
        }

        //ゲームの決着はついていない
        this.isGameEnd = false;

        //白先攻
        this.nextPiece = 1;

        //resultPanelとpausePanelの無効化
        //resultとpauseのどちらかからの呼び出しか分からないため、どちらも無効化
        this.resultPanel.SetActive(false);
        this.pausePanel.SetActive(false);
        //pause解除
        this.isPause = false;


        //arrowButtonの有効化
        for (int dir = 0; dir < 4; dir++)
        {
            for(int pos = 0; pos < GRID_NUM; pos++)
            {
                this.arrowButtons[dir, pos].SetActive(true);
            }
        }
    }

    public void OnClick_title()
    {
        SceneManager.LoadScene("TitleScene");
    }

    //ポーズ画面を表示する
    //ポーズ中にこの関数を呼び出されても特に影響がないため、ポーズ中の無効化はしない
    public void OnClick_pause()
    {
        this.isPause = true;
        this.pausePanel.SetActive(true);
    }

    //ゲームを再開する
    public void OnClick_resume()
    {
        this.isPause = false;
        this.pausePanel.SetActive(false);
    }

    //pieceがnum以上ならんでいる列数を数える
    //Opponentクラスでも使うためstatic
    public static int CountLine_pieceNum(int[,] board, int piece,int num, int gridNum)
    {
        //peiceがnum以上並んでいる列数
        int countLine = 0;

        //縦の列を見る
        for (int x = 0; x < gridNum; x++)
        {
            //各列のpieceの数
            int cnt = 0;
            for (int y = 0; y < gridNum; y++)
            {
                if (board[x, y] == piece)
                {
                    cnt++;
                }
            }

            if (cnt >=num)
            {
                countLine++;
            }

        }

        //横の列をみる
        for (int y = 0; y < gridNum; y++)
        {
            //各列のpieceの数
            int cnt = 0;
            for (int x = 0; x < gridNum; x++)
            {
                if (board[x, y] == piece)
                {
                    cnt++;
                }
            }

            if (cnt >=num)
            {
                countLine++;
            }

        }

        //左下から右上の斜めの列をみる
        {
            //各列のpieceの数
            int cnt = 0;
            for (int x = 0; x < gridNum; x++)
            {
                if (board[x, x] == piece)
                {
                    cnt++;
                }
            }

            if (cnt >= num)
            {
                countLine++;
            }

        }

        //左上から右下のななめがそろっているか
        {
            //各列のpieceの数
            int cnt = 0;
            for (int x = 0; x < gridNum; x++)
            {
                if (board[x, gridNum - 1 - x] == piece)
                {
                    cnt++;
                }
            }

            if (cnt >= num)
            {
                countLine++;
            }

        }



        return countLine;
    }

    //insertPos,insertDirで表される矢印ボタンがActiveにできるか
    //挿入する位置から端までみたときに、どこかに空白があるか端が挿入するコマと同じ色ならture
    //端までみたときに、空白のマスがなく、かつ端のコマが挿入するコマと違う色ならばfalse
    //Insertと処理が一部似ているため、新たな関数を作ったほうが良いかも...
    public static bool CanActivate_ArrBut(int[,] board, int insertPos, int insertDir,int gridNum, int nextPiece)
    {
        bool canActivate = false;

        switch (insertDir)
        {
            case 0:
                //空白があるか
                for (int x = gridNum - 1; x >= 0; x--)
                {
                    if (board[x, insertPos] == 0)
                    {
                        canActivate = true;
                    }
                }
                //端が挿入するコマと同じ色か
                if (board[0, insertPos] == nextPiece)
                {
                    canActivate = true;
                }
                break;

            case 1:
                //空白があるか
                for (int y = gridNum - 1; y >= 0; y--)
                {
                    if (board[insertPos, y] == 0)
                    {
                        canActivate = true;
                    }
                }
                //端が挿入するコマと同じ色か
                if (board[insertPos, 0] == nextPiece)
                {
                    canActivate = true;
                }
                break;

            case 2:
                //空白があるか
                for (int x = 0; x < gridNum; x++)
                {
                    if (board[x, insertPos] == 0)
                    {
                        canActivate = true;
                    }
                }
                //端が挿入するコマと同じ色か
                if (board[gridNum - 1, insertPos] == nextPiece)
                {
                    canActivate = true;
                }
                break;
            case 3:
                //空白があるか
                for (int y = 0; y < gridNum; y++)
                {
                    if (board[insertPos, y] == 0)
                    {
                        canActivate = true;
                    }
                }
                //端が挿入するコマと同じ色か
                if (board[insertPos, gridNum - 1] == nextPiece)
                {
                    canActivate = true;
                }
                break;
        }

        return canActivate;
    }
}
