namespace Quaver.API.Maps
{
    public class HitObject
    {
        public int StartTime { get; set; }
        public int Lane { get; set; }

        private int? endTime = null;
        public int EndTime
        {
            get
            {
                if (endTime.HasValue)
                    return endTime.Value;

                return StartTime;
            }
            set => endTime = value;
        }

        public bool IsLongNote => endTime.HasValue;
    }
}
