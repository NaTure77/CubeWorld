using System;
using System.Threading;
using System.Threading.Tasks;
namespace VirtualCam
{
	class MainApp
	{
		public static void Main(string[] args)
		{
			XYZ camSize = new XYZ(200,600,120);
			Init(camSize);
			World world = new World(new XYZ(400,400,400));
			Camera camera = new Camera(camSize,new XYZ_d(200,200,180).Mul(world.frameLength),world);
			
			
			Controller controller = new Controller(world,camera);
			DateTime startTime = DateTime.Now;
			DateTime current;
			double accumulateTime = 0.1f;
			int sleepTime = 0;
			while(true)
			{
				camera.Print0();
				camera.Spin_XZAxis5();
				
				
			}
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