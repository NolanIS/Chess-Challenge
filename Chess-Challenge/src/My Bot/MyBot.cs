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
        public int abflag;
        public StateInfo(float s, Move m, float depth, int flag)
        {
            score = s;
            move = m;
            examined_depth = depth;
            abflag = flag;
        }
    }
    private Dictionary<ulong, StateInfo> Transpositions = new Dictionary<ulong, StateInfo>();


    public Move Think(Board board, Timer timer)
    {
        Transpositions.Clear();


        float bestScore = float.NegativeInfinity;
        Move bestMove = Move.NullMove;

        for(int i  = 0; i < 6; i++) { 
            (float score, Move move) = negamax(board, i, float.NegativeInfinity, float.PositiveInfinity);
            bestScore = score;
            bestMove = move;
        }
        if (bestMove != Move.NullMove)
        {
            return bestMove;
        }
        Console.WriteLine("!!!!Returned a Null Move!!!!");
        
        return board.GetLegalMoves()[0];
    }

    public (float, Move) negamax(Board board, int depth, float alpha, float beta)
    {
        float a = alpha;
        if(board.IsInCheckmate() || board.IsDraw())
        {
            return (-90000, Move.NullMove);
        }
        Move TMove = Move.NullMove;
        
        if (Transpositions.ContainsKey(board.ZobristKey))
        {
            StateInfo s = Transpositions[board.ZobristKey];
            TMove = s.move;
            if (s.examined_depth >= depth)
            {// This search is not going further than previous searches,
             // we can return the previously computed score.
                if(s.abflag == 1)
                    beta = Math.Min(beta, s.score);
                if(s.abflag == 2)
                    alpha = Math.Max(alpha, s.score);
                if(alpha >= beta || s.abflag == 0)
                    return (s.score, s.move);
            }
        }
        if(depth <= 0) { 
            float standing_pat = evaluate(board);
            if (standing_pat >= beta)
                return (beta, Move.NullMove);
            if (alpha < standing_pat)
                alpha = standing_pat;
        }


        //Can I replace this with board.GetLegalMovesNonAlloc?
        Move[] possibleMoves = board.GetLegalMoves(depth <= 0);
        int[] moveScores = new int[possibleMoves.Length];
        for(int i = 0; i < possibleMoves.Length; i++)
        {
            if (possibleMoves[i] == TMove)
                moveScores[i] = (int)PieceType.King;
            if (possibleMoves[i].IsCapture)
                moveScores[i] = 5*(int)possibleMoves[i].CapturePieceType - (int)possibleMoves[i].MovePieceType;
            else
                moveScores[i] = (int)possibleMoves[i].MovePieceType;
        }
        (Move[] sortedMoves, int[] sortedScores) = mergeSort(possibleMoves, moveScores, 0, possibleMoves.Length-1);

        Move bestMove = Move.NullMove;
        foreach(Move move in sortedMoves)
        {
            board.MakeMove(move);
            float score = -negamax(board, depth - 1, -beta, -alpha).Item1;
            board.UndoMove(move);
            if(score > alpha)
            {
                alpha = score;
                bestMove = move;
                if (score >= beta) { 
                    alpha = beta;
                    break;
                }
            }
        }

        if (Transpositions.ContainsKey(board.ZobristKey))
            Transpositions.Remove(board.ZobristKey);
        
        int abflag = 0;
        if(alpha <= a)
            abflag = 1;
        if(alpha >= beta)
            abflag = 2;
        Transpositions.Add(board.ZobristKey, new StateInfo(alpha, bestMove, depth, abflag));
        
        return (alpha, bestMove);
    }

    public (Move[], int[]) mergeSort(Move[] moves, int[] scores, int first, int last)
    {
        if(last < first)
            return (moves, scores);
        
        if(first == last)
            return (new Move[] { moves[first] }, new int[] { scores[first] });
        
        int mid = (first + last) / 2;
        (Move[] leftm, int[] lefts) = mergeSort(moves, scores, first, mid);
        (Move[] rightm, int[] rights) = mergeSort(moves, scores, mid + 1, last);

        int i = 0, j = 0;
        Move[] resultMoves = new Move[leftm.Length + rightm.Length];
        int[] resultScores = new int[lefts.Length + rights.Length];

        while (i < lefts.Length && j < rights.Length)
        {
            if (lefts[i] < rights[j])
            {
                resultScores[i + j] = rights[j];
                resultMoves[i + j] = rightm[j];
                j++;
            }
            else
            {
                resultScores[i + j] = lefts[i];
                resultMoves[i + j] = leftm[i];
                i++;
            }
        }

        while(i < lefts.Length)
        {
            resultMoves[i + j] = leftm[i];
            resultScores[i + j] = lefts[i];
            i++;
        }
        while (j < rights.Length)
        {
            resultMoves[i + j] = rightm[j];
            resultScores[i + j] = rights[j];
            j++;
        }



        return (resultMoves, resultScores);
    }




    public float evaluate(Board board)
    {
        
        float materialScore = 0;
        if (board.IsInCheckmate())  // Check if the CURRENT player is in checkmate (NEGATIVE SCORE)
        {
            return -90000;
        }
        int[] values = {0, 1, 3, 4, 5, 9 };
        for(int i = 1; i <= 5; i++)
        {
            materialScore += values[i] * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard((PieceType)i, board.IsWhiteToMove));
            materialScore += -values[i] * BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard((PieceType)i, !board.IsWhiteToMove));

        }



        float positionalScore = 10 * board.GetLegalMoves().Length;
        if(board.TrySkipTurn()) {
            positionalScore -= board.GetLegalMoves().Length;

            board.UndoSkipTurn();
        }


        return 100 * materialScore + positionalScore;
    }
}

