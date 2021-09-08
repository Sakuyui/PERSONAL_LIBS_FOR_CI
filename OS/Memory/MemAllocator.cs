using System.Linq;
using KdTree;

namespace CIExam.OS.Memory
{
    public partial class MemAllocator
    {
        public delegate MemoryPage MemAllocationStrategy(MemoryStrut mem, int blockSize);
        public MemAllocator()
        {
            
        }

        public MemoryPage TryAllocate(MemoryStrut mem, int blockSize, MemAllocationStrategy memAllocationStrategy)
        {
            return memAllocationStrategy.Invoke(mem, blockSize);
        }
        
    }


    partial class MemAllocator
    {
        public MemAllocationStrategy FirstFitStrategy => (mem, size) =>
        {
            foreach (var p in mem.PageEnumerator)
            {
                if(p.MemoryPageSpaceRanges[0].Size < size)
                    continue;
                p.MemoryPageSpaceRanges[0].InnerPageOffset += size;
                return p;
            }
            return null;
        };
        
        public MemAllocationStrategy BestFitStrategy => (mem, size) =>
        {
            var q = new Structure.PriorityQueue<int, MemoryPage>();
            foreach (var p in mem.PageEnumerator)
            {
                if(p.MemoryPageSpaceRanges[0].Size < size)
                    continue;
                q.EnQueue(p.MemoryPageSpaceRanges[0].Size, p);
                return p;
            }

            if (!q.Any())
                return null;
            q.Peek().Value.MemoryPageSpaceRanges[0].InnerPageOffset += size;
            return q.Peek().Value;
        };
        
        public MemAllocationStrategy WorstFitStrategy => (mem, size) =>
        {
            var q = new Structure.PriorityQueue<int, MemoryPage>((p1, p2) => p2 - p1);
            foreach (var p in mem.PageEnumerator)
            {
                if(p.MemoryPageSpaceRanges[0].Size < size)
                    continue;
                q.EnQueue(p.MemoryPageSpaceRanges[0].Size, p);
                return p;
            }

            if (!q.Any())
                return null;
            q.Peek().Value.MemoryPageSpaceRanges[0].InnerPageOffset += size;
            return q.Peek().Value;
        };
    }
}