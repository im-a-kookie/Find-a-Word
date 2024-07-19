using FindaWord;
using FindaWord.Properties;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

class MainClass
{
    public static void Main(string[] args)
    {

        Console.WriteLine("Welcome to Word Builder!");

        Console.WriteLine("Commands: examplewords [letters], generate [W] [H], exit");

        while(true)
        {
            string? s = Console.ReadLine();
            if (s == null) continue;
            if (s == "exit")
            {
                return;
            }
            else if (s.StartsWith("examplewords"))
            {
                int n = s.IndexOf(' ');
                if (n <= 0)
                {
                    Console.WriteLine("Invalid syntax!");
                    continue;
                }

                string letter_str = s.Substring(n);
                var letters = GetLetters(letter_str);
                Console.WriteLine("Constructing Dictionary...");
                Stopwatch sw = Stopwatch.StartNew();
                WordDictionary dictionary = new WordDictionary();
                dictionary.BuildDictionary(File.ReadAllText("Resources/english.txt"), String.Join("", letters));
                Console.WriteLine($"Dictionary Built. Words: {dictionary.allwords.Count} ({sw.ElapsedMilliseconds}ms. )");
                Console.WriteLine();

                //Now we need to select some words
                HashSet<string> wordChoices = [];
                int k = 0;
                while (wordChoices.Count < 30 && dictionary.allwords.Count > wordChoices.Count)
                {
                    ++k;
                    if (k > 30) break; ;
                    for (int len = 4; len < 9; ++len)
                    {
                        var l = dictionary.allwords.Where(x => x.Length == len).ToList();
                        for (int i = 0; i < 5 && l.Count > 0; i++)
                        {
                            int index = Random.Shared.Next(l.Count);
                            string word = l[index];
                            //remove some dumb things
                            if (word.EndsWith("s")) continue;
                            wordChoices.Add(word);
                            l.RemoveAt(index);
                        }
                    }
                }

                Console.WriteLine("Suggested words:");
                Console.WriteLine(String.Join(", ", wordChoices));

            }
            else if (s.StartsWith("generate"))
            {
                var parts = s.Split(' ');
                int W, H;
                if (int.TryParse(parts[1], out W) && int.TryParse(parts[2], out H))
                {
                    List<string> words = null;
                    while (words == null || words.Count <= 0)
                    {
                        Console.WriteLine("Enter Words (single line): ");
                        words = Console.ReadLine()!.Replace(",", " ").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    //get the letters for the puzzle
                    List<char> letters = [];
                    int _maxW = -1;
                    foreach (string word in words)
                    {
                        foreach (char c in word)
                        {
                            if (!letters.Contains(c)) letters.Add(c);
                        }
                        _maxW = int.Max(_maxW, word.Length);
                    }

                    if (_maxW > W || _maxW > H)
                    {
                        Console.WriteLine("Error: Grid is too small!");
                        continue;
                    }

                    Stopwatch sw = Stopwatch.StartNew();
                    WordDictionary dictionary = new WordDictionary();
                    dictionary.BuildDictionary(File.ReadAllText("Resources/english.txt"), String.Join("", letters));
                    Console.WriteLine($"Dictionary Built. Words: {dictionary.allwords.Count} ({sw.ElapsedMilliseconds}ms. )");
                    Console.WriteLine();

                    //Now we can build the grid yay
                    Console.WriteLine("Building Puzzle...");
                    var puzzle = Generator.MakePuzzle(dictionary, words, W, H);
                    using var img = Renderer.DrawPuzzle(W, H, puzzle, words);
                    img.Save("Puzzle.png", ImageFormat.Png);
                    Console.WriteLine("Saved to Puzzle.jpg");
                }


            }
            else
            {
                Console.WriteLine("Please enter a valid command!");
            }
              

        }


    }







    /// <summary>
    /// Prompts the user to get a list of letters
    /// </summary>
    /// <returns></returns>
    static IEnumerable<char> GetLetters(string str)
    {

        //process into a sorted list
        List<char> letters = [];
        if ((str?.Contains("*") ?? true) || str.Length == 0) str = "abcdefghjiklmnopqrstuvwxyz";
        str = str.ToLower().Replace(" ", "");
        foreach (char c in str)
        {
            if (!letters.Contains(c)) letters.Add(c);
        }
        letters.Sort();
        //Good.
        return letters;

    }


}