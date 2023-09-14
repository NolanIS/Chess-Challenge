using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{

    struct StateInfo
    {
        public ulong key;
        public float score;
        public Move move;
        public float examined_depth;
        public int abflag;
        public StateInfo(float s, Move m, float depth, int flag, ulong k)
        {
            key = k;
            score = s;
            move = m;
            examined_depth = depth;
            abflag = flag;
        }
    }
    private Dictionary<ulong, StateInfo> Transpositions = new Dictionary<ulong, StateInfo>();


    public Move Think(Board board, Timer timer)
    {
        //Transpositions.Clear();


        float bestScore = float.NegativeInfinity;
        Move bestMove = Move.NullMove;

        for(int i  = 0; i < 6; i++) { 
            (float score, Move move) = negamax(board, i, float.NegativeInfinity, float.PositiveInfinity);
            bestScore = score;
            bestMove = move;
        }
        if (bestMove != Move.NullMove)
            return bestMove;
        
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
        PriorityQueue<Move, int> pq = new PriorityQueue<Move, int>();
        foreach(Move move in possibleMoves)
        {
            if (move == TMove) {
                pq.Enqueue(move, -(int)PieceType.King);
            } else if (move.IsCapture)
                pq.Enqueue(move, -5*(int)move.CapturePieceType - (int)move.MovePieceType);
            else
                pq.Enqueue(move, -(int)move.MovePieceType);
        }

        Move bestMove = Move.NullMove;
        while(pq.Count > 0)
        {
            Move move = pq.Dequeue();
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
        Transpositions.Add(board.ZobristKey, new StateInfo(alpha, bestMove, depth, abflag, board.ZobristKey));
        
        return (alpha, bestMove);
    }

    public float evaluate(Board board)
    {
        
        float materialScore = 0;
        float phase = 0;
        if (board.IsInCheckmate())  // Check if the CURRENT player is in checkmate (NEGATIVE SCORE)
        {
            return -900000;
        }
        int[] values = {0, 1, 3, 4, 5, 9 };
        int[] phases = { 0, 0, 1, 1, 2, 4 }; //Phase in opening = 24, phase with only kings/pawns = 0
        for(int i = 1; i <= 5; i++)
        {
            int numPlayer = BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard((PieceType)i, board.IsWhiteToMove));
            int numEnemy = BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard((PieceType)i, !board.IsWhiteToMove));
            materialScore += values[i] * numPlayer;
            materialScore += -values[i] * numEnemy;
            phase += phases[i] * (numPlayer + numEnemy);
        }
        phase = 24 - phase;
        phase = phase < 16 ? phase / 16 : 1;

        // Bishop pair penalty (drops the value of a remaining bishop if it doesn't have a pair)
        if (BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, board.IsWhiteToMove)) == 1)
            materialScore -= 1;
        

        float movabilityScore = 10 * board.GetLegalMoves().Length;
        
        if(board.TrySkipTurn()) {
            movabilityScore -= board.GetLegalMoves().Length;

            board.UndoSkipTurn();
        }




        //Connected pawns and passed pawns
        ulong pawnBitboard = board.GetPieceBitboard(PieceType.Pawn, board.IsWhiteToMove);
        ulong unmodBitboard = pawnBitboard | board.GetPieceBitboard(PieceType.Pawn, !board.IsWhiteToMove);
        int connectedPawns = 0;
        int passedPawns = 0;

        int negativeCorrection = board.IsWhiteToMove ? 1 : -1;

        while (pawnBitboard != 0)
        {
            int pos = BitboardHelper.ClearAndGetIndexOfLSB(ref pawnBitboard);
            foreach (int i in new int[] {-7, 7, -9, 9})
            {
                int y = i + pos;
                if (y >= 7 && y <= 55 && y / 8 != pos / 8)
                    if (BitboardHelper.SquareIsSet(pawnBitboard, new Square(y)))
                    {
                        connectedPawns++;
                        break;
                    }
            }
            bool passed = true;
            foreach (int i in new int[] {pos-1, pos, pos+1})
            {
                if (i / 8 == pos / 8)
                {
                    int y = i + 8 * negativeCorrection;
                    while(y <= 63 && y >= 0)
                    {
                        if(BitboardHelper.SquareIsSet(unmodBitboard, new Square(y)))
                        {
                            passed = false;
                            break;
                        }

                        y += 8 * negativeCorrection;
                    }
                }
            }
            passedPawns += passed ? 1 : 0;
        }
        float openingScore = 1000 * materialScore + 20 * movabilityScore + 500 * connectedPawns;
        float endingScore = 1000 * materialScore + 100 * passedPawns;



        return (1-phase) * openingScore + phase * endingScore;
    }
}

