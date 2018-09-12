using NSeetafaceDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Seetaface
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FaceInfo
    {
        public Rect bbox;

        public double roll;
        public double pitch;
        public double yaw;

        public double score; /**< Larger score should mean higher confidence. */
    }

    [StructLayout(LayoutKind.Sequential)] 
    public struct FacialLandmark
    {
        public double x;
        public double y;
    }

    /// <summary>
    /// 人脸对齐结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AlignmentResult
    {
        public FaceInfo face;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 5)]
        public FacialLandmark[] landmark;
    }


    public delegate void LogCallBack(string logInfo);

    public class SeetafaceHelper
    {

        /// <summary>
        /// 设置日志回调函数(用于日志打印)
        /// </summary>
        /// <param name="logCallBack"></param>
        [DllImport("NSeetaface.dll", EntryPoint = "SetDisplayLog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetDisplayLog(LogCallBack logCallBack);

        /// <summary>
        /// 设置人脸模型的目录
        /// </summary>
        /// <param name="dirPath"></param>
        [DllImport("NSeetaface.dll", EntryPoint = "SetModelDirectory", CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetModelDirectory(byte[] dirPath);

        /// <summary>
        /// 初始化人脸识别
        /// </summary>
        /// <returns></returns>
        [DllImport("NSeetaface.dll", EntryPoint = "Init")]
        public extern static bool Init();

        /// <summary>
        /// 检测图片中的人脸.获取第一张人脸
        /// </summary>
        /// <param name="path"></param>
        /// <param name="faceInfo"></param>
        /// <returns></returns>
        [DllImport("NSeetaface.dll", EntryPoint = "DetectFace", CallingConvention = CallingConvention.Cdecl)]
        public extern static int DetectFace(
            string path,
            ref FaceInfo faceInfo
        );

        /// <summary>
        /// 检测图片中的人脸,返回多张人脸JSON数据
        /// </summary>
        /// <param name="path"></param>
        /// <param name="faceInfo"></param>
        /// <returns></returns>
        [DllImport("NSeetaface.dll", EntryPoint = "DetectFaces", CallingConvention = CallingConvention.Cdecl)]
        public extern static int DetectFaces(string path, StringBuilder json);

        /// <summary>
        /// 人脸对齐
        /// </summary>
        /// <param name="path"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        [DllImport("NSeetaface.dll", EntryPoint = "Alignment", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Alignment(string path, StringBuilder json);

        /// <summary>
        /// 提取特征
        /// </summary>
        /// <param name="picPath"></param>
        /// <param name="face"></param>
        /// <param name="pt5"></param>
        /// <returns></returns>
        [DllImport("NSeetaface.dll", EntryPoint = "ExtractFeature", CallingConvention = CallingConvention.Cdecl)]
        public extern static bool ExtractFeature(string picPath, 
            //ref FaceInfo face, ref FacialLandmark[] pt5
            ref AlignmentResult alignmentResult,
            float[] feat
        );

        /// <summary>
        /// 计算相似度
        /// </summary>
        /// <param name="feat1">人脸特征1</param>
        /// <param name="feat2">人脸特征2</param>
        /// <returns></returns>
        [DllImport("NSeetaface.dll", EntryPoint = "CalcSimilarity", CallingConvention = CallingConvention.Cdecl)]
        public extern static double CalcSimilarity(float[] feat1, float[] feat2);


      
    }
}
