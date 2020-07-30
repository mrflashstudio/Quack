namespace Quaver.API.Memory.Objects
{
    public class QuaverRuleset : QuaverObject
    {
        public QuaverScoreProcessor ScoreProcessor { get; private set; }

        public QuaverRuleset()
        {
            Children = new QuaverObject[]
            {
                ScoreProcessor = new QuaverScoreProcessor
                {
                    Offset = 0x30
                }
            };
        }
    }
}
