using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.View
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
        [SerializeField] private Color defaultColor = Color.white;

        // 초기화 메서드
        public void Initialize(Material material = null, Color? color = null)
        {
            // 파티클 시스템 참조 확인
            if (mainParticleSystem == null)
                mainParticleSystem = GetComponent<ParticleSystem>();

            // 렌더러 참조 확인
            if (particleRenderers == null || particleRenderers.Length == 0)
                particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>();

            // 재질 설정
            if (material != null)
                SetMaterial(material);

            // 색상 설정
            if (color.HasValue)
                SetColor(color.Value);
        }

        /// <summary>
        /// 파티클 재질 설정
        /// </summary>
        public void SetMaterial(Material material)
        {
            if (particleRenderers == null || material == null) return;

            foreach (var renderer in particleRenderers)
            {
                if (renderer != null)
                    renderer.material = material;
            }
        }

        /// <summary>
        /// 파티클 색상 설정
        /// </summary>
        public void SetColor(Color color)
        {
            if (mainParticleSystem == null) return;

            var main = mainParticleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
        }

        /// <summary>
        /// 파티클 크기 설정
        /// </summary>
        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        /// <summary>
        /// 파티클 방향 설정
        /// </summary>
        public void SetDirection(Vector3 direction)
        {
            if (mainParticleSystem == null) return;

            var shape = mainParticleSystem.shape;
            if (shape.enabled)
            {
                shape.rotation = Quaternion.LookRotation(direction).eulerAngles;
            }
        }

        /// <summary>
        /// 파티클 효과 재생
        /// </summary>
        public void Play(float duration = 0f)
        {
            if (mainParticleSystem == null) return;

            // 지속 시간 설정
            if (duration > 0)
            {
                var main = mainParticleSystem.main;
                main.duration = duration;
            }

            // 파티클 재생
            mainParticleSystem.Play();

            // 지정된 시간 후 자동 제거
            float actualDuration = duration > 0 ? duration : GetEffectDuration();
            Destroy(gameObject, actualDuration + 0.5f); // 여유 시간 추가
        }

        /// <summary>
        /// 파티클 효과를 특정 위치에서 재생
        /// </summary>
        public void PlayAtPosition(Vector3 position, float duration = 0f)
        {
            transform.position = position;
            Play(duration);
        }

        /// <summary>
        /// 파티클 효과 종료
        /// </summary>
        public void Stop()
        {
            if (mainParticleSystem != null)
                mainParticleSystem.Stop();
        }

        /// <summary>
        /// 효과 지속 시간 반환
        /// </summary>
        private float GetEffectDuration()
        {
            if (mainParticleSystem != null)
            {
                var main = mainParticleSystem.main;
                return main.duration + main.startLifetime.constant;
            }
            return defaultDuration;
        }
    }
}