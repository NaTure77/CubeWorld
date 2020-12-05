using System;
using System.Threading;
using System.Threading.Tasks;
namespace b
{
	class MainApp
	{
		public static void Main(string[] args)
		{
			char a =  ' ';
			while(true)
			{
				a = Console.ReadKey().KeyChar;
				
				for(int i = 0 ; i < 20; i++)
				{
					for(int j = 0; j < 50; j++)
						Console.Write(a);
					Console.WriteLine();
				}
			}
		}
	}
}