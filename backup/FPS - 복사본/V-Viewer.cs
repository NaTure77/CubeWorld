using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        Bitmap _backBuffer2;
        Bitmap _backBufferTemp;
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
            Width = screenSize.x * pixelSize;
            Height = screenSize.y * pixelSize;
            graphics = CreateGraphics();
            this.pixelSize = pixelSize;
            components = new System.ComponentModel.Container();
           
            _backBuffer = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _backBuffer2 = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Console.WriteLine(_backBuffer.Width);
            Console.WriteLine(_backBuffer.Height);

            
            timer1 = new System.Windows.Forms.Timer(this.components);
            timer1.Enabled = true;
            timer1.Interval = 20;
            timer1.Tick += new System.EventHandler(timer1_Tick);

        }

        void timer1_Tick(object sender, System.EventArgs e)
        {
            InitDraw();
            cam.Spin_XZAxis4();
            ParallelDraw(cam.color);
            UpdateBuffer();
            ShowImage();
            //Graphics r = Graphics.FromImage(_backBuffer);

            ////중간에 계산작업 해주고

            //r.Clear(Color.White);
            ////MakeImage(r);
            //graphics.DrawImageUnscaled(_backBuffer, 0, 0);
        }

        void UpdateBuffer()
        {
            _backBufferTemp = _backBuffer2;
            _backBuffer2 = _backBuffer;
            _backBuffer = _backBufferTemp;
        }
        Graphics graphics2;
        public void InitDraw()
        {
            graphics2 = Graphics.FromImage(_backBuffer);
            graphics2.Clear(Color.Black);
        }
        public void Draw(int x, int y,int level)
        {
            x *= pixelSize;
            y *= pixelSize;
            byte levRatio = (byte)(255 * (level / 9f));
            brush = new SolidBrush(Color.FromArgb(255, levRatio, levRatio, 0));

            graphics2.FillRectangle(brush, x, y, pixelSize, pixelSize);
        }
        public void ShowImage()
        {
            graphics.DrawImageUnscaled(_backBuffer,0,0);
        }

        public void ParallelDraw(XYZ_b[,] data)
        {
            unsafe
            {
                BitmapData bitmapData = _backBuffer.LockBits(new Rectangle(0, 0, _backBuffer.Width, _backBuffer.Height), ImageLockMode.ReadWrite, _backBuffer.PixelFormat);

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(_backBuffer.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInPixels = bitmapData.Width;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;
                //Console.WriteLine("{0} {1}", widthInPixels, heightInPixels);
                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    int _y = y / this.pixelSize;
                    Parallel.For(0, widthInPixels, x =>
                    {
                        int _x = x / this.pixelSize;
                        currentLine[x * bytesPerPixel] = data[_y, _x].z;
                        currentLine[x * bytesPerPixel + 1] = data[_y, _x].y;
                        currentLine[x * bytesPerPixel + 2] = data[_y, _x].x;
                    });
                });
                _backBuffer.UnlockBits(bitmapData);
            }
        }
    }
}