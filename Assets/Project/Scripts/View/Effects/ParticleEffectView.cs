using UnityEngine;
using Project.Scripts.View.Effects;

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
        [SerializeField] private float defaultScale = 1f;
        [SerializeField] private Color defaultColor = Color.white;

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
                var startColor = main.startColor;
                startColor.color = color;
                main.startColor = startColor;
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
                var startSize = main.startSize;
                startSize.constant = size;
                main.startSize = startSize;
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
        /// 파티클 효과 재생
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
        /// 파티클 효과를 특정 위치에서 재생
        /// </summary>
        public void PlayAtPosition(Vector3 position)
        {
            transform.position = position;
            Play();
        }

        /// <summary>
        /// 파티클 효과를 특정 색상과 크기로 재생
        /// </summary>
        public void PlayWithProperties(Color color, float size, Vector3 position)
        {
            SetColor(color);
            SetSize(size);
            PlayAtPosition(position);
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
                var main = mainParticleSystem.main;
                return main.duration + main.startLifetime.constant;
            }
            return defaultDuration;
        }

        /// <summary>
        /// 파티클 스케일 변경
        /// </summary>
        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        /// <summary>
        /// 파티클 발산률 조절
        /// </summary>
        public void SetEmissionRate(float rate)
        {
            if (mainParticleSystem != null)
            {
                var emission = mainParticleSystem.emission;
                var rateOverTime = emission.rateOverTime;
                rateOverTime.constant = rate;
                emission.rateOverTime = rateOverTime;
            }
        }

        /// <summary>
        /// 새 파티클 효과 인스턴스 생성
        /// </summary>
        public static ParticleEffectView Spawn(ParticleSystem prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            ParticleSystem instance = Instantiate(prefab, position, rotation);
            ParticleEffectView view = instance.GetComponent<ParticleEffectView>();

            if (view == null)
            {
                view = instance.gameObject.AddComponent<ParticleEffectView>();
            }

            return view;
        }
    }
}