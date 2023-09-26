# Chess Coding Challenge (C#)

This is my attempt at [SebLague's Chess-Challenge](https://github.com/SebLague/Chess-Challenge)

## Iterations:
1) NegaMax with alpha-beta pruning
NegaMax is used along with alpha-beta pruning to search for the best performing sequence of moves, based on the evaluation function.
2) Implementing MergeSort and squashing bugs
MergeSort is used to sort the list of potential moves so that they are listed in order of interest. A move is considered interesting if it captures a higher value piece with a lower value one, or if it moves a higher value piece. This heuristic function will probabily also be fine tuned in the future. The goal of this is to help optimize the alpha-beta process by searching potentially more rewarding branches first.
3) Added Quiescence Search and Iterative Deepening. Various bug fixes and token count improvements were also made. The performance of Iterative Deepening is somewhat limited, probably because of an unoptimized heuristic function and a poorly implemented Transposition Table, so improving on those will be my next step.  
4) Replaced previous heuristic function (Based on material and movability scores) with a Piece-Table Only evaluation function. The heuristic function I used is the one created by Ronald Friederich, [PeSTO's Evaluation Function](https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function). I still want to experiment more with heuristic functions, however, I first need to look at a more optimized Transposition Table implementation, as well as incorperating the game's clock.
5) Replaced maximum search depth with time management. Iterative Deepening will continue until a certain amount of time has passed (currently based on a fraction of the remaining time), instead of a hard-coded maximum depth. The search will also cancel if it goes over the time limit. If there is only one legal move, it will be played without searching down the tree.

## Useful Links:
- https://www.chessprogramming.org/Negamax
- https://www.chessprogramming.org/Transposition_Table
- https://www.chessprogramming.org/Alpha-Beta 
- https://www.chessprogramming.org/Quiescence_Search 
- https://www.chessprogramming.org/Iterative_Deepening 
- https://www.chessprogramming.org/MVV-LVA
- https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
- http://www.talkchess.com/forum3/viewtopic.php?f=2&t=68311&start=19
- https://www.chessprogramming.org/Time_Management
