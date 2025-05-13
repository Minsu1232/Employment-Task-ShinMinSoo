using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Controller
{
    /// <summary>
    /// 블록 드래그 핸들러 - 조정된 클래스
    /// </summary>
    public class BlockDragHandler : MonoBehaviour
    {
        // 블록 속성 정보
        public int horizon = 1;
        public int vertical = 1;
        public int uniqueIndex;
        public List<ObjectPropertiesEnum.BlockGimmickType> gimmickType = new List<ObjectPropertiesEnum.BlockGimmickType>();
        public List<BlockObject> blocks = new List<BlockObject>();
        public List<Vector2> blockOffsets = new List<Vector2>();
        public bool Enabled = true;

        // 내부 상태
        public bool IsDragging { get; set; } = false;
        public Collider col { get; set; }

        // 컴포넌트 참조
        private BlockInputHandler inputHandler;
        private BlockPhysicsHandler physicsHandler;
        private BlockGridHandler gridHandler;

        // 이벤트 델리게이트
        public delegate void BlockDestroyedHandler(BlockObject block);
        public BlockDestroyedHandler OnBlockDestroyed;

        private void Awake()
        {
            // 필요한 컴포넌트 추가 및 참조 설정
            inputHandler = GetComponent<BlockInputHandler>() ?? gameObject.AddComponent<BlockInputHandler>();
            physicsHandler = GetComponent<BlockPhysicsHandler>() ?? gameObject.AddComponent<BlockPhysicsHandler>();
            gridHandler = GetComponent<BlockGridHandler>() ?? gameObject.AddComponent<BlockGridHandler>();
        }

        /// <summary>
        /// X축 중심 위치 계산
        /// </summary>
        public Vector3 GetCenterX()
        {
            if (gridHandler != null)
            {
                return gridHandler.GetCenterX();
            }

            // GridHandler가 없는 경우 기존 로직 사용
            if (blocks == null || blocks.Count == 0)
            {
                return Vector3.zero; // 리스트가 비어있으면 기본값 반환
            }

            // X 좌표의 최소/최대값 계산
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            foreach (var block in blocks)
            {
                float blockX = block.transform.position.x;

                if (blockX < minX) minX = blockX;
                if (blockX > maxX) maxX = blockX;
            }

            // 최소값과 최대값의 중간 계산
            return new Vector3((minX + maxX) / 2f, transform.position.y, 0);
        }

        /// <summary>
        /// Z축 중심 위치 계산
        /// </summary>
        public Vector3 GetCenterZ()
        {
            if (gridHandler != null)
            {
                return gridHandler.GetCenterZ();
            }

            // GridHandler가 없는 경우 기존 로직 사용
            if (blocks == null || blocks.Count == 0)
            {
                return Vector3.zero; // 리스트가 비어있으면 기본값 반환
            }

            // Z 좌표의 최소/최대값 계산
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (var block in blocks)
            {
                float blockZ = block.transform.position.z;

                if (blockZ < minZ) minZ = blockZ;
                if (blockZ > maxZ) maxZ = blockZ;
            }

            // 최소값과 최대값의 중간 계산
            return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
        }

        /// <summary>
        /// 이전 보드 블록 참조 정리
        /// </summary>
        private void ClearPreboardBlockObjects()
        {
            foreach (var b in blocks)
            {
                if (b.preBoardBlockObject != null)
                {
                    b.preBoardBlockObject.playingBlock = null;
                }
            }
        }

        /// <summary>
        /// 블록 파괴 애니메이션
        /// </summary>
        public void DestroyMove(Vector3 pos, ParticleSystem particle)
        {
            ClearPreboardBlockObjects();

            // 애니메이션 실행 후 정리 작업
            transform.DOMove(pos, 1f).SetEase(Ease.Linear)
                .OnComplete(() => {
                    if (particle != null)
                    {
                        Destroy(particle.gameObject);
                    }

                    // 블록 파괴 이벤트 호출
                    OnBlockDestroyed?.Invoke(blocks.Count > 0 ? blocks[0] : null);

                    Destroy(gameObject);
                });
        }

        /// <summary>
        /// 입력 강제 해제
        /// </summary>
        public void ReleaseInput()
        {
            if (inputHandler != null)
            {
                inputHandler.ReleaseInput();
            }
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        private void OnDisable()
        {
            transform.DOKill(true);
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        private void OnDestroy()
        {
            transform.DOKill(true);
        }
    }
}