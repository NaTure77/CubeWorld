using System;
using System.Threading;
using System.Threading.Tasks;
namespace VirtualCam
{
	class Controller
	{
		private World world;
		private Camera camera;
		private XYZ_d Position;
		XYZ_d scalaVector = new XYZ_d();
		XYZ halfBodySize = new XYZ(5,5,10);
		double speed = 20;
		double PI = Math.PI / 180d;
		InputManager inputManager;
		public Controller(World w, Camera c)
		{
			world = w; 
			camera = c;
			Position = camera.GetPosition();
			
			inputManager = new InputManager();
			RegistKey();
			inputManager.StartInputLoop();
			//Pixel p = null;
			/* for(int i = 0; i < worldSize.x; i++)
				for(int j = 0; j < worldSize.y; j++)
					for(int k = 0; k < worldSize.z; k++)
					{
						p = world.GetPixel(i,j,k);
						if(p != null) Console.WriteLine("{0} {1} {2}",p.Position.x,p.Position.y,p.Position.z);
					} */
		}
		public void Start()
		{
			
		}
		
		void RegistKey()
		{
			inputManager.Regist(ConsoleKey.W,new Func(()=>{Move(0,1,0);}));
			inputManager.Regist(ConsoleKey.S,new Func(()=>{Move(0,-1,0);}));
			inputManager.Regist(ConsoleKey.D,new Func(()=>{Move(1,0,0);}));
			inputManager.Regist(ConsoleKey.A,new Func(()=>{Move(-1,0,0);}));
			inputManager.Regist(ConsoleKey.C,new Func(()=>{Move(0,0,1);}));
			inputManager.Regist(ConsoleKey.V,new Func(()=>{Move(0,0,-1);}));		
			inputManager.Regist(ConsoleKey.Escape, new Func(()=>{camera.isPaused = ! camera.isPaused;}));
			inputManager.Regist(ConsoleKey.P, new Func(()=>{camera.gridEnabled = ! camera.gridEnabled; }));
			
			inputManager.Regist(ConsoleKey.D2,new Func(()=>{color = (byte)15;}));		
			inputManager.Regist(ConsoleKey.D3,new Func(()=>{color = (byte)14;}));		
			inputManager.Regist(ConsoleKey.D4,new Func(()=>{color = (byte)4;}));		
			inputManager.Regist(ConsoleKey.D5,new Func(()=>{color = (byte)5;}));		
			inputManager.Regist(ConsoleKey.D6,new Func(()=>{color = (byte)6;}));		
			inputManager.Regist(ConsoleKey.D7,new Func(()=>{color = (byte)7;}));		
			inputManager.Regist(ConsoleKey.D8,new Func(()=>{color = (byte)8;}));		
			inputManager.Regist(ConsoleKey.D9,new Func(()=>{color = (byte)9;}));		
			inputManager.Regist(ConsoleKey.Spacebar,new Func(()=>{AddBlock();}));		
			inputManager.Regist(ConsoleKey.X,new Func(()=>{DeleteBlock();}));		
		}
		
		public byte color = 6;
		public void AddBlock()
		{
			if(world.IsInFrame(camera.addFrameIndex))
			{
				world.SetColor(camera.addFrameIndex,color);
			}
		}
		
		public void DeleteBlock()
		{
			if(world.IsInFrame(camera.deleteFrameIndex))
			{
				world.SetColor(camera.deleteFrameIndex,0);
			}
		}
		
		public void Move(XYZ_d vector){Move(vector.x,vector.y,vector.z);}
		public void Move(double x, double y, double z)
		{
			Spin_matrix_z(x,y,z,camera.GetCursorPos().x,scalaVector,XYZ_d.ZERO);
			scalaVector.Mul(speed);
			if(!Check_Wall(scalaVector))
				Position.Add(scalaVector);
		}
		bool Check_Wall(XYZ_d p)
		{
			XYZ framePos = new XYZ();
			XYZ_d nextPos = new XYZ_d(p).Add(Position);
			world.GetFrameIndex(nextPos,framePos);
			if(world.IsInFrame(framePos))
			{
				if(world.isFrameEnabled(framePos)) return true;
				else return false; 
			}
			else return true;
		}
		void Spin_matrix_z(double x, double y, double z, double d, XYZ_d position, XYZ_d point)
		{
			double degree = d * PI;
			double sin = Math.Sin(degree);
			double cos = Math.Cos(degree);
			position.x = (x - point.x) * cos + (y - point.y) * sin + point.x;
			position.y = (y - point.y) * cos + (x - point.x) * (-sin) + point.y;
			position.z = z;
		}
	}
}