using System;
using CIExam.Math;

namespace CIExam.Geometry
{
    public class TransFormUtil
    {
        public Matrix<double> Transform3D(int x, int y, int z)
        {
            return new Matrix<double>(
                Utils.CreateTwoDimensionList(
                    new double[16]
                    {
                        1, 0, 0, x,
                        0, 1, 0, y,
                        0, 0, 1, z,
                        0, 0, 0, 1
                    },
                    4,4
                    )
            );
        }
        
        public Matrix<double> Transform2D(int x, int y)
        {
            return new Matrix<double>(
                Utils.CreateTwoDimensionList(
                    new double[9]
                    {
                        1, 0, x,
                        0, 1, y,
                        0, 0, 1
                    },
                    3,3
                )
            );
        }
        
        //逆时针旋转角度
        public static Matrix<double> Rotation2D(double theta)
        {
            theta = System.Math.PI * theta / 180.0;
            return new Matrix<double>(
                Utils.CreateTwoDimensionList(
                    new double[9]
                    {
                        System.Math.Cos(theta), - System.Math.Sin(theta), 0,
                        System.Math.Sin(theta), System.Math.Cos(theta), 0,
                        0, 0, 1
                    },
                    3,3
                )
            );
        }
        
        //0 x 1 y 2 z
        public static Matrix<double> Rotation3D(int theta, int axis = 0)
        {
            switch (axis)
            {
                case 2:
                    return new Matrix<double>(
                        Utils.CreateTwoDimensionList(
                            new double[16]
                            {
                                System.Math.Cos(theta), - System.Math.Sin(theta), 0,0,
                                System.Math.Sin(theta), System.Math.Cos(theta), 0,0,
                                0, 0, 1, 0,
                                0, 0, 0, 1
                            },
                            4,4
                        )
                    );
                case 0:
                    return new Matrix<double>(
                        Utils.CreateTwoDimensionList(
                            new double[16]
                            {
                                1, 0, 0, 0,
                                0, System.Math.Cos(theta), - System.Math.Sin(theta), 0,
                                0, System.Math.Sin(theta), System.Math.Cos(theta), 0, 
                                0, 0, 0, 1
                            },
                            4,4
                        )
                    );
                case 1:
                    return new Matrix<double>(
                        Utils.CreateTwoDimensionList(
                            new double[16]
                            {
                                System.Math.Cos(theta), 0,  System.Math.Sin(theta), 0,
                                0, 1, 0, 0,
                                - System.Math.Sin(theta),0 , System.Math.Cos(theta), 0, 
                                0, 0, 0, 1
                            },
                            4,4
                        )
                    );
                    
            }

            return null;
           
        }
    }
}