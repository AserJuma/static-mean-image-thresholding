using System;
using System.Windows.Forms;
using System.Drawing;

namespace study1
{
    public class Form1 : Form
    {
        //Bitmap
        private Bitmap _myBitmap;
        //Main form items
        private MainMenu _myMainMenu;
        private MenuItem _menuItem1;
        private MenuItem _menuItem2;
        private MenuItem _menuItem3;
        //In menuItem1:
        private MenuItem _fileLoad;
        private MenuItem _fileExit;
        //In menuItem2:
        private MenuItem _filterGrayScale;
        private MenuItem _staticThreshold;
        private MenuItem _meanThreshold;
        private TextBox _textBox1;
        private Label _label1;
        private readonly System.ComponentModel.Container _components = null;

        private Form1()
        {
            InitializeComponent();
            _myBitmap = new Bitmap(2, 2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // Inits the main menu, it's tabs, then their child tabs
            this._myMainMenu = new MainMenu();
            this._menuItem1 = new MenuItem();
            this._menuItem2 = new MenuItem();

            this._menuItem3 = new MenuItem();
            
            this._fileLoad = new MenuItem();
            this._fileExit = new MenuItem();
            
            this._filterGrayScale = new MenuItem();
            this._staticThreshold = new MenuItem();
            this._meanThreshold = new MenuItem();
            this._label1 = new Label();
            
            //Main Menu Tabs
            this._myMainMenu.MenuItems.AddRange(new[]
            {
                this._menuItem1,
                this._menuItem2,
                //this._menuItem3,
            });

            //First Dropdown Menu Tab
            //Name
            this._menuItem1.Text = "File";
            //Index
            this._menuItem1.Index = 0;
            //Child nodes
            this._menuItem1.MenuItems.AddRange(new []
            {
            this._fileLoad,
            this._fileExit,
                });

            //File Load
            this._fileLoad.Index = 0;
            this._fileLoad.Shortcut = Shortcut.CtrlL;
            this._fileLoad.Text = "Load";
            this._fileLoad.Click += this.File_Load;

            //File Exit
            this._fileExit.Index = 1;
            this._fileExit.Text = "Exit";
            this._fileExit.Click += this.File_Exit;

            //Third Dropdown Menu Tab
            //Name
            this._menuItem2.Text = "Operations";
            //Index
            this._menuItem2.Index = 1;
            //Child nodes
            this._menuItem2.MenuItems.AddRange(new []
            {
            this._filterGrayScale,
            this._staticThreshold,
            this._meanThreshold,
                });

            this._filterGrayScale.Index = 0;
            this._filterGrayScale.Text = "Grayscale";
            this._filterGrayScale.Click += this._applyGrayScale;

            this._staticThreshold.Index = 1;
            this._staticThreshold.Text = "Static Threshold";
            this._staticThreshold.Click += this._applyStaticThreshold;
            
            this._meanThreshold.Index = 2;
            this._meanThreshold.Text = "Dynamic Threshold";
            this._meanThreshold.Click += this._applyDynamicThreshold;
            
            this._textBox1 = new TextBox();
            this._textBox1.Name = "_textBox1";
            this._textBox1.ReadOnly = true;
            this._textBox1.Size = new System.Drawing.Size(142, 20);
            
            this._label1.AutoSize = true;
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(109, 13);
            this._label1.TabIndex = 4;
            this._label1.Text = "Otsu Threshold Value:";
            
            //Form1
            this.AutoScaleBaseSize = new Size(5, 13);
            this.BackColor = SystemColors.ControlLight;
            this.ClientSize = new Size(350, 350);
            this.Menu = this._myMainMenu;
            this.Name = $"Study into .NET GUI & Image Processing";
            this.Text = "GrayScale & Thresholding";
            ActiveControl = null;
            this.Focus();
            
            this.Load += this.Form1_Load;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        //Helper function to resize form window to fit the image
        private void ResizeFormToFitImage(Bitmap image)
        {
            this.ClientSize = new Size((image.Width + 200), (image.Height + 5));
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void Load_window(Bitmap bmp)
        {
            ResizeFormToFitImage(bmp); 
            
            int textBoxX = bmp.Width + 10; 
            int textBoxY = 100; 
            
            _textBox1.Location = new Point(textBoxX, textBoxY);
            _label1.Location = new Point(textBoxX, textBoxY - 16);
            _textBox1.Text = "";
            
            Controls.Add(this._label1);
            Controls.Add(this._textBox1);
        }
        //File Load Function
        private void File_Load(object sender, EventArgs e)
        {
            
            var openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp|Jpeg files (*.jpg)|*.jpg|All valid files (*.bmp/*.jpg)|*.bmp/*.jpg";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;

            if (DialogResult.OK != openFileDialog.ShowDialog()) return;
            
            _myBitmap = (Bitmap)Image.FromFile(openFileDialog.FileName, false);
            
            Load_window(_myBitmap);
            this.Invalidate();
        }
        //File_Exit Function
        private void File_Exit(object sender, EventArgs e)
        {
            this.Close();
        }
        //GrayScale Function
        private void _applyGrayScale(object sender, EventArgs e)
        {
            if (BitmapOperations.Convert2GrayScaleFast(_myBitmap))
                this.Invalidate();
        }
        private void _applyStaticThreshold(object sender, EventArgs e)
        {
            var input = new Input();
            input.Text = "Threshold";
            
            if (DialogResult.OK != input.ShowDialog()) return;
            BitmapOperations.ApplyThreshold(_myBitmap, input.Nvalue);
            this.Invalidate();
        }
        private void _applyDynamicThreshold(object sender, EventArgs e)
        {
            int th = BitmapOperations.GetOptimalThreshold(_myBitmap);
            BitmapOperations.ApplyThreshold(_myBitmap, th);
            _textBox1.Text = th.ToString();
            this.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics gg = e.Graphics;
            gg.DrawImage(_myBitmap, new Rectangle
            (this.AutoScrollPosition.X, this.AutoScrollPosition.Y, 
                (_myBitmap.Width), (_myBitmap.Height)
            ));
        }
        [STAThread]
        // Program Entry Point
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
