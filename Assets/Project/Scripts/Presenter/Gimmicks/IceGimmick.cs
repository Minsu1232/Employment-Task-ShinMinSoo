using Project.Scripts.Model;
using UnityEngine;
using DG.Tweening;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 얼음 기믹 프레젠터
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

            // 특정 기믹 데이터로 캐스팅
            if (gimmickData is GimmickIceData)
            {
                iceData = gimmickData as GimmickIceData;
                remainingHits = iceData.Count;

                // 시각적 표현 설정
                SetupVisuals();
            }
            else
            {
                Debug.LogError("IceGimmick에 잘못된 데이터 타입 제공됨");
            }
        }

        private void SetupVisuals()
        {
            // 얼음 효과 생성
            iceEffectObj = new GameObject("IceEffect");
            iceEffectObj.transform.SetParent(targetObject.transform);
            iceEffectObj.transform.localPosition = Vector3.zero;

            // 얼음 오브젝트 생성
            GameObject iceVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            iceVisual.transform.SetParent(iceEffectObj.transform);
            iceVisual.transform.localPosition = Vector3.zero;

            // 블록 크기 기반으로 얼음 크기 조정
            float baseSize = 0.7f;
            iceVisual.transform.localScale = new Vector3(
                baseSize * targetObject.transform.localScale.x,
                0.1f,
                baseSize * targetObject.transform.localScale.z
            );

            // 얼음 머티리얼 생성
            Renderer renderer = iceVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material iceMaterial = new Material(Shader.Find("Standard"));
                iceMaterial.color = new Color(0.7f, 0.9f, 1.0f, 0.7f);
                iceMaterial.SetFloat("_Mode", 3); // Transparent 모드
                iceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                iceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                iceMaterial.SetInt("_ZWrite", 0);
                iceMaterial.DisableKeyword("_ALPHATEST_ON");
                iceMaterial.EnableKeyword("_ALPHABLEND_ON");
                iceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                iceMaterial.renderQueue = 3000;

                renderer.material = iceMaterial;
            }

            // 얼음 레이어 수에 따른 스케일 조정
            iceEffectObj.transform.localScale = Vector3.one * (1 + 0.1f * remainingHits);

            // 얼음 깨짐 파티클 생성
            GameObject particleObj = new GameObject("BreakParticle");
            particleObj.transform.SetParent(iceEffectObj.transform);
            particleObj.transform.localPosition = Vector3.zero;

            breakParticle = particleObj.AddComponent<ParticleSystem>();
            var main = breakParticle.main;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = new Color(0.8f, 0.9f, 1.0f, 0.8f);

            // 파티클 설정
            var emission = breakParticle.emission;
            emission.enabled = false; // 기본적으로 비활성화

            var shape = breakParticle.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 0.1f, 0.5f);
        }

        public override bool OnBlockMove(Vector3 position)
        {
            if (!isActive) return true;

            // 얼음 상태에서는 이동 불가
            if (remainingHits > 0)
            {
                // 움직임 시도 효과 (흔들림)
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

            // 블록 배치 시도 시 얼음 한 겹 깨짐
            DecreaseIce();
        }

        public override bool OnDestroyAttempt()
        {
            if (!isActive) return true;

            // 얼음이 남아있으면 파괴 불가
            return remainingHits <= 0;
        }

        /// <summary>
        /// 얼음 한 겹 감소
        /// </summary>
        public void DecreaseIce()
        {
            if (remainingHits <= 0) return;

            remainingHits--;

            // 얼음 깨짐 효과
            if (breakParticle != null)
            {
                var emission = breakParticle.emission;
                emission.enabled = true;

                // 한번 폭발
                var burst = new ParticleSystem.Burst(0.0f, 20);
                emission.SetBurst(0, burst);

                breakParticle.Play();

                // 1초 후 파티클 중단
                emission.enabled = false;
            }

            // 얼음 크기 감소
            if (iceEffectObj != null)
            {
                iceEffectObj.transform.DOScale(Vector3.one * (1 + 0.1f * remainingHits), 0.3f);
            }

            // 모든 얼음이 깨졌는지 확인
            if (remainingHits <= 0)
            {
                // 얼음 제거
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