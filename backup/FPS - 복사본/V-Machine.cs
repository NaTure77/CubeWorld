using System;
using System.Threading;
using System.Threading.Tasks;
namespace VirtualCam
{
	abstract class Machine
	{
		public XYZ_d Position;
		public bool isWorking = false;
		public Modifier modifier;
		public Machine(XYZ_d p, Modifier m)
		{
			Position = p; modifier = m;
		}
		protected abstract void DoSomething();
		public void Working()
		{
			if(isWorking)
			{
				isWorking = false; return;
			}
			isWorking = true;
			Task.Factory.StartNew(()=>
			{
				while(isWorking) DoSomething();
				isWorking = false;
			});
		}
	}
	
	class MovingCube : Machine
	{
		private XYZ_d moveDelta = null;
		private XYZ_d rotateDelta = null;
		XYZ_d degree;
		public Object obj;
		
		public MovingCube(XYZ_d p, Modifier m) : base(p,m)
		{
			degree = new XYZ_d();
			moveDelta = new XYZ_d(0,0.1d,0);
			rotateDelta = new XYZ_d(0,2,0);
			obj = new Cube(Position,new XYZ(10,30,10));
			modifier.DrawObject(obj);
			modifier.SetSwitch(obj,new Modifier.Work(Working));
		}
		
		protected override void DoSomething()
		{
			modifier.SpinObject(obj,degree);
			modifier.MoveObject(obj,moveDelta);
			Thread.Sleep(10);
			degree.Add(rotateDelta);
			degree.Remain(360);
		}
	}
	
	class FollowLight : Machine
	{
		Object obj;
		XYZ_d target;
		private XYZ size;
		double speed = 2d;
		XYZ_d delta;
		XYZ_d direction;
		
		public FollowLight(Modifier m, XYZ_d p, XYZ s, XYZ_d t) : base(p,m)
		{
			size = s;
			target = t;
			obj = new LightSphere(Position,size.x,size.x*4,20);
			modifier.DrawObject(obj);
			modifier.SetSwitch(obj,new Modifier.Work(Working));
			direction = new XYZ_d();
			delta = new XYZ_d();
		}
		protected override void DoSomething()
		{
			delta.Set(target).Sub(obj.Position);
			direction.Add(delta).Div(4);
			delta.Set(direction).Div(direction.Length()).Mul(speed);
			delta.z = (target.z - obj.Position.z - 20) / 20;
			
			modifier.MoveObject(obj,delta);
			Thread.Sleep(1);
		}
	}
	
	class AliveSinFlow : Machine
	{
		Object obj;
		private XYZ_d moveDelta = null;
		int val = 0;
			
		public AliveSinFlow(XYZ_d p, XYZ size, Modifier m) : base(p,m)
		{
			obj = new Cube(Position,size);
			modifier.SetColor(obj,128,128,128);
			modifier.SetSwitch(obj,new Modifier.Work(Working));
			moveDelta = new XYZ_d(0,0,-0.5d);
			modifier.DrawObject(obj);
		}
		protected override void DoSomething()
		{
			modifier.MakeSinFlow(obj,val++);
			//modifier.Move
			Thread.Sleep(1);
		}
	}
}