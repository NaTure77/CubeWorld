using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace VirtualCam
{
	class MainApp
	{
		public static void Main(string[] args)
		{
            Console.Read();
            XYZ camSize = new XYZ(120,1000,90);
			//Init(camSize);
			World world = new World(new XYZ(150,300,150));
			Camera camera = new Camera(camSize,new XYZ_d(100,100,50),world);
			Controller controller = new Controller(world,camera);
			
			new MovingCube(new XYZ_d(30,30,30),controller.modifier);
			new FollowLight(controller.modifier,new XYZ_d(50),new XYZ(10,30,10),camera.GetPosition());
			new AliveSinFlow(new XYZ_d(75,100,130),controller.modifier);
            //Controller.Start();
            //while(true){camera.Print(); camera.Spin_XZAxis4();}
            //Console.WriteLine("Finish");
            //Console.Read();
            Application.Run(camera.viewer);
        }
		
		public static void Init(XYZ camSize)
		{
			Console.Read();
			Console.Clear();
			Console.Title = "FPS_TEST";
			Console.CursorVisible = false;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.SetWindowSize(1,1);
			Console.SetBufferSize(camSize.x + 20, camSize.z + 20);
			Console.SetWindowSize(camSize.x + 1, camSize.z + 2);
			Console.Write("Loading...");
		}
	}
}