using System;
using System.Drawing;
//using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace study1
{
    public class Input : Form
    {
        private Button _ok;
        private TextBox _value;
        private Label _label;

        private readonly Container _components = null;
        
        public int Nvalue => (Convert.ToInt32(_value.Text, 10));

        //set => _value.Text = value.ToString();
        public Input()
        {
            InitializeComponent();

            _ok.DialogResult = DialogResult.OK;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_components != null)
                {
                    _components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        private void InitializeComponent()
        {
            this._ok = new Button();
            this._value = new TextBox();
            this._label = new Label();
            this.SuspendLayout();
            
            this._ok.Location = new Point(16, 56);
            this._ok.Name = "_ok";
            this._ok.TabIndex = 0;
            this._ok.Text = "OK";
            
            this._value.Location = new Point(80, 16);
            this._value.Name = "_value";
            this._value.TabIndex = 2;
            this._value.Text = "";
             
            this._label.Location = new Point(16, 16);
            this._label.Name = "_label";
            this._label.Size = new Size(56, 23);
            this._label.TabIndex = 3;
            this._label.Text = "Enter a Value";
            
            this.AcceptButton = this._ok;
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(250, 85);
            this.Controls.AddRange(new Control[] 
            {
                this._label,
                this._value,
                this._ok,
                                                                          
            });
            this.Name = "Input";
            this.Text = "Input";
            this.Load += this.Input_Load;
            this.ResumeLayout(false);

        }
        private void Input_Load(object sender, EventArgs e) {}
    }
}
