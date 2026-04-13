using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ex_plorer
{
    internal static class ToolbarImages
    {
        internal const int Back = 0;
        internal const int Forward = 1;
        internal const int Up = 2;
        internal const int Cut = 3;
        internal const int Copy = 4;
        internal const int Paste = 5;
        internal const int Delete = 6;
        internal const int Properties = 7;
        internal const int LargeIcons = 8;
        internal const int SmallIcons = 9;
        internal const int List = 10;
        internal const int Details = 11;

        private const int IDB_STD_SMALL_COLOR = 120;
        private const int IDB_VIEW_SMALL_COLOR = 124;
        private const int IDB_HIST_SMALL_COLOR = 128;

        private const int STD_CUT = 0;
        private const int STD_COPY = 1;
        private const int STD_PASTE = 2;
        private const int STD_DELETE = 5;
        private const int STD_PROPERTIES = 10;

        private const int VIEW_LARGEICONS = 0;
        private const int VIEW_SMALLICONS = 1;
        private const int VIEW_LIST = 2;
        private const int VIEW_DETAILS = 3;
        private const int VIEW_PARENTFOLDER = 8;

        private const int HIST_BACK = 0;
        private const int HIST_FORWARD = 1;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadBitmap(IntPtr hInstance, IntPtr lpBitmapName);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        internal static ImageList Create()
        {
            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(16, 16);
            imageList.TransparentColor = Color.Magenta;

            string dllPath = GetComctl32Path();
            IntPtr hLib = LoadLibraryEx(dllPath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);

            if (hLib == IntPtr.Zero)
                throw new InvalidOperationException("Failed to load comctl32.dll from assets folder: " + dllPath);

            try
            {
                Bitmap[] stdImages = LoadBitmapStrip(hLib, IDB_STD_SMALL_COLOR, 16);
                Bitmap[] viewImages = LoadBitmapStrip(hLib, IDB_VIEW_SMALL_COLOR, 16);
                Bitmap[] histImages = LoadBitmapStrip(hLib, IDB_HIST_SMALL_COLOR, 16);

                // Back (0)
                imageList.Images.Add(histImages != null ? histImages[HIST_BACK] : CreateFallbackArrow(false));
                // Forward (1)
                imageList.Images.Add(histImages != null ? histImages[HIST_FORWARD] : CreateFallbackArrow(true));
                // Up (2)
                imageList.Images.Add(viewImages[VIEW_PARENTFOLDER]);
                // Cut (3)
                imageList.Images.Add(stdImages[STD_CUT]);
                // Copy (4)
                imageList.Images.Add(stdImages[STD_COPY]);
                // Paste (5)
                imageList.Images.Add(stdImages[STD_PASTE]);
                // Delete (6)
                imageList.Images.Add(stdImages[STD_DELETE]);
                // Properties (7)
                imageList.Images.Add(stdImages[STD_PROPERTIES]);
                // LargeIcons (8)
                imageList.Images.Add(viewImages[VIEW_LARGEICONS]);
                // SmallIcons (9)
                imageList.Images.Add(viewImages[VIEW_SMALLICONS]);
                // List (10)
                imageList.Images.Add(viewImages[VIEW_LIST]);
                // Details (11)
                imageList.Images.Add(viewImages[VIEW_DETAILS]);
            }
            finally
            {
                FreeLibrary(hLib);
            }

            return imageList;
        }

        private static string GetComctl32Path()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "ex_plorer_comctl32.dll");
            if (!System.IO.File.Exists(tempPath))
            {
                Stream stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("ex_plorer.Assets.comctl32.dll");
                if (stream == null)
                    throw new FileNotFoundException("Embedded comctl32.dll resource not found.");
                try
                {
                    FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                    try
                    {
                        byte[] buffer = new byte[8192];
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, read);
                        }
                    }
                    finally
                    {
                        fs.Dispose();
                    }
                }
                finally
                {
                    stream.Dispose();
                }
            }
            return tempPath;
        }

        private static Bitmap[] LoadBitmapStrip(IntPtr hLib, int resourceId, int iconSize)
        {
            IntPtr hBitmap = LoadBitmap(hLib, (IntPtr)resourceId);
            if (hBitmap == IntPtr.Zero)
                return null;

            try
            {
                Bitmap strip = Image.FromHbitmap(hBitmap);
                try
                {
                    int count = strip.Width / iconSize;
                    Bitmap[] images = new Bitmap[count];
                    for (int i = 0; i < count; i++)
                    {
                        Bitmap icon = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb);
                        Graphics g = Graphics.FromImage(icon);
                        try
                        {
                            g.DrawImage(strip,
                                new Rectangle(0, 0, iconSize, iconSize),
                                new Rectangle(i * iconSize, 0, iconSize, iconSize),
                                GraphicsUnit.Pixel);
                        }
                        finally
                        {
                            g.Dispose();
                        }
                        icon.MakeTransparent(Color.FromArgb(255, 0, 255));
                        images[i] = icon;
                    }
                    return images;
                }
                finally
                {
                    strip.Dispose();
                }
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        private static Bitmap CreateFallbackArrow(bool forward)
        {
            Bitmap bitmap = new Bitmap(16, 16);
            bitmap.MakeTransparent();
            Graphics graphics = Graphics.FromImage(bitmap);
            Pen pen = new Pen(Color.FromArgb(0, 0, 128), 2f);
            SolidBrush fillBrush = new SolidBrush(Color.FromArgb(0, 0, 128));
            try
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                if (forward)
                {
                    graphics.DrawArc(pen, 4, 4, 8, 8, 250f, 240f);
                    graphics.FillPolygon(fillBrush, new Point[3] { new Point(11, 6), new Point(14, 8), new Point(11, 10) });
                }
                else
                {
                    graphics.DrawArc(pen, 4, 4, 8, 8, 70f, 240f);
                    graphics.FillPolygon(fillBrush, new Point[3] { new Point(5, 6), new Point(2, 8), new Point(5, 10) });
                }
            }
            finally
            {
                fillBrush.Dispose();
                pen.Dispose();
                graphics.Dispose();
            }
            return bitmap;
        }
    }
}
