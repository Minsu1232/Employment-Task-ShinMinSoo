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
    /// 벽 생성 및 관리를 담당하는 빌더 클래스
    /// </summary>
    public class WallBuilder : MonoBehaviour
    {
        private GameConfig gameConfig;

        // 벽 데이터
        public List<GameObject> walls = new List<GameObject>();
        private Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> wallCoorInfoDic;

        /// <summary>
        /// GameConfig를 통한 초기화
        /// </summary>
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;
            walls.Clear();
        }

        /// <summary>
        /// 벽 생성
        /// </summary>
        public async Task<Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>>> CreateCustomWalls(
            int stageIdx, GameObject boardParent)
        {
            Debug.Log($"Stage {stageIdx} - 벽 데이터 개수: {gameConfig.stageDatas[stageIdx].walls.Count}");
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
        /// 개별 벽 생성
        /// </summary>
        private void CreateWall(WallData wallData, GameObject parent)
        {
            Quaternion rotation;
            float blockDistance = gameConfig.boardConfig.blockDistance;

            // 기본 위치 계산
            var position = new Vector3(
                wallData.x * blockDistance,
                0f,
                wallData.y * blockDistance);

            DestroyWallDirection destroyDirection = DestroyWallDirection.None;
            bool shouldAddWallInfo = false;

            // 벽 방향과 유형에 따라 위치와 회전 조정
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
                    // 왼쪽 위 모서리
                    position.x -= 0.5f;
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Left_Down:
                    // 왼쪽 아래 모서리
                    position.x -= 0.5f;
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    break;

                case ObjectPropertiesEnum.WallDirection.Right_Up:
                    // 오른쪽 위 모서리
                    position.x += 0.5f;
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 270f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Right_Down:
                    // 오른쪽 아래 모서리
                    position.x += 0.5f;
                    position.z -= 0.5f;
                    rotation = Quaternion.Euler(0f, 0f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Up:
                    // 위쪽이 열린 벽
                    position.z += 0.5f;
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Down:
                    // 아래쪽이 열린 벽
                    position.z -= 0.5f;
                    rotation = Quaternion.identity;
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Left:
                    // 왼쪽이 열린 벽
                    position.x -= 0.5f;
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    break;

                case ObjectPropertiesEnum.WallDirection.Open_Right:
                    // 오른쪽이 열린 벽
                    position.x += 0.5f;
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                    break;

                default:
                    Debug.LogError($"지원되지 않는 벽 방향: {wallData.WallDirection}");
                    return;
            }    
            // 벽 정보 추가
            if (shouldAddWallInfo && wallData.wallColor != ColorType.None)
            {
                AddWallInfo(wallData, destroyDirection);
            }

            // 길이에 따른 위치 조정
            AdjustPositionByLength(ref position, wallData);

            // 벽 오브젝트 생성
            CreateWallObject(wallData, position, rotation, parent);
        }

        /// <summary>
        /// 벽 정보 추가
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
        /// 길이에 따른 위치 조정
        /// </summary>
        private void AdjustPositionByLength(ref Vector3 position, WallData wallData)
        {
            float blockDistance = gameConfig.boardConfig.blockDistance;

            if (wallData.Length > 1)
            {
                // 수평 벽의 중앙 위치 조정 (Up, Down 방향)
                if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Up ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Down ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Up ||
                    wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Down)
                {
                    // x축으로 중앙으로 이동
                    position.x += (wallData.Length - 1) * blockDistance * 0.5f;
                }
                // 수직 벽의 중앙 위치 조정 (Left, Right 방향)
                else if (wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Left ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Single_Right ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Left ||
                         wallData.WallDirection == ObjectPropertiesEnum.WallDirection.Open_Right)
                {
                    // z축으로 중앙으로 이동
                    position.z += (wallData.Length - 1) * blockDistance * 0.5f;
                }
            }
        }

        /// <summary>
        /// 벽 오브젝트 생성
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
        /// 벽 머티리얼 반환
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