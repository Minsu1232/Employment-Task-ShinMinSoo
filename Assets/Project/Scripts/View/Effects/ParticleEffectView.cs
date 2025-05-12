using UnityEngine;
using Project.Scripts.View.Effects;

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
        [SerializeField] private float defaultScale = 1f;
        [SerializeField] private Color defaultColor = Color.white;

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
                var startColor = main.startColor;
                startColor.color = color;
                main.startColor = startColor;
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
                var startSize = main.startSize;
                startSize.constant = size;
                main.startSize = startSize;
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
        /// ��ƼŬ ȿ�� ���
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
        /// ��ƼŬ ȿ���� Ư�� ��ġ���� ���
        /// </summary>
        public void PlayAtPosition(Vector3 position)
        {
            transform.position = position;
            Play();
        }

        /// <summary>
        /// ��ƼŬ ȿ���� Ư�� ����� ũ��� ���
        /// </summary>
        public void PlayWithProperties(Color color, float size, Vector3 position)
        {
            SetColor(color);
            SetSize(size);
            PlayAtPosition(position);
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
                var main = mainParticleSystem.main;
                return main.duration + main.startLifetime.constant;
            }
            return defaultDuration;
        }

        /// <summary>
        /// ��ƼŬ ������ ����
        /// </summary>
        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        /// <summary>
        /// ��ƼŬ �߻�� ����
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
        /// �� ��ƼŬ ȿ�� �ν��Ͻ� ����
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