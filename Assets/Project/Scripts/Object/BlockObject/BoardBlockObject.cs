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
        public List<int> checkGroupIdx = new List<int>();
        public List<ColorType> colorType = new List<ColorType>();
        public List<bool> isHorizon = new List<bool>();
        public List<int> len = new List<int>();
        public int x;
        public int y;

        // 뷰 컴포넌트 참조
        private BoardBlockView boardBlockView;

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

        private void Awake()
        {
            // 뷰 컴포넌트 참조 가져오기
            boardBlockView = GetComponent<BoardBlockView>();
            if (boardBlockView == null)
            {
                boardBlockView = gameObject.AddComponent<BoardBlockView>();
            }
        }

        private void Start()
        {
            // 체크 블록 표시 설정
            if (boardBlockView != null && isCheckBlock)
            {
                boardBlockView.ShowCheckBlockIndicator(true);
            }
        }

        public bool CheckAdjacentBlock(BlockObject block, Vector3 destroyStartPos)
        {
            if (!isCheckBlock) return false;
            if (!block.dragHandler.enabled) return false;

            for (int i = 0; i < colorType.Count; i++)
            {
                if (block.colorType == colorType[i])
                {
                    int length = 0;
                    if (isHorizon[i])
                    {
                        if (block.dragHandler.horizon > len[i]) return false;
                        // 이벤트 호출로 변경
                        if (OnCheckDestroy == null || !OnCheckDestroy(this, block)) return false;
                        length = block.dragHandler.vertical;
                    }
                    else
                    {
                        if (block.dragHandler.vertical > len[i]) return false;
                        // 이벤트 호출로 변경
                        if (OnCheckDestroy == null || !OnCheckDestroy(this, block)) return false;
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
                            break;
                        case LaunchDirection.Left:
                            centerPos += Vector3.left * 0.55f;
                            rotation = Quaternion.Euler(0, 90, 0);
                            break;
                        case LaunchDirection.Right:
                            centerPos += Vector3.right * 0.55f;
                            centerPos.x = transform.position.x;
                            centerPos.x += 0.65f;
                            rotation = Quaternion.Euler(0, -90, 0);
                            break;
                    }

                    int blockLength = isHorizon[i] ? block.dragHandler.horizon : block.dragHandler.vertical;

                    // 파티클 프리팹 가져오기
                    ParticleSystem particlePrefab = OnGetDestroyParticle != null
                        ? OnGetDestroyParticle()
                        : null;

                    if (particlePrefab != null)
                    {
                        // 파티클 설정
                        ParticleSystem[] pss = particlePrefab.GetComponentsInChildren<ParticleSystem>();
                        foreach (var ps in pss)
                        {
                            ParticleSystemRenderer psrs = ps.GetComponent<ParticleSystemRenderer>();
                            // 재질 가져오기 이벤트 사용
                            if (OnGetMaterial != null)
                            {
                                psrs.material = OnGetMaterial((int)block.colorType);
                            }
                        }

                        // 파티클 생성
                        ParticleSystem particle = Instantiate(particlePrefab, transform.position, rotation);
                        particle.transform.position = centerPos;
                        particle.transform.localScale = new Vector3(blockLength * 0.4f, 0.5f, blockLength * 0.4f);

                        // 보드 블록 뷰에 파괴 효과 적용
                        if (boardBlockView != null)
                        {
                            boardBlockView.PlayDestroyEffect();
                        }

                        // 파괴 애니메이션 실행 (View를 통해 실행)
                        BlockView blockView = block.GetComponent<BlockView>();
                        if (blockView != null)
                        {
                            blockView.PlayDestroyAnimation(pos, 1f, () => {
                                if (particle != null)
                                {
                                    Destroy(particle.gameObject);
                                }

                                if (block.dragHandler != null)
                                {
                                    block.dragHandler.OnBlockDestroyed?.Invoke(block);
                                    Destroy(block.dragHandler.gameObject);
                                }
                            });
                        }
                        else
                        {
                            // View가 없는 경우 기존 로직 사용
                            block.dragHandler.DestroyMove(pos, particle);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 블록을 놓을 때 배치 가능 여부를 시각적으로 표시합니다.
        /// </summary>
        public void ShowPlacementIndicator(BlockObject block, bool canPlace)
        {
            if (boardBlockView != null)
            {
                boardBlockView.ShowPlacementIndicator(canPlace);

                // 체크 블록인 경우, 색상 매칭 표시
                if (isCheckBlock && canPlace)
                {
                    for (int i = 0; i < colorType.Count; i++)
                    {
                        if (block.colorType == colorType[i])
                        {
                            boardBlockView.ShowDestroyableIndicator(true, block.colorType);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 발사 방향을 결정합니다.
        /// </summary>
        private LaunchDirection GetLaunchDirection(int x, int y, bool isHorizon, int boardWidth, int boardHeight)
        {
            // 모서리 케이스들
            if (x == 0 && y == 0)
                return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;

            if (x == 0 && y == boardHeight)
                return isHorizon ? LaunchDirection.Up : LaunchDirection.Left;

            if (x == boardWidth && y == 0)
                return isHorizon ? LaunchDirection.Down : LaunchDirection.Right;

            if (x == boardWidth && y == boardHeight)
                return isHorizon ? LaunchDirection.Up : LaunchDirection.Right;

            // 기본 경계 케이스들
            if (x == 0)
                return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;

            if (y == 0)
                return isHorizon ? LaunchDirection.Down : LaunchDirection.Left;

            if (x == boardWidth)
                return isHorizon ? LaunchDirection.Down : LaunchDirection.Right;

            if (y == boardHeight)
                return isHorizon ? LaunchDirection.Up : LaunchDirection.Right;

            // 기본값
            return LaunchDirection.Up;
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