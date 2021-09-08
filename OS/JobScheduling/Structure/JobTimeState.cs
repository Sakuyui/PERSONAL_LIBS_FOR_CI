using System;
using System.Collections.Generic;

namespace CIExam.OS.JobScheduling
{

    public class JobTimeState : IComparable
    {
        public int JoinTime;
        public int RestTime;
        
        public JobTimeState(int joinTime, int restTime)
        {
            JoinTime = joinTime;
            RestTime = restTime;
        }

        public int CompareTo(object? obj)
        {
            if (obj is not JobTimeState state) return -1;
            return state.RestTime == RestTime ? JoinTime.CompareTo(state.JoinTime) : RestTime.CompareTo(state.RestTime);
        }

        public override int GetHashCode()
        {
            return JoinTime.GetHashCode() + RestTime.GetHashCode();
        }

        public override string ToString()
        {
            return (JoinTime, RestTime).ToString();
        }
    }
    //非抢占式版本

    //抢占式
}