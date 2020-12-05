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
            XYZ camSize = new XYZ(800,300,480);
			World world = new World(new XYZ(300,300,300));
			Camera camera = new Camera(camSize,new XYZ_d(280,280,147).Mul(world.frameLength),world);
            Controller controller = new Controller(world, camera);

            XYZ t = new XYZ();
            world.GetFrameIndex(new XYZ_d(100, 100, 120).Mul(world.frameLength), t);
            //world.MakeMirror(t);
            //world.MakeSphere(t,30,14, new XYZ_b(10));
			world.MakePenetration(t);
			t.Add(100,100,-50);
            //world.MakeSphere(t,30,1, new XYZ_b(100));
            world.MakeCone(new XYZ_d(280,280,151),35,60);

           
           

            SetForegroundWindow(camera.viewer.Handle);
            Application.Run(camera.viewer);
        }
	}
}