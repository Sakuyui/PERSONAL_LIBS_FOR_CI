namespace CIExam.OS.JobScheduling
{
    public abstract class BaseOsJobScheduler
    {
        // public int eatenApples(int[] apples, int[] days)
        // {
            // 优先队列，队首是最早过期的  int[0]:苹果个数  int[1]:过期时间
            //截至时间调度 吃掉最多的苹果
        //     PriorityQueue<int[]> queue = new PriorityQueue<>((o1, o2)->o1[1] < o2[1] ? -1 : 1);
        //     int eatNum = 0;
        //     for (int i = 0; i < apples.length || queue.size() > 0; i++)
        //     {
        //         //1.移除过期的
        //         while (!queue.isEmpty())
        //         {
        //             int[] apple = queue.peek();
        //             if (apple[1] <= i)
        //             {
        //                 queue.poll();
        //             }
        //             else
        //             {
        //                 break;
        //             }
        //         }
        //
        //         //2.添加当天新长出来的
        //         if (i < apples.length && apples[i] > 0)
        //         {
        //             queue.add(new int[] {apples[i], days[i] + i});
        //         }
        //
        //         //3.吃掉已有的（优先吃最早过期的）
        //         int[] ap = queue.peek();
        //         if (ap != null && ap[0] > 0)
        //         {
        //             eatNum++;
        //             ap[0] -= 1;
        //             if (ap[0] == 0)
        //             {
        //                 queue.poll();
        //             }
        //         }
        //     }
        //
        //     return eatNum;
        // }


        public delegate object JobSchedulerCallBack(params object[] objects);
        public abstract void Refresh();
        public abstract void JoinJob(OsJob osJob, int joinTime, JobSchedulerCallBack jobJoinCallback = null);
        public abstract void RemoveJob(OsJob osJob);
        public abstract void BeginSimulate(int endTime, 
          JobSchedulerCallBack jobExecuteCallBack = null, JobSchedulerCallBack jobFinishCallBack = null
          , JobSchedulerCallBack jobSwitchCallBack = null);
        public abstract object ClockCallBack(int curClock, params object[] objects);
    }
}