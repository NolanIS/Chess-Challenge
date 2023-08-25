# Chess Coding Challenge (C#)

This is my attempt at [SebLague's Chess-Challenge](https://github.com/SebLague/Chess-Challenge)

## Iterations:
1) NegaMax with alpha-beta pruning
NegaMax is used along with alpha-beta pruning to search for the best performing sequence of moves, based on the evaluation function.
2) Implementing MergeSort and squashing bugs
MergeSort is used to sort the list of potential moves so that they are listed in order of interest. A move is considered interesting if it captures a higher value piece with a lower value one, or if it moves a higher value piece. This heuristic function will probabily also be fine tuned in the future. The goal of this is to help optimize the alpha-beta process by searching potentially more rewarding branches first.
3) Added Quiescence Search and Iterative Deepening. Various bug fixes and token count improvements were also made. The performance of Iterative Deepening is somewhat limited, probably because of an unoptimized heuristic function and a poorly implemented Transposition Table, so improving on those will be my next step.  


## Useful Links:
- https://www.chessprogramming.org/Negamax
- https://www.chessprogramming.org/Transposition_Table
- https://www.chessprogramming.org/Alpha-Beta 
- https://www.chessprogramming.org/Quiescence_Search 
- https://www.chessprogramming.org/Iterative_Deepening 
- https://www.chessprogramming.org/MVV-LVA
