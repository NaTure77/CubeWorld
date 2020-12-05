using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
namespace VirtualCam
{
	public delegate void Func();
	class InputManager
	{
		Dictionary<ConsoleKey, Func> dic;
		public InputManager()
		{
			dic = new Dictionary<ConsoleKey, Func>();
		}
		
		public void Regist(ConsoleKey key, Func val)
		{
			dic[key] = val;
		}
		public void StartInputLoop()
		{
			Task.Factory.StartNew(()=>
			{
				ConsoleKey input;
				while(true)
				{
					input = Console.ReadKey(true).Key;
					if(dic.ContainsKey(input)) dic[input]();
				}
			});
		}
	}
}