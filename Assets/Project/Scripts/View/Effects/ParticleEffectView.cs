using UnityEngine;

namespace Project.Scripts.View.Effects
{
    /// <summary>
    /// ��ƼŬ ȿ���� �����ϴ� �� ������Ʈ
    /// </summary>
    public class ParticleEffectView : MonoBehaviour
    {
        [Header("��ƼŬ ����")]
        [SerializeField] private ParticleSystem mainParticleSystem;
        [SerializeField] private ParticleSystemRenderer[] particleRenderers;

        [Header("ȿ�� ����")]
        [SerializeField] private float defaultDuration = 1f;

        private void Awake()
        {
            // ��ƼŬ �ý��� ���� ã��
            if (mainParticleSystem == null)
            {
                mainParticleSystem = GetComponent<ParticleSystem>();
            }

            // ������ ���� ã��
            if (particleRenderers == null || particleRenderers.Length == 0)
            {
                particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>();
            }
        }

        /// <summary>
        /// ��ƼŬ ��Ƽ���� ����
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
        /// ��ƼŬ ���� ����
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
        /// ��ƼŬ ũ�� ����
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
        /// ��ƼŬ ���� ����
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
        /// ��ƼŬ ���� �ð� ����
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
        /// ��ƼŬ ȿ�� ����
        /// </summary>
        public void Play()
        {
            if (mainParticleSystem != null)
            {
                mainParticleSystem.Play();

                // ������ �ð� �� �ڵ� ����
                Destroy(gameObject, GetEffectDuration());
            }
        }

        /// <summary>
        /// ��ƼŬ ȿ�� ����
        /// </summary>
        public void Stop()
        {
            if (mainParticleSystem != null)
            {
                mainParticleSystem.Stop();
            }
        }

        /// <summary>
        /// ȿ�� ���� �ð� ��ȯ
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