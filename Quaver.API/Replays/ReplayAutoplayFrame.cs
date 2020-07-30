using Quaver.API.Maps;

namespace Quaver.API.Replays
{
    public struct ReplayAutoplayFrame
    {
        public ReplayAutoplayFrameType Type { get; }

        public int Time { get; }

        public ReplayKeyPressState Keys { get; }

        public HitObject HitObject { get; }

        public ReplayAutoplayFrame(HitObject hitObject, ReplayAutoplayFrameType type, int time, ReplayKeyPressState keys)
        {
            HitObject = hitObject;
            Type = type;
            Time = time;
            Keys = keys;
        }
    }

    public enum ReplayAutoplayFrameType
    {
        Press,
        Release
    }
}
