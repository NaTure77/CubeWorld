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
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Timer timer1;
        Bitmap _backBuffer;
        //protected override void OnPaintBackground(PaintEventArgs pevent) { }
        /* protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (components != null)
                    components.Dispose();
            base.Dispose(disposing);
        } */
        public void UpdateBufferSize(XYZ camSize)
        {
            _backBuffer = new Bitmap(camSize.x, camSize.z, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Graphics.FromImage(_backBuffer).Clear(Color.White);
        }

        public int qualityLevel = 0;
		bool isPaused = false;
        public Viewer(Camera cam, XYZ camSize)
        {
			//뷰어 세팅
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            Width = camSize.x;
            Height = camSize.z;
            graphics = CreateGraphics();
            components = new System.ComponentModel.Container();
            UpdateBufferSize(camSize);

			//업데이트 함수 추가
            timer1 = new System.Windows.Forms.Timer(components);
            timer1.Interval = 1;
            timer1.Tick += new System.EventHandler((sender,e)=>
			{
				if (isPaused) return;
				if (InputManager.currentFuncs != null)
				{
				   InputManager.DoLogic();                
				}         
				cam.Spin_XZAxis5();
				Controller.instance.Falling();
				DrawImage(cam.MakeImage());
			});
            timer1.Enabled = true;

            InputManager.Regist(Keys.Escape, new Func(() => { isPaused = !isPaused; }), false);
            
			
			cam.Resize(camSize.x/4,camSize.y,camSize.z/4);
		    UpdateBufferSize(camSize); 
        }
		
		//키입력 이벤트 추가 함수
		public void SetKeyDownEvent(Action<object, KeyEventArgs> func)
		{
			this.KeyDown += new KeyEventHandler(func);
		}
		public void SetKeyUpEvent(Action<object, KeyEventArgs> func)
		{
			this.KeyUp += new KeyEventHandler(func);
		}

		//이미지를 화면에 그리는 함수
        public void DrawImage(XYZ_b[,] data)
        {
            unsafe
            {
                BitmapData bitmapData = _backBuffer.LockBits(new Rectangle(0, 0, _backBuffer.Width, _backBuffer.Height), ImageLockMode.ReadWrite, _backBuffer.PixelFormat);

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(_backBuffer.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInPixels = bitmapData.Width;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;
				
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
			graphics.DrawImage(_backBuffer, 0,0,Width,Height);
        }
    }
}