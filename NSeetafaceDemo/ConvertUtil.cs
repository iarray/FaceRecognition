using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSeetafaceDemo
{
    public static class  ConvertUtil
    {
        /// <summary>
        /// float数组转字节数组
        /// </summary>
        /// <param name="farr"></param>
        /// <returns></returns>
        [Obsolete("Java byte[]转float会丢失精度, 推荐使用FloatArrayToStringBytes")]
        public static byte[] FloatArrayToByteArray(float[] farr)
        {
            byte[] barr = new byte[farr.Length * 4];
            for (int i = 0, j = 0; i < barr.Length; i += 4, j++)
            {
                var bts = BitConverter.GetBytes(farr[j]);
                barr[i] = bts[0];
                barr[i + 1] = bts[1];
                barr[i + 2] = bts[2];
                barr[i + 3] = bts[3];
            }

            return barr;
        }

        [Obsolete("Java byte[]转float会丢失精度, 推荐使用StringBytesToFloatArray")]
        public static float[] ByteArrayToFloatArray(byte[] barr, int startIndex = 0)
        {
            int len = (barr.Length - startIndex) / 4;
            float[] farr = new float[len];
            for (int i = startIndex, j = 0; j < len; i += 4, j++)
            {
                farr[j] = BitConverter.ToSingle(barr, i);
            }
            return farr;
        }

        public static byte[] FloatArrayToStringBytes(float[] farr)
        {
            string strFloat = string.Join(",", farr.Select(f => f.ToString()).ToArray());
            return Encoding.ASCII.GetBytes(strFloat);
        }


        public static float[] StringBytesToFloatArray(byte[] barr)
        {
            string strFloat = Encoding.ASCII.GetString(barr);
            return strFloat.Split(',').Select(s => float.Parse(s)).ToArray();
        }
    }
}
