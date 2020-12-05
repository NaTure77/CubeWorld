using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
		//InputManager inputManager;
		public Controller(World w, Camera c)
		{
			world = w; 
			camera = c;
			Position = camera.GetPosition();
			
			//inputManager = new InputManager();
			RegistKey();
            //InputManager.StartInputLoop();
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
            InputManager.Regist(Keys.W, new Func(() => { Move(0, 1, 0); }),true);
            InputManager.Regist(Keys.S, new Func(() => { Move(0, -1, 0); }), true);
            InputManager.Regist(Keys.D, new Func(() => { Move(1, 0, 0); }), true);
            InputManager.Regist(Keys.A, new Func(() => { Move(-1, 0, 0); }), true);
            InputManager.Regist(Keys.C, new Func(() => { Move(0, 0, 1); }), true);
            InputManager.Regist(Keys.V, new Func(() => { Move(0, 0, -1); }), true);
            InputManager.Regist(Keys.Escape, new Func(() => { Application.Exit(); }), false);
            InputManager.Regist(Keys.P, new Func(() => { camera.gridEnabled = !camera.gridEnabled; }), false);

            InputManager.Regist(Keys.D2, new Func(() => { color = (byte)15; }), false);
            InputManager.Regist(Keys.D3, new Func(() => { color = (byte)14; }), false);
            InputManager.Regist(Keys.D4, new Func(() => { color = (byte)4; }), false);
            InputManager.Regist(Keys.D5, new Func(() => { color = (byte)5; }), false);
            InputManager.Regist(Keys.D6, new Func(() => { color = (byte)6; }), false);
            InputManager.Regist(Keys.D7, new Func(() => { color = (byte)7; }), false);
            InputManager.Regist(Keys.D8, new Func(() => { color = (byte)8; }), false);
            InputManager.Regist(Keys.D9, new Func(() => { color = (byte)9; }), false);
            InputManager.Regist(Keys.Space, new Func(() => { AddBlock(); }), false);
            InputManager.Regist(Keys.X, new Func(() => { DeleteBlock(); }), false);
        }
		
		public byte color = 6;
        public byte code = 3;
		public void AddBlock()
		{
			if(world.IsInFrame(camera.addFrameIndex))
			{
                world.SetBlock(camera.addFrameIndex, 15);
                world.SetColor(camera.addFrameIndex,new XYZ_b((byte)(color * 25)));
			}
		}
		
		public void DeleteBlock()
		{
			if(world.IsInFrame(camera.deleteFrameIndex))
			{
				world.SetBlock(camera.deleteFrameIndex,0);
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