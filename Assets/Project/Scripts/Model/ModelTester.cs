
using Project.Scripts.Controller;
using UnityEngine;

namespace Project.Scrpts.Model
{
    public class ModelTester : MonoBehaviour
    {
        void Start()
        {
            TestStageData();
            TestGimmickData();
            TestWallData();
            TestDataSerialization();
        }

        void TestStageData()
        {
            Debug.Log("=== StageData �׽�Ʈ ���� ===");

            Project.Scripts.Model.StageData stageData = ScriptableObject.CreateInstance<Project.Scripts.Model.StageData>();
            stageData.stageIndex = 1;

            // ���� ��� �߰� �׽�Ʈ
            Project.Scripts.Model.BoardBlockData boardBlock = new Project.Scripts.Model.BoardBlockData(0, 0);
            boardBlock.AddColorType(ColorType.Red, 1);
            stageData.boardBlocks.Add(boardBlock);

            // �÷��� ��� �߰� �׽�Ʈ
            Project.Scripts.Model.PlayingBlockData playingBlock = new Project.Scripts.Model.PlayingBlockData(0, 0, new Vector2Int(1, 1), 0, ColorType.Blue);
            playingBlock.AddShape(new Project.Scripts.Model.ShapeData(0, 0));
            playingBlock.AddGimmick(new Project.Scripts.Model.GimmickIceData(2));
            stageData.playingBlocks.Add(playingBlock);

            Debug.Log($"�������� ���� ����: �ε���={stageData.stageIndex}, ������={stageData.boardBlocks.Count}, �÷��̺��={stageData.playingBlocks.Count}");

            // ���� �׽�Ʈ
            Project.Scripts.Model.StageData cloned = stageData.Clone();
            Debug.Log($"�������� ���� ����: �ε���={cloned.stageIndex}, ������={cloned.boardBlocks.Count}, �÷��̺��={cloned.playingBlocks.Count}");

            Debug.Log("=== StageData �׽�Ʈ �Ϸ� ===");
        }

        void TestGimmickData()
        {
            Debug.Log("=== GimmickData �׽�Ʈ ���� ===");

            // ���� ��� �׽�Ʈ
            Project.Scripts.Model.GimmickIceData iceData = new Project.Scripts.Model.GimmickIceData(2);
            Debug.Log($"���� ��� ����: ī��Ʈ={iceData.Count}, Ÿ��={iceData.GetGimmickEnum()}");

            bool result = iceData.DecreaseCount();
            Debug.Log($"���� ��� ����: ī��Ʈ={iceData.Count}, ���={result}");

            result = iceData.DecreaseCount();
            Debug.Log($"���� ��� ����: ī��Ʈ={iceData.Count}, ���={result}");

            // �ٸ� ��� Ÿ�� �׽�Ʈ
            Project.Scripts.Model.GimmickMultipleData multipleData = new Project.Scripts.Model.GimmickMultipleData(ColorType.Red);
            Debug.Log($"���� ��� ����: ����={multipleData.ColorType}, Ÿ��={multipleData.GetGimmickEnum()}");

            Project.Scripts.Model.GimmickKeyData keyData = new Project.Scripts.Model.GimmickKeyData(1);
            Debug.Log($"���� ��� ����: ID={keyData.KeyId}, Ÿ��={keyData.GetGimmickEnum()}");

            Debug.Log("=== GimmickData �׽�Ʈ �Ϸ� ===");
        }

        void TestWallData()
        {
            Debug.Log("=== WallData �׽�Ʈ ���� ===");

            Project.Scripts.Model.WallData wall = new Project.Scripts.Model.WallData(
                1, 2,
                ObjectPropertiesEnum.WallDirection.Single_Right,
                3, ColorType.Red
            );

            Vector3 position = wall.GetWallPosition();
            Quaternion rotation = wall.GetWallRotation();

            Debug.Log($"�� ������ ����: ��ġ=({wall.X}, {wall.Y}), ����={wall.Length}, ����={wall.ColorType}");
            Debug.Log($"��ȯ�� ���� ��ġ: {position}, ȸ��: {rotation}");

            Debug.Log("=== WallData �׽�Ʈ �Ϸ� ===");
        }

        void TestDataSerialization()
        {
            Debug.Log("=== ������ ����ȭ �׽�Ʈ ���� ===");

            // �׽�Ʈ ������ ����
            Project.Scripts.Model.StageData stageData = ScriptableObject.CreateInstance<Project.Scripts.Model.StageData>();
            stageData.stageIndex = 99;

            Project.Scripts.Model.BoardBlockData boardBlock = new Project.Scripts.Model.BoardBlockData(1, 1);
            boardBlock.AddColorType(ColorType.Green, 2);
            stageData.boardBlocks.Add(boardBlock);

            Project.Scripts.Model.PlayingBlockData playingBlock = new Project.Scripts.Model.PlayingBlockData(2, 2, new Vector2Int(2, 2), 1, ColorType.Purple);
            playingBlock.AddShape(new Project.Scripts.Model.ShapeData(0, 0));
            playingBlock.AddShape(new Project.Scripts.Model.ShapeData(1, 0));
            playingBlock.AddGimmick(new Project.Scripts.Model.GimmickStarBlockData(1, 200));
            stageData.playingBlocks.Add(playingBlock);

            Project.Scripts.Model.WallData wall = new Project.Scripts.Model.WallData(0, 0, ObjectPropertiesEnum.WallDirection.Single_Up, 2, ColorType.Blue);
            stageData.walls.Add(wall);

            // JSON ����ȭ
            string json = stageData.ToJson();
            Debug.Log($"JSON ����ȭ ���: {json}");

            // JSON ������ȭ
            Project.Scripts.Model.StageData deserialized = Project.Scripts.Model.StageData.FromJson(json);
            Debug.Log($"������ȭ ���: �ε���={deserialized.stageIndex}, ������={deserialized.boardBlocks.Count}, �÷��̺��={deserialized.playingBlocks.Count}, ��={deserialized.walls.Count}");

            Debug.Log("=== ������ ����ȭ �׽�Ʈ �Ϸ� ===");
        }
    }
}
