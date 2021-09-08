using System.Collections.Generic;
using System.Linq;
using CIExam.FunctionExtension;

namespace CIExam.OS.Memory
{
    public class MemoryStrut
    {
        public IEnumerable<MemoryPage> PageEnumerator => _page.Select(e => e);
        private List<MemoryPage> _page;
        public int MemSize;
        public MemoryStrut(int pageSize, int memSize)
        {
            var pageCount = memSize / pageSize;
            memSize = pageSize * pageCount;
            Enumerable.Range(0, 7).ElementInvoke(_ => _page.Add(new MemoryPage(pageSize)));
        }
    }
}