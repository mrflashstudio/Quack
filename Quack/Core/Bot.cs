using Quack.Configuration;
using Quack.Helpers;
using Quaver.API;
using Quaver.API.Configuration.Enums;
using Quaver.API.Enums.Maps;
using Quaver.API.Replays;
using SimpleDependencyInjection;
using System;
using System.Collections.Generic;
using WindowsInput;
using WindowsInput.Native;

namespace Quack.Core
{
    public class Bot
    {
        private QuaverManager quaverManager;
        private ConfigManager configManager;
        private InputSimulator input;

        private Replay currentReplay;

        public Bot()
        {
            quaverManager = DependencyContainer.Get<QuaverManager>();
            configManager = DependencyContainer.Get<ConfigManager>();
            input = new InputSimulator();
        }

        public void Start(Replay replay, bool flipInputs)
        {
            currentReplay = replay;

            int keyCount = currentReplay.Mode == GameMode.Keys4 ? 4 : 7;

            var replayKeys = new List<int>();
            for (int i = 0; i < keyCount; i++)
                replayKeys.Add(i);

            int index = nearestFrameIndex(quaverManager.QuaverBase.GameplayScreen.GameplayAudioTiming.Time);
            while (quaverManager.QuaverBase.GameplayScreen.IsLoaded && index < currentReplay.Frames.Count)
            {
                double currentTime = quaverManager.QuaverBase.GameplayScreen.GameplayAudioTiming.Time + configManager.AudioOffset;
                if (currentTime >= currentReplay.Frames[index].Time)
                {
                    var keyState = Replay.KeyPressStateToLanes(currentReplay.Frames[index].Keys);
                    for (int i = 0; i < replayKeys.Count; i++)
                    {
                        var key = getKeyByLaneIndex(flipInputs ? keyCount - 1 - replayKeys[i] : replayKeys[i]);
                        if (keyState.Contains(replayKeys[i]))
                            input.Keyboard.KeyDown(key);
                        else
                            input.Keyboard.KeyUp(key);
                    }

                    index++;
                }

                TimingHelper.Delay(1);
            }

            releaseAllKeys();

            //old auto algo
            /*int index = 0;
            var queuedKeyUps = new List<(int, int)>();
            while (quaverManager.QuaverBase.GameplayScreen.IsLoaded && (index < currentMap.HitObjects.Count || queuedKeyUps.Count != 0))
            {
                var queuedForDeletion = new List<(int, int)>();
                foreach (var element in queuedKeyUps)
                {
                    if (quaverManager.QuaverBase.GameplayScreen.GameplayAudioTiming.Time >= element.Item2)
                    {
                        inputSimulator.Keyboard.KeyUp(getKeyByLaneIndex(element.Item1 - 1));
                        queuedForDeletion.Add(element);
                    }
                }
                queuedKeyUps = queuedKeyUps.Except(queuedForDeletion).ToList();

                if (index < currentMap.HitObjects.Count)
                {
                    if (quaverManager.QuaverBase.GameplayScreen.GameplayAudioTiming.Time >= currentMap.HitObjects[index].StartTime)
                    {
                        inputSimulator.Keyboard.KeyDown(getKeyByLaneIndex(currentMap.HitObjects[index].Lane - 1));
                        queuedKeyUps.Add((currentMap.HitObjects[index].Lane, currentMap.HitObjects[index].IsLongNote ? currentMap.HitObjects[index].EndTime : currentMap.HitObjects[index].StartTime + 50));
                        index++;
                    }
                }

                Thread.Sleep(1);
            }

            releaseAllKeys();*/
        }

        private void releaseAllKeys()
        {
            for (int i = 0; i < (currentReplay.Mode == GameMode.Keys4 ? 4 : 7); i++)
                input.Keyboard.KeyUp(getKeyByLaneIndex(i));
        }

        private VirtualKeyCode getKeyByLaneIndex(int laneIndex)
        {
            string key = $"KeyMania{(currentReplay.Mode == GameMode.Keys4 ? 4 : 7)}K{laneIndex + 1}";

            if (Enum.TryParse(typeof(QuaverKey), key, out var result))
                return quaverManager.ConfigManager.GetBindedKey((QuaverKey)result);

            return VirtualKeyCode.SPACE;
        }

        private int nearestFrameIndex(double offset)
        {
            for (int i = currentReplay.Frames.Count - 1; i >= 0; i--)
                if (currentReplay.Frames[i].Time <= offset)
                    return i;

            return 0;
        }
    }
}
