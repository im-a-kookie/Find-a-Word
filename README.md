# FindaWord

A really simple console application for making word search puzzles that increase difficulty by using limited numbers of letters. There may be bugs, as this is a proof of concept that has not been rigorously tested.

Simple command line usage, with an inbuilt utility for choosing words from an included dictionary. Inspired by a social media post lamenting the difficulty of creating such puzzles for a spouse.

An example puzzle can be found [here](FindaWord/Resources/Puzzle.png).

# Algorithmic Details

Essentially, the algorithm takes a given set of words, then generates a word search puzzle from those words. The remaining spaces are filled exclusively with the letters of these words, but in such a way that no additional words will be formed, resulting in puzzles that will, quote; "make your eyes bleed."

Words are excluded by (1) graphing a dictionary of common English words as a progessive tree, and (2) walking this graph as letters are placed with orthogonal adjacency, ensuring that no such placement arrives at a word-terminating node.

While easily configurable, unspecified words of up to 3 letters may still appear, as it proves mathematically unreasonable to exclude them.

The progressive tree can be constructed very efficiently through the use of Span operations. However, tree construction is relatively slow (500ms+) and the final tree has significant memory requirements. In cases where this is not suitable, the algorithm may be significantly sped up by multithreading, as partitioning the list by initial letter obviates the need for synchronization.

A further future optimization observes that, with only 26 possible branches, significant optimizations are possible in place of Dictionary lookup. For example, by conceiving of slices as List<Slice>, they can be referenced by integer index, allowing subslices to be referenced via int[]. Now, the first index can be used as a bitset to denote the presence of a given letter, and bit counting can be used to determine the index of that letter in the array. This reduces memory **and** is more performant than hash lookup.



