using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.View
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
        [SerializeField] private Color defaultColor = Color.white;

        // �ʱ�ȭ �޼���
        public void Initialize(Material material = null, Color? color = null)
        {
            // ��ƼŬ �ý��� ���� Ȯ��
            if (mainParticleSystem == null)
                mainParticleSystem = GetComponent<ParticleSystem>();

            // ������ ���� Ȯ��
            if (particleRenderers == null || particleRenderers.Length == 0)
                particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>();

            // ���� ����
            if (material != null)
                SetMaterial(material);

            // ���� ����
            if (color.HasValue)
                SetColor(color.Value);
        }

        /// <summary>
        /// ��ƼŬ ���� ����
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
        /// ��ƼŬ ���� ����
        /// </summary>
        public void SetColor(Color color)
        {
            if (mainParticleSystem == null) return;

            var main = mainParticleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
        }

        /// <summary>
        /// ��ƼŬ ũ�� ����
        /// </summary>
        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        /// <summary>
        /// ��ƼŬ ���� ����
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
        /// ��ƼŬ ȿ�� ���
        /// </summary>
        public void Play(float duration = 0f)
        {
            if (mainParticleSystem == null) return;

            // ���� �ð� ����
            if (duration > 0)
            {
                var main = mainParticleSystem.main;
                main.duration = duration;
            }

            // ��ƼŬ ���
            mainParticleSystem.Play();

            // ������ �ð� �� �ڵ� ����
            float actualDuration = duration > 0 ? duration : GetEffectDuration();
            Destroy(gameObject, actualDuration + 0.5f); // ���� �ð� �߰�
        }

        /// <summary>
        /// ��ƼŬ ȿ���� Ư�� ��ġ���� ���
        /// </summary>
        public void PlayAtPosition(Vector3 position, float duration = 0f)
        {
            transform.position = position;
            Play(duration);
        }

        /// <summary>
        /// ��ƼŬ ȿ�� ����
        /// </summary>
        public void Stop()
        {
            if (mainParticleSystem != null)
                mainParticleSystem.Stop();
        }

        /// <summary>
        /// ȿ�� ���� �ð� ��ȯ
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