using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.OS.JobScheduling
{
    public class FcfsOsJobScheduler : BaseOsJobScheduler
    {
        private readonly PriorityQueue<int, OsJob> _priorityQueue = new();
        private int _curClock = 0;
        public override void Refresh()
        {
            _priorityQueue.Clear();
            _curClock = 0;
        }

        public override void JoinJob(OsJob osJob, int joinTime, JobSchedulerCallBack jobJoinCallback = null)
        {
            if(_priorityQueue.ContainsKey(joinTime))
                throw new ArithmeticException();
            _priorityQueue.EnQueue(joinTime, osJob);
        }

        public override void RemoveJob(OsJob osJob)
        {
            _priorityQueue.RemoveByItem(osJob);
        }

        public override void BeginSimulate(int endTime
             , JobSchedulerCallBack jobExecuteCallBack = null,
             JobSchedulerCallBack jobFinishCallBack = null
             , JobSchedulerCallBack jobSwitchCallBack = null)
        {
            ">>>>>>>>> Fifo Scheduler Begin Simulation...".PrintToConsole();
            while (_priorityQueue.Any())
            {
                $"+ time = {_curClock}".PrintToConsole();
                var j = _priorityQueue.DeQueue();
                $"find job {j}".PrintToConsole();
                jobExecuteCallBack?.Invoke(j,_curClock);
                ClockCallBack(_curClock, j.item, j.priority);
                jobFinishCallBack?.Invoke(j, _curClock, j.priority);
                "".PrintToConsole();
            }
            $">>>>>>>>> FCFS END... Total Time = {_curClock}".PrintToConsole(); 
        }

        public override object ClockCallBack(int curClock, params object[] objects)
        {
            var e = (OsJob) objects[0];
            var joinTime = (int) objects[1];
            _curClock += e.RTime;
            var diff = _curClock - joinTime;
            var wait = curClock - joinTime;
            $"in clock call back, job {e} finish, now clock = {_curClock}".PrintToConsole();
            $"> Turn Around Time : {diff}".PrintToConsole();
            $"> Wait Time: {wait}".PrintToConsole();
            return e.DoWork();
        }
    }
}