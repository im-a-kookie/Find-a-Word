using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FindaWord
{
    internal class Renderer
    {
        /// <summary>
        /// Renders the puzzle and returns an image with all the things
        /// </summary>
        /// <param name="grid_width"></param>
        /// <param name="grid_height"></param>
        /// <param name="results"></param>
        /// <param name="words"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static System.Drawing.Image DrawPuzzle(int grid_width, int grid_height, char[] results, IEnumerable<string> words, int width = 1000)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            float size = width / 25;
            using System.Drawing.Font f = new System.Drawing.Font("Consolas", size);
            //calculate the bounds
            //we fit the entire grid to the width
            float boundary = 0.05f;
            float height = width * ((float)grid_height / grid_width);

            RectangleF grid_bounds = new(boundary * width, boundary * width, width * (1 - 2 * boundary), height * (1 - 2 * boundary));

            List<string> _w = new List<string>(words);
            List<SizeF> _s = new List<SizeF>();
            float max_w = -1;
            using (Graphics gg = Graphics.FromImage(new Bitmap(1, 1)))
            {
                foreach(string w in _w)
                {
                    _s.Add(gg.MeasureString(w, f));
                    max_w = float.Max(_s.Last().Width, max_w);
                }
            }

            max_w += size / 2;
            int fits = (int)Math.Floor(grid_bounds.Width / max_w);
            int rows = 2 + _w.Count / fits;

            RectangleF text_bounds = new(boundary * width, grid_bounds.Bottom + 2 * boundary, grid_bounds.Width, rows * (size * 1.1f + boundary));


            Bitmap b = new Bitmap((int)width, (int)(text_bounds.Bottom + 2 * boundary));
            using Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);
            for(int y = 0; y < grid_height; ++y)
            {
                for(int x = 0; x < grid_width; ++x)
                {
                    g.DrawString(results[x + y * grid_width].ToString(), f, Brushes.Black, grid_bounds.Left + x * grid_bounds.Width / grid_width, grid_bounds.Top + y * grid_bounds.Height / grid_height);
                }
            }

            for(int i = 0; i < _w.Count; ++i)
            {
                float x = ((i % fits) * max_w);
                float y = text_bounds.Top + (i / fits) * size * 1.1f;
                g.DrawString(_w[i], f, Brushes.Black, x, y);

            }

            return b;

#pragma warning restore CA1416 // Validate platform compatibility
        }



    }
}
