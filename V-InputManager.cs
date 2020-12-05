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
		static Dictionary<Keys, Func> dic1 = new Dictionary<Keys, Func>();
		static Dictionary<Keys, Func> dic2 = new Dictionary<Keys, Func>();
        public static Func currentFunc = null;
        public static Func currentFuncs = null;
        public static Func deleteFuncs = null;
        public static void Regist(Keys key, Func val,bool isLongClick)
        {
            if (isLongClick) dic2[key] = val;
            else dic1[key] = val;
        }

        public static void AddLongClick(Keys key)
        {
            if (dic2.ContainsKey(key))
            {
                //dic2[key](); // 적어도 한번 실행.
                currentFuncs -= dic2[key];
                currentFuncs += dic2[key];
            }
           
        }
        public static void SubLongClick(Keys key)
        {
            if (dic2.ContainsKey(key)) currentFuncs -= dic2[key];
        }
        public static void DoLogic()
        {
            currentFuncs();
        }

        public static void DoOneClick(Keys key)
        {
            if (dic1.ContainsKey(key)) dic1[key]();
        }
	}
}