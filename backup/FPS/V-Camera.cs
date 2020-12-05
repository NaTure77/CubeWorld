using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
namespace VirtualCam
{
	class Camera
	{
		public static Camera instance;
		private char[,] board;
		private World world;
		private XYZ worldSize;
		
		private XYZ camSize;
		private XYZ camPoint;
		private XYZ_d Position;
		private XYZ_d position;
		private XYZ_d[,] perspArray;
		
		private double[,,] lightArray;
		private bool[,] circle;
		private XYZ_d[,] indexArray;
		private XYZ_d[,] spinArray;
		private XYZ_d[,] deltaArray;
		private ConsoleColor[,] color;
		double sensitivity = 0.1d;
		Point screenPoint = new Point(640,512);
		XY<double> cursor = new XY<double>(0,0);
		public bool isLightMove = false;
		public bool isLighting = false;
		public XY<double> lightAngle = new XY<double>(0,0);
		double PI = Math.PI / 180d;
		char[] level = {' ', '.', ':', '!', 'l', '1', 'G', 'D', 'H', '#'};
		public bool isRendering = false;
		
		public XYZ_d rayDelta;
		public XYZ_d rayStrike;
        public Viewer viewer;
		List<Ps> result = new List<Ps>();
		class Ps
		{
			string str;
			ConsoleColor color;
			public Ps(string s, ConsoleColor c){str = s; color = c;}
			public void Print()
			{
				Console.ForegroundColor = color;
				
				Console.Write(str);
			}
		}
		public Camera(XYZ cs, XYZ_d cPos, World w)
		{
			instance = this;
			camSize = cs;
			Position = cPos;
			world = w;
			worldSize = w.GetWorldSize();
			camPoint = new XYZ(camSize).Div(2);
			position = new XYZ_d(Position);
			
			board = new char[camSize.z,camSize.x];
			
			color = new ConsoleColor[camSize.z,camSize.x];
			perspArray = new XYZ_d[camSize.x,camSize.z];
			indexArray = new XYZ_d[camSize.x,camSize.z];
			spinArray = new XYZ_d[camSize.x,camSize.z];
			deltaArray = new XYZ_d[camSize.x,camSize.z];
			
			lightArray = new double[camSize.x,camSize.y,camSize.z];
			circle = new bool[camPoint.x,camPoint.x];
			
			//double rc_ratio = (1f * camSize.x) / camSize.z;
			
			double fov = 1d/60;
			
			for(int i = 0; i < camSize.x; i++)
				for(int j = 0; j < camSize.z; j++)
				{
					perspArray[i,j] = new XYZ_d();
					perspArray[i,j].x = (i - camPoint.x) * (camSize.y - 1) * fov;
					perspArray[i,j].y = camSize.y - 1;
					perspArray[i,j].z = (j - camPoint.z) * (camSize.y - 1) * fov;
					
					indexArray[i,j] = new XYZ_d();
					spinArray[i,j] = new XYZ_d();
					deltaArray[i,j] = new XYZ_d();
				}
				
				
			for(int i = 0; i < camPoint.x; i++)
				for(int j = 0; j < camPoint.x; j++)
					circle[i,j] = false;
				
			XYZ_d newPos = new XYZ_d();
			for(int i = 0; i < camPoint.x/2; i++)
				for(double d = 0; d < 360; d+=0.1d)
				{
					Spin_matrix_z(i,0,0,Math.Sin(d*PI),Math.Cos(d*PI),newPos);
					newPos.Add(camPoint.x/2);
					int x = newPos.iX;
					int y = newPos.iY;
					if(x < camPoint.x && x >= 0 && y < camPoint.x && y >= 0) circle[x,y] = true;
				}
				
			for(int i = 0; i < camSize.x; i++)
				for(int j = 0; j < camSize.y; j++)
					for(int k = 0; k < camSize.z; k++)
						lightArray[i,j,k] = 0;
					
			Spin_Light2();
			rayDelta = new XYZ_d();
			rayStrike = new XYZ_d();
            viewer = new Viewer(this,camSize,3);
		}
		public XYZ_d GetPosition(){return Position;}
		public XY<double> GetCursorPos(){return cursor;}
		public void SetPosition(XYZ_d p)
		{
			
		}
		
		void Make_CrossHead()
		{
			for(int i = camPoint.z - 3; i < camPoint.z + 3; i++)
			for(int j = camPoint.x - 3; j < camPoint.x + 3; j++)
				if(Math.Pow(i-camPoint.z,2) + Math.Pow(j-camPoint.x,2) < 8 &&
				   Math.Pow(i-camPoint.z,2) + Math.Pow(j-camPoint.x,2) > 4) board[i,j] = '+';
		}
		
		public void Print()
		{
			 Console.SetCursorPosition(0,0);
			string a = "";
			/* Console.ForegroundColor = ConsoleColor.White; */
			Make_CrossHead();
			
			ConsoleColor current = ConsoleColor.White;
			
			for(int i = 0; i < camSize.z; i++)
			{
				for(int j = 0; j < camSize.x; j++)
				{
					if(current!=color[i,j])
					{
						result.Add(new Ps(a,current));
						a = "";
						current = color[i,j];
					}
					a += board[i,j];
				}
				a += "\r\n";
			}
			result.Add(new Ps(a,current));
			foreach(Ps p in result)
			{
				p.Print();
			}
			result.Clear();
			
		}
		public void Spin_Light2()
		{
			double degreeX = -lightAngle.y * PI;
			double degreeY = lightAngle.x * PI;
			double sinX = Math.Sin(degreeX);
			double sinY = Math.Sin(degreeY);
			double cosX = Math.Cos(degreeX);
			double cosY = Math.Cos(degreeY);
			
			int x = 0; int y = 0; int z = 0;
			XYZ_d startPos = new XYZ_d();
			XYZ_d endPos = new XYZ_d();
			XYZ_d index = new XYZ_d();
			for(int i = 0; i < camSize.x; i++)
				for(int j = 0; j < camSize.y; j++)
					for(int k = 0; k < camSize.z; k++)
						lightArray[i,j,k] = 0;
					
			int radius = camPoint.x/2;
			for(int k = 0; k < camPoint.x; k++)
				for(int i = 0; i < camPoint.x; i++)
				{
					if(circle[i,k])
					{
						double length = 200;
						Spin_matrix_x(i - radius,0,k - radius, sinX,cosX,startPos);
						Spin_matrix_z(startPos.x,startPos.y,startPos.z,sinY,cosY,startPos);
						
						Spin_matrix_x(i - radius,length,k - radius, sinX,cosX,endPos);
						Spin_matrix_z(endPos.x,endPos.y,endPos.z,sinY,cosY,endPos);
						endPos.Sub(startPos).Div(length);
						index.Set(startPos);
						index.x += camPoint.x;
						index.z += camPoint.z;
						for(int j = 0; j < length; j++)
						{
							index.Add(endPos);
							x = index.iX;
							y = index.iY;
							z = index.iZ;
							if(x >= 0 && x < camSize.x && y >= 0 && y < camSize.y && z >= 0 && z < camSize.z)
							{
								double distance = Math.Sqrt(Math.Pow(i - radius,2) + Math.Pow(k - radius,2));
								double power = 14 * (camSize.y - j)/(camSize.y * 1f);
								if(power < 1) power = 1;
								lightArray[x,y,z] = power*(radius-distance)/radius;
							}
						}
					}
				}
				
		}
		public void Spin_XZAxis4()
		{
			isRendering = true;
			position.Set(Position);
			if(isLighting && isLightMove)
			{
				lightAngle.x += (Cursor.Position.X - screenPoint.X) * sensitivity;
				lightAngle.y += (Cursor.Position.Y - screenPoint.Y) * sensitivity;
				Spin_Light2();
			}
			else
			{
				cursor.x += (Cursor.Position.X - screenPoint.X) * sensitivity;
				cursor.y += (Cursor.Position.Y - screenPoint.Y) * sensitivity;
				
				cursor.x %= 360;
				cursor.y %= 360;
				
				cursor.y = cursor.y > 90 ? 90 :
				           (cursor.y < -90 ? -90 : cursor.y);
			}
			Cursor.Position = screenPoint;
			double degreeX = cursor.y * -PI;
			double degreeY = cursor.x * PI;
			double sinX = Math.Sin(degreeX);
			double sinY = Math.Sin(degreeY);
			double cosX = Math.Cos(degreeX);
			double cosY = Math.Cos(degreeY);

            for (int k = 0; k < camSize.z; k++)
            {
                //Parallel.For(0,camSize.x,(int i) =>
                for (int i = 0; i < camSize.x; i++)
                {
                    int depth = 0;
					Pixel pixel;
					board[k,i] = ' ';
					color[k,i] = ConsoleColor.Black;
					
					Spin_matrix_x(perspArray[i,k].x,perspArray[i,k].y,perspArray[i,k].z,sinX,cosX,spinArray[i,k]);
					Spin_matrix_z(spinArray[i,k].x,spinArray[i,k].y,spinArray[i,k].z,sinY,cosY,spinArray[i,k]);
					
					int length = camSize.y;
					
					XYZ_d delta = deltaArray[i,k].Set(spinArray[i,k]).Div(length*2);
					XYZ_d index = indexArray[i,k].Set(position);
					
					for(int j = 0; j < length*2; j++)
					{
						index.Add(delta);
						pixel = world.GetPixel(index);
						
						if(pixel != null)
						{
							if(pixel.Position == null) break;
							if(!pixel.isVisible) continue;
							
							if(isLighting)
							{
								/* depth = (int)(13 * (70 - position.Distance(pixel.Position)) / (70d));
								depth = depth < 0 ? 0 :
										depth > 9 ? 9 : depth; */
								depth += (int)lightArray[i,j/2,k];
							}
							depth += world.GetLevel(index) + pixel.level;
							depth = depth < 0 ? 0 :
									depth > 9 ? 9 : depth;
							
							board[k,i] = level[depth];
                            viewer.Draw(i,k,depth);
							color[k,i] = pixel.color;
							/* Console.Write(rayStrike.x);
							Console.Write(",");
							Console.Write(rayStrike.y);
							Console.Write(",");
							Console.Write(rayStrike.z);
							Console.WriteLine(); */
							break;
						}
					}
				}//);
			}//);
			isRendering = false;
			rayDelta.Set(spinArray[camPoint.x-1,camPoint.z-1]).Div(camSize.y);
			rayStrike.Set(indexArray[camPoint.x-1,camPoint.z-1]);
		}
		void Spin_matrix_x(double x, double y, double z, double sin, double cos, XYZ_d position)
		{
			position.y = y * cos + z * sin;
			position.z = z * cos + y * sin * -1;
			position.x = x;
		}
		void Spin_matrix_z(double x, double y, double z, double sin, double cos, XYZ_d position)
		{
			position.x = x * cos + y * sin;
			position.y = y * cos + x * sin * -1;
		}
		
		
	}
}