using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C5;
using CIExam.FileSystem;
using CIExam.Math;

namespace CIExam.Test
{



    public static class FileSysTest
    {
       
        
        
        //cast转换数据类型
        //select可以整体操作
        //where可以过滤
        public static void TestReadFile()
        {
            IEnumerable<string> lines = FileSysHelper.ReadFileAsLines("D:\\a.txt");
            foreach (var line in lines)
            {
                var pixels = line.Split(' ').Select(p => p.Trim()).
                    Where(p =>!p.Equals("")).Select(int.Parse).ToList();

                var pixelList = (
                    from l in line.Split(' ')
                    where !"".Equals(l.Trim())
                    select int.Parse(l.Trim())).ToList();
                Console.WriteLine(pixels.Count);
            }

            var str = FileSysHelper.ReadFileAsString("D:\\a.txt");
            var ps = str.Replace('\n',' ').Split(' ')
                .Select(p => p.Trim()).Where(p => !"".Equals(p)).ToList();
            
            Console.WriteLine(ps.Count + " " + Utils.ListToString(ps));
            
            var psFinal = ps.Select((item, index) => new {index, item})
                .GroupBy(x => x.index % 5)
                .Select(x => x.Select(y => y.item)).ToList();
            
            



        }
        
        
    }
}