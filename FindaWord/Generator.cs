using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindaWord
{
    internal class Generator
    {
        static (int dx, int dy)[] dirs =
[
    (-1, -1), (0, -1),
        (1, -1), (1, 0),
        (1, 1), (0,  1),
        (-1,  1), (-1,  0)
];

        /// <summary>
        /// Makes a puzzle using the given list of words. Uses letters of those words to fill in the remainder of
        /// the grid.
        /// 
        /// <para>I haven't made this super-foolproof and it's possible for it to explodify. If you have a good choice
        /// of letters though, it should generally always find a way to fill the grid.</para>
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="words"></param>
        /// <param name="W"></param>
        /// <param name="H"></param>
        /// <returns></returns>
       public static char[] MakePuzzle(WordDictionary wd, IEnumerable<string> words, int W, int H)
        {
            int word_max_len = 3;

            List<char> letters = [];
            foreach (string w in words)
            {
                foreach (char c in w)
                {
                    if (!letters.Contains(c)) letters.Add(c);
                }
            }


            char[] results = new char[W * H];

            //simple funcs to help with things
            var f = (int x, int y) => x + y * W;
            var g = (int x, int y) => results[f(x, y)];

            Queue<string> words_left = new Queue<string>(words);

            Stack<char[]> grids = [];
            grids.Push((char[])results.Clone());

            Stack<(string word, (int x, int y) start, (int x, int y) end)> added_words = [];

            while (words_left.Count > 0)
            {
                //start at a random cell and iterate
                bool inserted = false;

                int start_pos = Random.Shared.Next(results.Length);
                string w = words_left.Peek();
                for (int pos_offset = 0; pos_offset < results.Length; ++pos_offset)
                {
                    if (inserted) break;
                    int cell_index = (start_pos + pos_offset) % results.Length;
                    int x = cell_index % W;
                    int y = cell_index / W;

                    //1. Check that we're compatible
                    int start_dir = Random.Shared.Next(8);
                    for (int dir_offset = 0; dir_offset < 8; ++dir_offset)
                    {
                        if (inserted) break;
                        int dir_index = (start_dir + dir_offset) & 0x7; ;
                        var dir = dirs[dir_index];
                        //get the last x/y coord
                        int ex = x + dir.dx * (w.Length - 1);
                        int ey = y + dir.dy * (w.Length - 1);
                        //check that the word falls within the grid
                        if (ex != int.Clamp(ex, 0, W - 1) || ey != int.Clamp(ey, 0, H - 1)) continue;

                        //now check that every character can be inserted without creating words
                        //that do not fall on the line (x,y) to (ex, ey)
                        //that is, coords_used.contains(f(ix,iy)) == false
                        //move in the line
                        bool fits = true;
                        for (int i = 0; i < w.Length; i++)
                        {
                            //find the cell under this letter
                            int ix = x + dir.dx * i;
                            int iy = y + dir.dy * i;

                            //check that we do actually fit here
                            char c = g(ix, iy);
                            bool is_placing = (c == 0);
                            if (!is_placing && c != w[i])
                            {
                                fits = false;
                                break;
                            }

                            //now check the 4 main axis (N/S, E/W, NE/SW, NW/SE) 
                            for (int check_dir = 0; check_dir < 4; ++check_dir)
                            {
                                var check_axis = dirs[check_dir];
                                //we need to start from the earliest point that we can reach
                                int cx = ix;
                                int cy = iy;

                                List<char> axis_letters = [];
                                List<(int x, int y)> axis_steps = [];

                                //walk back to the edge, or the first empty cell
                                while (true)
                                {
                                    cx -= check_axis.dx;
                                    cy -= check_axis.dy;
                                    if (cx != int.Clamp(cx, 0, W - 1) || cy != int.Clamp(cy, 0, H - 1)) break;
                                    if (cx != ix && cy != iy && g(cx, cy) == 0) break;
                                }

                                //now walk forwards to the edge, or the first empty cell
                                while (true)
                                {
                                    cx += check_axis.dx;
                                    cy += check_axis.dy;
                                    if (cx != int.Clamp(cx, 0, W - 1) || cy != int.Clamp(cy, 0, H - 1)) break;
                                    //add the letter to this step
                                    //the cell is technically empty
                                    if (cx == ix && cy == iy) axis_letters.Add(w[i]);
                                    else
                                    {
                                        if (g(cx, cy) == 0) break;
                                        axis_letters.Add(g(cx, cy));
                                    }
                                    axis_steps.Add((cx, cy));
                                }

                                //now check every string of at least 3 letters
                                for (int start = 0; start < axis_letters.Count; ++start)
                                {
                                    //1. Walk backwards
                                    if (start < axis_letters.Count - word_max_len)
                                    {
                                        var r = wd.root;
                                        List<(int x, int y)> check = [];
                                        for (int end = start; end < axis_letters.Count; ++end)
                                        {
                                            check.Add(axis_steps[end]);
                                            if (r.Children.TryGetValue((byte)g(axis_steps[end].x, axis_steps[end].y), out var rr) && rr != null)
                                            {
                                                r = rr;
                                                if (r.IsWord && check.Contains((ix, iy)))
                                                {
                                                    //now check that either (a) the start or the end is not x/y
                                                    if (axis_steps[start] != (x, y) && axis_steps[end] != (x, y) && int.Abs(start - end) >= word_max_len)
                                                    {
                                                        fits = false;
                                                        goto bigloop;
                                                    }
                                                }
                                            }
                                            else break;
                                        }
                                    }

                                    //1. Walk backwards
                                    if (start >= word_max_len)
                                    {
                                        var r = wd.root;
                                        List<(int x, int y)> check = [];
                                        for (int end = start; end >= 0; --end)
                                        {
                                            check.Add(axis_steps[end]);
                                            if (r.Children.TryGetValue((byte)g(axis_steps[end].x, axis_steps[end].y), out var rr) && rr != null)
                                            {
                                                r = rr;
                                                if (r.IsWord && check.Contains((ix, iy)))
                                                {
                                                    //now check that either (a) the start or the end is not x/y
                                                    if (axis_steps[start] != (x, y) && axis_steps[end] != (x, y) && int.Abs(start - end) >= word_max_len)
                                                    {
                                                        fits = false;
                                                        goto bigloop;
                                                    }
                                                }
                                            }
                                            else break;
                                        }
                                    }

                                }
                            }
                        }
                    bigloop:
                        if (fits)
                        {
                            for (int i = 0; i < w.Length; i++)
                            {
                                //find the cell under this letter
                                int ix = x + dir.dx * i;
                                int iy = y + dir.dy * i;
                                results[f(ix, iy)] = w[i];
                            }
                            //push the new state into the grid stack
                            //in case we need to undoify
                            grids.Push((char[])results.Clone());
                            words_left.Dequeue();
                            added_words.Push((w, (x, y), (ex, ey)));
                            inserted = true;
                            break;
                        }
                    }


                }
                //if we failed to insert, pop the stack and try again
                if (!inserted)
                {
                    words_left.Enqueue(added_words.Pop().word);
                    results = grids.Pop();
                }
            }

            //store the map of things
            var old_chars = (char[])results.Clone();
            var steps = new int[W * H];
            var rands = new byte[W * H];

            Random.Shared.NextBytes(rands);

            for (int i = 0; i < H; ++i)
            {
                for (int j = 0; j < W; ++j)
                {
                    Console.Write((results[f(j, i)] == '\0' ? ' ' : results[f(j, i)]) + " ");
                }
                Console.WriteLine();
            }


            for (int i = 0; i < results.Length; ++i)
            {
                if (i < 0) continue;
                //try to fill
                int x = i % W;
                int y = i / W;
                if (old_chars[f(x, y)] != 0) continue;

                bool fits = true;
                if (steps[i] >= letters.Count())
                {
                    steps[i] = 0;
                    fits = false;
                    goto checking_letter;
                }

                for (int ln = steps[i]; ln < letters.Count; ++ln)
                {
                    ++steps[i];
                    //get the letter
                    var l = letters[(ln + rands[i]) % letters.Count];
                    results[(f(x, y))] = l;

                    //now check the 4 main axis (N/S, E/W, NE/SW, NW/SE) 
                    bool passes_check = true;
                    for (int check_dir = 0; check_dir < 4; ++check_dir)
                    {
                        var check_axis = dirs[check_dir];
                        //we need to start from the earliest point that we can reach
                        int cx = x;
                        int cy = y;

                        List<char> axis_letters = [];
                        List<(int x, int y)> axis_steps = [];

                        //walk back to the edge, or the first empty cell
                        while (true)
                        {
                            cx -= check_axis.dx;
                            cy -= check_axis.dy;
                            if (cx != int.Clamp(cx, 0, W - 1) || cy != int.Clamp(cy, 0, H - 1)) break;
                            if (cx != x && cy != y && g(cx, cy) == 0) break;
                        }

                        //now walk forwards to the edge, or the first empty cell
                        while (true)
                        {
                            cx += check_axis.dx;
                            cy += check_axis.dy;
                            if (cx != int.Clamp(cx, 0, W - 1) || cy != int.Clamp(cy, 0, H - 1)) break;
                            //add the letter to this step
                            //the cell is technically empty
                            if (g(cx, cy) == 0) break;
                            axis_letters.Add(g(cx, cy));
                            axis_steps.Add((cx, cy));
                        }

                        //now check every string of at least 3 letters
                        for (int start = 0; start < axis_letters.Count; ++start)
                        {
                            //1. Walk backwards
                            if (start < axis_letters.Count - word_max_len)
                            {
                                var r = wd.root;
                                List<(int x, int y)> check = [];

                                for (int end = start; end < axis_letters.Count; ++end)
                                {
                                    //add this step to the check
                                    check.Add(axis_steps[end]);
                                    if (r.Children.TryGetValue((byte)g(axis_steps[end].x, axis_steps[end].y), out var rr) && rr != null)
                                    {
                                        r = rr;
                                        if (r.IsWord && int.Abs(start - end) >= word_max_len && check.Contains((x, y)))
                                        {
                                            passes_check = false;
                                            goto bonk;
                                        }
                                    }
                                    else break;
                                }
                            }

                            //1. Walk backwards
                            if (start >= word_max_len)
                            {
                                var r = wd.root;
                                List<(int x, int y)> check = [];
                                for (int end = start; end < axis_letters.Count; ++end)
                                {
                                    check.Add(axis_steps[end]);
                                    if (r.Children.TryGetValue((byte)g(axis_steps[end].x, axis_steps[end].y), out var rr) && rr != null)
                                    {
                                        r = rr;
                                        if (r.IsWord && int.Abs(start - end) >= word_max_len && check.Contains((x, y)))
                                        {
                                            passes_check = false;
                                            goto bonk;
                                        }
                                    }
                                    else break;
                                }
                            }
                        }
                    }

                bonk:
                    //check if the letter fits
                    fits = passes_check;
                    if (fits) break;
                }

            checking_letter:
                if (!fits)
                {
                    //unset the letter
                    results[i] = '\0';
                    steps[i] = 0;
                    rands[i] = (byte)Random.Shared.Next(255);

                    while (true)
                    {
                        //move back until we find another originally empty cell
                        i -= 1;
                        if (i >= 0 && old_chars[i] != 0)
                            continue;
                        break;
                    }
                    //step back once more to account for the loop increment
                    i -= 1;
                }
            }



            for (int i = 0; i < H; ++i)
            {
                for (int j = 0; j < W; ++j)
                {
                    Console.Write((results[f(j, i)] == '\0' ? ' ' : results[f(j, i)]) + " ");
                }
                Console.WriteLine();
            }


            return results;

        }

    }
}
