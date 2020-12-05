using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
namespace VirtualCam
{
	class Viewer : Form
    {
        Brush brush = new SolidBrush(Color.FromArgb(255,0,0,0));
        Graphics graphics;
        int pixelSize;
        XY<int> screenSize;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Timer timer1;
        Bitmap _backBuffer;
        Camera cam;
        protected override void OnPaintBackground(PaintEventArgs pevent) { }
        protected override void Dispose(bool disposing)
        
        {
            if (disposing)
                if (components != null)
                    components.Dispose();
            base.Dispose(disposing);
        }

        public Viewer(Camera cam, XYZ camSize, int pixelSize)
        {
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.cam = cam;
            screenSize = new XY<int>(camSize.x,camSize.z);
            Width = screenSize.x * pixelSize + 20;
            Height = screenSize.y * pixelSize + 20;
            graphics = CreateGraphics();
            this.pixelSize = pixelSize;
            components = new System.ComponentModel.Container();
           
            _backBuffer = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Console.WriteLine(_backBuffer.Width);
            Console.WriteLine(_backBuffer.Height);

            
            timer1 = new System.Windows.Forms.Timer(this.components);
            timer1.Enabled = true;
            timer1.Interval = 20;
            timer1.Tick += new System.EventHandler(timer1_Tick);

           this.KeyDown += new KeyEventHandler(KeyDownEvent);
           this.KeyUp += new KeyEventHandler(KeyUpEvent);

        }

        void timer1_Tick(object sender, System.EventArgs e)
        {
            InitDraw();
            if (InputManager.currentFuncs != null)
            {
                InputManager.DoLogic();                
            }
            cam.Spin_XZAxis5();
            ShowImage();
        }


        Graphics graphics2;
        public void InitDraw()
        {
            graphics2 = Graphics.FromImage(_backBuffer);
            graphics2.Clear(Color.White);
        }
        public void Draw(int x, int y,XYZ_b color)
        {
            x *= pixelSize;
            y *= pixelSize;
           // byte levRatio = (byte)(255 * (level / 9f));
            //brush = new SolidBrush(Color.FromArgb(255, levRatio, levRatio, 0));
            brush = new SolidBrush(Color.FromArgb(255, color.x, color.y,color.z));

            graphics2.FillRectangle(brush, x, y, pixelSize, pixelSize);
        }
        public void ShowImage()
        {
            graphics.DrawImageUnscaled(_backBuffer,0,0);
        }

        public void KeyDownEvent(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.KeyCode);
            InputManager.AddLongClick(e.KeyCode);
            InputManager.DoOneClick(e.KeyCode);

        }
        public void KeyUpEvent(object sender, KeyEventArgs e)
        {
            InputManager.SubLongClick(e.KeyCode);
        }

    }
}