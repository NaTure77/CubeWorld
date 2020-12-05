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
		public Block[,,] Map2;
		public bool IsExistPixelInFrame(XYZ p)
		{
			//return Map[p.x,p.y,p.z] != 0;
			return Map2[p.x,p.y,p.z].code != 0;
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
			//return !IsInFrame(temp) || Map[temp.x,temp.y,temp.z] != 0;
			return !IsInFrame(temp) || Map2[temp.x,temp.y,temp.z].code != 0;
		}
		public bool isFrameEnabled(XYZ p)
		{
			//return !IsInFrame(p) || Map[p.x,p.y,p.z] != 0;
			return !IsInFrame(p) || Map2[p.x,p.y,p.z].code != 0;
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
                //Map[x,y,z] = b ? (byte)1 : (byte)0;
                Map2[x, y, z].code = b ? (byte)1 : (byte)0;
            }
		}
		//public byte GetColor(int x, int y, int z){return Map[x,y,z];}
		//public byte GetColor(XYZ p){return Map[p.x,p.y,p.z];}
		//public void SetColor(int x, int y, int z, byte b){if(IsInFrame(x,y,z)) Map[x,y,z] = b;}
		//public void SetColor(XYZ p, byte b){SetColor(p.x,p.y,p.z,b);}
		
		//public void AddColor(XYZ p, byte b){ AddColor(p.x,p.y,p.z,b);}
		//public void AddColor(int x, int y, int z, byte b)
		//{
		//	if(IsInFrame(x,y,z))
		//	{
		//		Map[x,y,z] = Map[x,y,z] + b > 13 ? (byte)13 :
		//								  Map[x,y,z] + b < 0 ? (byte)0 : (byte)(Map[x,y,z] + b);
		//	}
		//}


        public Block GetBlock(int x, int y, int z) { return Map2[x, y, z]; }
        public Block GetBlock(XYZ p) { return Map2[p.x, p.y, p.z]; }

        public void SetBlock(XYZ i, byte b) {SetBlock(i.x, i.y, i.z, b);}
        public void SetBlock(int x, int y, int z, byte b)
        {
            if (IsInFrame(x, y, z))
            {
                Map2[x, y, z].code = b;
            }
        }

        public XYZ_b GetColor(int x, int y, int z) { return Map2[x, y, z].color; }
        public XYZ_b GetColor(XYZ p) { return Map2[p.x, p.y, p.z].color; }

        public void SetColor(int x, int y, int z, XYZ_b c) { if (IsInFrame(x, y, z)) Map2[x, y, z].color = new XYZ_b(c); }
        public void SetColor(XYZ p, XYZ_b c) { SetColor(p.x, p.y, p.z, c); }

        public void AddColor(XYZ p, XYZ_b c) { AddColor(p.x, p.y, p.z, c); }
        public void AddColor(int x, int y, int z, XYZ_b c)
        {
            if (IsInFrame(x, y, z))
            {
                Map2[x, y, z].color.Add(c);
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
			//Map = new byte[frameSize.x,frameSize.y,frameSize.z];
			Map2 = new Block[frameSize.x,frameSize.y,frameSize.z];

            for (int i = 0; i < frameSize.x; i++)
            for (int j = 0; j < frameSize.y; j++)
            for (int k = 0; k < frameSize.z; k++)
            {
                Map2[i, j, k] = new Block(0, new XYZ_b());
            }

            for (int i = 0; i < frameSize.x; i++)
			for(int j = 0; j < frameSize.y; j++)
			for(int k = frameSize.z/2; k < frameSize.z; k++)
			{
				//Map[i,j,k] = (byte)((i * j * k / 11) % 6 + 1);
                Map2[i,j,k].color.Set
                            ((byte)(255 - 255 * ((i * j) / (worldSize.x * worldSize.y * 1f))),
                            (byte)(255 - 255 * ((j * k) / (worldSize.y * worldSize.z * 1f))), 
                            (byte)(255 - 255 * ((i * k) / (worldSize.x * worldSize.z * 1f))));
                Map2[i, j, k].code = 1;
				/* if(k < frameSize.z/2 + 4) Map[i,j,k] = (byte)15; */
			}


            
	}
    public void MakeMirror(XYZ pos)
    {
            for (int w = -1; w < 21; w++)
            {
                for (int h = -1; h < 11; h++)
                {
                    Map2[pos.x + w, pos.y-5, frameSize.z / 2 - h].code = (byte)1;
                    Map2[pos.x + w, pos.y - 5, frameSize.z / 2 - h].color.Set((byte)100);
                }
            }
            for (int w = -1; w < 21; w++)
            {
                for (int h = -1; h < 11; h++)
                {
                    Map2[pos.x + w, pos.y + 5, frameSize.z / 2 - h].code = (byte)1;
                    Map2[pos.x + w, pos.y + 5, frameSize.z / 2 - h].color.Set((byte)100);
                }
            }

            for (int w = 0; w < 20; w++)
            {
                for (int h = 0; h < 10; h++)
                {
                    Map2[pos.x + w, pos.y - 5, frameSize.z / 2 - h].code = (byte)14;
                }
            }
            for (int w = 0; w < 20; w++)
            {
                for (int h = 0; h < 10; h++)
                {
                    Map2[pos.x + w, pos.y + 5, frameSize.z / 2 - h].code = (byte)14;
                }
            }
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                    {
                        Map2[pos.x + 10 - i, pos.y - j, frameSize.z / 2 - k].code = (byte)1;
                        Map2[pos.x + 10 - i, pos.y - j, frameSize.z / 2 - k].color.Set((byte)0);
                    }
        }
    }
}