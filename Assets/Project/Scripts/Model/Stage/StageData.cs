using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Model
{
    [System.Serializable]
    public class CheckGroupInfo
    {
        public int groupId;
        public List<Vector2Int> blockPositions = new List<Vector2Int>();
    }
    [CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
    public class StageData : ScriptableObject, IGameData
    {
        public int stageIndex;
        public List<BoardBlockData> boardBlocks = new List<BoardBlockData>();
        public List<PlayingBlockData> playingBlocks = new List<PlayingBlockData>();
        public List<WallData> walls = new List<WallData>();
        // üũ �׷� ���� �߰�
        public List<CheckGroupInfo> checkGroups = new List<CheckGroupInfo>();
        /// <summary>
        /// �������� ������ ����
        /// </summary>
        public StageData Clone()
        {
            StageData clone = CreateInstance<StageData>();
            clone.stageIndex = stageIndex;

            // ���� ���� ����
            clone.boardBlocks = new List<BoardBlockData>(boardBlocks);
            clone.playingBlocks = new List<PlayingBlockData>(playingBlocks);
            clone.walls = new List<WallData>(walls);

            return clone;
        }

        /// <summary>
        /// JSON ����ȭ
        /// </summary>
        public string ToJson()
        {
            StageJsonWrapper wrapper = new StageJsonWrapper
            {
                Stage = new StageJsonData
                {
                    stageIndex = this.stageIndex,
                    boardBlocks = this.boardBlocks,
                    playingBlocks = this.playingBlocks,
                    walls = this.walls
                }
            };

            return JsonUtility.ToJson(wrapper, true);
        }

        /// <summary>
        /// JSON���� �������� ������ �ε�
        /// </summary>
        public static StageData FromJson(string json)
        {
            StageJsonWrapper wrapper = JsonUtility.FromJson<StageJsonWrapper>(json);

            StageData stageData = CreateInstance<StageData>();
            stageData.stageIndex = wrapper.Stage.stageIndex;
            stageData.boardBlocks = wrapper.Stage.boardBlocks;
            stageData.playingBlocks = wrapper.Stage.playingBlocks;
            stageData.walls = wrapper.Stage.walls;

            return stageData;
        }
    }
}