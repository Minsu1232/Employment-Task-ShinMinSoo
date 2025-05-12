
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
            Debug.Log("=== StageData 테스트 시작 ===");

            Project.Scripts.Model.StageData stageData = ScriptableObject.CreateInstance<Project.Scripts.Model.StageData>();
            stageData.stageIndex = 1;

            // 보드 블록 추가 테스트
            Project.Scripts.Model.BoardBlockData boardBlock = new Project.Scripts.Model.BoardBlockData(0, 0);
            boardBlock.AddColorType(ColorType.Red, 1);
            stageData.boardBlocks.Add(boardBlock);

            // 플레이 블록 추가 테스트
            Project.Scripts.Model.PlayingBlockData playingBlock = new Project.Scripts.Model.PlayingBlockData(0, 0, new Vector2Int(1, 1), 0, ColorType.Blue);
            playingBlock.AddShape(new Project.Scripts.Model.ShapeData(0, 0));
            playingBlock.AddGimmick(new Project.Scripts.Model.GimmickIceData(2));
            stageData.playingBlocks.Add(playingBlock);

            Debug.Log($"스테이지 생성 성공: 인덱스={stageData.stageIndex}, 보드블록={stageData.boardBlocks.Count}, 플레이블록={stageData.playingBlocks.Count}");

            // 복제 테스트
            Project.Scripts.Model.StageData cloned = stageData.Clone();
            Debug.Log($"스테이지 복제 성공: 인덱스={cloned.stageIndex}, 보드블록={cloned.boardBlocks.Count}, 플레이블록={cloned.playingBlocks.Count}");

            Debug.Log("=== StageData 테스트 완료 ===");
        }

        void TestGimmickData()
        {
            Debug.Log("=== GimmickData 테스트 시작 ===");

            // 얼음 기믹 테스트
            Project.Scripts.Model.GimmickIceData iceData = new Project.Scripts.Model.GimmickIceData(2);
            Debug.Log($"얼음 기믹 생성: 카운트={iceData.Count}, 타입={iceData.GetGimmickEnum()}");

            bool result = iceData.DecreaseCount();
            Debug.Log($"얼음 기믹 감소: 카운트={iceData.Count}, 결과={result}");

            result = iceData.DecreaseCount();
            Debug.Log($"얼음 기믹 감소: 카운트={iceData.Count}, 결과={result}");

            // 다른 기믹 타입 테스트
            Project.Scripts.Model.GimmickMultipleData multipleData = new Project.Scripts.Model.GimmickMultipleData(ColorType.Red);
            Debug.Log($"다중 기믹 생성: 색상={multipleData.ColorType}, 타입={multipleData.GetGimmickEnum()}");

            Project.Scripts.Model.GimmickKeyData keyData = new Project.Scripts.Model.GimmickKeyData(1);
            Debug.Log($"열쇠 기믹 생성: ID={keyData.KeyId}, 타입={keyData.GetGimmickEnum()}");

            Debug.Log("=== GimmickData 테스트 완료 ===");
        }

        void TestWallData()
        {
            Debug.Log("=== WallData 테스트 시작 ===");

            Project.Scripts.Model.WallData wall = new Project.Scripts.Model.WallData(
                1, 2,
                ObjectPropertiesEnum.WallDirection.Single_Right,
                3, ColorType.Red
            );

            Vector3 position = wall.GetWallPosition();
            Quaternion rotation = wall.GetWallRotation();

            Debug.Log($"벽 데이터 생성: 위치=({wall.X}, {wall.Y}), 길이={wall.Length}, 색상={wall.ColorType}");
            Debug.Log($"변환된 월드 위치: {position}, 회전: {rotation}");

            Debug.Log("=== WallData 테스트 완료 ===");
        }

        void TestDataSerialization()
        {
            Debug.Log("=== 데이터 직렬화 테스트 시작 ===");

            // 테스트 데이터 생성
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

            // JSON 직렬화
            string json = stageData.ToJson();
            Debug.Log($"JSON 직렬화 결과: {json}");

            // JSON 역직렬화
            Project.Scripts.Model.StageData deserialized = Project.Scripts.Model.StageData.FromJson(json);
            Debug.Log($"역직렬화 결과: 인덱스={deserialized.stageIndex}, 보드블록={deserialized.boardBlocks.Count}, 플레이블록={deserialized.playingBlocks.Count}, 벽={deserialized.walls.Count}");

            Debug.Log("=== 데이터 직렬화 테스트 완료 ===");
        }
    }
}
