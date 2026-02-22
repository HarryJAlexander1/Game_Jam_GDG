# Music memory game

## Create game logic handler
1. Generate a collection of random sets of symbols (probably use Q,W,E,R,T) to represent notes but could just as easily use 1,2,3,4,5
2. Store in memory before player starts the minigame. Sequences should start easy i.e. [Q,E,R,R] and get progressively harder [Q,E,R,R,Q,T,R,T,E,Q,W,W]
3. Once the player starts the game the flow should be as follows:
    a. Easiest sequence is selected
    b. Sequence is output to user note-by-note. (Maybe use a co-routine for this)
    c. Player inputs answer using keyboard.
    d. Game compares player's answer to output.
    e. If correct, progress to next sequence. Else, return to start (easiest) sequence.
    f. If player completes the last (hardest) sequence, they win - music memory game ends.
4. Optional: Could add a time parameter to complete all the sequences within a certain time limit.

## Create audio/visual handler
1. Create GameObject that builds a 2D representation of the minigame. i.e. a grid of NxN values representing the notes (symbols) a player has input and their order.
2. This object should listen to data from the game logic handler (outlined above) and update accordingly.
3. Update visuals to show incorrect and correct sequences from user input.
4. Play corresponding audio file when symbol is pressed.
