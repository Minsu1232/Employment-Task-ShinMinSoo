using System.Collections.Generic;

namespace Project.Scripts.Model
{
    [System.Serializable]
    public class StageJsonData
    {
        public int stageIndex;
        public List<BoardBlockData> boardBlocks;
        public List<PlayingBlockData> playingBlocks;
        public List<WallData> Walls;
    }

    [System.Serializable]
    public class StageJsonWrapper
    {
        public StageJsonData Stage;
    }
}