using UnityEngine;

namespace Project.Scripts.View.Effects
{
    /// <summary>
    /// 파티클 효과를 관리하는 뷰 컴포넌트
    /// </summary>
    public class ParticleEffectView : MonoBehaviour
    {
        [Header("파티클 참조")]
        [SerializeField] private ParticleSystem mainParticleSystem;
        [SerializeField] private ParticleSystemRenderer[] particleRenderers;

        [Header("효과 설정")]
        [SerializeField] private float defaultDuration = 1f;

        private void Awake()
        {
            // 파티클 시스템 참조 찾기
            if (mainParticleSystem == null)
            {
                mainParticleSystem = GetComponent<ParticleSystem>();
            }

            // 렌더러 참조 찾기
            if (particleRenderers == null || particleRenderers.Length == 0)
            {
                particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>();
            }
        }

        /// <summary>
        /// 파티클 머티리얼 설정
        /// </summary>
        public void SetMaterial(Material material)
        {
            if (particleRenderers != null)
            {
                foreach (var renderer in particleRenderers)
                {
                    renderer.material = material;
                }
            }
        }

        /// <summary>
        /// 파티클 색상 설정
        /// </summary>
        public void SetColor(Color color)
        {
            if (mainParticleSystem != null)
            {
                var main = mainParticleSystem.main;
                main.startColor = color;
            }
        }

        /// <summary>
        /// 파티클 크기 설정
        /// </summary>
        public void SetSize(float size)
        {
            if (mainParticleSystem != null)
            {
                var main = mainParticleSystem.main;
                main.startSize = size;
            }
        }

        /// <summary>
        /// 파티클 방향 설정
        /// </summary>
        public void SetDirection(Vector3 direction)
        {
            if (mainParticleSystem != null)
            {
                var shape = mainParticleSystem.shape;
                if (shape.enabled)
                {
                    shape.rotation = Quaternion.LookRotation(direction).eulerAngles;
                }
            }
        }

        /// <summary>
        /// 파티클 지속 시간 설정
        /// </summary>
        public void SetDuration(float duration)
        {
            if (mainParticleSystem != null)
            {
                var main = mainParticleSystem.main;
                main.duration = duration;
            }
        }

        /// <summary>
        /// 파티클 효과 실행
        /// </summary>
        public void Play()
        {
            if (mainParticleSystem != null)
            {
                mainParticleSystem.Play();

                // 지정된 시간 후 자동 제거
                Destroy(gameObject, GetEffectDuration());
            }
        }

        /// <summary>
        /// 파티클 효과 정지
        /// </summary>
        public void Stop()
        {
            if (mainParticleSystem != null)
            {
                mainParticleSystem.Stop();
            }
        }

        /// <summary>
        /// 효과 지속 시간 반환
        /// </summary>
        private float GetEffectDuration()
        {
            if (mainParticleSystem != null)
            {
                return mainParticleSystem.main.duration + mainParticleSystem.main.startLifetime.constant;
            }
            return defaultDuration;
        }
    }
}