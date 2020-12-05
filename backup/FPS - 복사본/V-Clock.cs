using System;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualCam
{
	class Clock
	{
		int [] seg = {0x1110111, 0x0100100, 0x1011101, 0x1101101, 0x0101110,
					  0x1101011, 0x1111011, 0x0100111, 0x1111111, 0x1101111};
		
		char[,] face;
		char[,] picture;
		XY<int>[] SnPos;
		XY<int>[] MnPos;
		XY<int>[] HnPos;
		XY<int>[] CnPos;
		XY<int> segPos;
		XY<int> nPointer = new XY<int>(0,0);
		XY<int> cronoPos;
		
		int HneedleLength;
		int MneedleLength;
		int SneedleLength;
		
		int faceSize;
		int radius;
		int crono_radius;
		XY<int>[,] decoration;
		XYZ position;
		//World world;
		/* public static void Main(string[] args)
		{
			new Clock(new XYZ(100,100,50),40);
		} */
		public Clock(XYZ p, int radius)//, World w)
		{
			this.radius = radius;
			this.position = p;
			crono_radius = radius/3;
			faceSize = radius * 2 + 1;
			face = new char[faceSize,faceSize];
			picture = new char[faceSize,faceSize];
			SneedleLength = radius - 4;
			MneedleLength = radius - 12;
			HneedleLength = radius - 18;
			SnPos = new XY<int>[SneedleLength];
			MnPos = new XY<int>[MneedleLength];
			HnPos = new XY<int>[HneedleLength];
			CnPos = new XY<int>[crono_radius];
			segPos = new XY<int>(radius/2,radius/2+5);
			decoration = new XY<int>[5,8];
			cronoPos = new XY<int>(radius + crono_radius / 2 + 1, radius + crono_radius / 2 + 1);
			/* DateTime n = DateTime.Now;
			if(n.Hour */
			init();
			Start_Clock();
		}
		public void init()
		{
			for(int i = 0; i < faceSize; i++)
				for(int j = 0; j < faceSize; j++)
				{
					if(Math.Pow(i - radius-1,2) + Math.Pow(j - radius - 1, 2) > Math.Pow(radius - 1, 2) &&
					   Math.Pow(i - radius-1,2) + Math.Pow(j - radius - 1, 2) < Math.Pow(radius, 2))
					   face[i,j] = '#';
					  
					else if(Math.Pow(i - radius-1,2) + Math.Pow(j - radius - 1, 2) < Math.Pow(radius - 5, 2)&&
					        Math.Pow(i - radius-1,2) + Math.Pow(j - radius - 1, 2) > Math.Pow(radius - 7, 2))
							face[i,j] = '=';
					else face[i,j] = ' ';
				}
			
			
			for(int i = 0; i < SneedleLength; i++)
				SnPos[i] = new XY<int>(radius + 1, radius - i + 1);
			for(int i = 0; i < MneedleLength; i++)
				MnPos[i] = new XY<int>(radius + 1, radius - i + 1);
			for(int i = 0; i < HneedleLength; i++)
				HnPos[i] = new XY<int>(radius + 1, radius - i + 1);
			for(int i = 0; i < crono_radius; i++)
				CnPos[i] = new XY<int>(radius + 1, radius - i + 1);
			for(int i = 0; i < 5; i++)
				for(int j = 0; j < 8; j++)
					decoration[i,j] = new XY<int>(radius - 1 + i, j + 2);
				
			for(int i = 0; i < 12; i++)
				for(int j = 0; j < 5; j++)
					for(int k = 0; k < 6; k++)
					{
						Spin_Matrix(decoration[j,k].x,decoration[j,k].y, - i * 30, nPointer);
						face[nPointer.x,nPointer.y] = '-';
					}
			/* for(int i = 0; i < faceSize; i++)
			{
				for(int j = 0; j < faceSize; j++)
				{
					Console.Write(face[i,j]);
				}
				Console.WriteLine();
			}
			Console.Read(); */
		}
		
		public void Display_Segment(int x, int y, int number, int size, char shape)
		{
			int[] numbers = {number / 10, number % 10};
			for(int n = 0; n < 2; n++)
			{
				if((seg[numbers[n]] & 0x0000001) != 0)for(int i = 0; i < size; i++) picture[x+i+1,y] = shape;
				if((seg[numbers[n]] & 0x0000010) != 0)for(int i = 0; i < size; i++) picture[x,y+i+1] = shape;
				if((seg[numbers[n]] & 0x0000100) != 0)for(int i = 0; i < size; i++) picture[x+size+1,y+i+1] = shape;
				if((seg[numbers[n]] & 0x0001000) != 0)for(int i = 0; i < size; i++) picture[x+i+1,y+size+1] = shape;
				if((seg[numbers[n]] & 0x0010000) != 0)for(int i = 0; i < size; i++) picture[x,y+i+size+1+1] = shape;
				if((seg[numbers[n]] & 0x0100000) != 0)for(int i = 0; i < size; i++) picture[x+size+1,y+i+size+1+1] = shape;
				if((seg[numbers[n]] & 0x1000000) != 0)for(int i = 0; i < size; i++) picture[x+i+1,y+size+1+size+1] = shape;
				x += size + 3;
			}
		}
		
		public void Start_Clock()
		{
			new Thread(()=>
			{
				DateTime n = DateTime.Now;
				int second = DateTime.Now.Second;
				while(true)
				{
					n = DateTime.Now;
					n.AddSeconds(1);
					Working(n.Hour,n.Minute,n.Second);
					Working_Crono(n.Year,n.Month,n.Day);
					while(n >= DateTime.Now){Thread.Sleep(30);}
					Print();
				}
			}).Start();
		}
		
		public void Working(int hour, int minute, int second)
		{
			Display_Segment(segPos.x,segPos.y,hour,3,'*');
			Display_Segment(segPos.x + 15,segPos.y,minute,3,'*');
			Display_Segment(segPos.x + 30,segPos.y,second,3,'*');
			int i = hour * 3600 + minute * 60 + second;
			for(int j = 0; j < HneedleLength; j++)
			{
				Spin_Matrix(HnPos[j].x,HnPos[j].y,-i/120,nPointer);
				picture[nPointer.x,nPointer.y] = 'H';
			}
			for(int j = 0; j < MneedleLength; j++)
			{
				Spin_Matrix(MnPos[j].x,MnPos[j].y,-i/10,nPointer);
				picture[nPointer.x,nPointer.y] = 'M';
			}
			for(int j = 0; j < SneedleLength; j++)
			{
				Spin_Matrix(SnPos[j].x,SnPos[j].y,-i * 6,nPointer);
				picture[nPointer.x,nPointer.y] = 'S';
			}
			for(int k = 0; k < faceSize; k++)
				for(int j = 0; j < faceSize; j++)
				{
					if(Math.Pow(k - radius-1,2) + Math.Pow(j - radius - 1, 2) < Math.Pow(3,2))
					{
						if(Math.Pow(k - radius-1,2) + Math.Pow(j - radius - 1, 2) < Math.Pow(2,2))
							picture[k,j] = ' ';
						else picture[k,j] = 'O';
					}
				}

		}
		
		public void Working_Crono(int year, int Date, int day)
		{
			Display_Segment(cronoPos.x - 10,cronoPos.y + 4,Date,2,'*');
			Display_Segment(cronoPos.x,cronoPos.y + 4,day,2,'*');
			Display_Segment(cronoPos.x - 24,cronoPos.y + 4,year % 2000,2,'*');
		}
		
		public void Print()
		{
			Console.SetCursorPosition(0,0);
			string s = "";
			for(int i = 0; i < faceSize; i++)
			{
				for(int j = 0; j < faceSize; j++)
				{
					s += picture[j,i];
				}
			s += '\n';
			}
			Console.Write(s);
			for(int i = 0; i < faceSize; i++)
				for(int j = 0; j < faceSize; j++)
				{
					picture[i,j] = face[i,j];
				}
		}
		
		void Spin_Matrix(int x, int y, int degree, XY<int> position)
		{
			double d = degree * (Math.PI / 180d);
			position.x = (int)Math.Round((x - radius -1 ) * Math.Cos(d) + (y - radius - 1) * Math.Sin(d)) + radius + 1;
			position.y = (int)Math.Round((y - radius -1 ) * Math.Cos(d) + (x - radius - 1) * (-Math.Sin(d))) + radius + 1;
		}
	}
}