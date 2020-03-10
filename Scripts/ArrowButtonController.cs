using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowButtonController : MonoBehaviour
{
    //ボードの状態取得のためにGameDirectorを取得
    GameDirector gameDirector;

    //押されたボタンの位置と方向
    //insertPos：左または下から何個目か。0始まり
    //insertDir：0 右から,1 上から,2 左から,3 下から
    public int insertPos;
    public int insertDir;
    
    // Start is called before the first frame update
    void Start()
    {
        this.gameDirector = GameObject.FindWithTag("GameDirector").GetComponent<GameDirector>();
    }

    //insertPosとinsertDirの初期化
    public void SetInsertPosDir(int pos,int dir)
    {
        this.insertPos = pos;
        this.insertDir = dir;
    }

    //ボタンが押されたときの動作
    public void OnClick_Arrow()
    {
        //ポーズ中は何もしない
        if (gameDirector.isPause)
        {
            return;
        }
        //一人用かつ次のコマと自分のコマの色がそろっていない場合何もしない。
        if (this.gameDirector.isOnePlayer && this.gameDirector.nextPiece != this.gameDirector.playerPiece)
        {
            return;
        }

        //コマの挿入
        GameDirector.Insert(gameDirector.board,this.insertPos, this.insertDir,GameDirector.GRID_NUM,gameDirector.nextPiece);

        //次のコマへ色の変更
        this.gameDirector.nextPiece *= -1;
    }
}
