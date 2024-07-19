using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindaWord
{
    class WordDictionary
    {
        public Slice root = new Slice(null, -1);
        public List<string> allwords;

        public WordDictionary()
        {
            allwords = new List<string>();
        }

        /// <summary>
        /// Builds the dictionary from the provided list of words (newline separator)
        /// </summary>
        /// <param name="w">The word stringe</param>
        /// <param name="allowed_chars">A string with all of the allowed letters. Speeds up dictionary building if known and small.</param>
        public void BuildDictionary(string w, string? allowed_chars = null)
        {
            bool[] flags = new bool[255];
            if (allowed_chars == null) Array.Fill(flags, true);
            else
            {
                Array.Fill(flags, false);
                foreach(char b in allowed_chars)
                {
                    flags[(int)b] = true;
                }
            }

            int pos = 0;
            var ss = w.AsSpan();
            //keep jumping word to word
            while (ss.Length > 1)
            {
                //get the next line
                int n = ss.IndexOf('\n');
                if (n < 0) n = ss.Length;

                //take out this part of the stream
                var s = ss.Slice(0, n);
                if (s[^1] == '\r') s = s.Slice(0, s.Length - 1); 
                if (s.Length > 0)
                {
                    //check that it only contains the desired letters
                    bool valid = true;
                    foreach(var c in s)
                    {
                        if (!flags[(int)c] || c < 'a' || c > 'z')
                        {
                            valid = false;
                            break;
                        }
                    }
                    //It does, so build it into root
                    if (valid)
                    {
                        root.BuildSlice(s.ToString(), 0);
                        allwords.Add(s.ToString());
                    }
                }
                if (n < ss.Length)
                    ss = ss.Slice(n + 1, ss.Length - n - 1);
                else break;
            }
        }

        public bool IsValid(string s, bool full = false)
        {
            var c = root;
            for(int i = 0; i < s.Length; i++)
            {
                if (!c.Children.ContainsKey((byte)s[i]))
                    return false;
                c = c.Children[(byte)s[i]];
            }
            return !full || c.IsWord;
        }
    }

    /// <summary>
    /// A node for the dictionary TRIE
    /// </summary>
    public class Slice
    {
        public static int memory = 0;
        public Slice? Parent;
        public int Index;
        public bool IsWord;
        public Dictionary<byte, Slice> Children = [];
        public string myword = "";

        public Slice(Slice parent, int index)
        {
            Parent = parent;
            Index = index;
        }

        public void BuildSlice(string word, int pos)
        {
            myword = word.Remove(pos);
            if (pos >= word.Length)
            {
                IsWord = true;
                return;
            }

            byte n = (byte)(word[pos]);

            if (n < 'a' || n > 'z')
                return;

            if (Children.ContainsKey(n))
            {
                Children[n].BuildSlice(word, pos + 1);
            }
            else
            {
                var s = new Slice(this, n);
                s.BuildSlice(word, pos + 1);
                Children.TryAdd(n, s);
            }
        }
    }
}
