using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Config;
using Project.Scripts.Controller;
using Project.Scripts.Events;
using DG.Tweening;
using static UnityEngine.ParticleSystem;

namespace Project.Scripts.View
{
    /// <summary>
    /// 시각 효과 관리를 담당하는 매니저 클래스
    /// </summary>
    public class VisualEffectManager : MonoBehaviour,
        IGameEventListener<BlockObject>
    {
        private GameConfig gameConfig;
       
     



        // 오브젝트 참조
        private List<GameObject> walls = new List<GameObject>();
        private List<GameObject> blocks = new List<GameObject>();

        private VertexStencilEffectView vertexStencilEffectView;

        /// <summary>
        /// 초기화
        /// </summary>
        // 기존 Initialize 메서드 수정
        public void Initialize(GameConfig config)
        {
            this.gameConfig = config;

            // 버텍스 스텐실 효과 초기화
            vertexStencilEffectView = new VertexStencilEffectView();

            // 셰이더 머티리얼 가져오기
            Material wallStencilMaterial = null;
            Material blockStencilMaterial = null;

            if (gameConfig != null && gameConfig.visualConfig != null)
            {
                // VisualConfig에서 머티리얼 참조 가져오기
                if (gameConfig.visualConfig.vertexStencilDissolveMaterial != null)
                {
                    blockStencilMaterial = gameConfig.visualConfig.vertexStencilDissolveMaterial;
                }

                // 벽 스텐실 머티리얼 생성 (벽은 URP/Lit 등 기본 셰이더 사용 가능)
                Shader wallShader = Shader.Find("Custom/WallStencilWriter");
                if (wallShader != null)
                {
                    wallStencilMaterial = new Material(wallShader);
                }
            }

            // 스텐실 효과 초기화
            vertexStencilEffectView.Initialize(wallStencilMaterial, blockStencilMaterial);

            // 이벤트 등록
            RegisterEvents();
        }


        /// <summary>
        /// 이벤트 등록
        /// </summary>
        private void RegisterEvents()
        {
            gameConfig.gameEvents.onBlockDragStart.RegisterListener(this);
            gameConfig.gameEvents.onBlockDestroy.RegisterListener(this);

            // BoardBlockObject의 정적 이벤트 핸들러 설정
            BoardBlockObject.OnGetDestroyParticle += GetDestroyParticle;
            BoardBlockObject.OnGetMaterial += GetMaterial;
            BoardBlockObject.OnGetBoardSize += GetBoardSize;
        }
        /// <summary>
        /// 디졸브 셰이더 머티리얼 인스턴스 반환
        /// </summary>
    

      
        // OnDestroy 수정
        private void OnDestroy()
        {
            // 이벤트 해제
            if (gameConfig != null)
            {
                gameConfig.gameEvents.onBlockDragStart.UnregisterListener(this);
                gameConfig.gameEvents.onBlockDestroy.UnregisterListener(this);
            }

            BoardBlockObject.OnGetDestroyParticle -= GetDestroyParticle;
            BoardBlockObject.OnGetMaterial -= GetMaterial;
            BoardBlockObject.OnGetBoardSize -= GetBoardSize;

            // 스텐실 효과 정리
            if (vertexStencilEffectView != null)
            {
                vertexStencilEffectView.Cleanup();
            }



        }

        /// <summary>
        /// 블록 드래그 시작 이벤트 처리
        /// </summary>
        public void OnEventRaised(BlockObject block)
        {
            // 블록을 스텐실 효과 대상으로 등록
            RegisterBlock(block.gameObject);
        }

        /// <summary>
        /// 벽 오브젝트 등록
        /// </summary>
        public void RegisterWalls(List<GameObject> wallObjects)
        {
            walls.Clear();
            walls.AddRange(wallObjects);

            // 스텐실 마스크 설정 (벽과 블록 모두 등록)
            if (vertexStencilEffectView != null)
            {
                vertexStencilEffectView.SetupStencilMasking(walls, blocks);
            }

        }

        /// <summary>
        /// 블록 오브젝트 등록
        /// </summary>
        public void RegisterBlock(GameObject blockObject)
        {
            if (!blocks.Contains(blockObject))
            {
                blocks.Add(blockObject);

                // 스텐실 마스킹 설정 업데이트
               
            }
        }
        // 블록이 벽과 접촉할 때 호출할 새 메서드
        public void ApplyWallClippingToBlock(GameObject block, Vector3 wallPosition, LaunchDirection direction)
        {
            if (vertexStencilEffectView == null || block == null) return;

            // 벽 방향에 따른 클리핑 방향 계산
            Vector3 clipNormal = vertexStencilEffectView.CalculateClipNormal(direction);

            // 디버그 로그 추가
            Debug.Log($"Wall Direction: {direction}, Clip Normal: {clipNormal}, Wall Position: {wallPosition}");

            // 블록에 클리핑 효과 적용
            vertexStencilEffectView.ApplyClippingToBlock(block, wallPosition, clipNormal);
        }
        /// <summary>
        /// 블록에 스텐실 읽기 설정 적용
        /// </summary>
        public void PrepareBlocksForStencilReading(List<GameObject> blocks)
        {
            if (vertexStencilEffectView == null) return;

            // 버텍스 스텐실 효과에 블록 전달
            foreach (var block in blocks)
            {
                vertexStencilEffectView.PrepareBlockForStencilReading(block);
            }
        }
        /// <summary>
        /// 파괴 파티클 반환 (이벤트 핸들러)
        /// </summary>
        private ParticleSystem GetDestroyParticle()
        {
            return gameConfig.visualConfig.destroyParticlePrefab;
        }

        /// <summary>
        /// 머티리얼 반환 (이벤트 핸들러)
        /// </summary>
        private Material GetMaterial(int index)
        {
            if (index >= 0 && index < gameConfig.wallConfig.wallMaterials.Length)
            {
                return gameConfig.wallConfig.wallMaterials[index];
            }
            return null;
        }

        /// <summary>
        /// 보드 크기 반환 (이벤트 핸들러)
        /// </summary>
        private Vector2Int GetBoardSize()
        {
            if (StageController.Instance != null)
            {
                return new Vector2Int(StageController.Instance.boardWidth, StageController.Instance.boardHeight);
            }
            return new Vector2Int(10, 10); // 기본값
        }

        /// <summary>
        /// 파괴 파티클 생성
        /// </summary>
        public ParticleSystem CreateParticleEffect(
    Vector3 position,       // 이미 계산된 위치
    Quaternion rotation,    // 이미 계산된 회전
    ColorType colorType,    // 색상
    int blockLength)        // 블록 길이
        {
            if (gameConfig == null || gameConfig.visualConfig == null ||
                gameConfig.visualConfig.destroyParticlePrefab == null)
            {
                Debug.LogWarning("파티클 프리팹이 설정되지 않았습니다.");
                return null;
            }

            // 파티클 인스턴스 생성 - 이미 계산된 위치와 회전 사용
            ParticleSystem particlePrefab = gameConfig.visualConfig.destroyParticlePrefab;
            ParticleSystem particle = Instantiate(particlePrefab, position, rotation);

            // 크기 설정 - 블록 길이 기반
            particle.transform.localScale = new Vector3(blockLength * 0.4f, 0.5f, blockLength * 0.4f);

            // 파티클 재질 설정
            ParticleSystemRenderer[] renderers = particle.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (var renderer in renderers)
            {
                Material material = null;

                // 색상 타입에 맞는 재질 가져오기
                if (gameConfig != null && gameConfig.wallConfig != null)
                {
                    int index = (int)colorType;
                    if (index >= 0 && index < gameConfig.wallConfig.wallMaterials.Length)
                    {
                        material = gameConfig.wallConfig.wallMaterials[index];
                    }
                }

                // 재질 적용
                if (material != null)
                {
                    renderer.material = material;
                }
            }

            // 자동 제거 타이머 설정
            float duration = particle.main.duration + particle.main.startLifetime.constant;
            Destroy(particle.gameObject, duration + 0.5f); // 여유 시간 추가

            return particle;
        }        
    }

}
