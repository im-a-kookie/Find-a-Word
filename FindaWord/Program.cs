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
        var letters = GetLetters();
        Console.WriteLine("Constructing Dictionary...");
        Stopwatch s = Stopwatch.StartNew();

        WordDictionary w = new WordDictionary();
        w.BuildDictionary(File.ReadAllText("Resources/english.txt"), String.Join("",letters));
        Console.WriteLine($"Dictionary Built. Words: {w.allwords.Count} ({s.ElapsedMilliseconds}ms. )");
        Console.WriteLine();

        
        //Now we need to select some words
        HashSet<string> wordChoices = [];
        for(int len = 4; len < 9; ++len)
        {
            var l = w.allwords.Where(x => x.Length == len).ToList();
            for(int i = 0; i < 5 && l.Count > 0; i++)
            {
                int n = Random.Shared.Next(l.Count);
                string word = l[n];
                //remove some dumb things
                if (word.EndsWith("s")) continue;
                wordChoices.Add(word);
                l.RemoveAt(n);
            }
        }

        Console.WriteLine("Suggested words:");
        Console.WriteLine(String.Join(", ", wordChoices));

        Console.WriteLine();
        List<string> words = null;
        while (words == null || words.Count <= 0)
        {
            Console.WriteLine("Enter Words (any number): ");
            words = Console.ReadLine()!.Replace(",", " ").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        Console.WriteLine($"Done! Words: {words.Count}");

        //Get the longest word
        int _maxW = 0;
        words.Select(x => _maxW = int.Max(x.Length, _maxW));

        Console.WriteLine("Ready to build grid. Commands:\ngenerate W H  -> generates a W * H grid.\nExit         -> exits");


        while (true)
        {
            string? str = Console.ReadLine();
            if (str != null && str.Contains("exit")) return;

            else if (str != null && str.Contains("generate"))
            {
                var parts = str.Split(' ');
                if(parts.Count() == 3)
                {
                    int W, H;
                    if (int.TryParse(parts[1], out W) && int.TryParse(parts[2], out H))
                    {
                        W = int.Max(W, _maxW);
                        H = int.Max(H, _maxW);
                        //Now we can build the grid yay
                        Console.WriteLine("Building Puzzle...");
                        var puzzle = Generator.MakePuzzle(w, words, W, H);
                        using var img = Renderer.DrawPuzzle(W, H, puzzle, words);
                        img.Save("Puzzle.png", ImageFormat.Png);
                        Console.WriteLine("Saved to Puzzle.jpg");
                        


                    }
                }
            }
        }

        Console.ReadLine();
        return;

    }







    /// <summary>
    /// Prompts the user to get a list of letters
    /// </summary>
    /// <returns></returns>
    static IEnumerable<char> GetLetters()
    {
        Console.Write("Enter the desired letters (empty or * for A-Z): ");
        var str = Console.ReadLine() ?? "";
        if (str == null)
        {
            Console.WriteLine("Bonk. Please enter usable letters!");
            GetLetters();
        }

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
        Console.WriteLine($"Letters: {String.Join(", ", letters)}");
        return letters;

    }


}