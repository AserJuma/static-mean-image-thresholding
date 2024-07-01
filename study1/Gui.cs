using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace study1
{
    public class Gui : Form
    {
        //Bitmap
        private Bitmap _sourceBitmap;
        private Bitmap _clonedBitmap;
        private Bitmap _retBitmap;
        //Main form items
        private MainMenu _myMainMenu;
        private MenuItem _menuItem1;
        private MenuItem _menuItem2;
        //In menuItem1:
        private MenuItem _fileLoad;
        private MenuItem _fileExit;
        //In menuItem2:
        private MenuItem _staticThreshold;
        private MenuItem _meanThreshold;
        
        private readonly System.ComponentModel.Container _components = null;

        private bool _thresholdApplied;
        private Gui()
        {
            InitializeComponent();
            _sourceBitmap = new Bitmap(2, 2);
        }

        
        protected override void Dispose(bool disposing)
        {
            if (disposing) _components?.Dispose();
            base.Dispose(disposing);
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics gg = e.Graphics;
            if (!_thresholdApplied)
            {
                gg.DrawImage(_sourceBitmap, new Rectangle
                (this.AutoScrollPosition.X, this.AutoScrollPosition.Y, 
                    (_sourceBitmap.Width), (_sourceBitmap.Height)
                ));
            }
            else
            {
                this.ClientSize = new Size((_sourceBitmap.Width * 2), (_sourceBitmap.Height));
                
                gg.DrawImage(_sourceBitmap, new Rectangle
                (this.AutoScrollPosition.X, this.AutoScrollPosition.Y, 
                    (_sourceBitmap.Width), (_sourceBitmap.Height)
                ));
                gg.DrawImage(_clonedBitmap, new Rectangle
                (this.AutoScrollPosition.X + (_sourceBitmap.Width), this.AutoScrollPosition.Y, 
                    (_clonedBitmap.Width), (_clonedBitmap.Height)
                ));
            }
        }

        private void InitializeComponent()
        {
            // Inits the main menu, it's tabs, then their child tabs
            this._myMainMenu = new MainMenu();
            this._menuItem1 = new MenuItem();
            this._menuItem2 = new MenuItem();
            
            this._fileLoad = new MenuItem();
            this._fileExit = new MenuItem();
            
            this._staticThreshold = new MenuItem();
            this._meanThreshold = new MenuItem();
            
            //Main Menu Tabs
            this._myMainMenu.MenuItems.AddRange(new[]
            {
                this._menuItem1,
                this._menuItem2,
            });

            // First Dropdown Menu Tab
            this._menuItem1.Text = "File";
            this._menuItem1.Index = 0;
            this._menuItem1.MenuItems.AddRange(new []
            {
            this._fileLoad,
            this._fileExit,
                });
            this._fileLoad.Index = 0;
            this._fileLoad.Shortcut = Shortcut.CtrlL;
            this._fileLoad.Text = "Load";
            this._fileLoad.Click += this.File_Load;

            this._fileExit.Index = 1;
            this._fileExit.Shortcut = Shortcut.CtrlE;
            this._fileExit.Text = "Exit";
            this._fileExit.Click += this.File_Exit;

            // Second Dropdown Menu Tab
            this._menuItem2.Text = "Binarization";
            this._menuItem2.Index = 1;
            this._menuItem2.MenuItems.AddRange(new []
            {
            this._staticThreshold,
            this._meanThreshold,
                });
            this._staticThreshold.Index = 0;
            this._staticThreshold.Text = "Static Threshold";
            this._staticThreshold.Click += this._applyStaticThreshold;
            
            this._meanThreshold.Index = 1;
            this._meanThreshold.Text = "Mean Threshold";
            this._meanThreshold.Click += this._applyMeanThreshold;
            
            //Gui
            this.AutoScaleBaseSize = new Size(5, 13);
            this.BackColor = SystemColors.ControlLight;
            this.ClientSize = new Size(350, 350);
            this.Menu = this._myMainMenu;
            this.Name = $"Study into .NET GUI & Image Processing";
            this.Text = "Thresholding";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void File_Load(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp|TIFF files (.tiff)|*.tiff|Jpeg files (*.jpg)|*.jpg|All valid files (*.bmp/.tiff/*.jpg)|*.bmp;*.tiff;*.jpg";
            openFileDialog.FilterIndex = 4;
            openFileDialog.RestoreDirectory = false;
            if (DialogResult.OK != openFileDialog.ShowDialog()) return;
            _sourceBitmap?.Dispose();
            _sourceBitmap = (Bitmap)Image.FromFile(openFileDialog.FileName, false);
            if(_sourceBitmap.PixelFormat != PixelFormat.Format8bppIndexed) return; // ONLY Format8bppIndexed will currently load
            this.ClientSize = new Size((_sourceBitmap.Width), (_sourceBitmap.Height));
            _thresholdApplied = false; _clonedBitmap = null; // _destBitmap = _cloneBitmapFromSource(_sourceBitmap); 
            this.AutoScroll = true;
            this.Invalidate();
        }
        private void Save_Btmp(Bitmap b)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.InitialDirectory = "c:\\";
            saveFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp|Jpeg files (*.jpg)|*.jpg|All valid files (*.bmp/*.jpg)|*.bmp;*.jpg";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = false;

            if (DialogResult.OK == saveFileDialog.ShowDialog())
            {
                b.Save(saveFileDialog.FileName);
            }
        }
        private void File_Exit(object sender, EventArgs e) { this.Close(); }

        private static Bitmap _cloneBitmap(Bitmap bitmap)
        {
            return (Bitmap)bitmap.Clone();
        }
        
        private void _applyStaticThreshold(object sender, EventArgs e)
        {
            var input = new Input();
            input.Text = "Threshold";
            if (DialogResult.OK != input.ShowDialog()) return;
            _clonedBitmap = _cloneBitmap(_sourceBitmap);
            _applyThresh(_clonedBitmap, input.Nvalue);
        }
        private void _applyMeanThreshold(object sender, EventArgs e)
        {
            int th = BitmapOperations.GetOptimalThreshold(_sourceBitmap);
            _clonedBitmap = _cloneBitmap(_sourceBitmap);
            _applyThresh(_clonedBitmap, th);
            Console.WriteLine("Optimal Threshold Value: " + th);
        }
        //private Stopwatch _stopwatch = new Stopwatch();
        private void _applyThresh(Bitmap b, int t)
        {
            //_stopwatch.Restart();
            if (t < 0) return;
            _retBitmap = BitmapOperations.ApplyThreshold(b, t);
            //_stopwatch.Stop();
            //Console.WriteLine($"Time Taken : {_stopwatch.ElapsedMilliseconds} ms");
            _thresholdApplied = true;
            this.Invalidate();
            Save_Btmp(_retBitmap);
        }
        private static void Main()
        {
            Application.Run(new Gui());
        }
    }
}

// TODO: GUI ISSUE: Fix Static input prompt
// TODO: GUI ISSUE: Fix window scaling issue when reloading to a smaller image than the previous