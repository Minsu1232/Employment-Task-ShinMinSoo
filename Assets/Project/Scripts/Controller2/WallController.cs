using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 벽 생성 및 관리를 담당하는 컨트롤러
    /// </summary>
    public class WallController : MonoBehaviour
    {
        [Header("프리팹 참조")]
        [SerializeField] private GameObject[] wallPrefabs;
        [SerializeField] private Material[] wallMaterials;

        [Header("벽 설정")]
        [SerializeField] private float blockDistance = 0.79f;

        private GameController gameController;
        private GameObject wallsParent;

        // 생성된 벽 오브젝트 컬렉션
        private List<GameObject> walls = new List<GameObject>();

        // 벽-보드 블록 관계 정보
        private Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic =
            new Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>();

        public void Initialize(GameController controller)
        {
            gameController = controller;
        }

        public async Task CreateWallsAsync(List<WallData> wallsData)
        {
            // 이전 벽 제거
            ClearWalls();

            // 새 벽 부모 오브젝트 생성
            wallsParent = new GameObject("WallsParent");
            wallsParent.transform.SetParent(transform);

            // 벽 좌표 정보 딕셔너리 초기화
            wallCoorInfoDic.Clear();

            // 벽 생성
            foreach (var wallData in wallsData)
            {
                await CreateWallAsync(wallData);
            }
        }

        private async Task CreateWallAsync(WallData wallData)
        {
            if (wallPrefabs == null || wallPrefabs.Length == 0)
            {
                Debug.LogError("벽 프리팹이 설정되지 않았습니다!");
                return;
            }

            // 벽 위치와 회전 계산
            Vector3 position = wallData.GetWallPosition(blockDistance);
            Quaternion rotation = wallData.GetWallRotation();

            // 벽 좌표 정보 설정
            SetupWallCoordinateInfo(wallData);

            // 벽 길이에 따른 프리팹 인덱스 결정
            int prefabIndex = Mathf.Clamp(wallData.Length - 1, 0, wallPrefabs.Length - 1);

            // 벽 오브젝트 생성
            GameObject wallObj = Instantiate(wallPrefabs[prefabIndex], wallsParent.transform);
            wallObj.transform.position = position;
            wallObj.transform.rotation = rotation;

            // 벽 컴포넌트 설정
            if (wallObj.TryGetComponent(out WallObject wall))
            {
                // 벽 색상 및 속성 설정
                int materialIndex = (int)wallData.ColorType;
                bool isColorableWall = wallData.ColorType != ColorType.None;

                if (materialIndex >= 0 && materialIndex < wallMaterials.Length)
                {
                    wall.SetWall(wallMaterials[materialIndex], isColorableWall);
                }
            }

            // 벽 오브젝트 저장
            walls.Add(wallObj);

            await Task.Yield();
        }

        private void SetupWallCoordinateInfo(WallData wallData)
        {
            // 파괴 가능한 벽만 정보 설정
            if (wallData.ColorType == ColorType.None) return;

            DestroyWallDirection destroyDirection = DetermineDestroyDirection(wallData.WallDirection);
            if (destroyDirection == DestroyWallDirection.None) return;

            // 벽 좌표 및 파괴 정보 설정
            var pos = (wallData.X, wallData.Y);
            var wallInfo = (destroyDirection, wallData.ColorType);

            if (!wallCoorInfoDic.ContainsKey(pos))
            {
                var wallInfoDic = new Dictionary<(DestroyWallDirection, ColorType), int>
                {
                    { wallInfo, wallData.Length }
                };
                wallCoorInfoDic.Add(pos, wallInfoDic);
            }
            else
            {
                wallCoorInfoDic[pos][wallInfo] = wallData.Length;
            }
        }

        private DestroyWallDirection DetermineDestroyDirection(ObjectPropertiesEnum.WallDirection direction)
        {
            switch (direction)
            {
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                    return DestroyWallDirection.Up;
                case ObjectPropertiesEnum.WallDirection.Single_Down:
                    return DestroyWallDirection.Down;
                case ObjectPropertiesEnum.WallDirection.Single_Left:
                    return DestroyWallDirection.Left;
                case ObjectPropertiesEnum.WallDirection.Single_Right:
                    return DestroyWallDirection.Right;
                default:
                    return DestroyWallDirection.None;
            }
        }

        public void ClearWalls()
        {
            foreach (var wall in walls)
            {
                if (wall != null)
                {
                    Destroy(wall);
                }
            }

            walls.Clear();

            if (wallsParent != null)
            {
                Destroy(wallsParent);
            }
        }

        // 벽 정보 액세스 메서드들
        public Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> GetWallCoordinateInfo()
        {
            return wallCoorInfoDic;
        }

        public bool HasWallAt(int x, int y, DestroyWallDirection direction, out ColorType wallColor, out int length)
        {
            wallColor = ColorType.None;
            length = 0;

            if (!wallCoorInfoDic.ContainsKey((x, y)))
            {
                return false;
            }

            foreach (var wallInfo in wallCoorInfoDic[(x, y)].Keys)
            {
                if (wallInfo.Item1 == direction)
                {
                    wallColor = wallInfo.Item2;
                    length = wallCoorInfoDic[(x, y)][wallInfo];
                    return true;
                }
            }

            return false;
        }

        // 벽 머티리얼 액세스 메서드
        public Material GetWallMaterial(int colorIndex)
        {
            if (colorIndex >= 0 && colorIndex < wallMaterials.Length)
            {
                return wallMaterials[colorIndex];
            }
            return null;
        }
    }
}