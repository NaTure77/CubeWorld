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
       // int pixelSize;
        XY<int> screenSize;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        Bitmap _backBuffer;
        Bitmap _backBuffer2;
        Bitmap _backBufferTemp;
        Camera cam;
        Rectangle rect;
        protected override void OnPaintBackground(PaintEventArgs pevent) { }
        protected override void Dispose(bool disposing)
        
        {
            if (disposing)
                if (components != null)
                    components.Dispose();
            base.Dispose(disposing);
        }
        public void UpdateBufferSize(XYZ camSize)
        {
            _backBuffer = new Bitmap(camSize.x, camSize.z, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _backBuffer2 = new Bitmap(camSize.x, camSize.z, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			InitDraw();
			UpdateBuffer();
			InitDraw();
        }
        public Viewer(Camera cam, XYZ camSize)
        {
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.cam = cam;
            screenSize = new XY<int>(camSize.x,camSize.z);
            Width = screenSize.x;
            Height = screenSize.y;
            rect = new Rectangle(0,0,Width,Height);
            graphics = CreateGraphics();
           // this.pixelSize = pixelSize;
            components = new System.ComponentModel.Container();
            UpdateBufferSize(camSize);

            timer1 = new System.Windows.Forms.Timer(this.components);
            
            timer1.Interval = 1;
            timer1.Tick += new System.EventHandler(timer1_Tick);
            timer1.Enabled = true;

            //timer2 = new System.Windows.Forms.Timer(this.components);
            //timer2.Interval = 20;
            //timer2.Tick += new System.EventHandler(timer2_Tick);
            //timer2.Enabled = true;

            this.KeyDown += new KeyEventHandler(KeyDownEvent);
           this.KeyUp += new KeyEventHandler(KeyUpEvent);
		   
		   
		   cam.Resize(camSize.x/8,camSize.y,camSize.z/8);
				UpdateBufferSize(camSize); 
            InputManager.Regist(Keys.Escape, new Func(() => { isPaused = !isPaused; }), false);
            InputManager.Regist(Keys.M, new Func(() => 
			{ 
				cam.Resize(camSize.x/2,camSize.y,camSize.z/2);
				UpdateBufferSize(camSize); 
			}), false);
			
            InputManager.Regist(Keys.N, new Func(() => 
			{ 
				cam.Resize(camSize.x*2,camSize.y,camSize.z*2);
				UpdateBufferSize(camSize);
			}), false);
			
			//graphics.CompositingMode = CompositingMode.SourceCopy;
        }
        bool isPaused = false;
        DateTime startTime = DateTime.Now;
        DateTime current;
        double dt = 1 / 50d;
        double accumulateTime = 0.2f;
        void timer1_Tick(object sender, System.EventArgs e)
        {
            if (isPaused) return;
            startTime = DateTime.Now;
            //InitDraw();
            if (InputManager.currentFuncs != null)
            {
               InputManager.DoLogic();                
            }         
            //if (accumulateTime > 0.05f) accumulateTime = 0.05f;
            //while(accumulateTime > 0)
            //{
            //    cam.Spin_XZAxis5();
            //    Controller.instance.Falling();
            //    accumulateTime -= dt;
            //}
            //current = DateTime.Now;
            //accumulateTime = current.Subtract(startTime).TotalSeconds;

            cam.Spin_XZAxis5();
           // Console.WriteLine(cam.PositionIndex.x + " " + cam.PositionIndex.y + " " + cam.PositionIndex.z);
            Controller.instance.Falling();
            current = DateTime.Now;
            Console.WriteLine(current.Subtract(startTime).TotalSeconds);
            ParallelDraw(cam.MakeImage());
            UpdateBuffer();
            ShowImage();
        }
        void UpdateBuffer()
        {
            _backBufferTemp = _backBuffer2;
            _backBuffer2 = _backBuffer;
            _backBuffer = _backBufferTemp;
        }
        void timer2_Tick(object sender, System.EventArgs e)
        {
            ParallelDraw(cam.MakeImage());
            UpdateBuffer();
            ShowImage();
        }
        Graphics graphics2;
        public void InitDraw()
        {
            graphics2 = Graphics.FromImage(_backBuffer);
            graphics2.Clear(Color.White);
        }
        public void ShowImage()
        {
			
            graphics.DrawImage(_backBuffer, rect);
        }

        public void KeyDownEvent(object sender, KeyEventArgs e)
        {
           // Console.WriteLine(e.KeyCode);
            InputManager.AddLongClick(e.KeyCode);
            InputManager.DoOneClick(e.KeyCode);

        }
        public void KeyUpEvent(object sender, KeyEventArgs e)
        {
            InputManager.SubLongClick(e.KeyCode);
        }


        public void ParallelDraw(XYZ_b[,] data)
        {
            unsafe
            {
                BitmapData bitmapData = _backBuffer.LockBits(new Rectangle(0, 0, _backBuffer.Width, _backBuffer.Height), ImageLockMode.ReadWrite, _backBuffer.PixelFormat);

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(_backBuffer.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInPixels = bitmapData.Width;
                //int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;
				
				//byte*[,] currLArr = new byte*[,];
                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    Parallel.For(0, widthInPixels, x =>
                    {
                        currentLine[x * bytesPerPixel] = data[x, y].z;
                        currentLine[x * bytesPerPixel + 1] = data[x, y].y;
                        currentLine[x * bytesPerPixel + 2] = data[x, y].x;
                    });
                });
                _backBuffer.UnlockBits(bitmapData);
            }
        }
    }
}