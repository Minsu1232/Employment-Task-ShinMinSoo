using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Project.Scripts.View
{
    public class VertexDissolveView : MonoBehaviour
    {
        // ���� Ŭ���� - ������ ���� ����
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

        // ������Ʈ �޼��� - ���� ���� ��� ������ ȿ�� ������Ʈ
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

            // �Ϸ�� ȿ�� ����
            foreach (var obj in completedEffects)
            {
                RemoveEffect(obj);
            }
        }

        // ������ ȿ�� ����
        public void StartDissolveEffect(GameObject targetObject, Vector3 contactPoint, Vector3 dissolveDirection, float duration = 1.0f, System.Action onComplete = null)
        {
            if (targetObject == null || dissolveShaderMaterial == null) return;

            // �̹� ���� ���� ȿ���� ������ ����
            if (activeEffects.ContainsKey(targetObject))
            {
                RemoveEffect(targetObject);
            }

            // �޽� �������� �޽� ���� ��������
            MeshRenderer renderer = targetObject.GetComponentInChildren<MeshRenderer>();
            MeshFilter meshFilter = targetObject.GetComponentInChildren<MeshFilter>();

            if (renderer == null || meshFilter == null)
            {
                Debug.LogWarning("Target object must have MeshRenderer and MeshFilter components");
                return;
            }

            // ���� �޽ÿ� ��Ƽ���� ����
            Material originalMaterial = renderer.material;

            // �� ��Ƽ���� �ν��Ͻ� ���� �� ����
            Material dissolveMaterial = new Material(dissolveShaderMaterial);
            dissolveMaterial.SetColor("_Color", originalMaterial.color);
            dissolveMaterial.SetTexture("_MainTex", originalMaterial.mainTexture);
            dissolveMaterial.SetInt("_StencilRef", stencilRefValue);

            // ���� �������� ������ ��ȯ
            Vector3 localContactPoint = targetObject.transform.InverseTransformPoint(contactPoint);
            Vector3 localDirection = targetObject.transform.InverseTransformDirection(dissolveDirection.normalized);

            // ������ �Ӽ� ����
            dissolveMaterial.SetVector("_DissolveOrigin", localContactPoint);
            dissolveMaterial.SetVector("_DissolveDirection", localDirection);
            dissolveMaterial.SetFloat("_DissolveProgress", 0);
            dissolveMaterial.SetFloat("_DissolveEdgeWidth", 0.1f);
            dissolveMaterial.SetColor("_DissolveEdgeColor", Color.white);

            // ��Ƽ���� ����
            renderer.material = dissolveMaterial;

            // ȿ�� ���� ����
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

        // ��� �׷쿡 ������ ȿ�� ����
        public void ApplyDissolveToBlockGroup(GameObject blockGroup, Vector3 contactPoint, Vector3 dissolveDirection, float duration = 1.0f, System.Action onComplete = null)
        {
            if (blockGroup == null) return;

            // ��� �׷��� ��� �ڽ� ��� ã��
            BlockObject[] blocks = blockGroup.GetComponentsInChildren<BlockObject>();

            if (blocks.Length == 0)
            {
                Debug.LogWarning("No BlockObjects found in the block group");
                return;
            }

            // �� ��ϱ����� �Ÿ� ���
            Dictionary<BlockObject, float> blockDistances = new Dictionary<BlockObject, float>();
            foreach (var block in blocks)
            {
                float distance = Vector3.Distance(contactPoint, block.transform.position);
                blockDistances.Add(block, distance);
            }

            // �Ÿ��� ���� ��� ���� (����� �ͺ���)
            BlockObject[] sortedBlocks = new BlockObject[blocks.Length];
            System.Array.Copy(blocks, sortedBlocks, blocks.Length);
            System.Array.Sort(sortedBlocks, (a, b) => blockDistances[a].CompareTo(blockDistances[b]));

            // �� ��Ͽ� ������ �ΰ� ������ ȿ�� ����
            float delayPerBlock = duration * 0.2f;
            float maxDelay = (sortedBlocks.Length - 1) * delayPerBlock;

            for (int i = 0; i < sortedBlocks.Length; i++)
            {
                float delay = i * delayPerBlock;
                float blockDuration = duration - maxDelay + delay;

                // ����Ʈ ���� ����
                StartCoroutine(StartDelayedDissolve(sortedBlocks[i].gameObject, contactPoint, dissolveDirection, blockDuration, delay, i == sortedBlocks.Length - 1 ? onComplete : null));
            }
        }

        // ������ ������ ȿ�� ����
        private IEnumerator StartDelayedDissolve(GameObject targetObject, Vector3 contactPoint, Vector3 dissolveDirection, float duration, float delay, System.Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            StartDissolveEffect(targetObject, contactPoint, dissolveDirection, duration, onComplete);
        }

        // ȿ�� ���� �� ����
        public void RemoveEffect(GameObject targetObject)
        {
            if (activeEffects.TryGetValue(targetObject, out DissolveEffectData effectData))
            {
                if (effectData.renderer != null)
                {
                    // ���� ��Ƽ����� ����
                    effectData.renderer.material = effectData.originalMaterial;
                }

                activeEffects.Remove(targetObject);
            }
        }

        // ��� ȿ�� ����
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