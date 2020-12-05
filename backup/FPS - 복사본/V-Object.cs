using System;
using System.Threading;
using System.Threading.Tasks;
namespace VirtualCam
{
	abstract class Object
	{
		public XYZ size;
		public XYZ center;
		public XYZ axis;
		public XYZ_d Position;
		private Pixel[,,] shape;
		protected XYZ_d toGlobal;
		public Object(XYZ_d p, XYZ s)
		{
			size = new XYZ(s);
			center = new XYZ(s).Div(2);
			axis = new XYZ(s).Div(2);
			Position = new XYZ_d(p);
			toGlobal = new XYZ_d(Position).Sub(center);
			shape = new Pixel[s.x,s.y,s.z];
		}
		protected abstract void MakeShape();
		private bool isIn(int x, int y, int z)
		{
			return x >= 0 && x < size.x && y >= 0 && y < size.y && z >= 0 && z < size.z;
		}
		public Pixel GetPixel(int x, int y, int z)
		{
			if(isIn(x,y,z)) return shape[x,y,z];
			else return null;
		}
		protected void SetPixel(int x, int y, int z, Pixel p){ if(isIn(x,y,z)) shape[x,y,z] = p;}
		protected void SetLevel(int x, int y, int z, int l){ if(isIn(x,y,z) && shape[x,y,z] != null) shape[x,y,z].level = l;}
		protected void AddLevel(int x, int y, int z, int l){ if(isIn(x,y,z) && shape[x,y,z] != null) shape[x,y,z].level += l;}
		protected void SetVisibility(int x, int y, int z, bool b){ if(isIn(x,y,z)) shape[x,y,z].isVisible = b;}
		public void AddLevel(int l)
		{
			Pixel p;
			for(int i = 0; i < size.x; i++)
				for(int j = 0; j < size.y; j++)
					for(int k = 0; k < size.z; k++)
					{
						p = GetPixel(i,j,k);
						if(p != null)p.level += l;
					}
		}
	}
	
	class Cube : Object
	{
		public Cube(XYZ_d p, XYZ s) : base(p,s){MakeShape();}
		
		
		protected override void MakeShape()
		{
			for(int i = 0; i < size.x; i++)
				for(int j = 0; j < size.y; j++)
					for(int k = 0; k < size.z; k++)
					{
						SetPixel(i,j,k,new Pixel(new XYZ_d(i,j,k).Add(toGlobal),3));
						GetPixel(i,j,k).color.Set(128,128,128);
					}
			int endX = size.x -1;
			int endY = size.y -1;
			int endZ = size.z -1;
			
			for(int i = 0; i < size.x; i++)
			{
				AddLevel(i,0,0,8);
				AddLevel(i,0,endZ,8);
				AddLevel(i,endY,0,8);
				AddLevel(i,endY,endZ,8);
			}
			for(int j = 0; j < size.y; j++)
			{
				AddLevel(0,j,0,8);
				AddLevel(0,j,endZ,8);
				AddLevel(endX,j,0,8);
				AddLevel(endX,j,endZ,8);
			}
			for(int k = 0; k < size.z; k++)
			{
				AddLevel(0,0,k,8);
				AddLevel(0,endY,k,8);
				AddLevel(endX,0,k,8);
				AddLevel(endX,endY,k,8);
			}
		}
	}
	
	class Sphere : Object
	{
		int radius = 0;
		public Sphere(XYZ_d p, int r) : base(p,new XYZ(r * 2)){ radius = r; MakeShape();}
		protected override void MakeShape()
		{
			for(int i = 0; i < size.x; i++)
				for(int j = 0; j < size.y; j++)
					for(int k = 0; k < size.z; k++)
					{
						SetPixel(i,j,k,null);
						if(Math.Pow(i - radius,2) + Math.Pow(j - radius,2) + Math.Pow(k - radius,2) < Math.Pow(radius,2))
						{
							SetPixel(i,j,k,new Pixel(new XYZ_d(i,j,k).Add(toGlobal),3));
							GetPixel(i, j, k).color.Set(139, 0, 139);// = ConsoleColor.DarkMagenta;
						}
					}
		}
	}
	
	class Cylinder : Object
	{
		int radius = 0;
		int heigh = 0;
		public Cylinder(XYZ_d p, int r, int h) : base(p,new XYZ(r * 2, h, r * 2))
		{
			radius = r;
			heigh = h;
			MakeShape();
		}
		protected override void MakeShape()
		{
			
			toGlobal.Add(center);
			axis.Set(center);
			axis.y = 0;
			for(int i = 0; i < size.x; i++)
				for(int k = 0; k < size.y; k++)
				
				{
					if(Math.Pow(i - radius,2) + Math.Pow(k - radius,2) < Math.Pow(radius,2))
					{
						for(int j = 0; j < heigh; j++)
						{
							SetPixel(i,j,k,new Pixel(new XYZ_d(i,j,k).Add(toGlobal),3));
							GetPixel(i, j, k).color.Set(255, 255, 0); //ConsoleColor.Yellow;
						}
						if(Math.Pow(i - radius,2) + Math.Pow(k - radius,2) >= Math.Pow(radius-1,2))
						{
							for(int j = 0; j < 1; j++)
							{
								GetPixel(i, j, k).color.Set(0);// = ConsoleColor.Black;
								GetPixel(i, heigh - j - 1, k).color.Set(0);// = ConsoleColor.Black;
							}
							
						}
					}
					else for(int j = 0; j < heigh; j++)
							SetPixel(i,j,k,null);
				}
		}
	}
	
	class LightSphere : Object
	{
		int radius = 0;
		int range = 0;
		int maxLevel = 13;
		public LightSphere(XYZ_d p, int r, int ra, int l) : base(p,new XYZ(ra*2))
		{
			range = ra; radius = r; maxLevel = l; MakeShape();
		}
		protected override void MakeShape()
		{
			for(int i = 0; i < size.x; i++)
				for(int j = 0; j < size.y; j++)
					for(int k = 0; k < size.z; k++)
					{
						double distance = Math.Sqrt(Math.Pow(i - range,2) + Math.Pow(j - range,2) + Math.Pow(k - range,2));
						SetPixel(i,j,k,null);
						if(distance < radius && distance > radius - 2)
						{
							SetPixel(i,j,k,new Pixel(new XYZ_d(i,j,k).Add(toGlobal),10));
						}
						else if(distance > radius && distance < range)
						{
							SetPixel(i,j,k,new Pixel(new XYZ_d(i,j,k).Add(toGlobal),(int)(maxLevel * (range - distance) / range)));
							SetVisibility(i,j,k,false);
						}
					}
		}
	}
	
}