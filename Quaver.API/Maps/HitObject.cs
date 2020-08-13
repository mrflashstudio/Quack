namespace Quaver.API.Maps
{
    public class HitObject
    {
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int Lane { get; set; }

        public bool IsLongNote => EndTime > 0;
    }
}
