using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.View.Effects
{
    /// <summary>
    /// ���ٽ� ����ŷ ȿ���� �����ϴ� �� ������Ʈ
    /// </summary>
    public class StencilMaskView : MonoBehaviour
    {
        [Header("���ٽ� ���̴� ����")]
        [SerializeField] private Material stencilMaskMaterial;    // ����ũ �ۼ� ��Ƽ����
        [SerializeField] private Material stencilUseMaterial;     // ����ũ ��� ��Ƽ����

        [Header("���ٽ� ����")]
        [SerializeField] private int stencilRef = 1;              // ���ٽ� ������

        private List<Renderer> maskWriters = new List<Renderer>(); // ����ũ�� ���� ��������
        private List<Renderer> maskUsers = new List<Renderer>();   // ����ũ�� ����ϴ� ��������

        private void Awake()
        {
            // ���ٽ� ��Ƽ������ ���� ��� �⺻ ��Ƽ���� ����
            if (stencilMaskMaterial == null)
            {
                CreateDefaultStencilMaterials();
            }
        }

        /// <summary>
        /// �⺻ ���ٽ� ��Ƽ���� ����
        /// </summary>
        private void CreateDefaultStencilMaterials()
        {
            // ����ũ �ۼ� ��Ƽ���� ����
            Shader maskShader = Shader.Find("Custom/StencilMask");
            if (maskShader != null)
            {
                stencilMaskMaterial = new Material(maskShader);
                stencilMaskMaterial.SetInt("_StencilRef", stencilRef);
            }

            // ����ũ ��� ��Ƽ���� ����
            Shader useShader = Shader.Find("Custom/StencilUse");
            if (useShader != null)
            {
                stencilUseMaterial = new Material(useShader);
                stencilUseMaterial.SetInt("_StencilRef", stencilRef);
            }
        }

        /// <summary>
        /// ���ٽ� ����ŷ ����
        /// </summary>
        public void SetupStencilMasking(List<GameObject> maskWriterObjects, List<GameObject> maskUserObjects)
        {
            // ���� ���� �ʱ�ȭ
            ClearMaskingSetup();

            // ����ũ �ۼ� ������Ʈ ����
            if (maskWriterObjects != null)
            {
                foreach (var obj in maskWriterObjects)
                {
                    if (obj != null)
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // ���� ��Ƽ���� ���� �ʿ� �� ���⿡ �ڵ� �߰�

                            // ���ٽ� ����ũ ��Ƽ���� ����
                            renderer.material = stencilMaskMaterial;
                            maskWriters.Add(renderer);
                        }
                    }
                }
            }

            // ����ũ ��� ������Ʈ ����
            if (maskUserObjects != null)
            {
                foreach (var obj in maskUserObjects)
                {
                    if (obj != null)
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // ���� ��Ƽ���� ���� �ʿ� �� ���⿡ �ڵ� �߰�

                            // ���ٽ� ��� ��Ƽ���� ����
                            renderer.material = stencilUseMaterial;
                            maskUsers.Add(renderer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ����ŷ ���� �ʱ�ȭ
        /// </summary>
        public void ClearMaskingSetup()
        {
            // TODO: ���� ��Ƽ����� �����ϴ� �ڵ� �ʿ� �� �߰�

            maskWriters.Clear();
            maskUsers.Clear();
        }

        /// <summary>
        /// ���ٽ� ������ ����
        /// </summary>
        public void SetStencilReferenceValue(int value)
        {
            stencilRef = value;

            if (stencilMaskMaterial != null)
            {
                stencilMaskMaterial.SetInt("_StencilRef", stencilRef);
            }

            if (stencilUseMaterial != null)
            {
                stencilUseMaterial.SetInt("_StencilRef", stencilRef);
            }
        }
    }
}