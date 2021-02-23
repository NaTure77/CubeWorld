using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace VirtualCam
{
	class Controller
	{
		private World world;
		private XYZ_d Position;
		XYZ_d scalaVector = new XYZ_d();
		XYZ halfBodySize = new XYZ(5,5,10);
		double speed = 80;
		double PI = Math.PI / 180d;
        public static Controller instance;
        XYZ_d Vector = new XYZ_d();
        XYZ_d gravity = new XYZ_d();
        bool isJumpable = true;
		public Controller(World w, Camera c)
		{
            instance = this;
			world = w; 
			Position = c.GetPosition();
			RegistKey(c);
			renderer = world.renderer_block;
		}
		void RegistKey(Camera camera)
		{
            InputManager.Regist(Keys.W, new Func(() => { Move(0, 1, 0,camera.GetCursorPos().x); }),true);
            InputManager.Regist(Keys.S, new Func(() => { Move(0, -1, 0,camera.GetCursorPos().x); }), true);
            InputManager.Regist(Keys.D, new Func(() => { Move(1, 0, 0,camera.GetCursorPos().x); }), true);
            InputManager.Regist(Keys.A, new Func(() => { Move(-1, 0, 0,camera.GetCursorPos().x); }), true);
            InputManager.Regist(Keys.E, new Func(() => { Shoot(new XYZ_b(255,0,0),camera.rayDelta); }), false);
            InputManager.Regist(Keys.R, new Func(() => { Shoot(new XYZ_b(0,255,0),camera.rayDelta); }), false);
            InputManager.Regist(Keys.T, new Func(() => { Shoot(new XYZ_b(0,0,255),camera.rayDelta); }), false);
           // InputManager.Regist(Keys.C, new Func(() => { Move(0, 0, 1); }), true);
           // InputManager.Regist(Keys.V, new Func(() => { Move(0, 0, -1); }), true);
            InputManager.Regist(Keys.V, new Func(() => { if (isJumpable) { Vector.z = -80; isJumpable = false; } }), true);
            InputManager.Regist(Keys.Q, new Func(() => {
                double boostSpeed = 50;
                Vector.Add(camera.rayDelta.x * boostSpeed, camera.rayDelta.y * boostSpeed, camera.rayDelta.z * boostSpeed);
                //Vector.z += boostSpeed;
            }), true);
            
            InputManager.Regist(Keys.P, new Func(() => { camera.gridEnabled = !camera.gridEnabled; }), false);
            
            /* InputManager.Regist(Keys.D2, new Func(() => { color = (byte)15; }), false);
            InputManager.Regist(Keys.D3, new Func(() => { color = (byte)14; }), false);
            InputManager.Regist(Keys.D4, new Func(() => { color = (byte)4; }), false);
            InputManager.Regist(Keys.D5, new Func(() => { color = (byte)5; }), false);
            InputManager.Regist(Keys.D6, new Func(() => { color = (byte)6; }), false);
            InputManager.Regist(Keys.D7, new Func(() => { color = (byte)7; }), false);
            InputManager.Regist(Keys.D8, new Func(() => { code = (byte)14; }), false);
            InputManager.Regist(Keys.D9, new Func(() => { code = (byte)15; }), false);
            InputManager.Regist(Keys.D0, new Func(() => { code = (byte)1; }), false); */
            InputManager.Regist(Keys.D0, new Func(() => { renderer = world.renderer_block; }), false);
            InputManager.Regist(Keys.D1, new Func(() => { renderer = world.renderer_air; }), false);
            InputManager.Regist(Keys.D2, new Func(() => { renderer = world.renderer_mirror; }), false);
            InputManager.Regist(Keys.Space, new Func(() => { AddBlock(camera.addFrameIndex,camera.PositionIndex); }), false);
            InputManager.Regist(Keys.X, new Func(() => { DeleteBlock(camera.deleteFrameIndex); }), false);
			
			InputManager.Regist(Keys.M, new Func(() => 
			{
                if (camera.viewer.qualityLevel > -2)
                {
                    camera.Resize(camera.camSize.x / 2, camera.camSize.y, camera.camSize.z / 2);
                    camera.viewer.UpdateBufferSize(camera.camSize);
                    camera.viewer.qualityLevel--;
                }
			}), false);
            InputManager.Regist(Keys.N, new Func(() => 
			{
                if (camera.viewer.qualityLevel < 2)
                {
                    camera.Resize(camera.camSize.x * 2, camera.camSize.y, camera.camSize.z * 2);
                    camera.viewer.UpdateBufferSize(camera.camSize);
                    camera.viewer.qualityLevel++;
                }
			}), false);
			camera.viewer.SetKeyDownEvent((sender,e)=>
			{
				InputManager.AddLongClick(e.KeyCode);
				InputManager.DoOneClick(e.KeyCode);
			});
			
			camera.viewer.SetKeyUpEvent((sender,e)=>
			{
				InputManager.SubLongClick(e.KeyCode);
			});
			
            

        }
		
		public byte color = 6;
        public byte code = 3;
		public Func<XYZ_d, XYZ, XYZ, int, bool> renderer;
		public void AddBlock(XYZ blockPosition, XYZ positionIndex)
		{
           // XYZ pos = camera.PositionIndex;
            if (world.IsInFrame(blockPosition) && 
                !blockPosition.Equal(positionIndex) && !blockPosition.Equal(positionIndex.x,positionIndex.y,positionIndex.z+1))
			{
                //world.SetBlock(camera.addFrameIndex, code);
                world.SetBlock(blockPosition,true);
				world.SetRender(blockPosition,renderer);
                world.SetColor(blockPosition,new XYZ_b((byte)(color * 25)));
			}
		}
		
		public void DeleteBlock(XYZ blockPosition)
		{
			if(world.IsInFrame(blockPosition))
			{
				world.SetBlock(blockPosition,false);
				world.SetColor(blockPosition,0,0,0);
				world.SetRender(blockPosition,world.renderer_air);
			}
		}

		public void Move(XYZ_d vector, double degree){Move(vector.x,vector.y,vector.z, degree);}
		public void Move(double x, double y, double z, double degree)
		{
			Spin_matrix_z(x,y,degree,scalaVector);
			scalaVector.Mul(speed);
            //scalaVector.Set(camera.rayDelta).Mul(y,y,y).Add(camera.basisX.x * x,camera.basisX.y * x,0).Mul(speed);
			
			if (!Check_Wall(scalaVector))
            {
                Position.Add(scalaVector);
                world.ConvertToInfinity(Position);
            }
            else
            {
                XYZ_d pos = new XYZ_d(Position).Add(scalaVector);
                XYZ index = new XYZ();
                world.GetFrameIndex(pos, index);
                if (!world.isFrameEnabled(index))
                {
                    Vector.z = -40;
                }
            }
		}
		
        bool Check_Wall(XYZ_d p)
        {
            XYZ_d pos = new XYZ_d(Position).Add(p);
            pos.z += world.frameLength;

            bool a = world.isFrameEnabled(pos);
            pos.z -= world.frameLength;
            bool b = world.isFrameEnabled(pos);
            return a || b;
        }
		
		//camera.PositionIndex
        public void CrushFrame(int scale, XYZ positionIndex)
        {
            XYZ temp = Vector.ToXYZ();
            temp.Div((int)(temp.Length() / (scale / 4 * 3)));
            XYZ brokePos = new XYZ(positionIndex).Sub(temp);
            XYZ lightPos = new XYZ(positionIndex);
            int maxScale = 60;
            if (scale > maxScale) scale = maxScale;
            int lightScale = scale + 2;
            XYZ color = new XYZ(255 * scale / 30, 0, 0);
            XYZ gap = new XYZ();

            for(int i = -lightScale; i < lightScale; i++)
            for(int j = -lightScale; j < lightScale; j++)
            for(int k = -lightScale; k < lightScale; k++)
            {
                temp.Set(brokePos).Add(i, j, k);
                world.ConvertToInfinity(temp);
                //if (!world.IsInFrame(temp)) continue;
                if (Math.Sqrt(i * i + j * j + k * k) < scale)
                {
                    world.SetFrame(temp, false);
                }
                //double distance = lightPos.Distance(temp);
                //if (distance < scale)
                //{
                //    XYZ_b c = world.GetColor(temp);
                //    if (world.isFrameEnabled(temp))
                //    {
                //        gap.x = (int)color.x - (int)c.x;
                //        gap.y = (int)color.y - (int)c.y;
                //        gap.z = (int)color.z - (int)c.z;
                //        gap.Mul((int)(maxScale - distance)).Div(maxScale);
                //        c.x = (byte)(c.x + gap.x);
                //        c.y = (byte)(c.y + gap.y);
                //        c.z = (byte)(c.z + gap.z);
                //    }
                //}
            }
        }
        
        public void Falling()
        {
            Vector.z += 4;
            if(!Check_Wall(Vector))
            {
                Position.Add(Vector);
                world.ConvertToInfinity(Position);
            }
            else if(Vector.Length() != 0)
            {
                XYZ index = new XYZ();
                world.GetFrameIndex(new XYZ_d(Position).Add(Vector), index);
                
                double vLength = Vector.Length();
                if (vLength > 90)
                {
                   // if(world.IsInFrame(index))
                     //   CrushFrame((int)(vLength / 4));
                    Vector.Div(4);
                    return;
                }
                else isJumpable = true;

                world.ConvertIndexToPosition(index);
                if (Vector.z > 0)
                {
                    Position.z = index.z - world.frameLength / 2 - 1;
                   
                }
                else
                {
                    Position.z = index.z + world.frameLength / 2 + 4;
                    isJumpable = false;
                }
                Vector.Set(0);

            }
        }

        void Shoot(XYZ_b color, XYZ_d rayDelta)
        {
			XYZ_d delta = new XYZ_d(rayDelta);
            XYZ_d lpos = new XYZ_d(delta).Mul(world.frameLength * 2).Add(Position);
            XYZ frameIndex = new XYZ();
            world.GetFrameIndex(lpos, frameIndex);
            world.ConvertToFramePos(frameIndex,lpos );
            XYZ frameIndex2 = new XYZ(frameIndex);
			int nextDir = 0;
            XYZ_d target = new XYZ_d();
            XYZ_d deltaSign = new XYZ_d();
			XYZ_d maxNumOfDelta = new XYZ_d(world.frameLength);
            if (delta.x != 0) deltaSign.x = (Math.Abs(delta.x) / delta.x);
            if (delta.y != 0) deltaSign.y = (Math.Abs(delta.y) / delta.y);
            if (delta.z != 0) deltaSign.z = (Math.Abs(delta.z) / delta.z);
			
			maxNumOfDelta.Mul(deltaSign).Div(delta);
			target.Set(world.halfFrameLength).Mul(deltaSign);//delta벡터 방향으로 이동시 접촉가능한 경계면들 구하기.
            target.Sub(lpos).Div(delta);//경계면들로부터 현재위치의 거리를 구하고 delta로 나누기. deltasign으로 한번 곱했었기때문에 x,y,z축 서로에 대한 정확한 비교값이 나오게된다.
            Task.Factory.StartNew(() =>
            {
                while (!world.isFrameEnabled(frameIndex))
                {
                    world.ConvertToInfinity(frameIndex);
                    world.SetFrame(frameIndex2, false);
                    world.SetFrame(frameIndex, true);
                    world.SetColor(frameIndex, color);
                  
                    if (target.x < target.y)
						if(target.x < target.z) nextDir = 0;
						else nextDir = 2;
					else
						if(target.y < target.z) nextDir = 1;
						else nextDir = 2;
					target.element[nextDir] += maxNumOfDelta.element[nextDir];
					frameIndex2.Set(frameIndex);
					frameIndex.element[nextDir] += (int)deltaSign.element[nextDir];
                    Thread.Sleep(10);
                }
				
                world.SetFrame(frameIndex2, false);
                XYZ temp = new XYZ();
                int scale = 10;
                double distance = 0;
                for (int i = -scale; i < scale; i++)
                    for (int j = -scale; j < scale; j++)
                        for (int k = -scale; k < scale; k++)
                        {
                            temp.Set(frameIndex).Add(i, j, k);
                            distance = frameIndex.Distance(temp);
                            if (distance < scale)
                            {
								world.ConvertToInfinity(temp);
                                XYZ_b c = world.GetColor(temp);
                                //gap.x = (int)color.x - (int)c.x;
                                //gap.y = (int)color.y - (int)c.y;
                                //gap.z = (int)color.z - (int)c.z;
                                //gap.Mul((int)(scale - distance)).Div(scale);
                                //c.x = (byte)(c.x + gap.x);
                                //c.y = (byte)(c.y + gap.y);
                                //c.z = (byte)(c.z + gap.z);
                                temp.x = (int)color.x * (int)(scale - distance) / scale;
                                temp.y = (int)color.y * (int)(scale - distance) / scale;
                                temp.z = (int)color.z * (int)(scale - distance) / scale;
                                c.Add((byte)temp.x, (byte)temp.y, (byte)temp.z);
                                // world.SetFrame(temp, false);
                                //world.SetColor(temp, 255, 0, 0);
                            }
                        }
                    });
        }
		void Spin_matrix_z(double x, double y, double d, XYZ_d position)
		{
			double degree = d * PI;
			double sin = Math.Sin(degree);
			double cos = Math.Cos(degree);
	
			position.x = x * cos + y * sin;
			position.y = y * cos + x * (-sin);
		}
		
		/* void Spin_matrix_z(double x, double y, double d, XYZ_d position, XYZ_d point)
		{
			double degree = d * PI;
			double sin = Math.Sin(degree);
			double cos = Math.Cos(degree);
	
			position.x = (x - point.x) * cos + (y - point.y) * sin + point.x;
			position.y = (y - point.y) * cos + (x - point.x) * (-sin) + point.y;
		} */
	}
}