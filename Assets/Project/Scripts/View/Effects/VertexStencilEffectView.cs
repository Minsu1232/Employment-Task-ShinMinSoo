using UnityEngine;
using System.Collections.Generic;
using Project.Scripts.Controller;

namespace Project.Scripts.View
{
    /// <summary>
    /// ���ؽ� ��� ���ٽ� ȿ���� �����ϴ� Ŭ����
    /// </summary>
    public class VertexStencilEffectView
    {
        // ���̴� ��Ƽ����
        private Material wallStencilWriterMaterial;
        private Material blockStencilReaderMaterial;

        // ���ٽ� ����
        private int stencilRefValue = 1;

        // ���� ��Ƽ���� ����
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        // ���� ȿ�� �������� ���
        private Dictionary<GameObject, ClipData> clippedBlocks = new Dictionary<GameObject, ClipData>();

        // Ŭ���� ������ Ŭ����
        private class ClipData
        {
            public GameObject block;
            public Vector3 planePosition;
            public Vector3 planeNormal;

            public ClipData(GameObject block, Vector3 pos, Vector3 normal)
            {
                this.block = block;
                this.planePosition = pos;
                this.planeNormal = normal;
            }
        }

        /// <summary>
        /// �ʱ�ȭ
        /// </summary>
        public void Initialize(Material wallWriter, Material blockReader)
        {
            wallStencilWriterMaterial = wallWriter;
            blockStencilReaderMaterial = blockReader;

            // ��Ƽ������ ���� ��� �⺻ ���̴��� ����
            if (wallStencilWriterMaterial == null)
            {
                Shader wallShader = Shader.Find("Custom/WallStencilWriter");
                if (wallShader != null)
                {
                    wallStencilWriterMaterial = new Material(wallShader);
                }
            }

            if (blockStencilReaderMaterial == null)
            {
                Shader blockShader = Shader.Find("Custom/BlockStencilReader");
                if (blockShader != null)
                {
                    blockStencilReaderMaterial = new Material(blockShader);
                }
            }

            // ���ٽ� ������ ����
            SetStencilRef(1);
        }

        /// <summary>
        /// ���ٽ� ������ ����
        /// </summary>
        public void SetStencilRef(int value)
        {
            stencilRefValue = value;

            // ��Ƽ���� ������ ����
            if (wallStencilWriterMaterial != null)
            {
                wallStencilWriterMaterial.SetInt("_StencilRef", stencilRefValue);
            }

            if (blockStencilReaderMaterial != null)
            {
                blockStencilReaderMaterial.SetInt("_StencilRef", stencilRefValue);
            }
        }

        /// <summary>
        /// ���ٽ� ����ŷ ����
        /// </summary>
       
        public void SetupStencilMasking(List<GameObject> walls, List<GameObject> blocks)
        {
            // ���� ���� ����
            CleanupMaterials(false);

            // �� ������Ʈ�� ���ٽ� Writer ����
            foreach (var wall in walls)
            {
                ApplyStencilWriterToObject(wall);
            }

            // ��Ͽ� ���ٽ� Reader ���� ����
            foreach (var block in blocks)
            {
                PrepareBlockForStencilReading(block);
            }
        }
        /// <summary>
        /// ��Ͽ� ���ٽ� �б� ���� ����
        /// </summary>
        public void PrepareBlockForStencilReading(GameObject blockObj)
        {
            if (blockObj == null || blockStencilReaderMaterial == null) return;

            // ����� ��� ������ ã��
            Renderer[] renderers = blockObj.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // ���� ��Ƽ���� �̹� ���ٽ� ������ ����� ��� ��ŵ
                if (renderer.material.shader.name.Contains("BlockStencilReader"))
                    continue;

                // ���� ��Ƽ���� ����
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials.Add(renderer, renderer.material);
                }

                // ���ٽ� ���� ��Ƽ���� ���� �� ����
                Material stencilMaterial = new Material(blockStencilReaderMaterial);

                // ���ٽ� ������ ����
                stencilMaterial.SetInt("_StencilRef", stencilRefValue);

                // �������� ��Ƽ���� ����
                renderer.material = stencilMaterial;
            }
        }
        /// <summary>
        /// �� ������Ʈ�� ���ٽ� Writer ����
        /// </summary>
        private void ApplyStencilWriterToObject(GameObject obj)
        {
            if (wallStencilWriterMaterial == null) return;

            // Board ���̾��� ��� ��ŵ
            if (obj.layer == LayerMask.NameToLayer("Board"))
                return;

            // ���� �� Renderer�� ã��
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
        /// ��Ͽ� Ŭ���� ȿ�� ����
        /// </summary>
        public void ApplyClippingToBlock(GameObject blockGroup, Vector3 wallPosition, Vector3 wallNormal)
        {
            if (blockGroup == null || blockStencilReaderMaterial == null) return;

            // �̹� Ŭ���� ���� ����̸� ������Ʈ
            if (clippedBlocks.ContainsKey(blockGroup))
            {
                ClipData clipData = clippedBlocks[blockGroup];
                clipData.planePosition = wallPosition;
                clipData.planeNormal = wallNormal;
                // ��� �������� ������Ʈ�� Ŭ���� ���� ����
                UpdateBlockClipping(blockGroup, clipData);
                return;
            }

            // Ŭ���� ������ ����
            ClipData newClipData = new ClipData(blockGroup, wallPosition, wallNormal);
            clippedBlocks.Add(blockGroup, newClipData);

            // ����� ��� ������ ã��
            Renderer[] renderers = blockGroup.GetComponentsInChildren<Renderer>();

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

                // Ŭ���� ��Ƽ���� ����
                Material clipMaterial = new Material(blockStencilReaderMaterial);

                // ----- �ʼ� �Ӽ��� ���� -----

                // �⺻ �ؽ�ó ���� (URP�� _BaseMap ���)
                if (originalMat.HasProperty("_BaseMap"))
                {
                    clipMaterial.SetTexture("_BaseMap", originalMat.GetTexture("_BaseMap"));
                }

                // ���� ���� (URP�� _BaseColor ���)
                if (originalMat.HasProperty("_BaseColor"))
                {
                    clipMaterial.SetColor("_BaseColor", originalMat.GetColor("_BaseColor"));
                }

                // ��Ż�� �� ����
                if (originalMat.HasProperty("_Metallic"))
                {
                    clipMaterial.SetFloat("_Metallic", originalMat.GetFloat("_Metallic"));
                }

                // �������Ͻ� �� ����
                if (originalMat.HasProperty("_Smoothness"))
                {
                    clipMaterial.SetFloat("_Smoothness", originalMat.GetFloat("_Smoothness"));
                }

                // ���� ���� (Surface Type)
                if (originalMat.HasProperty("_Surface"))
                {
                    float surfaceType = originalMat.GetFloat("_Surface");
                    clipMaterial.SetFloat("_Surface", surfaceType);

                    // ���� ���� �Ӽ��� ����
                    if (surfaceType > 0.5f)  // Surface Type�� Transparent
                    {
                        clipMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

                        // ���� ���� ����
                        if (originalMat.HasProperty("_SrcBlend") && originalMat.HasProperty("_DstBlend"))
                        {
                            clipMaterial.SetFloat("_SrcBlend", originalMat.GetFloat("_SrcBlend"));
                            clipMaterial.SetFloat("_DstBlend", originalMat.GetFloat("_DstBlend"));
                        }

                        if (originalMat.HasProperty("_ZWrite"))
                        {
                            clipMaterial.SetFloat("_ZWrite", originalMat.GetFloat("_ZWrite"));
                        }

                        if (originalMat.HasProperty("_Blend"))
                        {
                            float blendMode = originalMat.GetFloat("_Blend");
                            clipMaterial.SetFloat("_Blend", blendMode);

                            // ���� ������Ƽ�ö��� ����
                            if (blendMode == 1)  // 1�� Premultiply
                            {
                                clipMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            }
                        }
                    }
                }

                // ������ ť ����
                clipMaterial.renderQueue = originalMat.renderQueue;

                // ----- Ŭ���� ���� �Ӽ� ���� -----

                // Ŭ���� �Ķ���� ����
                clipMaterial.SetVector("_ClipPlanePos", wallPosition);
                clipMaterial.SetVector("_ClipPlaneNormal", wallNormal);

                // ��Ƽ���� ����
                renderer.material = clipMaterial;
            }
        }

        /// <summary>
        /// ��� Ŭ���� ���� ������Ʈ
        /// </summary>
        private void UpdateBlockClipping(GameObject blockObj, ClipData clipData)
        {
            Renderer[] renderers = blockObj.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // Ŭ���� �Ķ���� ������Ʈ
                renderer.material.SetVector("_ClipPlanePos", clipData.planePosition);
                renderer.material.SetVector("_ClipPlaneNormal", clipData.planeNormal);
            }
        }

        /// <summary>
        /// ���⿡ ���� Ŭ���� ���� ���
        /// </summary>
        // ���� ��� �Լ� ����
        public Vector3 CalculateClipNormal(LaunchDirection direction)
        {
            switch (direction)
            {
                case LaunchDirection.Up:
                    return Vector3.back;     // -Z ���� (���� ���ʿ� ������ �Ʒ���)
                case LaunchDirection.Down:
                    return Vector3.forward;  // +Z ���� (���� �Ʒ��ʿ� ������ ����)
                case LaunchDirection.Left:
                    return Vector3.right;    // +X ���� (���� ���ʿ� ������ ����������)
                case LaunchDirection.Right:
                    return Vector3.left;     // -X ���� (���� �����ʿ� ������ ��������)
                default:
                    return Vector3.up;       // �⺻��
            }
        }

        /// <summary>
        /// ��Ƽ���� ����
        /// </summary>
        public void CleanupMaterials(bool includeClipping = true)
        {
            List<Renderer> restoreList = new List<Renderer>();

            foreach (var pair in originalMaterials)
            {
                if (pair.Key == null) continue;

                bool shouldRestore = true;

                // Ŭ���� ���� ������Ʈ�� ����
                if (!includeClipping)
                {
                    GameObject rendererObj = pair.Key.gameObject;
                    Transform parent = rendererObj.transform;

                    // �θ� ���� Ȯ��
                    while (parent != null)
                    {
                        if (clippedBlocks.ContainsKey(parent.gameObject))
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

            // Ŭ���� ��ü ����
            if (includeClipping)
            {
                clippedBlocks.Clear();
            }
        }

        /// <summary>
        /// ��Ͽ��� Ŭ���� ����
        /// </summary>
        public void RemoveClippingFromBlock(GameObject blockObj)
        {
            if (!clippedBlocks.ContainsKey(blockObj)) return;

            // ����� ��� ������ ����
            Renderer[] renderers = blockObj.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                if (originalMaterials.ContainsKey(renderer))
                {
                    renderer.material = originalMaterials[renderer];
                    originalMaterials.Remove(renderer);
                }
            }

            // Ŭ���� ����Ʈ���� ����
            clippedBlocks.Remove(blockObj);
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        public void Cleanup()
        {
            CleanupMaterials(true);
        }
    }
}