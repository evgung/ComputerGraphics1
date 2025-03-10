using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly Random random = new();
        private readonly int cellSize = 10;
        private readonly Brush fillBrush = new SolidBrush(Color.Gray);
        private Vector2 brezenhamBegin;

        private readonly Vector2[] points = new[]
        {
            new Vector2(200, 20),
            new Vector2(100, 70),
            new Vector2(100, 200),
            new Vector2(300, 70),
            new Vector2(300, 200),
            new Vector2(200, 250),
        };

        private readonly Dictionary<int, List<int>> toConnect = new()
        {
            [1] = new() { 0, 2, 3, 5 },
            [3] = new() { 0, 4, 5 },
            [5] = new() { 2, 4 },
        };

        public Form1()
        {
            InitializeComponent();
            pictureBox.Paint += DrawGrid;
            pictureBox.Paint += DrawFigureCDA;
            pictureBox.Paint += DrawFigureBresenham;
            brezenhamBegin = new(pictureBox.Width / 2, 0);
        }

        public void DrawGrid(object? sender, PaintEventArgs e)
        {
            var graph = e.Graphics;
            var width = pictureBox.Width;
            var height = pictureBox.Height;

            for (var x = 0; x < width; x += cellSize)
            {
                graph.DrawLine(Pens.Black, x, 0, x, Height);
            }

            for (var y = 0; y < height; y += cellSize)
            {
                graph.DrawLine(Pens.Black, 0, y, Width, y);
            }

            var pen = new Pen(Color.Red, 3);
            graph.DrawLine(pen, 0, height / 2, width, height / 2);
            graph.DrawLine(pen, width / 2, 0, width / 2, height / 2);
        }

        public void DrawFigureCDA(object? sender, PaintEventArgs e)
        {
            foreach (var pair in toConnect)
            {
                foreach (var point in pair.Value)
                {
                    DrawLineCDA(points[pair.Key], points[point], e.Graphics);
                }
            }
        }

        public void DrawLineCDA(Vector2 from, Vector2 to, Graphics graphics)
        {
            var pixelFrom = ToPixelCoords(from);
            var pixelTo = ToPixelCoords(to);
            var dx = (pixelTo.X - pixelFrom.X) / (float)cellSize;
            var dy = (pixelTo.Y - pixelFrom.Y) / (float)cellSize;
            var L = (Math.Abs(dx) > Math.Abs(dy)) ? Math.Abs(dx) : Math.Abs(dy);
            dx = dx / L * cellSize;
            dy = dy / L * cellSize;

            var x = from.X;
            var y = from.Y;

            for (var i = 0; i <= L; i++)
            {
                FillPixel(x, y, graphics);
                x += dx;
                y += dy;
            }
        }

        public void DrawFigureBresenham(object? sender, PaintEventArgs e)
        {
            foreach (var pair in toConnect)
            {
                foreach (var point in pair.Value)
                {
                    DrawLineBresenham(
                        brezenhamBegin + points[pair.Key],
                        brezenhamBegin + points[point], 
                        e.Graphics
                    );
                }
            }
        }

        public void DrawLineBresenham(Vector2 from, Vector2 to, Graphics graphics)
        {
            var x0 = ToPixelCoords(from).X;
            var y0 = ToPixelCoords(from).Y;
            var x1 = ToPixelCoords(to).X;
            var y1 = ToPixelCoords(to).Y;
            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? cellSize : -cellSize; // Направление по X
            var sy = (y0 < y1) ? cellSize : -cellSize; // Направление по Y
            var err = dx - dy;

            while (true)
            {
                FillPixel(x0, y0, graphics);
                if (x0 == x1 && y0 == y1) break;

                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public Point ToPixelCoords(Vector2 coords)
        {
            return new Point(
                (int)coords.X / cellSize * cellSize,
                (int)coords.Y / cellSize * cellSize
            );
        }

        public void FillPixel(float x, float y, Graphics graphics)
        {
            var coords = ToPixelCoords(new Vector2(x, y));
            graphics.FillRectangle(fillBrush, coords.X, coords.Y, cellSize, cellSize);
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            var from = new Vector2(
                random.Next(0, pictureBox.Width),
                random.Next(pictureBox.Height / 2, pictureBox.Height)
            );
            var to = new Vector2(
                random.Next(0, pictureBox.Width),
                random.Next(pictureBox.Height / 2, pictureBox.Height)
            );

            using (var graphics = pictureBox.CreateGraphics())
            {
                DrawLineBresenham(from, to, graphics);
            }
        }
    }
}
