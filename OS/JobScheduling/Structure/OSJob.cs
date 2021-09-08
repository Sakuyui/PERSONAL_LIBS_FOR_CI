using System;

namespace CIExam.OS.JobScheduling
{
    public class OsJob
    {
        public delegate object OsJobDetails(params object[] objs);

        public OsJobDetails _osJobDetails;
        public int RTime;
        public string JobID = "default job";
        public OsJob(int requireTime, string jobId = "defaultJob", OsJobDetails osJobDetails = null)
        {
            RTime = requireTime;
            _osJobDetails = osJobDetails;
            JobID = jobId;
        }

        public override string ToString()
        {
            return $"(JobID:{JobID}, ReqTime:{RTime})";
        }


        public object DoWork(params object[] objects)
        {
            return _osJobDetails?.Invoke(objects) ?? null;
        }
    }
}