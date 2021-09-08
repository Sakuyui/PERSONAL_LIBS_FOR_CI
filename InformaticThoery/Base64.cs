using System;
using System.Text;

namespace CIExam.InformaticThoery
{
    public class Base64
    {
        public static void Test()
        {
            var bytes = Encoding.Default.GetBytes( " 要转换的字符串 " );
            Convert.ToBase64String(bytes);
            //解码：
            // "ztKwrsTj"是“我爱你”的base64编码
            var outputb  =  Convert.FromBase64String( " ztKwrsTj " );
            var  orgStr =  Encoding.Default.GetString(outputb);
        }
    }
}