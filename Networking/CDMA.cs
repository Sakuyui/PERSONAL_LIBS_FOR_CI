using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using CIExam.FunctionExtension;

namespace CIExam.Network
{
    public class Cdma
    {
        public static void Test()
        {
            var random = new Random();
            
            // 利用walsh码生成正交码向量
            var codeSnippet = Cdma.GenerateWalshCode(4).ToList();
            //生成用户词典
            var userNames = new[] {"userA", "userB", "userC", "userD"};
            var users = codeSnippet
                .Zip(userNames, (vec, name) => (name, vec))
                .ToDictionary(e => e.name, e=> e.vec);
            ("用户向量：" + users.ToEnumerationString()).PrintToConsole();
            //产生要传输的信息
            var msg = userNames.Select(e => (uname: e, b: (byte) random.Next(0, 2)))
                .ToDictionary(e => e.uname, e => e.b);
            ("发送消息：" + msg.ToEnumerationString()).PrintToConsole();
            //实际传输的信号向量
            var signal = Cdma.GenerateSignal(users, msg);
            ("传输信号：" + signal.ToEnumerationString()).PrintToConsole();
            //解码
            var decoded = Cdma.CdmaDeCode(users, signal);
            decoded.PrintCollectionToConsole();
        }
        
        public static Math.Vector<int> GenerateSignal(Dictionary<string, Math.Vector<int>> codeSnippet, Dictionary<string, byte> msg)
        {
            var s = msg.Aggregate(new[] {0, 0, 0, 0}.ToVector(), (sum, pair) =>
            {
                var (key, value) = pair;
                var c = codeSnippet[key];
                return value == 1 ? (Math.Vector<int>)(sum + c) : (Math.Vector<int>)(sum - c);
            });
            
            return s;
        }

        public static Dictionary<string, byte> CdmaDeCode(Dictionary<string, Math.Vector<int>> codeSnippet,
            Math.Vector<int> combinedSignal)
        {
            var t = codeSnippet.Select(c => 
                    (c.Key, ((Math.Vector<int>) ((dynamic)c.Value * (dynamic)combinedSignal)).Sum() > 0 ? (byte) 1 : (byte) 0))
                .ToDictionary(e => e.Key, e => e.Item2);
            return t;
        }

        public static IEnumerable<Math.Vector<int>> GenerateWalshCode(int count)
        {
            var n = (int)System.Math.Ceiling(System.Math.Log2(count));
            var cur = new List<Math.Vector<int>> {new[] {1}.ToVector()};
            
            for (var i = 1; i <= n; i++)
            {
                cur = cur.SelectMany(v => new[]
                {
                    v.Concat(v).ToVector(), 
                    v.Concat(-v).ToVector()
                }).ToList();
            }
            
            return cur.Take(count);
        }
    }
}