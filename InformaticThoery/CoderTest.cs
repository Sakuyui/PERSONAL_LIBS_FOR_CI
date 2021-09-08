using System;
using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.InformationThoery
{
    public class CoderTest
    {
       

        public static void Test()
        {
            //数据分布
            var nums = new[] {4, 4, 2, 45, 5, 1, 44, 28};
            
            //频率分布字典
            var frequencyDictionary = ArithmeticCoding.GetFreqDictionary(nums);
            //输入数据
            var inputData = new int[] {1, 4, 4, 2, 5, 28}.ToList();
            var code = ArithmeticCoding.Encode(frequencyDictionary,inputData);
            
            (inputData.ToEnumerationString() +  " 编码 = " + code).PrintToConsole();
            var plain = ArithmeticCoding.Decode(frequencyDictionary,  0.62755547048825, 6);
            ("0.62755547048825 解码后 = " + plain.ToEnumerationString()).PrintToConsole();
            
        }
       
        public static void HuffmanEnCoderTest()
        {
            var nums = new[] {4, 4, 2, 45, 5, 1, 44, 28};
            var dict = HuffmanEnCoder.Encode(nums);
            var data = HuffmanEnCoder.Decode("011101011011100000110011", dict);
            data.PrintCollectionToConsole();
            
        }
       
    }
}