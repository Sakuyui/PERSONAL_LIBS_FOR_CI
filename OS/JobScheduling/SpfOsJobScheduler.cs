using System.Linq;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.OS.JobScheduling
{
    public class SpfOsJobScheduler : BaseOsJobScheduler
    {
        private readonly PriorityQueue<JobTimeState, OsJob> _priorityQueue = new();
        private readonly PriorityQueue<int, OsJob> _joinOrderQueue = new();
        private int _curClock;
        public override void Refresh()
        {
            _priorityQueue.Clear();
            _curClock = 0;
        }

        public override void JoinJob(OsJob osJob, int joinTime, JobSchedulerCallBack jobJoinCallback = null)
        {
            _joinOrderQueue.EnQueue(joinTime, osJob);
            jobJoinCallback?.Invoke(joinTime, osJob);
        }

        public override void RemoveJob(OsJob osJob)
        {
            _priorityQueue.RemoveByItem(osJob);
        }


        
        public override void BeginSimulate(int endTime, JobSchedulerCallBack jobExecuteCallBack = null,
            JobSchedulerCallBack jobFinishCallBack = null, JobSchedulerCallBack jobSwitchCallBack = null)
        {
            ">>>>>>>>> Spf(非抢占最短作业) Scheduler Begin Simulation...".PrintToConsole();
            while (_priorityQueue.Any() || _joinOrderQueue.Any())
            {
                $"+ time = {_curClock}".PrintToConsole();
                if (!_priorityQueue.Any())
                {
                    "System Idle. Get Next Join Job".PrintToConsole();
                    var firstJob = _joinOrderQueue.DeQueue();
                    $"clock to {firstJob.priority}".PrintToConsole();
                    _priorityQueue.EnQueue(new JobTimeState(firstJob.priority, firstJob.item.RTime), firstJob.item);
                }
                else
                {
                    $"Queue Jobs {_priorityQueue.Select(e => e).ToEnumerationString()}".PrintToConsole();
                }
                
                var j = _priorityQueue.DeQueue();
                $"Get job {j.item}, rest time = {j.priority.RestTime}".PrintToConsole();
                $"do job... {j.item}".PrintToConsole();
                jobExecuteCallBack?.Invoke(j,_curClock);
                
                //执行期间可能有任务进入
                var expectFinishTime = _curClock + j.item.RTime;
                while (_joinOrderQueue.Any() && _joinOrderQueue.Peek().Key < expectFinishTime)
                {
                    var peekJob = _joinOrderQueue.DeQueue();
                    $"join job {peekJob.item} in clock {peekJob.priority}".PrintToConsole();
                    _priorityQueue.EnQueue(new JobTimeState(peekJob.priority, peekJob.item.RTime), peekJob.item);
                    
                }
                $"end job... {j.item}".PrintToConsole();
                
                ClockCallBack(_curClock, j.item, j.priority);
                jobFinishCallBack?.Invoke(j, _curClock, j.priority);
                "".PrintToConsole();
            }
            $">>>>>>>>> SPF END... Total Time = {_curClock}".PrintToConsole(); 
        }

        public override object ClockCallBack(int curClock, params object[] objects)
        {
            var job = (OsJob) objects[0];
            var jobState = (JobTimeState) objects[1];
            var joinTime = jobState.JoinTime;
            _curClock = curClock + job.RTime;
            var turnAround = _curClock - joinTime;
            var wait = curClock - joinTime;
            $"in clock call back, job {job} finish, now clock = {_curClock}".PrintToConsole();
            ($"Join Time = {joinTime}, Begin Time = {curClock}, end Time = {_curClock}, Turn around Time = {turnAround}," +
             $" Wait Time = {wait}").PrintToConsole();
            $"> Clock to {_curClock}".PrintToConsole();
            return null;
        }
    }
}