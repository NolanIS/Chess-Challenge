using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{

    struct StateInfo
    {
        public float score;
        public Move move;
        public float examined_depth;
        public StateInfo(float s, Move m, float depth)
        {
            score = s;
            move = m;
            examined_depth = depth;
        }
    }
    private Dictionary<ulong, StateInfo> Transpositions = new Dictionary<ulong, StateInfo>();


    public Move Think(Board board, Timer timer)
    {
        //Can this be removed?
        //I need to figure out how to analyze memory usage in VisualStudio
        Transpositions.Clear();
        

        return negamax(board, 5, float.NegativeInfinity, float.PositiveInfinity).Item2;
    }

    public (float, Move) negamax(Board board, int depth, float alpha, float beta)
    {
        if(depth == 0)
        {
            return (evaluate(board), Move.NullMove);
        }
        if(board.IsInCheckmate() || board.IsDraw())
        {
            return (float.NegativeInfinity, Move.NullMove);
        }

        if(Transpositions.ContainsKey(board.ZobristKey))
        {
            StateInfo s = Transpositions[board.ZobristKey];
            if(s.examined_depth >= depth)
            {// This search is not going further than previous searches,
             // we can return the previously computed score.
                return (s.score, s.move);
            }
        }

        float bestScore = float.NegativeInfinity;
        Move bestMove = Move.NullMove;
        foreach(Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            float score = -negamax(board, depth - 1, -beta, -alpha).Item1;
            board.UndoMove(move);


            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;

                alpha = Math.Max(bestScore, alpha);
                if(alpha >= beta)
                {
                    break;
                }
            }

            
        }
        if(bestMove == Move.NullMove)
        {
            bestMove = board.GetLegalMoves()[0];
        }
        if (Transpositions.ContainsKey(board.ZobristKey)) Transpositions.Remove(board.ZobristKey);
        Transpositions.Add(board.ZobristKey, new StateInfo(bestScore, bestMove, depth));
        
        return (bestScore, bestMove);
    }


    public float evaluate(Board board)
    {

        float sum = 0;
        if (board.IsInCheckmate())  // Check if the CURRENT player is in checkmate (NEGATIVE SCORE)
        {
            sum = float.NegativeInfinity;
        }
        int[] values = {0, 1, 3, 4, 5, 9 };
        for(int i = 1; i < 5; i++)
        {
            sum += values[i] * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard((PieceType)i, board.IsWhiteToMove));
            sum += -values[i] * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard((PieceType)i, !board.IsWhiteToMove));

        }
        return sum;
    }
}

