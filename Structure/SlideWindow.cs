using System;
using System.Collections.Generic;
using System.Linq;

namespace CIExam.Structure
{
    public delegate void SlideWindowEvent(object source, object[] args);
    public class SlideWindow<T>
    {
        private int Pos = 0;
        public int WinSize {  get; private set; }
        public int Count => _elements.Count;
        private List<T> _elements;
        public SlideWindowEvent WindowMoved = null;
        public SlideWindowEvent ArriveEnd = null;
        
        public SlideWindow(int wSize, T[] elements = null)
        {
            WinSize = wSize;
            if (wSize <= 0)
            {
                throw new ArithmeticException("Windows Size can't <= 0");
            }

            _elements = (elements == null ? new List<T>() : elements.ToList());
        }
        
        public List<T> GetCurWindow()
        {
            return Pos + WinSize > Count ? null : _elements.GetRange(0, WinSize);
        }

        public bool MoveAhead(int step = 1)
        {
            if (Pos + step + WinSize > Count)
            {
                ArriveEnd?.Invoke(this, null);
                return false;
            }

            //arg1: 步长 arg2: 消失元素 arg3:新增元素
            var args = new object[] {step, 
                _elements.GetRange(Pos,step),
                _elements.GetRange(Pos + WinSize , step)
            };
            Pos += step;
                
            WindowMoved?.Invoke(this, args);
            return true;
        }

        public bool MoveBack(int step = 1)
        {
            if (Pos - step < 0)
            {
                return false;
            }
            //arg1: 步长 arg2: 消失元素 arg3:新增元素
            var args = new object[] {step, 
                _elements.GetRange(Pos - step,step),
                _elements.GetRange(Pos + WinSize - step , step)
            };
            Pos += step;
                
            WindowMoved?.Invoke(this, args);
            return true;
        }

        public void SetPos(int pos)
        {
            if (pos >= 0 && pos + WinSize <= Count)
            {
                Pos = pos;
            }
        }
        
    }
}