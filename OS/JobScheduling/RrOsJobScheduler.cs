using System.Collections.Generic;
using System.Linq;
using CIExam.Structure;

namespace CIExam.OS.JobScheduling
{
    public class RrOsJobScheduler : BaseOsJobScheduler
    {
        private int _clock = 0;
        private readonly PriorityQueue<int, OsJob> _jobs = new();
        public override void Refresh()
        {
            _clock = 0;
            _jobs.Clear();
            _schedulingTasks.Clear();
        }

        public override void JoinJob(OsJob osJob, int joinTime, JobSchedulerCallBack jobJoinCallback = null)
        {
            _jobs.EnQueue(joinTime,  osJob);
        }

        public override void RemoveJob(OsJob osJob)
        {
            _jobs.RemoveByItem(osJob);
        }
        PriorityDynamicQueue<OsJob, int> _schedulingTasks = new();
        public override void BeginSimulate(int endTime, JobSchedulerCallBack jobExecuteCallBack = null,
            JobSchedulerCallBack jobFinishCallBack = null, JobSchedulerCallBack jobSwitchCallBack = null)
        {
           
            //离散化+事件驱动
            
            while(_schedulingTasks.Any() || _jobs.Any())
            {
               
                //处理 curClock -> 下一个任务进入期间内的RR
                var schedulingTaskCount = _schedulingTasks.Count();
                var tNextIn = _jobs.Peek().Key;
                var t1 = _schedulingTasks.GetMin().priority * schedulingTaskCount; //最早可能结束的任务需要时间
                var diff = System.Math.Max(tNextIn - _clock, t1);
                var s = diff / schedulingTaskCount;
                var r = diff % schedulingTaskCount;
                if (t1 >= tNextIn - _clock)
                {
                    //下一个任务到达前足够完成最早结束的任务
                    var l = _schedulingTasks.NodeEnumerator.ToArray();
                    for (var i = 0; i < l.Length; i++)
                    {
                        var node = l[i];
                        _schedulingTasks.Update(node.Key, node.Priority - s - (i < r ? 1 : 0));
                    }

                    _clock += diff;
                    while (_schedulingTasks.GetMin().priority <= 0)
                    {
                        _schedulingTasks.RemoveMin();
                    }

                    while (_jobs.Peek().Key == 0)
                    {
                        var f = _jobs.DeQueue();
                        _schedulingTasks.AddOrUpdate(f.item, f.priority);
                    }
                }
                else
                {
                    //还没有任何任务完成，就有新任务到达
                    
                    var l = _schedulingTasks.NodeEnumerator.ToArray();
                    for (var i = 0; i < l.Length; i++)
                    {
                        var node = l[i];
                        _schedulingTasks.Update(node.Key, node.Priority - s - (i < r ? 1 : 0));
                    }
                    _clock += diff;
                    while (_schedulingTasks.GetMin().priority <= 0)
                    {
                        _schedulingTasks.RemoveMin();
                    }

                    while (_jobs.Peek().Key == 0)
                    {
                        var f = _jobs.DeQueue();
                        _schedulingTasks.AddOrUpdate(f.item, f.priority);
                    }
                }
                
               
            }
            
            
            //暴力递增1方式
            // ClockCallBack(_clock);
            // if(!_schedulingTasks.Any() && ! _jobs.Any())
            //     return;
            // _clock++;
        }

        public override object ClockCallBack(int curClock, params object[] objects)
        {
            var e = _schedulingTasks.NodeEnumerator.ToList();
            foreach (var node in e)
            {
                _schedulingTasks.Update(node.Key, node.Priority - 1);
            }

            while (_schedulingTasks.Any() && _schedulingTasks.GetMin().priority == 0)
                _schedulingTasks.RemoveMin();
            
            //if join in
            if (!_jobs.Any() || _jobs.Peek().Key != curClock) 
                return null;
            
            var job = _jobs.DeQueue();
            _schedulingTasks.AddOrUpdate(job.item, job.item.RTime);

            return null;
        }
    }
}