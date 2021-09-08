using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.OS
{
    public class DiskScheduler
    {
        public static (List<int> order, int step) FcfsSchedule(int[] queue, int @from)
        {
            var s = 0;
            var cur = @from;
            var order = new List<int>();
            foreach (var first in queue)
            {
                s += System.Math.Abs(cur - first);
                cur = first;
                order.Add(first);
            }
            return (order, cur);
        }

        public static (List<int> order, int step) SstfSchedule(int[] queue, int @from)
        {
            //优先级动态变化可以用优先队列。每次只取top
            var q = new PriorityQueue<int, int>();
            var cur = @from;
            foreach(var req in queue)
                q.EnQueue(System.Math.Abs(req - cur),req);
            var s = 0;
            var order = new List<int>();
            while (q.Any())
            {
                var peek = q.DeQueue();
                order.Add(peek.item);
                var tmp = order.ToList();
                q.Clear();
                cur = peek.item;
                s += peek.priority;
                var cur1 = cur;
                FunctionExt.ElementInvoke(tmp, e => q.EnQueue(System.Math.Abs(cur1 - e), e));
            }

            return (order,s) ;
            
        }


        public static (List<int> order, int step) Scan(int[] queue, int @from)
        {
            var s = 0;
            var cur = @from;
            var order = new List<int>();
            var orderReqs = queue.OrderBy(e => e).ToList();
            var next = orderReqs.BinarySearchLeftRightEdge(cur).l;
            next = next >= 0 ? next : 0;
           
            order.AddRange(orderReqs.Skip(next).Take(orderReqs.Count - next));
            s += order.Sum();
            if (next <= 0) return (order, s);
            var part2 = orderReqs.Take(next).Reverse();
            order.AddRange(part2);
            s += part2.Sum();

            return (order, s);
        }
        
        public static (List<int> order, int step) CScan(int[] queue, int @from)
        {
            var s = 0;
            var cur = @from;
            var order = new List<int>();
            var orderReqs = queue.OrderBy(e => e).ToList();
            var next = orderReqs.BinarySearchLeftRightEdge(cur).l;
            next = next >= 0 ? next : 0;
           
            order.AddRange(orderReqs.Skip(next).Take(orderReqs.Count - next));
            s += order.Sum();
            if (next <= 0) return (order, s);
            var part2 = orderReqs.Take(next);
            order.AddRange(part2);
            s += part2.Sum();

            return (order, s);
        }
    }
}