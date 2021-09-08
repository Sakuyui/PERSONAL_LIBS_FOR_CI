using System.Collections.Generic;

namespace CIExam.OS.Memory
{
   
    public class MemoryPage
    {
        public class MemoryPageSpaceRange
        {
            public int InnerPageOffset = 0;
            public int Length = 0;
            public int Size => Length - InnerPageOffset;
            public MemoryPageSpaceRange(int from, int length)
            {
                InnerPageOffset = from;
                Length = length;
            }

            public override bool Equals(object? obj)
            {
                if (obj is MemoryPageSpaceRange memoryPageSpaceRange)
                {
                    return memoryPageSpaceRange.Size == Size && 
                           memoryPageSpaceRange.InnerPageOffset == InnerPageOffset;
                }
                return false;
            }
        }
        
        
        public readonly int PageSize;
        public int RestPageSize;
        public List<MemoryPageSpaceRange> MemoryPageSpaceRanges; 
        public MemoryPage(int pageSize)
        {
            PageSize = pageSize;
            RestPageSize = pageSize;
            MemoryPageSpaceRanges = new List<MemoryPageSpaceRange>()
            {
                new MemoryPageSpaceRange(0, pageSize)
            };
        }
    }
}