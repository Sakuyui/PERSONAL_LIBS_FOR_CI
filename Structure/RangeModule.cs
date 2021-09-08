using System.Collections.Generic;
using System.Linq;

namespace CIExam.Structure
{
    public class RangeModule
    {
        // private SortedDictionary<int, int> Map;
        // RangeModule() 
        // {
        // }
        //
        // void AddRange(int left, int right) 
        // {
        //     map<int, int>::iterator it = right_left.lower_bound(left);
        //     while (it != right_left.end() && it->second <= right)
        //     {
        //         left = min(left, it->second);
        //         right = max(right, it->first);
        //         it = right_left.erase(it);
        //     }
        //     right_left[right] = left;
        //
        // }
        //
        // bool queryRange(int left, int right) 
        // {
        //     map<int, int>::iterator it = right_left.lower_bound(right);
        //     return it != right_left.end() && it->second <= left;
        // }
        //
        // void removeRange(int left, int right) 
        // {
        //     auto it = right_left.upper_bound(left);
        //     while (it != right_left.end() && it->second < right)
        //     {
        //         int old_l = it->second;
        //         int old_r = it->first;
        //         it = right_left.erase(it);
        //
        //         if (old_l < left)
        //             right_left[left] = old_l;
        //         if (right < old_r)
        //             right_left[old_r] = right;
        //     }
        //
        // }

    }
}