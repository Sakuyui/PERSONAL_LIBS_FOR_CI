using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;
using CIExam.FunctionExtension;

namespace CIExam.OS.JobScheduling
{
    public class SrtnOsJobScheduler : BaseOsJobScheduler
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
        private OsJob _executingJob;
        private JobTimeState _executingJobState;

        public void ClockElapse(int elapse)
        {
           
            if (elapse == 0)
                return;
            _curClock += elapse;
            $"Clock elapse + {elapse} to {_curClock}".PrintToConsole();
           
            //update scheduler
            var t = _priorityQueue.KvEnumerator.ToList();

            if (_executingJobState == null) return;
            
            _executingJobState.RestTime -= elapse;
            _executingJobState.PrintToConsole();
            
        }
        
        public override void BeginSimulate(int endTime, JobSchedulerCallBack jobExecuteCallBack = null,
            JobSchedulerCallBack jobFinishCallBack = null, JobSchedulerCallBack jobSwitchCallBack = null)
        {
            var t = 0;
            ">>>>>>>>> Srtn(抢占最短作业) Scheduler Begin Simulation...".PrintToConsole();
            while (_joinOrderQueue.Any() || _executingJob != null)
            {
                //当前没有执行的
                if (_executingJob == null && !_priorityQueue.Any())
                {
                    var first = _joinOrderQueue.DeQueue();
                    $"> In time {_curClock}, Get job {first.item}".PrintToConsole();
                    //加入调度
                    var state = new JobTimeState(first.priority, first.item.RTime);
                    
                    $"Clock to {first.priority}".PrintToConsole();
                    var diff = first.priority - _curClock;
                    ClockElapse(diff);
                    _executingJob = first.item;
                    _executingJobState = state;
                }else if (_executingJob == null && _priorityQueue.Any())
                {
                    var first = _priorityQueue.DeQueue();
                    $"In Clock {_curClock}, Get job {first.item}".PrintToConsole();
                    _executingJob = first.item;
                    _executingJobState = first.priority;
                }
                else
                {
                    //寻找下一个事件触发时间点，并处理事件
                    //1. 下一个最先的任务抵达
                    //2. 当前任务结束
                    //3. 被等待队列中的任务抢占
                    
                    var curJobRestTime = _executingJobState.RestTime;
                    var nextJobEnter = _joinOrderQueue.Any() ? _joinOrderQueue.Peek().Key : int.MaxValue;
                    var curJobExpectEnd = _curClock + curJobRestTime;
                    var nextJobInQueueRest = _priorityQueue.Any()?_priorityQueue.Peek().Key.RestTime:int.MaxValue;
                    var diff = _priorityQueue.Any() ? _executingJobState.RestTime - nextJobInQueueRest : int.MaxValue;
                    var swapExpect = diff > 0 && _priorityQueue.Any() ? _curClock + diff: int.MaxValue;
                    
                    $"==> Now Scheduling Queue: {_priorityQueue.KvEnumerator.ToEnumerationString()}".PrintToConsole();
                    $"Judge next Event time = {nextJobEnter},{curJobExpectEnd},{swapExpect}".PrintToConsole();
                    
                    var elapse = System.Math.Min(nextJobEnter, System.Math.Min(curJobExpectEnd, swapExpect)) - _curClock;
                    ClockElapse(elapse);
                    
                    //Process Event
                    if (_curClock == nextJobEnter)
                    {
                        $"In {_curClock}, Job join in {_joinOrderQueue.Peek()}".PrintToConsole();
                        var nextJob = _joinOrderQueue.DeQueue();
                      
                        //可能抢占
                        if (nextJob.item.RTime < _executingJobState.RestTime)
                        {
                            $"New Job {nextJob} Grab Change".PrintToConsole();
                            _priorityQueue.EnQueue(_executingJobState, _executingJob);
                            _executingJob = nextJob.item;
                            _executingJobState = new JobTimeState(nextJob.priority, nextJob.item.RTime);
                        }
                        else
                        {
                            _priorityQueue.EnQueue(new JobTimeState(nextJob.priority, nextJob.item.RTime), nextJob.item);
                            _priorityQueue.PrintMultiDimensionCollectionToConsole();
                        }
                    }
                    if (_curClock == curJobExpectEnd)
                    {
                        $"In {_curClock}, Job {_executingJob} end".PrintToConsole();
                        _executingJob = null;
                        _executingJobState = null;
                        _priorityQueue.PrintMultiDimensionCollectionToConsole();
                        if (_priorityQueue.Any())
                        {
                            var next = _priorityQueue.DeQueue();
                            _executingJob = next.item;
                            _executingJobState = next.priority;
                            $"Job Change to {next}".PrintToConsole();
                        }
                    }
                    if (_curClock == swapExpect)
                    {
                        var swapJob = _priorityQueue.DeQueue();
                        if (_executingJob != null)
                        {
                            $"{_executingJobState.RestTime},{swapJob.priority.RestTime}".PrintToConsole();
                            _priorityQueue.EnQueue(_executingJobState, _executingJob);
                            _executingJob = null;
                            _executingJobState = null;
                        }
                        $"In {_curClock}, swap to {swapJob}".PrintToConsole();
                        _executingJob = swapJob.item;
                        _executingJobState = swapJob.priority;
                    }

                    t++;
                    if(t > 7)
                        break;
                    "".PrintToConsole();
                }
            }
          
            $">>>>>>>>> SRTN END... Total Time = {_curClock}".PrintToConsole(); 
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