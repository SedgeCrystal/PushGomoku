
using System;
using UnityEngine;
using Random = UnityEngine.Random;

//対戦相手のクラス
public class Opponent
{

    //探索の深さを表す
    //探索の深さは4以上だと時間がかかりすぎるため3以下で設定
    int maxDepth;

    //自身のコマ
    int oppPiece;

    //ランダムに打つ確率
    //0が最小で10が最大
    int randomRatio;

    const int INFTY = 1000000000;

    public Opponent(int difficulty,int oppPiece)
    {
        //difficultyからrandomRatioとmaxDepthの場合分け
        switch (difficulty)
        {
            case 0:
                this.maxDepth = 1;
                this.randomRatio = 6;
                break;
            case 1:
                this.maxDepth = 2;
                this.randomRatio = 3;
                break;
            case 2:
                this.maxDepth = 1;
                this.randomRatio = 0;
                break;

        }

        this.oppPiece = oppPiece;
    }

    //次の手
    //返り値は順に挿入位置insertPos、挿入方向insertDir
    public int[] NextHand(int[,] board)
    {
        //this.randomRatio/10の割合で最適でない解を返す
        if (Random.Range(0, 10) < this.randomRatio)
        {
            
            return this.RandomHand(board);
        }
        else
        //深さmaxDepthにおけるNegaAlpha法による解の探索
        {
            int[] negaAlpha = this.NegaAlpha(board, this.maxDepth, this.oppPiece, -INFTY, INFTY);

            return new int[] { negaAlpha[1], negaAlpha[2] };
        }
    }

    //ランダムに次の手を決める
    //返り値は順に挿入位置insertPos、挿入方向insertDir
    int[] RandomHand(int[,] board)
    {
        //次の手が何種類あるか数える
        int maxHandCnt = 0;
        for (int dir = 0; dir < 4; dir++)
        {
            for (int pos = 0; pos < GameDirector.GRID_NUM; pos++)
            {
                //方向がdir、位置がiの矢印ボタンから挿入可能か
                if (GameDirector.CanActivate_ArrBut(board, pos, dir, GameDirector.GRID_NUM, this.oppPiece))
                {
                    //挿入可能ならばそれを一手と数える
                    maxHandCnt++;
                }
            }
        }

       
        //何手目を選ぶかランダムに決定する
        int rand = Random.Range(1, maxHandCnt+1);
        //何手目か数える
        int cnt = 0;
        int[] nextHand = new int[] { -1, -1 };
        for (int dir = 0; dir < 4; dir++)
        {
            for (int pos = 0; pos < GameDirector.GRID_NUM; pos++)
            {
                //方向がdir、位置がiの矢印ボタンから挿入可能か
                if (GameDirector.CanActivate_ArrBut(board, pos, dir, GameDirector.GRID_NUM, this.oppPiece))
                {

                    cnt++;

                    //今の手がrandom手目だったらnextHandに代入する
                    if(cnt == rand)
                    {
                        nextHand[0] = pos;
                        nextHand[1] = dir;

                        break;
                    }
                    

                }
            }

            //すでにnextHandが決まっていたらループから抜ける
            if(nextHand[0] != -1)
            {
                break;
            }
        }

        return nextHand;
    }

    //深さdepthにおけるNegaAlpha法による解の探索
    //返り値は順にスコアalpha、挿入位置insertPos、挿入方向insertDir
    int[] NegaAlpha(int[,] board, int depth, int nextPiece, int alpha,int beta)
    {
        int[] negaAlpha = new int[3];
        negaAlpha[0] = alpha;

        //白と黒のコマの並んだ列数
        int countLine = GameDirector.CountLine_pieceNum(board, 1, GameDirector.GRID_NUM, GameDirector.GRID_NUM);
        countLine += GameDirector.CountLine_pieceNum(board, -1, GameDirector.GRID_NUM, GameDirector.GRID_NUM);

        //深さ0、または並んだ列数が1以上となりゲームが終了する場合、boardからスコアを関数ValueBoardを用いて求める
        if (depth == 0 || countLine > 0)
        {
            int v = this.ValueBoard(board,nextPiece);

            
            return new int[] { v, -1, -1 };
        }
        
        //全パターンの挿入位置、挿入方向に対して探索する
        for (int dir = 0; dir < 4; dir++)
        {
            for (int i = 0; i < GameDirector.GRID_NUM; i++)
            {
                
                //方向がdir、位置がiの矢印ボタンから挿入可能か
                if (GameDirector.CanActivate_ArrBut(board, i, dir, GameDirector.GRID_NUM, nextPiece))
                {
                    //挿入可能な場合、現在のボードをコピーし、挿入する
                    int[,] nextBoard = new int[GameDirector.GRID_NUM, GameDirector.GRID_NUM];
                    Array.Copy(board, nextBoard, GameDirector.GRID_NUM * GameDirector.GRID_NUM);
                    GameDirector.Insert(nextBoard, i, dir, GameDirector.GRID_NUM, nextPiece);

                    //挿入したボードの状態から次の深さの探索をする。
                    int alpha1 = -this.NegaAlpha(nextBoard, depth - 1, -nextPiece, -beta, -alpha)[0];
                    int tmp = depth - 1;
                    //スコアが改善された場合
                    if(alpha < alpha1)
                    {
                        
                        alpha = alpha1;
                        negaAlpha[0] = alpha;
                        negaAlpha[1] = i;
                        negaAlpha[2] = dir;
                    }

                    //スコアがこれ以上改善されない場合
                    if(alpha >= beta)
                    {
                        return negaAlpha;
                    }
                }

            }
        }
        
        return negaAlpha;
    }

    //ボードの状態の評価
    //nextPeiceでないコマにとって悪い状況なら高いスコア
    //nextPeiceのコマがある列にnum個以上あるとき、スコアに20^(num+1)
    //nextPeiceでないコマがある列にnum個以上あるとき、スコアに-20^(num)
    //ただし、^は累乗を表す
    //これを各列に対して行い、ボードの状態を評価する
    int ValueBoard(int[,] board,int nextPiece)
    {
        //ボードの状態のスコア
        
        int value = 0;
        for (int num = GameDirector.GRID_NUM; num > 0; num--)
        {
            
            value += GameDirector.CountLine_pieceNum(board, nextPiece, num, GameDirector.GRID_NUM);
            value *= 10;
            value -= GameDirector.CountLine_pieceNum(board, -nextPiece, num, GameDirector.GRID_NUM);
            
        }

        
        //Debug.Log(value);
        return value;
    }

}