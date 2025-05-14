using Project.Scripts.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Project.Scripts.Config;
using static Project.Scripts.Model.BoardBlockData;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// �� ���� �� ������ ����ϴ� ���� Ŭ����
    /// </summary>
    public class WallBuilder : MonoBehaviour
    {
        private GameConfig gameConfig;

        // �� ������
        public List<GameObject> walls = new List<GameObject>();
        private Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic;

        /// <summary>
        /// GameConfig�� ���� �ʱ�ȭ
        /// </summary>
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;
            walls.Clear();
        }

        /// <summary>
        /// �� ����
        /// </summary>
        public async Task<Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>> CreateCustomWalls(
            int stageIdx, GameObject boardParent)
        {
            Debug.Log($"Stage {stageIdx} - �� ������ ����: {gameConfig.stageDatas[stageIdx].walls.Count}");
            if (stageIdx < 0 || stageIdx >= gameConfig.stageDatas.Length || gameConfig.stageDatas[stageIdx].walls == null)
            {               
                return new Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>();
            }

            GameObject wallsParent = new GameObject("CustomWallsParent");
            wallsParent.transform.SetParent(boardParent.transform);

            wallCoorInfoDic = new Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>();

            foreach (var wallData in gameConfig.stageDatas[stageIdx].walls)
            {
                CreateWall(wallData, wallsParent);
            }

            await Task.Yield();

            return wallCoorInfoDic;
        }

        /// <summary>
        /// ���� �� ����
        /// </summary>
        private void CreateWall(WallData wallData, GameObject parent)
        {
            Quaternion rotation;
            float blockDistance = gameConfig.boardConfig.blockDistance;

            // �⺻ ��ġ ���
            var position = new Vector3(
                wallData.x * blockDistance,
                0f,
                wallData.y * blockDistance);

            DestroyWallDirection destroyDirection = DestroyWallDirection.None;
            bool shouldAddWallInfo = false;

            // �� ����� ������ ���� ��ġ�� ȸ�� ����
            switch (wallData.WallDirection)
            {
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Up;
                    break;

                case ObjectPropertiesEnum.WallDirection.Single_Down:
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Down;
                    break;

                case ObjectPropertiesEnum.WallDirection.Single_Left:
                    position.x -= 0.5f;
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Left;
                    break;

                case ObjectPropertiesEnum.WallDirection.Single_Right:
                    position.x += 0.5f;
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                    shouldAddWallInfo = true;
                    destroyDirection = DestroyWallDirection.Right;
                    break;

                case ObjectPropertiesEnum.WallDirection.Left_Up:
                    // ���� �� �𼭸�
                    position.x -= 0.5f;
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Left_Down:
                    // ���� �Ʒ� �𼭸�
                    position.x -= 0.5f;
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    break;

                case ObjectPropertiesEnum.WallDirection.Right_Up:
                    // ������ �� �𼭸�
                    position.x += 0.5f;
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 270f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Right_Down:
                    // ������ �Ʒ� �𼭸�
                    position.x += 0.5f;
                    position.z -= 0.5f;
                    rotation = Quaternion.Euler(0f, 0f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Up:
                    // ������ ���� ��
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Down:
                    // �Ʒ����� ���� ��
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Left:
                    // ������ ���� ��
                    position.x -= 0.5f;
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Right:
                    // �������� ���� ��
                    position.x += 0.5f;
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                    break;

                default:
                    Debug.LogError($"�������� �ʴ� �� ����: {wallData.WallDirection}");
                    return;
            }    
            // �� ���� �߰�
            if (shouldAddWallInfo && wallData.wallColor != ColorType.None)
            {
                AddWallInfo(wallData, destroyDirection);
            }

            // ���̿� ���� ��ġ ����
            AdjustPositionByLength(ref position, wallData);

            // �� ������Ʈ ����
            CreateWallObject(wallData, position, rotation, parent);
        }

        /// <summary>
        /// �� ���� �߰�
        /// </summary>
        private void AddWallInfo(WallData wallData, DestroyWallDirection destroyDirection)
        {
            var pos = (wallData.x, wallData.y);
            var wallInfo = (destroyDirection, wallData.wallColor);

            if (!wallCoorInfoDic.ContainsKey(pos))
            {
                Dictionary<(DestroyWallDirection, ColorType), int> wallInfoDic =
                    new Dictionary<(DestroyWallDirection, ColorType), int> { { wallInfo, wallData.Length } };
                wallCoorInfoDic.Add(pos, wallInfoDic);
            }
            else
            {
                wallCoorInfoDic[pos].Add(wallInfo, wallData.Length);
            }
        }

        /// <summary>
        /// ���̿� ���� ��ġ ����
        /// </summary>
        private void AdjustPositionByLength(ref Vector3 position, WallData wallData)
        {
            float blockDistance = gameConfig.boardConfig.blockDistance;

            if (wallData.Length > 1)
            {
                // ���� ���� �߾� ��ġ ���� (Up, Down ����)
                if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Up ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Down ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Up ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Down)
                {
                    // x������ �߾����� �̵�
                    position.x += (wallData.Length - 1) * blockDistance * 0.5f;
                }
                // ���� ���� �߾� ��ġ ���� (Left, Right ����)
                else if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Left ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Right ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Left ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Right)
                {
                    // z������ �߾����� �̵�
                    position.z += (wallData.Length - 1) * blockDistance * 0.5f;
                }
            }
        }

        /// <summary>
        /// �� ������Ʈ ����
        /// </summary>
        private void CreateWallObject(WallData wallData, Vector3 position, Quaternion rotation, GameObject parent)
        {
            GameObject[] wallPrefabs = gameConfig.wallConfig.wallPrefabs;
            Material[] wallMaterials = gameConfig.wallConfig.wallMaterials;

            if (wallData.Length - 1 >= 0 && wallData.Length - 1 < wallPrefabs.Length)
            {
                GameObject wallObj = Instantiate(wallPrefabs[wallData.Length - 1], parent.transform);
                wallObj.transform.position = position;
                wallObj.transform.rotation = rotation;

                WallObject wall = wallObj.GetComponent<WallObject>();
                if (wall != null)
                {
                    wall.SetWall(wallMaterials[(int)wallData.wallColor], wallData.wallColor != ColorType.None);

                }

                walls.Add(wallObj);

            }           
        }

        /// <summary>
        /// �� ��Ƽ���� ��ȯ
        /// </summary>
        public Material GetWallMaterial(int index)
        {
            if (gameConfig != null &&
                gameConfig.wallConfig != null &&
                index >= 0 &&
                index < gameConfig.wallConfig.wallMaterials.Length)
            {
                return gameConfig.wallConfig.wallMaterials[index];
            }
            return null;
        }
    }
}