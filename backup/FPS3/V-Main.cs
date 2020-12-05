using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace VirtualCam
{
	class MainApp
	{
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr windowHandle);

		public static void Main(string[] args)
		{
            Console.Read();
            //ShowWindow(GetConsoleWindow(), 0);
            XYZ camSize = new XYZ(400,50,250);
			//Init(camSize);
			World world = new World(new XYZ(300,300,100));
			Camera camera = new Camera(camSize,new XYZ_d(100,100,20).Mul(world.frameLength),world);
            XYZ t = new XYZ();
            world.GetFrameIndex(camera.GetPosition(), t);
            world.MakeMirror(t);

            Controller controller = new Controller(world,camera);

            SetForegroundWindow(camera.viewer.Handle);
            Application.Run(camera.viewer);
        }
		
		public static void Init(XYZ camSize)
		{
			Console.Read();
			Console.Clear();
			Console.Title = "FPS_TEST";
			Console.CursorVisible = false;
			Console.SetWindowSize(1,1);
			Console.SetBufferSize(camSize.x + 20, camSize.z + 20);
			Console.SetWindowSize(camSize.x + 1, camSize.z + 2);
			Console.Write("Loading...");
		}
	}
}