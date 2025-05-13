using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Project.Scripts.View
{
    public class VertexDissolveView : MonoBehaviour
    {
        // 내부 클래스 - 디졸브 정보 저장
        public class DissolveEffectData
        {
            public GameObject targetObject;
            public MeshRenderer renderer;
            public MeshFilter meshFilter;
            public Vector3 contactPoint;
            public Vector3 dissolveDirection;
            public float startTime;
            public float duration;
            public System.Action onComplete;
            public Material originalMaterial;
            public Material dissolveMaterial;
            public bool isActive = false;
        }

        private Dictionary<GameObject, DissolveEffectData> activeEffects = new Dictionary<GameObject, DissolveEffectData>();
        private Material dissolveShaderMaterial;
        private int stencilRefValue = 1;

        public void Initialize(Material dissolveShaderMaterial, int stencilRefValue = 1)
        {
            this.dissolveShaderMaterial = dissolveShaderMaterial;
            this.stencilRefValue = stencilRefValue;
            activeEffects.Clear();
        }

        // 업데이트 메서드 - 실행 중인 모든 디졸브 효과 업데이트
        private void Update()
        {
            List<GameObject> completedEffects = new List<GameObject>();

            foreach (var effect in activeEffects.Values)
            {
                if (effect.isActive)
                {
                    float progress = (Time.time - effect.startTime) / effect.duration;

                    if (progress >= 1.0f)
                    {
                        completedEffects.Add(effect.targetObject);
                        effect.onComplete?.Invoke();
                    }
                    else
                    {
                        effect.dissolveMaterial.SetFloat("_DissolveProgress", progress);
                    }
                }
            }

            // 완료된 효과 제거
            foreach (var obj in completedEffects)
            {
                RemoveEffect(obj);
            }
        }

        // 디졸브 효과 시작
        public void StartDissolveEffect(GameObject targetObject, Vector3 contactPoint, Vector3 dissolveDirection, float duration = 1.0f, System.Action onComplete = null)
        {
            if (targetObject == null || dissolveShaderMaterial == null) return;

            // 이미 실행 중인 효과가 있으면 제거
            if (activeEffects.ContainsKey(targetObject))
            {
                RemoveEffect(targetObject);
            }

            // 메시 렌더러와 메시 필터 가져오기
            MeshRenderer renderer = targetObject.GetComponentInChildren<MeshRenderer>();
            MeshFilter meshFilter = targetObject.GetComponentInChildren<MeshFilter>();

            if (renderer == null || meshFilter == null)
            {
                Debug.LogWarning("Target object must have MeshRenderer and MeshFilter components");
                return;
            }

            // 원본 메시와 머티리얼 저장
            Material originalMaterial = renderer.material;

            // 새 머티리얼 인스턴스 생성 및 설정
            Material dissolveMaterial = new Material(dissolveShaderMaterial);
            dissolveMaterial.SetColor("_Color", originalMaterial.color);
            dissolveMaterial.SetTexture("_MainTex", originalMaterial.mainTexture);
            dissolveMaterial.SetInt("_StencilRef", stencilRefValue);

            // 로컬 공간으로 접촉점 변환
            Vector3 localContactPoint = targetObject.transform.InverseTransformPoint(contactPoint);
            Vector3 localDirection = targetObject.transform.InverseTransformDirection(dissolveDirection.normalized);

            // 디졸브 속성 설정
            dissolveMaterial.SetVector("_DissolveOrigin", localContactPoint);
            dissolveMaterial.SetVector("_DissolveDirection", localDirection);
            dissolveMaterial.SetFloat("_DissolveProgress", 0);
            dissolveMaterial.SetFloat("_DissolveEdgeWidth", 0.1f);
            dissolveMaterial.SetColor("_DissolveEdgeColor", Color.white);

            // 머티리얼 적용
            renderer.material = dissolveMaterial;

            // 효과 정보 저장
            DissolveEffectData effectData = new DissolveEffectData
            {
                targetObject = targetObject,
                renderer = renderer,
                meshFilter = meshFilter,
                contactPoint = contactPoint,
                dissolveDirection = dissolveDirection,
                startTime = Time.time,
                duration = duration,
                onComplete = onComplete,
                originalMaterial = originalMaterial,
                dissolveMaterial = dissolveMaterial,
                isActive = true
            };

            activeEffects.Add(targetObject, effectData);
        }

        // 블록 그룹에 디졸브 효과 적용
        public void ApplyDissolveToBlockGroup(GameObject blockGroup, Vector3 contactPoint, Vector3 dissolveDirection, float duration = 1.0f, System.Action onComplete = null)
        {
            if (blockGroup == null) return;

            // 블록 그룹의 모든 자식 블록 찾기
            BlockObject[] blocks = blockGroup.GetComponentsInChildren<BlockObject>();

            if (blocks.Length == 0)
            {
                Debug.LogWarning("No BlockObjects found in the block group");
                return;
            }

            // 각 블록까지의 거리 계산
            Dictionary<BlockObject, float> blockDistances = new Dictionary<BlockObject, float>();
            foreach (var block in blocks)
            {
                float distance = Vector3.Distance(contactPoint, block.transform.position);
                blockDistances.Add(block, distance);
            }

            // 거리에 따라 블록 정렬 (가까운 것부터)
            BlockObject[] sortedBlocks = new BlockObject[blocks.Length];
            System.Array.Copy(blocks, sortedBlocks, blocks.Length);
            System.Array.Sort(sortedBlocks, (a, b) => blockDistances[a].CompareTo(blockDistances[b]));

            // 각 블록에 시차를 두고 디졸브 효과 적용
            float delayPerBlock = duration * 0.2f;
            float maxDelay = (sortedBlocks.Length - 1) * delayPerBlock;

            for (int i = 0; i < sortedBlocks.Length; i++)
            {
                float delay = i * delayPerBlock;
                float blockDuration = duration - maxDelay + delay;

                // 이펙트 시작 지연
                StartCoroutine(StartDelayedDissolve(sortedBlocks[i].gameObject, contactPoint, dissolveDirection, blockDuration, delay, i == sortedBlocks.Length - 1 ? onComplete : null));
            }
        }

        // 지연된 디졸브 효과 시작
        private IEnumerator StartDelayedDissolve(GameObject targetObject, Vector3 contactPoint, Vector3 dissolveDirection, float duration, float delay, System.Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            StartDissolveEffect(targetObject, contactPoint, dissolveDirection, duration, onComplete);
        }

        // 효과 제거 및 정리
        public void RemoveEffect(GameObject targetObject)
        {
            if (activeEffects.TryGetValue(targetObject, out DissolveEffectData effectData))
            {
                if (effectData.renderer != null)
                {
                    // 원래 머티리얼로 복원
                    effectData.renderer.material = effectData.originalMaterial;
                }

                activeEffects.Remove(targetObject);
            }
        }

        // 모든 효과 정리
        public void Cleanup()
        {
            foreach (var effect in activeEffects.Values)
            {
                if (effect.renderer != null)
                {
                    effect.renderer.material = effect.originalMaterial;
                }
            }

            activeEffects.Clear();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}