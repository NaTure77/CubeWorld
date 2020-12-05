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
		private World world;
		
		private XYZ camSize;
		private XYZ camPoint;
		private XYZ_d Position;
		private XYZ_d position;
		public XYZ PositionIndex;
		
		private XYZ_d[,] perspArray;		
		private XYZ_d[,] indexArray;
		private XYZ_d[,] spinArray;
		private XYZ_d[,] deltaArray;
		private XYZ_d[,] perspBasisX;
		private XYZ_d[,] perspBasisZ;
		double sensitivity = 0.1d;
		public Point screenPoint = new Point(640,512);
		XY<double> cursor = new XY<double>(0,0);
		public bool isPaused = false;
		public bool gridEnabled = true;
		double PI = Math.PI / 180d;
		char[] level = {' ', '.', ':', '!', 'l', '1', 'G', 'D', 'H', '#'};	
		public XYZ_d rayDelta = new XYZ_d();
		public XYZ deleteFrameIndex = new XYZ();
		public XYZ addFrameIndex = new XYZ();
		public XYZ_d basisX;
		public XYZ_d basisZ;
		XYZ_d basisY;
        public Viewer viewer;

        public XYZ_d[,] board;
		public Camera(XYZ cs, XYZ_d cPos, World w)
		{
           
			camSize = cs;
			Position = cPos;
			PositionIndex = new XYZ();
			world = w;
			camPoint = new XYZ(camSize).Div(2);
			position = new XYZ_d(Position);
			
			perspArray = new XYZ_d[camSize.x,camSize.z];
			indexArray = new XYZ_d[camSize.x,camSize.z];
			spinArray = new XYZ_d[camSize.x,camSize.z];
			deltaArray = new XYZ_d[camSize.x,camSize.z];
			perspBasisX = new XYZ_d[camSize.x,camSize.z];
			perspBasisZ = new XYZ_d[camSize.x,camSize.z];


            board = new XYZ_d[camSize.x, camSize.z];
            //double rc_ratio = (1f * camSize.x) / camSize.z;

            double fov = 1d/120;
			
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
				perspBasisX[i,j] = new XYZ_d();
				perspBasisZ[i,j] = new XYZ_d();
			}
			basisX = new XYZ_d(1,0,0);
			basisY = new XYZ_d(0,camSize.y - 1,0);
			basisZ = new XYZ_d(0,0,1);

            viewer = new Viewer(this,camSize,2);
        }
		public XYZ_d GetPosition(){return Position;}
		public XY<double> GetCursorPos(){return cursor;}
		
		public void MakePauseView()
		{
			for(int i = camPoint.z - camSize.z/4; i < camPoint.z + camSize.z/4; i++)
			{
				Console.SetCursorPosition(camPoint.x - camSize.x/4,i);
				for(int j = camPoint.x - camSize.x/4; j < camPoint.x + camSize.x/4; j++)
				{
					Console.Write("@");
				}
			}
		}
        
		public void Spin_XZAxis5()
		{
			position.Set(Position);
			cursor.x += (Cursor.Position.X - screenPoint.X) * sensitivity;
			cursor.y += (Cursor.Position.Y - screenPoint.Y) * sensitivity;
			
			cursor.x %= 360;
			cursor.y %= 360;
			
			cursor.y = cursor.y > 90 ? 90 :
					   (cursor.y < -90 ? -90 : cursor.y);
			Cursor.Position = screenPoint;
			double degreeX = cursor.y * -PI;
			double degreeY = cursor.x * PI;
			double sinX = Math.Sin(degreeX);
			double sinY = Math.Sin(degreeY);
			double cosX = Math.Cos(degreeX);
			double cosY = Math.Cos(degreeY);
			
			
			bool isTargetSet = false;
			
			basisX.Set(1,0,0);
			basisY.Set(0,camSize.y - 1,0);
			basisZ.Set(0,0,1);
			
			Spin_matrix_x(basisX.x,basisX.y,basisX.z,sinX,cosX,basisX);
			Spin_matrix_z(basisX.x,basisX.y,basisX.z,sinY,cosY,basisX);
			
			Spin_matrix_x(basisY.x,basisY.y,basisY.z,sinX,cosX,basisY);
			Spin_matrix_z(basisY.x,basisY.y,basisY.z,sinY,cosY,basisY);
			
			Spin_matrix_x(basisZ.x,basisZ.y,basisZ.z,sinX,cosX,basisZ);
			Spin_matrix_z(basisZ.x,basisZ.y,basisZ.z,sinY,cosY,basisZ);
			world.GetFrameIndex(position,PositionIndex);
          //  Parallel.For(0,camSize.z,(int k) =>
            for(int k = 0; k < camSize.z; k++)
			{
                //Parallel.For(0,camSize.x,(int i) =>
                for (int i = 0; i < camSize.x; i++)
                {
					XYZ_b color = new XYZ_b();
                    Block block;
					
					perspBasisX[i,k].Set(basisX).Mul(perspArray[i,k].x);
					perspBasisZ[i,k].Set(basisZ).Mul(perspArray[i,k].z);
					
					spinArray[i,k].Set(perspBasisX[i,k]).Add(basisY).Add(perspBasisZ[i,k]);

					XYZ_d delta = deltaArray[i,k].Set(spinArray[i,k]).Div(camSize.y);
					XYZ_d index = indexArray[i,k].Set(position);
					
					XYZ_d gap = new XYZ_d();
					XYZ_d lpos = new XYZ_d();
					XYZ frameIndex = new XYZ();
					XYZ nextFrameIndexDelta = new XYZ();
					XYZ_d target = new XYZ_d();
					XYZ_d deltaSign = new XYZ_d();
					
					if(delta.x != 0) deltaSign.x = (Math.Abs(delta.x) / delta.x);
					if(delta.y != 0) deltaSign.y = (Math.Abs(delta.y) / delta.y);
					if(delta.z != 0) deltaSign.z = (Math.Abs(delta.z) / delta.z);
					frameIndex.Set(PositionIndex);
					double numOfDelta = 0;
					
					for(int j = 0; j < camSize.y; j++)
					{						
						if(!world.IsInFrame(frameIndex) || j == camSize.y - 1) break;
						
						world.ConvertToFramePos(lpos.Set(index),frameIndex); // index를 frameIndex에 해당하는 블록의 로컬포지션으로 변환. 

						if(world.IsExistPixelInFrame(frameIndex))
						{

                            block = world.GetBlock(frameIndex);
                            board[i, k] = index;
							if(i == camPoint.x && k == camPoint.z && !isTargetSet)
							{
								deleteFrameIndex.Set(frameIndex);
								addFrameIndex.Set(frameIndex).Sub(nextFrameIndexDelta);
								isTargetSet = true;
							}
                            
                            XYZ_b inv_color = new XYZ_b(255).Sub(block.color);
                            if (frameIndex.Equal(deleteFrameIndex))//쳐다보는 블록면 강조
                            {
                                XYZ vector = new XYZ(addFrameIndex).Sub(deleteFrameIndex);

                                if (vector.x != 0 && lpos.x * vector.x > world.frameLength / 2 - 1)
                                {
                                    viewer.Draw(i, k, inv_color);
                                    break;
                                }
                                else if (vector.y != 0 && lpos.y * vector.y > world.frameLength / 2 - 1)
                                {
                                    viewer.Draw(i, k, inv_color);
                                    break;
                                }
                                else if (vector.z != 0 && lpos.z * vector.z > world.frameLength / 2 - 1)
                                {
                                    viewer.Draw(i, k, inv_color);
                                    break;
                                }
                            }
							if(gridEnabled && block.code != 14)// 경계선표시.
							{
								bool Xaxis = Math.Abs(lpos.x) > world.frameLength/2 - 1; //경계선 테두리 크기 1
								bool Yaxis = Math.Abs(lpos.y) > world.frameLength/2 - 1;
								bool Zaxis = Math.Abs(lpos.z) > world.frameLength/2 - 1;
                                
                                if (Xaxis && Yaxis) { viewer.Draw(i, k, inv_color); break; }
                                if (Yaxis && Zaxis) { viewer.Draw(i, k, inv_color); break; }
                                if (Zaxis && Xaxis) { viewer.Draw(i, k, inv_color); break; }
                            }
							if(block.code == 14) // 거울만나면 반사.
							{
                                color.Set(block.color);
								if(nextFrameIndexDelta.x != 0)
								{
									delta.x = -delta.x;
									deltaSign.x = -deltaSign.x;
								}
								else if(nextFrameIndexDelta.y != 0)
								{
									delta.y = -delta.y;
									deltaSign.y = -deltaSign.y;
								}
								else if(nextFrameIndexDelta.z != 0)
								{
									delta.z = -delta.z;
									deltaSign.z = -deltaSign.z;
								}
								frameIndex.Sub(nextFrameIndexDelta); // 일보후퇴
								continue;
							}
							else if(block.code == 15) // 유리만나면 살짝 하얘지고 계속 진행.
							{
                                color.Add(10);
							}
							else
							{
                                color.Add(block.color);
                                viewer.Draw(i, k,color);
                                break;
							}
						}
                        //현위치에서 delta벡터방향으로 이동할 경우 가장 먼저 만나는 경계면 구하기.
						target.Set(world.frameLength/2);
						target.Mul(deltaSign);//delta벡터 방향으로 이동시 접촉가능한 경계면들 구하기.
						target.Sub(lpos).Div(delta);//경계면들로부터 현재위치의 거리를 구하고 delta로 나누기. deltasign으로 한번 곱했었기때문에 x,y,z축 서로에 대한 정확한 비교값이 나오게된다.
						nextFrameIndexDelta.Set(0);
						if(target.x < target.y && target.x < target.z)
						{
							numOfDelta = target.x;
							nextFrameIndexDelta.x = (int)deltaSign.x;
						}
						else if(target.y < target.x && target.y < target.z)
						{
							numOfDelta = target.y;
							nextFrameIndexDelta.y = (int)deltaSign.y;
						}
					    else if(target.z < target.x && target.z < target.y)
						{
							numOfDelta = target.z;
							nextFrameIndexDelta.z = (int)deltaSign.z;
						}
						frameIndex.Add(nextFrameIndexDelta);//가장가까운 경계면에 해당하는 블록으로 이동.
						index.Add(gap.Set(delta).Mul(numOfDelta));//delta방향으로 가장 가까운 경계면으로 이동.
						//여기까지 수정
					}
				}
                //);
			}
        //);
            rayDelta.Set(deltaArray[camPoint.x,camPoint.z]);
            for (int i = camPoint.z - 3; i < camPoint.z + 3; i++)
                for (int j = camPoint.x - 3; j < camPoint.x + 3; j++)
                    if (Math.Pow(i - camPoint.z, 2) + Math.Pow(j - camPoint.x, 2) < 8 &&
                       Math.Pow(i - camPoint.z, 2) + Math.Pow(j - camPoint.x, 2) > 4) viewer.Draw(j, i, new XYZ_b(255));
        }

        //void MakeImage()
        //{
        //    XYZ framePos = new XYZ();
        //    XYZ_d lpos = new XYZ_d();
        //    if (board[camPoint.x, camPoint.z] != null && !isTargetSet)
        //    {
        //        board[k, i].Add(position);
        //        world.GetFrameIndex(board[k, i], framePos);
        //        deleteFrameIndex.Set(framePos);
        //       // addFrameIndex.Set(frameIndex).Sub(nextFrameIndexDelta);
        //        isTargetSet = true;
        //    }
        //    for (int k = 0; k < camSize.z; k++)
        //    {
        //        for(int i = 0; i < camSize.x; i++)
        //        {
        //            if (board[k, i] != null)
        //            {
        //                board[k, i].Add(position);
        //                world.GetFrameIndex(board[k, i], framePos);
        //                world.ConvertToFramePos(lpos.Set(board[k, i]), framePos);
        //                Block block = world.GetBlock(frameIndex);
        //                if (framePos.Equal(deleteFrameIndex))//쳐다보는 블록면 강조
        //                {
        //                    XYZ_b inv_color = new XYZ_b(255).Sub(block.color);
        //                    XYZ vector = new XYZ(addFrameIndex).Sub(deleteFrameIndex);

        //                    if (vector.x != 0 && lpos.x * vector.x > world.frameLength / 2 - 1)
        //                    {
        //                        viewer.Draw(i, k, inv_color);
        //                        break;
        //                    }
        //                    else if (vector.y != 0 && lpos.y * vector.y > world.frameLength / 2 - 1)
        //                    {
        //                        viewer.Draw(i, k, inv_color);
        //                        break;
        //                    }
        //                    else if (vector.z != 0 && lpos.z * vector.z > world.frameLength / 2 - 1)
        //                    {
        //                        viewer.Draw(i, k, inv_color);
        //                        break;
        //                    }
        //                }
        //                if (gridEnabled && block.code != 14)// 경계선표시.
        //                {
        //                    bool Xaxis = Math.Abs(lpos.x) > world.frameLength / 2 - 1; //경계선 테두리 크기 1
        //                    bool Yaxis = Math.Abs(lpos.y) > world.frameLength / 2 - 1;
        //                    bool Zaxis = Math.Abs(lpos.z) > world.frameLength / 2 - 1;

        //                    if (Xaxis && Yaxis) { viewer.Draw(i, k, inv_color); break; }
        //                    if (Yaxis && Zaxis) { viewer.Draw(i, k, inv_color); break; }
        //                    if (Zaxis && Xaxis) { viewer.Draw(i, k, inv_color); break; }
        //                }
        //            }
        //        }
        //    }
            
        //}
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