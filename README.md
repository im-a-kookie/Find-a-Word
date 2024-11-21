# FindaWord

A really simple console application for making word search puzzles that increase difficulty by using limited numbers of letters. There may be bugs, as this is a proof of concept that has not been rigorously tested.

Simple command line usage, with an inbuilt utility for choosing words from an included dictionary. Inspired by a social media post lamenting the difficulty of creating such puzzles for a spouse. The algorithm is an absolute mess, I hacked it out in about 30 minutes. Modularity and Readability both leave much to be desired.

An example puzzle can be found [here](FindaWord/Resources/Puzzle.png).

# Algorithmic Details

Essentially, the algorithm takes a given set of words, then generates a word search puzzle from those words. The remaining spaces are filled exclusively with the letters of these words, but in such a way that no additional words will be formed, resulting in puzzles that will, quote; "make your eyes bleed."

Words are excluded by (1) graphing a dictionary of common English words as a progessive tree, and (2) walking this graph as letters are placed with orthogonal adjacency, ensuring that no such placement arrives at a word-terminating node. Easily configurable but words of up to 3 letters are still permitted due to... maths.

The dictionary is constructed as a progressive tree, and done so fairly efficiently via Span. Construction is a bit slow at 500ms+ for large lists and the tree has significant memory requirements. As partitioningthe list by initial letter can obviate the need for synchronization, a multithreaded implementation is simple enough if on-demand construction is wanted.

With only 26 possible branches, I suspect a dictionary lookup can be further optimized via indexing letters into uint and counting bits via LUT, thus indicating the real index of the letter in a continuous array.



