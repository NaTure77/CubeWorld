using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace VirtualCam
{
	class World
	{
		private XYZ worldSize;
		public int frameLength = 30;
		XYZ frameSize = new XYZ();
		public byte[,,] Map;
		public bool IsExistPixelInFrame(XYZ p)
		{
			return Map[p.x,p.y,p.z] != 0;
		}
		public void GetFrameIndex(XYZ_d p, XYZ i)
		{
			i.Set(p.iX/frameLength,p.iY/frameLength,p.iZ/frameLength);
		}
		public void GetFrameIndex(XYZ_d p, XYZ_d i)
		{
			i.Set(p.iX/frameLength,p.iY/frameLength,p.iZ/frameLength);
		}
		public void ConvertIndexToPosition(XYZ i)
		{
			i.Mul(frameLength).Add(frameLength/2);
		}
		public void ConvertIndexToPosition(XYZ_d i)
		{
			i.Mul(frameLength).Add(frameLength/2);
		}
		public bool isFrameEnabled(XYZ_d p)
		{
			XYZ temp = new XYZ();
			GetFrameIndex(p,temp);
			return !IsInFrame(temp) || Map[temp.x,temp.y,temp.z] != 0;
		}
		public bool isFrameEnabled(XYZ p)
		{
			return !IsInFrame(p) || Map[p.x,p.y,p.z] != 0;
		}
		public bool IsInFrame(int x, int y, int z)
		{
			return (x >= 0 && y >= 0 && z >= 0 && 
					x <frameSize.x && y < frameSize.y && z < frameSize.z);
		}
		public bool IsInFrame(XYZ p)
		{
			return (p.x >= 0 && p.y >= 0 && p.z >= 0 && 
					p.x <frameSize.x && p.y < frameSize.y && p.z < frameSize.z);
		}
		public void SetFrame(XYZ i, bool b)
		{
			SetFrame(i.x,i.y,i.z,b);
		}
		public void SetFrame(int x, int y, int z, bool b)
		{
			if(IsInFrame(x,y,z))
			{
				Map[x,y,z] = b ? (byte)1 : (byte)0;
			}
		}
		public byte GetColor(int x, int y, int z){return Map[x,y,z];}
		public byte GetColor(XYZ p){return Map[p.x,p.y,p.z];}
		public void SetColor(int x, int y, int z, byte b){if(IsInFrame(x,y,z)) Map[x,y,z] = b;}
		public void SetColor(XYZ p, byte b){SetColor(p.x,p.y,p.z,b);}
		
		public void AddColor(XYZ p, byte b){ AddColor(p.x,p.y,p.z,b);}
		public void AddColor(int x, int y, int z, byte b)
		{
			if(IsInFrame(x,y,z))
			{
				Map[x,y,z] = Map[x,y,z] + b > 13 ? (byte)13 :
										  Map[x,y,z] + b < 0 ? (byte)0 : (byte)(Map[x,y,z] + b);
			}
		}
		public void ConvertToFramePos(XYZ_d p, XYZ index)
		{
			int x = index.x * frameLength + frameLength/2;
			int y = index.y * frameLength + frameLength/2;
			int z = index.z * frameLength + frameLength/2;
			p.Sub(x,y,z);
		}
		public World(XYZ worldSize)
		{
			this.worldSize = worldSize;
			frameSize.Set(worldSize);
			Map = new byte[frameSize.x,frameSize.y,frameSize.z];
			bool Xaxis = false;
			bool Yaxis = false;
			bool Zaxis = false;
			/* for(int i = 0; i < frameSize.x; i++)
			for(int j = 0; j < frameSize.y; j++)
			for(int k = 0; k < frameSize.z; k++)
			{
				Map[i,j,k] = 0;
				Xaxis = (Math.Abs(i - frameSize.x/2) > frameSize.x/2 - 3);
				Yaxis = (Math.Abs(i - frameSize.y/2) > frameSize.y/2 - 3);
				Zaxis = (Math.Abs(i - frameSize.z/2) > frameSize.z/2 - 3);
				if(Xaxis && Yaxis) Map[i,j,k] = 3;
				if(Yaxis && Zaxis) Map[i,j,k] = 3;
				if(Xaxis && Zaxis) Map[i,j,k] = 3;
			} */
			for(int i = 0; i < frameSize.x; i++)
			for(int j = 0; j < frameSize.y; j++)
			for(int k = frameSize.z/2; k < frameSize.z; k++)
			{
				Map[i,j,k] = (byte)((i * j * k / 11) % 6 + 1);
				/* if(k < frameSize.z/2 + 4) Map[i,j,k] = (byte)15; */
			}
		}
	}
}