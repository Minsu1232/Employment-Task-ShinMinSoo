using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Model;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// �� ���� �� ������ ����ϴ� ��Ʈ�ѷ�
    /// </summary>
    public class WallController : MonoBehaviour
    {
        [Header("������ ����")]
        [SerializeField] private GameObject[] wallPrefabs;
        [SerializeField] private Material[] wallMaterials;

        [Header("�� ����")]
        [SerializeField] private float blockDistance = 0.79f;

        private GameController gameController;
        private GameObject wallsParent;

        // ������ �� ������Ʈ �÷���
        private List<GameObject> walls = new List<GameObject>();

        // ��-���� ��� ���� ����
        private Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic =
            new Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>();

        public void Initialize(GameController controller)
        {
            gameController = controller;
        }

        public async Task CreateWallsAsync(List<WallData> wallsData)
        {
            // ���� �� ����
            ClearWalls();

            // �� �� �θ� ������Ʈ ����
            wallsParent = new GameObject("WallsParent");
            wallsParent.transform.SetParent(transform);

            // �� ��ǥ ���� ��ųʸ� �ʱ�ȭ
            wallCoorInfoDic.Clear();

            // �� ����
            foreach (var wallData in wallsData)
            {
                await CreateWallAsync(wallData);
            }
        }

        private async Task CreateWallAsync(WallData wallData)
        {
            if (wallPrefabs == null || wallPrefabs.Length == 0)
            {
                Debug.LogError("�� �������� �������� �ʾҽ��ϴ�!");
                return;
            }

            // �� ��ġ�� ȸ�� ���
            Vector3 position = wallData.GetWallPosition(blockDistance);
            Quaternion rotation = wallData.GetWallRotation();

            // �� ��ǥ ���� ����
            SetupWallCoordinateInfo(wallData);

            // �� ���̿� ���� ������ �ε��� ����
            int prefabIndex = Mathf.Clamp(wallData.Length - 1, 0, wallPrefabs.Length - 1);

            // �� ������Ʈ ����
            GameObject wallObj = Instantiate(wallPrefabs[prefabIndex], wallsParent.transform);
            wallObj.transform.position = position;
            wallObj.transform.rotation = rotation;

            // �� ������Ʈ ����
            if (wallObj.TryGetComponent(out WallObject wall))
            {
                // �� ���� �� �Ӽ� ����
                int materialIndex = (int)wallData.ColorType;
                bool isColorableWall = wallData.ColorType != ColorType.None;

                if (materialIndex >= 0 && materialIndex < wallMaterials.Length)
                {
                    wall.SetWall(wallMaterials[materialIndex], isColorableWall);
                }
            }

            // �� ������Ʈ ����
            walls.Add(wallObj);

            await Task.Yield();
        }

        private void SetupWallCoordinateInfo(WallData wallData)
        {
            // �ı� ������ ���� ���� ����
            if (wallData.ColorType == ColorType.None) return;

            DestroyWallDirection destroyDirection = DetermineDestroyDirection(wallData.WallDirection);
            if (destroyDirection == DestroyWallDirection.None) return;

            // �� ��ǥ �� �ı� ���� ����
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

        // �� ���� �׼��� �޼����
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

        // �� ��Ƽ���� �׼��� �޼���
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