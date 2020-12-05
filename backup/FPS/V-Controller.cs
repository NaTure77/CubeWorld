using System;
using System.Threading;
using System.Threading.Tasks;
namespace VirtualCam
{
	class Controller
	{
		private World world;
		private Camera camera;
		private XYZ worldSize;
		private XYZ_d Position;
		XYZ_d scalaVector = new XYZ_d();
		XYZ halfBodySize = new XYZ(5,5,10);
		
		double PI = Math.PI / 180d;
		public Modifier modifier;
		InputManager inputManager;
		public Controller(World w, Camera c)
		{
			world = w; 
			worldSize = world.GetWorldSize();
			camera = c;
			Position = camera.GetPosition();
			
			inputManager = new InputManager();
			RegistKey();
			inputManager.StartInputLoop();
			modifier = new Modifier(world);
			modifier.rayDelta = camera.rayDelta;
			modifier.rayStrike = camera.rayStrike;
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
			inputManager.Regist(ConsoleKey.P,new Func(()=>{modifier.fire_Cylinder(Position,20,300);}));
			inputManager.Regist(ConsoleKey.O,new Func(()=>{modifier.fire_Moving_Sphere(Position,10,2d);}));
			inputManager.Regist(ConsoleKey.I,new Func(()=>{modifier.fire_Moving_Cube(Position,new XYZ(30,10,10),0.5d);}));
			inputManager.Regist(ConsoleKey.U,new Func(()=>{modifier.fire_Cube(new XYZ(10,10,10));}));
			inputManager.Regist(ConsoleKey.K,new Func(()=>{modifier.fire_LightSphere(10,60,22);}));
			inputManager.Regist(ConsoleKey.L,new Func(()=>{camera.isLighting = !camera.isLighting;}));
			inputManager.Regist(ConsoleKey.Oem2,new Func(()=>{if(camera.isLighting) camera.isLightMove = !camera.isLightMove;}));
			inputManager.Regist(ConsoleKey.Spacebar,new Func(()=>{modifier.Check_Switch();}));
			
			
		}
		
		public void Move(XYZ_d vector){Move(vector.x,vector.y,vector.z);}
		public void Move(double x, double y, double z)
		{
			Spin_matrix_z(x,y,z,camera.GetCursorPos().x,scalaVector,XYZ_d.ZERO);
			if(!Check_Wall(scalaVector))
				Position.Add(scalaVector);
		}
		bool Check_Wall(XYZ_d p)
		{
			int x = (int) Math.Round(p.x + Position.x,0);
			int y = (int) Math.Round(p.y + Position.y,0);
			int z = (int) Math.Round(p.z + Position.z,0);
			
			for(int i = x - halfBodySize.x; i < x + halfBodySize.x; i++)
				for(int j = y - halfBodySize.y; j < y + halfBodySize.y; j++)
					for(int k = z + halfBodySize.z; k > z; k--)
					{
						if(!world.isIn(x,y,z) || (world.GetPixel(x,y,z) != null && world.GetPixel(x,y,z).isVisible))
						{
							return true;
						}
					}
			return false;
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