using UnityEngine;
using Project.Scripts.View;
using System.Collections.Generic;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 보드 블록 오브젝트
    /// </summary>
    public class BoardBlockObject : MonoBehaviour
    {
        public BlockObject playingBlock;
        public bool isCheckBlock;
        public List<int> checkGroupIdx;
        public List<ColorType> colorType;
        public List<bool> isHorizon;
        public List<int> len;
        public int x;
        public int y;

        // 이벤트 정의
        public delegate bool CheckDestroyHandler(BoardBlockObject boardBlock, BlockObject block);
        public static event CheckDestroyHandler OnCheckDestroy;

        public delegate ParticleSystem GetParticleHandler();
        public static event GetParticleHandler OnGetDestroyParticle;

        public delegate Material GetMaterialHandler(int index);
        public static event GetMaterialHandler OnGetMaterial;

        public delegate Vector2Int GetBoardSizeHandler();
        public static event GetBoardSizeHandler OnGetBoardSize;

        public ColorType horizonColorType =>
            isHorizon.IndexOf(true) != -1 ? colorType[isHorizon.IndexOf(true)] : ColorType.None;

        public ColorType verticalColorType =>
            isHorizon.IndexOf(false) != -1 ? colorType[isHorizon.IndexOf(false)] : ColorType.None;

 

        public bool CheckAdjacentBlock(BlockObject block, Vector3 destroyStartPos)
        {
            for (int i = 0; i < colorType.Count; i++)
            {
                if (block.colorType == colorType[i])
                {                  

                    int length = 0;
                    if (isHorizon[i])
                    {
                        if (block.dragHandler.horizon > len[i])
                        {
                            
                            return false;
                        }
                        // 이벤트 호출로 변경
                        if (OnCheckDestroy == null || !OnCheckDestroy(this, block))
                        {
                            
                            return false;
                        }
                        length = block.dragHandler.vertical;
                    }
                    else
                    {
                        if (block.dragHandler.vertical > len[i])
                        {
                           
                            return false;
                        }
                        // 이벤트 호출로 변경
                        if (OnCheckDestroy == null || !OnCheckDestroy(this, block))
                        {
                            
                            return false;
                        }
                        length = block.dragHandler.horizon;
                        
                    }
                    block.dragHandler.transform.position = destroyStartPos;
                    block.dragHandler.ReleaseInput();

                    foreach (var blockObject in block.dragHandler.blocks)
                    {
                        blockObject.ColliderOff();
                    }

                    block.dragHandler.enabled = false;

                    // 보드 크기를 이벤트로 가져오기
                    Vector2Int boardSize = OnGetBoardSize != null ? OnGetBoardSize() : new Vector2Int(10, 10);
                    int boardWidth = boardSize.x;
                    int boardHeight = boardSize.y;

                    bool isRight = isHorizon[i] ? y < boardHeight / 2 : x < boardWidth / 2;
                    if (!isRight) length *= -1;
                    Vector3 pos = isHorizon[i]
                        ? new Vector3(block.dragHandler.transform.position.x, block.dragHandler.transform.position.y,
                            block.dragHandler.transform.position.z - length * 0.79f)
                        : new Vector3(block.dragHandler.transform.position.x - length * 0.79f,
                            block.dragHandler.transform.position.y, block.dragHandler.transform.position.z);

                    Vector3 centerPos =
                        isHorizon[i]
                            ? block.dragHandler.GetCenterX()
                            : block.dragHandler.GetCenterZ();
                    LaunchDirection direction = GetLaunchDirection(x, y, isHorizon[i], boardWidth, boardHeight);
                    Quaternion rotation = Quaternion.identity;

                    centerPos.y = 0.55f;                  

                    switch (direction)
                    {
                        case LaunchDirection.Up:
                            centerPos += Vector3.forward * 0.65f;
                            centerPos.z = transform.position.z;
                            centerPos.z += 0.55f;
                            rotation = Quaternion.Euler(0, 180, 0);
                            break;
                        case LaunchDirection.Down:
                            centerPos += Vector3.back * 0.65f;
                            centerPos.z = transform.position.z;
                            centerPos.z -= 0.55f;  // Down 방향 보정
                            break;
                        case LaunchDirection.Left:
                            centerPos += Vector3.left * 0.55f;
                            centerPos.x = transform.position.x;  // 좌측 위치 보정
                            centerPos.x -= 0.55f;  // 추가 보정
                            rotation = Quaternion.Euler(0, 90, 0);
                            break;
                        case LaunchDirection.Right:
                            centerPos += Vector3.right * 0.55f;
                            centerPos.x = transform.position.x;
                            centerPos.x += 0.65f;
                            rotation = Quaternion.Euler(0, -90, 0);
                            break;
                    }                  

                    BlockDestroyManager blockDestroyManager = StageController.Instance.GetBlockDestroyManager();
                    if (blockDestroyManager != null)
                    {
                        blockDestroyManager.DestroyBlockWithEffect(
                            block,
                            pos,              // 이동 목표 위치
                            centerPos,        // 효과 생성 위치 
                            direction,        // 발사 방향
                            block.colorType,   // 블록 색상
                            rotation
                        );
                        
                    }
                   

                    return true;
                }
            }

           
            return false;
        }

        /// <summary>
        /// 발사 방향을 결정합니다.
        /// </summary>
        private LaunchDirection GetLaunchDirection(int x, int y, bool isHorizon, int boardWidth, int boardHeight)
        {
            if (isHorizon)
            {
                if (y == 0) return LaunchDirection.Down;
                if (y == boardHeight) return LaunchDirection.Up;
                if (y < boardHeight / 2) return LaunchDirection.Down;
                else return LaunchDirection.Up;
            }
            else
            {
                if (x == 0) return LaunchDirection.Left;
                if (x == boardWidth) return LaunchDirection.Right;
                if (x < boardWidth / 2) return LaunchDirection.Left;
                else return LaunchDirection.Right;
            }
        }
        /// <summary>
        /// 보드 블록의 메테리얼을 설정합니다
        /// </summary>
        public void SetMaterial(Material material, bool isColorBlock)
        {
            // 렌더러 컴포넌트 찾기
            Renderer blockRenderer = GetComponentInChildren<Renderer>();
            if (blockRenderer != null && material != null)
            {
                // 메테리얼 설정
                blockRenderer.material = material;
            }

            // 색상 블록 여부에 따른 추가 설정
            if (isColorBlock)
            {
                // 색상 블록인 경우 체크 블록으로 설정
                isCheckBlock = true;
            }
        }
    }
    
    public enum LaunchDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum ColorType
    {
        None,
        Red,
        Orange,
        Yellow,
        Gray,
        Purple,
        Beige,
        Blue,
        Green
    }
}