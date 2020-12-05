using System;
using System.Threading;
using System.Threading.Tasks;
namespace VirtualCam
{
	class Modifier
	{
		World world;
		public XYZ_d rayDelta = null;
		public XYZ_d rayStrike = null;
		public Modifier(World w)
		{
			world = w;
			InitWorld();
		}
		public void InitWorld()
		{
			MakeWall(4);
			MakeFrame(20);
			DrawObject(new Cube(new XYZ_d(100,100,30),new XYZ(10)));
		}
		public Object DrawObject(Object o)
		{
			Pixel p = null;
			for(int i = 0; i < o.size.x; i++)
				for(int j = 0; j < o.size.y; j++)
					for(int k = 0; k < o.size.z; k++)
					{
						p = o.GetPixel(i,j,k);
						if(p == null) continue;
						
						world.SetPixel(p);
					}
			return o;
		}
		public void SetColor(Object o, ConsoleColor color)
		{
			Pixel p = null;
			for(int i = 0; i < o.size.x; i++)
				for(int j = 0; j < o.size.y; j++)
					for(int k = 0; k < o.size.z; k++)
					{
						p = o.GetPixel(i,j,k);
						if(p == null) continue;
						p.color = color;
					}
		}
		
		public void MoveObject(Object o, XYZ_d delta)
		{
			Pixel p = null;
			o.Position.Add(delta);
			for(int i = 0; i < o.size.x; i++)
				for(int j = 0; j < o.size.y; j++)
					for(int k = 0; k < o.size.z; k++)
					{
						p = o.GetPixel(i,j,k);
						if(p == null) continue;
						if(!p.isVisible) world.AddLevel(p.Position, -p.level);
						else if(world.GetPixel(p.Position) == p) world.ReSetPixel(p);
						p.Position.Add(delta);
						world.SetPixel(p);
					}
		}
		
		public void SpinObject(Object o, XYZ_d degree, Matrix matrix)
		{
			XYZ_d next = new XYZ_d();
			XYZ_d delta = new XYZ_d(0,0,o.size.z-1);
			XYZ_d axis = new XYZ_d(o.axis);
			Pixel p = null;
			
			matrix(axis,degree);
			matrix(delta,degree);
			delta.Div(o.size.z);
			
			for(int i = 0; i < o.size.x; i++)
				for(int j = 0; j < o.size.y; j++)
				{
					p = null;
					int temp = -1;
					while(p == null && temp < o.size.z)
					{
						temp++;
						p = o.GetPixel(i,j,temp);
					}
					if(p == null) continue;
					next.Set(i,j,0);
					matrix(next,degree);
					for(int k = temp; k < o.size.z; k++)
					{
						p = o.GetPixel(i,j,k);
						if(p == null) continue;
						if(world.GetPixel(p.Position) == p)
							world.ReSetPixel(p);
						p.Position.Set(delta).Mul(k + 1).Add(next).Sub(axis).Add(o.Position);
						world.SetPixel(p);
					}
				}
			DrawObject(o);
			
		}
		
		public void SpinObject(Object o, XYZ_d degree)
		{
			SpinObject(o,degree,new Matrix(Spin_matrix_x) + new Matrix(Spin_matrix_y) + new Matrix(Spin_matrix_z));
		}
		
		public delegate void Matrix(XYZ_d pos, XYZ_d degree);
		
		public void FollowObject(Object o, XYZ_d target, double speed)
		{
			XYZ_d vector = new XYZ_d(target).Sub(o.Position);
			vector.Div(vector.Length()).Mul(speed);
			MoveObject(o,vector);
		}
		public void DeleteObject(Object o)
		{
			Pixel p = null;
			for(int i = 0; i < o.size.x; i++)
				for(int j = 0; j < o.size.y; j++)
					for(int k = 0; k < o.size.z; k++)
					{
						p = o.GetPixel(i,j,k);
						if(p != null) world.ReSetPixel(p);
					}
		}
		
		public void fire_Cylinder(XYZ_d position, int radius, int heigh)
		{
			XYZ_d pointer = new XYZ_d(rayDelta).Mul(30).Add(position);
			Object cylinder = new Cylinder(pointer,radius,heigh);
			XYZ_d degree = new XYZ_d(-Camera.instance.GetCursorPos().y,0,Camera.instance.GetCursorPos().x);
			Matrix mat = new Matrix(Spin_matrix_x) + new Matrix(Spin_matrix_z);
			SpinObject(cylinder,degree,mat);
		}
		public void fire_Cube(XYZ size)
		{
			DrawObject(new Cube(rayStrike,size));
		}
		public void fire_LightSphere(int radius, int range, int level)
		{
			int diameter = range * 2;
			XYZ_d rs_origin = new XYZ_d(rayStrike);
			XYZ_d rs = new XYZ_d(rs_origin);
			Task.Factory.StartNew(()=>
			{
				for(int i = 0; i < diameter; i++)
				{
					for(int j = 0; j < diameter; j++)
						for(int k = 0; k < diameter; k++)
						{
							rs.Set(rs_origin);
							rs.Add(i - range, j - range, k - range);
							if(!world.isIn(rs))continue;
							double distance = Math.Sqrt(Math.Pow(i - range,2) + Math.Pow(j - range,2) + Math.Pow(k - range,2));
							if(distance <= radius && distance > radius - 2)
							{
								world.SetLevel(rs,10);
							}
							else if(distance > radius && distance < range)
							{
								world.AddLevel(rs,(int)(level * (range - distance) / range));
							}
						}
					Thread.Sleep(1);
				}
			});				
		}
		public void fire_Moving_Sphere(XYZ_d position, int radius, double speed)
		{
			XYZ_d delta = new XYZ_d(rayDelta);
			XYZ_d pointer = new XYZ_d(rayDelta).Mul(30).Add(position);
			
			Object s = new Sphere(pointer,radius);
			SetColor(s,ConsoleColor.DarkRed);
			DrawObject(s);
			
			Task.Factory.StartNew(()=>
			{
				while(world.isIn(pointer))
				{
					pointer.Add(delta);
					MoveObject(s,delta);
					Thread.Sleep(5);
				}
				DeleteObject(s);
			});
		}
		public void fire_Moving_Cube(XYZ_d position, XYZ size, double speed)
		{
			XYZ_d delta = new XYZ_d(rayDelta).Mul(speed);
			XYZ_d pointer = new XYZ_d(rayDelta).Mul(30).Add(position);
			
			Object c = new Cube(pointer,size);
			SetColor(c,ConsoleColor.Cyan);
			
			XYZ_d vector = new XYZ_d(-Camera.instance.GetCursorPos().y,0,Camera.instance.GetCursorPos().x);
			Matrix mat = new Matrix(Spin_matrix_x) + new Matrix(Spin_matrix_z);
			SpinObject(c,vector,mat);
			DrawObject(c);
			mat = new Matrix(Spin_matrix_y) + new Matrix(Spin_matrix_x) + new Matrix(Spin_matrix_z);
			XYZ_d rotateDelta = new XYZ_d(0,6,0);
			Task.Factory.StartNew(()=>
			{
				while(world.isIn(pointer))
				{
					pointer.Add(delta);
					MoveObject(c,delta);
					
					vector.Add(rotateDelta);
					SpinObject(c,vector,mat);
					vector.Remain(360);
					Thread.Sleep(5);
				}
				DeleteObject(c);
			});
		}
		
		public delegate void Work();
		
		public void SetSwitch(Object o, Work work)
		{
			Pixel p = null;
			for(int i = 0; i < o.size.x; i++)
				for(int j = 0; j < o.size.y; j++)
					for(int k = 0; k < o.size.z; k++)
					{
						p = o.GetPixel(i,j,k);
						if(p != null) p.work = work;
					}
		}
		public void Check_Switch()
		{
			Pixel p = world.GetPixel(rayStrike);
			if(p == null || p.Position == null) return;
			if(p.work != null) p.work();
		}
		
		void MakeWall(int thickness)
		{
			XYZ size = new XYZ(world.GetWorldSize());
			XYZ center = new XYZ(size).Div(2);
			DrawObject(new Cube(new XYZ_d(center.x,center.y,0),new XYZ(size.x,size.y,thickness))).AddLevel(-2);
			DrawObject(new Cube(new XYZ_d(center.x,0,center.z),new XYZ(size.x,thickness,size.z))).AddLevel(-2);
			DrawObject(new Cube(new XYZ_d(0,center.y,center.z),new XYZ(thickness,size.y,size.z))).AddLevel(-2);
			DrawObject(new Cube(new XYZ_d(center.x,center.y,size.z),new XYZ(size.x,size.y,thickness))).AddLevel(-2);
			DrawObject(new Cube(new XYZ_d(center.x,size.y,center.z),new XYZ(size.x,thickness,size.z))).AddLevel(-2);
			DrawObject(new Cube(new XYZ_d(size.x,center.y,center.z),new XYZ(thickness,size.y,size.z))).AddLevel(-2);
		}
		
		void MakeFrame(int thickness)
		{
			XYZ size = new XYZ(thickness);
			XYZ startPos = new XYZ(thickness).Div(2).Add(4);
			XYZ endPos = new XYZ(world.GetWorldSize()).Sub(startPos).Sub(4);
			//ConsoleColor c = Con
			for(int i = startPos.x; i <= endPos.x; i+= thickness)
			{
				DrawObject(new Cube(new XYZ_d(i,startPos.y,startPos.z),size));
				DrawObject(new Cube(new XYZ_d(i,startPos.y,endPos.z),size));
				DrawObject(new Cube(new XYZ_d(i,endPos.y,startPos.z),size));
				DrawObject(new Cube(new XYZ_d(i,endPos.y,endPos.z),size));
			}
			for(int i = startPos.y; i <= endPos.y; i+= thickness)
			{
				DrawObject(new Cube(new XYZ_d(startPos.x,i,startPos.z),size));
				DrawObject(new Cube(new XYZ_d(startPos.x,i,endPos.z),size));
				DrawObject(new Cube(new XYZ_d(endPos.x,i,startPos.z),size));
				DrawObject(new Cube(new XYZ_d(endPos.x,i,endPos.z),size));
			}
			for(int i = startPos.z; i <= endPos.z; i+= thickness)
			{
				DrawObject(new Cube(new XYZ_d(startPos.x,startPos.y,i),size));
				DrawObject(new Cube(new XYZ_d(startPos.x,endPos.y,i),size));
				DrawObject(new Cube(new XYZ_d(endPos.x,startPos.y,i),size));
				DrawObject(new Cube(new XYZ_d(endPos.x,endPos.y,i),size));
			}
		}
		
		public void MakeSinFlow(Object obj, int val)
		{
			XYZ size = obj.size;
			double Value = 0;
			Pixel p = null;
			for(int i = 0; i < size.x; i++)
				for(int j = 0; j < size.y; j++)
				{
					Value = (Math.Sin((j + val) * PI * 10d) + Math.Sin((i + val) * PI * 10d)) * size.z/3 * 2 + obj.Position.z;
					for(int k = 0; k < size.z; k++)
					{
						p = obj.GetPixel(i,j,k);
						if(p == null) continue;
						if(world.GetPixel(p.Position) == p)
							world.ReSetPixel(p);
						p.Position.z = Value + k;
						world.SetPixel(p);
					}
				}
		}
		
		double PI = Math.PI / 180d;
		
		void Spin_matrix_x(XYZ_d pos, XYZ_d degree){Spin_matrix_x(pos.x,pos.y,pos.z,degree.x,pos);}
		void Spin_matrix_y(XYZ_d pos, XYZ_d degree){Spin_matrix_y(pos.x,pos.y,pos.z,degree.y,pos);}
		void Spin_matrix_z(XYZ_d pos, XYZ_d degree){Spin_matrix_z(pos.x,pos.y,pos.z,degree.z,pos);}
		
		void Spin_matrix_x(double x, double y, double z, double d, XYZ_d position)
		{
			position.Set(x,y,z);
			if(d != 0) func(y,z,ref position.y, ref position.z, d);
		}
		void Spin_matrix_y(double x, double y, double z, double d, XYZ_d position)
		{
			position.Set(x,y,z);
			if(d != 0) func(x,z,ref position.x, ref position.z, d);
		}
		void Spin_matrix_z(double x, double y, double z, double d, XYZ_d position)
		{
			position.Set(x,y,z);
			if(d != 0) func(x,y,ref position.x, ref position.y, d);
		}
		void func(double a, double b, ref double aft_a, ref double aft_b, double d)
		{
			double degree = d * PI;
			double sin = Math.Sin(degree);
			double cos = Math.Cos(degree);
			aft_a = a * cos + b * sin;
			aft_b = b * cos + a * sin * -1;
		}
	}
}