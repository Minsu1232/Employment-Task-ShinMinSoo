using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.View
{
    /// <summary>
    /// ���ٽ� ���� ��� ȿ���� �����ϴ� Ŭ����
    /// </summary>
    public class StencilMaskView
    {
        // ���̴� ��Ƽ����
        private Material wallStencilWriterMaterial;
        private Material blockSlicingMaterial;

        // ���ٽ� ����
        private int stencilRefValue = 1;

        // ���� ��Ƽ���� ����
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        // �����̽� ������
        private class SlicingData
        {
            public GameObject target;
            public List<Material> materials = new List<Material>();
            public float amount;
            public float speed;
            public System.Action onComplete;

            public SlicingData(GameObject obj, float spd, System.Action callback)
            {
                target = obj;
                speed = spd;
                onComplete = callback;
                amount = 0f;
            }
        }

        // �����̽� ���� ������Ʈ ����
        private Dictionary<GameObject, SlicingData> slicingObjects = new Dictionary<GameObject, SlicingData>();

        // MonoBehaviour ���� (�ڷ�ƾ �����)
        private MonoBehaviour coroutineHost;

        /// <summary>
        /// �ʱ�ȭ
        /// </summary>
        public void Initialize(MonoBehaviour host, Material wallWriter, Material blockSlicing = null)
        {
            coroutineHost = host;
            wallStencilWriterMaterial = wallWriter;
            blockSlicingMaterial = blockSlicing;

            // ��Ƽ������ ���� ��� �⺻ ���̴��� ����
            if (blockSlicingMaterial == null)
            {
                Shader slicingShader = Shader.Find("Custom/BlockSlicingShader");
                if (slicingShader != null)
                {
                    blockSlicingMaterial = new Material(slicingShader);
                }
            }

            // ���ٽ� ������ ����
            SetStencilRef(1);
        }

        /// <summary>
        /// ���ٽ� ������ ����
        /// </summary>
        // ���ٽ� ������ ���� �޼��� ����
        public void SetStencilRef(int value)
        {
            stencilRefValue = value;

            // ��Ƽ���� ������ ����
            if (wallStencilWriterMaterial != null)
            {
                wallStencilWriterMaterial.SetInt("_StencilRef", stencilRefValue);
            }

            if (blockSlicingMaterial != null)
            {
                blockSlicingMaterial.SetInt("_StencilRef", stencilRefValue);
            }
        }

        // ���ٽ� ����ŷ ���� �޼��� ����
        public void SetupStencilMasking(List<GameObject> walls, List<GameObject> blocks)
        {
            // ���� ���� ���� (�����̽� ���� ��� ����)
            CleanupMaterials(false);

            // �� ������Ʈ�� ���ٽ� Writer ����
            foreach (var wall in walls)
            {
                ApplyStencilWriterToObject(wall);
            }

            // ��Ͽ��� ���ٽ� ȿ���� �������� ���� (�����̽� �ÿ��� ����)
        }

        /// <summary>
        /// �� ������Ʈ�� ���ٽ� Writer ����
        /// </summary>
        private void ApplyStencilWriterToObject(GameObject obj)
        {
            if (wallStencilWriterMaterial == null) return;

            // ���� �� Renderer�� ã�� (�ڽ� ������Ʈ ����)
            Renderer renderer = obj.GetComponent<Renderer>();

            // �������� ������ WallObject���� ã��
            if (renderer == null)
            {
                WallObject wallObj = obj.GetComponent<WallObject>();
                if (wallObj != null)
                {
                    // WallObject���� ���� ������ ���� ��������
                    renderer = wallObj.GetMainRenderer();
                }
            }

            if (renderer != null)
            {
                // ���� ��Ƽ���� ����
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials.Add(renderer, renderer.material);
                }

                // ���ٽ� Writer ��Ƽ���� �ν��Ͻ� ����
                Material material = new Material(wallStencilWriterMaterial);

                // ���� �ؽ�ó�� ���� ����
                if (originalMaterials[renderer].mainTexture != null)
                {
                    material.mainTexture = originalMaterials[renderer].mainTexture;
                }
                material.color = originalMaterials[renderer].color;

                // ��Ƽ���� ����
                renderer.material = material;
            }
        }
   

        /// <summary>
        /// ��� �����̽� ȿ�� ����
        /// </summary>
        public void StartBlockSlicing(GameObject blockObj, Vector3 sliceDirection, float speed = 1.0f, System.Action onComplete = null)
        {
            if (blockObj == null || blockSlicingMaterial == null || coroutineHost == null) return;

            // �̹� �����̽� ���� ����̸� �ߴ�
            if (slicingObjects.ContainsKey(blockObj))
            {
                return;
            }

            // �����̽� ������ ����
            SlicingData sliceData = new SlicingData(blockObj, speed, onComplete);
            slicingObjects.Add(blockObj, sliceData);

            // ��Ͽ� �����̽� ��Ƽ���� ����
            ApplySlicingMaterialToBlock(blockObj, sliceDirection, sliceData);

            // �����̽� �ִϸ��̼� ����
            coroutineHost.StartCoroutine(AnimateSlicing(blockObj));
        }
        /// <summary>
        /// ��Ͽ� �����̽� ��Ƽ���� ����
        /// </summary>
        private void ApplySlicingMaterialToBlock(GameObject blockObj, Vector3 sliceDirection, SlicingData sliceData)
        {
            // ����� ��� ������ ã��
            Renderer[] renderers = blockObj.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // ���� ��Ƽ���� ����
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials.Add(renderer, renderer.material);
                }

                // ���� ��Ƽ���� ����
                Material originalMat = originalMaterials[renderer];

                // �����̽� ��Ƽ���� �ν��Ͻ� ����
                Material sliceMaterial = new Material(blockSlicingMaterial);

                // ���� �ؽ�ó ����
                if (originalMat.HasProperty("_MainTex") && originalMat.mainTexture != null)
                {
                    sliceMaterial.SetTexture("_MainTex", originalMat.mainTexture);
                }

                // URP �ؽ�ó ���� (�ʿ��� ���)
                if (originalMat.HasProperty("_BaseMap") && originalMat.GetTexture("_BaseMap") != null)
                {
                    sliceMaterial.SetTexture("_MainTex", originalMat.GetTexture("_BaseMap"));
                }

                // ���� ���� ����
                if (originalMat.HasProperty("_Color"))
                {
                    sliceMaterial.SetColor("_Color", originalMat.GetColor("_Color"));
                }

                // URP ���� ���� (�ʿ��� ���)
                if (originalMat.HasProperty("_BaseColor"))
                {
                    sliceMaterial.SetColor("_Color", originalMat.GetColor("_BaseColor"));
                }

                // �����̽� �Ķ���� ����
                sliceMaterial.SetVector("_SliceDirection", new Vector4(sliceDirection.x, sliceDirection.y, sliceDirection.z, 0));
                sliceMaterial.SetFloat("_SliceAmount", 0f);

                // ��Ƽ���� ����
                renderer.material = sliceMaterial;
                sliceData.materials.Add(sliceMaterial);
            }
        }
        /// <summary>
        /// �����̽� �ִϸ��̼�
        /// </summary>
        private IEnumerator AnimateSlicing(GameObject blockObj)
        {
            if (!slicingObjects.ContainsKey(blockObj))
            {
                yield break;
            }

            SlicingData sliceData = slicingObjects[blockObj];

            // �����̽� �ִϸ��̼�
            while (sliceData.amount < 1.0f)
            {
                // ����� �ı��� ��� �ߴ�
                if (blockObj == null)
                {
                    slicingObjects.Remove(blockObj);
                    yield break;
                }

                // �����̽� ����
                sliceData.amount += Time.deltaTime * sliceData.speed;
                sliceData.amount = Mathf.Clamp01(sliceData.amount);

                // ��� ��Ƽ���� ������Ʈ
                foreach (var material in sliceData.materials)
                {
                    if (material != null)
                    {
                        material.SetFloat("_SliceAmount", sliceData.amount);
                    }
                }

                yield return null;
            }

            // �ִϸ��̼� �Ϸ� �� �ݹ� ȣ��
            if (sliceData.onComplete != null)
            {
                sliceData.onComplete.Invoke();
            }

            // �����̽� ��Ͽ��� ����
            slicingObjects.Remove(blockObj);
        }

        /// <summary>
        /// ���⿡ ���� �����̽� ���� ���
        /// </summary>
        public Vector3 CalculateSliceDirection(LaunchDirection direction)
        {
            switch (direction)
            {
                case LaunchDirection.Up:
                    return Vector3.forward;
                case LaunchDirection.Down:
                    return Vector3.back;
                case LaunchDirection.Left:
                    return Vector3.left;
                case LaunchDirection.Right:
                    return Vector3.right;
                default:
                    return Vector3.up;
            }
        }

        /// <summary>
        /// ��Ƽ���� ����
        /// </summary>
        public void CleanupMaterials(bool includeSlicing = true)
        {
            List<Renderer> restoreList = new List<Renderer>();

            foreach (var pair in originalMaterials)
            {
                if (pair.Key == null) continue;

                bool shouldRestore = true;

                // �����̽� ���� ������Ʈ�� ����
                if (!includeSlicing)
                {
                    GameObject rendererObj = pair.Key.gameObject;
                    Transform parent = rendererObj.transform;

                    // �θ� ���� Ȯ��
                    while (parent != null)
                    {
                        if (slicingObjects.ContainsKey(parent.gameObject))
                        {
                            shouldRestore = false;
                            break;
                        }
                        parent = parent.parent;
                    }
                }

                if (shouldRestore)
                {
                    pair.Key.material = pair.Value;
                    restoreList.Add(pair.Key);
                }
            }

            // ������ ������ ����
            foreach (var renderer in restoreList)
            {
                originalMaterials.Remove(renderer);
            }

            // �����̽� ��ü ����
            if (includeSlicing)
            {
                slicingObjects.Clear();
            }
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        public void Cleanup()
        {
            CleanupMaterials(true);

            if (coroutineHost != null)
            {
                coroutineHost.StopAllCoroutines();
            }
        }
    }
}