using System;
using System.Threading;
using System.Threading.Tasks;
namespace VirtualCam
{
	class World
	{
		private Pixel[,,] worldArray;
		private int[,,] lightArray;
		private XYZ worldSize;
		private Pixel NULL_PIXEL = new Pixel(null,0);
		public World(XYZ worldSize)
		{
			this.worldSize = worldSize;
			worldArray = new Pixel[worldSize.x,worldSize.y,worldSize.z];
			lightArray = new int[worldSize.x,worldSize.y,worldSize.z];
		}
		public bool isIn(XYZ_d p)
		{
			return isIn(p.x,p.y,p.z);
		}			
		public bool isIn(double x, double y, double z)
		{
			return isIn((int)Math.Round(x,0),(int)Math.Round(y,0),(int)Math.Round(z,0));
		}			
		public bool isIn(XYZ p)
		{
			return isIn(p.x,p.y,p.z);
		}			
		public bool isIn(int x, int y, int z)
		{
			return x >= 0 && x < worldSize.x && y >= 0 && y < worldSize.y && z >= 0 && z < worldSize.z;
		}
		
		public XYZ GetWorldSize(){return worldSize;}
		
		//public Pixel getWorldArray()
		
		public void SetPoint(XYZ_d p, int lev, bool v)
		{
			SetPoint(p.x,p.y,p.z,lev,v);
		}
		public void SetPoint(double x, double y, double z, int lev, bool v)
		{
			SetPoint((int)Math.Round(x,0),(int)Math.Round(y,0),(int)Math.Round(z,0),lev,v);
		}			
		public void SetPoint(XYZ p, int lev, bool v)
		{
			SetPoint(p.x,p.y,p.z,lev,v);
		}			
		public void SetPoint(int x, int y, int z, int lev, bool v)
		{
			if(isIn(x,y,z))
			{
				if(!v) worldArray[x,y,z] = null;
				else if(worldArray[x,y,z] == null)
					worldArray[x,y,z] = new Pixel(new XYZ_d(x,y,z),lev);
				else worldArray[x,y,z].level = lev;
			}
		}
		
		public int GetLevel(XYZ_d p)
		{
			return GetLevel(p.x,p.y,p.z);
		}
		public int GetLevel(double x, double y, double z)
		{
			return GetLevel((int)Math.Round(x,0),(int)Math.Round(y,0),(int)Math.Round(z,0));
		}
		public int GetLevel(XYZ p)
		{
			return GetLevel(p.x,p.y,p.z);
		}
		public int GetLevel(int x, int y, int z)
		{
			if(isIn(x,y,z)) return lightArray[x,y,z];
			else return 0;
		}
		
		public void SetLevel(XYZ_d p, int l)
		{
			SetLevel(p.x,p.y,p.z,l);
		}
		public void SetLevel(double x, double y, double z, int l)
		{
			SetLevel((int)Math.Round(x,0),(int)Math.Round(y,0),(int)Math.Round(z,0),l);
		}
		public void SetLevel(XYZ p, int l)
		{
			SetLevel(p.x,p.y,p.z,l);
		}
		public void SetLevel(int x, int y, int z, int l)
		{
			if(isIn(x,y,z))
			{
				if(worldArray[x,y,z] == null)
					worldArray[x,y,z] = new Pixel(new XYZ_d(x,y,z),l);
				worldArray[x,y,z].level = l;
			}
		}
		
		public void AddLevel(XYZ_d p, int l)
		{
			AddLevel(p.x,p.y,p.z,l);
		}
		public void AddLevel(double x, double y, double z, int l)
		{
			AddLevel((int)Math.Round(x,0),(int)Math.Round(y,0),(int)Math.Round(z,0),l);
		}
		public void AddLevel(XYZ p, int l)
		{
			AddLevel(p.x,p.y,p.z,l);
		}
		public void AddLevel(int x, int y, int z, int l)
		{
			if(isIn(x,y,z))
			{
				lightArray[x,y,z] += l;
			}
		}
		
		public Pixel GetPixel(XYZ_d p)
		{
			return GetPixel(p.x,p.y,p.z);
		}
		public Pixel GetPixel(double x, double y, double z)
		{
			return GetPixel((int)Math.Round(x,0),(int)Math.Round(y,0),(int)Math.Round(z,0));
		}
		public Pixel GetPixel(XYZ p)
		{
			return GetPixel(p.x,p.y,p.z);
		}
		public Pixel GetPixel(int x, int y, int z)
		{
			if(isIn(x,y,z))
			{
				return worldArray[x,y,z];
			}
			else return NULL_PIXEL;
		}
		
		public void SetPixel(Pixel p)
		{
			if(isIn(p.Position))
			{
				if(p.isVisible)
				{
					worldArray[p.Position.iX,p.Position.iY,p.Position.iZ] = p;
				}
				else lightArray[p.Position.iX,p.Position.iY,p.Position.iZ] += p.level;
				
			}
		}
		
		public void ReSetPixel(Pixel p)
		{
			ReSetPixel(p.Position);
		}
		public void ReSetPixel(XYZ_d p)
		{
			ReSetPixel(p.x,p.y,p.z);
		}
		public void ReSetPixel(double x, double y, double z)
		{
			ReSetPixel((int)Math.Round(x,0),(int)Math.Round(y,0),(int)Math.Round(z,0));
		}
		public void ReSetPixel(XYZ p)
		{
			ReSetPixel(p.x,p.y,p.z);
		}
		public void ReSetPixel(int x, int y, int z)
		{
			if(isIn(x,y,z))
			{
				worldArray[x,y,z] = null;
			}
		}
		
		public void SetVisibility(XYZ p, bool b)
		{
			SetVisibility(p.x,p.y,p.z,b);
		}
		public void SetVisibility(int x, int y, int z, bool b)
		{
			if(isIn(x,y,z) && worldArray[x,y,z] != null) worldArray[x,y,z].isVisible = b;
		}		
	}
	
}