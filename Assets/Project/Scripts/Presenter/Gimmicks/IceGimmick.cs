using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// ���� ��� ��������
    /// </summary>
    public class IceGimmick : GimmickPresenter
    {
        private GimmickIceData iceData;
        private int remainingHits;
        private GameObject iceEffectObj;
        private ParticleSystem breakParticle;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Frozen;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // Ư�� ��� �����ͷ� ĳ����
            if (gimmickData is GimmickIceData)
            {
                iceData = gimmickData as GimmickIceData;
                remainingHits = iceData.Count;

                // �ð��� ǥ�� ����
                SetupVisuals();
            }
            else
            {
                Debug.LogError("IceGimmick�� �߸��� ������ Ÿ�� ������");
            }
        }

        private void SetupVisuals()
        {
            // ���� ȿ�� ����
            iceEffectObj = new GameObject("IceEffect");
            iceEffectObj.transform.SetParent(targetObject.transform);
            iceEffectObj.transform.localPosition = Vector3.zero;

            // ���� ������Ʈ ����
            GameObject iceVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            iceVisual.transform.SetParent(iceEffectObj.transform);
            iceVisual.transform.localPosition = Vector3.zero;

            // ��� ũ�� ������� ���� ũ�� ����
            float baseSize = 0.7f;
            iceVisual.transform.localScale = new Vector3(
                baseSize * targetObject.transform.localScale.x,
                0.1f,
                baseSize * targetObject.transform.localScale.z
            );

            // ���� ��Ƽ���� ����
            Renderer renderer = iceVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material iceMaterial = new Material(Shader.Find("Standard"));
                iceMaterial.color = new Color(0.7f, 0.9f, 1.0f, 0.7f);
                iceMaterial.SetFloat("_Mode", 3); // Transparent ���
                iceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                iceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                iceMaterial.SetInt("_ZWrite", 0);
                iceMaterial.DisableKeyword("_ALPHATEST_ON");
                iceMaterial.EnableKeyword("_ALPHABLEND_ON");
                iceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                iceMaterial.renderQueue = 3000;

                renderer.material = iceMaterial;
            }

            // ���� ���̾� ���� ���� ������ ����
            iceEffectObj.transform.localScale = Vector3.one * (1 + 0.1f * remainingHits);

            // ���� ���� ��ƼŬ ����
            GameObject particleObj = new GameObject("BreakParticle");
            particleObj.transform.SetParent(iceEffectObj.transform);
            particleObj.transform.localPosition = Vector3.zero;

            breakParticle = particleObj.AddComponent<ParticleSystem>();
            var main = breakParticle.main;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = new Color(0.8f, 0.9f, 1.0f, 0.8f);

            // ��ƼŬ ����
            var emission = breakParticle.emission;
            emission.enabled = false; // �⺻������ ��Ȱ��ȭ

            var shape = breakParticle.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 0.1f, 0.5f);
        }

        public override bool OnBlockMove(Vector3 position)
        {
            if (!isActive) return true;

            // ���� ���¿����� �̵� �Ұ�
            if (remainingHits > 0)
            {
                // ������ �õ� ȿ�� (��鸲)
                Sequence shakeSequence = DOTween.Sequence();
                shakeSequence.Append(targetObject.transform.DOShakePosition(0.3f, 0.1f, 10, 90));
                shakeSequence.Play();

                return false;
            }

            return true;
        }

        public override void OnBlockPlace(Vector3 position)
        {
            if (!isActive) return;

            // ��� ��ġ �õ� �� ���� �� �� ����
            DecreaseIce();
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // ������ ���������� �ı� �Ұ�
            return remainingHits <= 0;
        }

        /// <summary>
        /// ���� �� �� ����
        /// </summary>
        public void DecreaseIce()
        {
            if (remainingHits <= 0) return;

            remainingHits--;

            // ���� ���� ȿ��
            if (breakParticle != null)
            {
                var emission = breakParticle.emission;
                emission.enabled = true;

                // �ѹ� ����
                var burst = new ParticleSystem.Burst(0.0f, 20);
                emission.SetBurst(0, burst);

                breakParticle.Play();

                // 1�� �� ��ƼŬ �ߴ�
                emission.enabled = false;
            }

            // ���� ũ�� ����
            if (iceEffectObj != null)
            {
                iceEffectObj.transform.DOScale(Vector3.one * (1 + 0.1f * remainingHits), 0.3f);
            }

            // ��� ������ �������� Ȯ��
            if (remainingHits <= 0)
            {
                // ���� ����
                if (iceEffectObj != null)
                {
                    iceEffectObj.transform.DOScale(Vector3.zero, 0.5f)
                        .OnComplete(() => {
                            Destroy(iceEffectObj);
                        });
                }
            }
        }
    }
}