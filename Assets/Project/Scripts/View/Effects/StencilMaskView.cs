using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.View;
using Project.Scripts.Controller;

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
        [SerializeField] private float vertexOffset = 0.01f;      // ���ؽ� Ȯ�� ��

        [Header("����� ����")]
        [SerializeField] private bool showDebugMesh = false;      // ����� �޽� ǥ�� ����

        private List<Renderer> maskWriters = new List<Renderer>(); // ����ũ�� ���� ��������
        private List<Renderer> maskUsers = new List<Renderer>();   // ����ũ�� ����ϴ� ��������

        // ���� ��Ƽ���� ����� ��ųʸ�
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        private void Awake()
        {
            // ���ٽ� ��Ƽ������ ���� ��� �⺻ ��Ƽ���� ����
            if (stencilMaskMaterial == null || stencilUseMaterial == null)
            {
                CreateDefaultStencilMaterials();
            }
        }
        /// <summary>
        /// �� �ʱ�ȭ
        /// </summary>
        /// <param name="camera">���� ī�޶�</param>
        /// <param name="material">���ٽ� ����ũ ����</param>
        public void Initialize(Camera camera, Material material)
        {
            // ī�޶�� ��Ƽ���� ����
            this.stencilMaskMaterial = material;

            // ���� �޽� ����
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // �޽� ���� �� ����
            Mesh quadMesh = CreateQuadMesh();
            meshFilter.mesh = quadMesh;

            // ���ٽ� ����ũ ��Ƽ���� ����
            meshRenderer.material = this.stencilMaskMaterial;

            // ����ũ �ۼ��� ��Ͽ� �߰�
            maskWriters.Add(meshRenderer);

            // ��Ƽ���� ���ٽ� ������ ����
            if (this.stencilMaskMaterial != null)
            {
                this.stencilMaskMaterial.SetInt("_StencilRef", stencilRef);
            }
        }

        /// <summary>
        /// ���� �޽� ����
        /// </summary>
        private Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            float size = 0.79f / 2; // ��� ũ���� ����

            // ���� (�簢��)
            Vector3[] vertices = new Vector3[4]
            {
        new Vector3(-size, 0, -size), // ���� �Ʒ�
        new Vector3(size, 0, -size),  // ������ �Ʒ�
        new Vector3(size, 0, size),   // ������ ��
        new Vector3(-size, 0, size)   // ���� ��
            };

            // �ﰢ�� (�� ���� �ﰢ������ ���� ����)
            int[] triangles = new int[6]
            {
        0, 1, 2,
        0, 2, 3
            };

            // ���� (���� ����)
            Vector3[] normals = new Vector3[4]
            {
        Vector3.up,
        Vector3.up,
        Vector3.up,
        Vector3.up
            };

            // UV ��ǥ
            Vector2[] uv = new Vector2[4]
            {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(0, 1)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.RecalculateBounds();

            return mesh;
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
                stencilMaskMaterial.SetFloat("_VertexOffset", vertexOffset);
            }
            else
            {
                Debug.LogError("���ٽ� ����ũ ���̴��� ã�� �� �����ϴ�.");
            }

            // ����ũ ��� ��Ƽ���� ����
            Shader useShader = Shader.Find("Custom/StencilUse");
            if (useShader != null)
            {
                stencilUseMaterial = new Material(useShader);
                stencilUseMaterial.SetInt("_StencilRef", stencilRef);
            }
            else
            {
                Debug.LogError("���ٽ� ��� ���̴��� ã�� �� �����ϴ�.");
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
                            // ���� ��Ƽ���� ����
                            if (!originalMaterials.ContainsKey(renderer))
                            {
                                originalMaterials.Add(renderer, renderer.material);
                            }

                            // ���ٽ� ����ũ ��Ƽ���� ����
                            renderer.material = new Material(stencilMaskMaterial);

                            // ���� �ؽ�ó �� ���� ����
                            if (originalMaterials[renderer].mainTexture != null)
                            {
                                renderer.material.mainTexture = originalMaterials[renderer].mainTexture;
                            }
                            renderer.material.color = originalMaterials[renderer].color;

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
                            // ���� ��Ƽ���� ����
                            if (!originalMaterials.ContainsKey(renderer))
                            {
                                originalMaterials.Add(renderer, renderer.material);
                            }

                            // ���� ó�� ���ο� ���� �ٸ� ���̴� ���
                            Material newMaterial;
                            if (originalMaterials[renderer].HasProperty("_Mode") &&
                                originalMaterials[renderer].GetFloat("_Mode") > 0) // ���� ����
                            {
                                newMaterial = new Material(Shader.Find("Custom/StencilUseTransparent"));
                            }
                            else // ������ ����
                            {
                                newMaterial = new Material(stencilUseMaterial);
                            }

                            // ���� �ؽ�ó �� ���� ����
                            if (originalMaterials[renderer].mainTexture != null)
                            {
                                newMaterial.mainTexture = originalMaterials[renderer].mainTexture;
                            }
                            newMaterial.color = originalMaterials[renderer].color;
                            newMaterial.SetInt("_StencilRef", stencilRef);

                            // ���ٽ� ��� ��Ƽ���� ����
                            renderer.material = newMaterial;
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
            // ���� ��Ƽ����� ����
            foreach (var renderer in maskWriters)
            {
                if (renderer != null && originalMaterials.ContainsKey(renderer))
                {
                    renderer.material = originalMaterials[renderer];
                }
            }

            foreach (var renderer in maskUsers)
            {
                if (renderer != null && originalMaterials.ContainsKey(renderer))
                {
                    renderer.material = originalMaterials[renderer];
                }
            }

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

            // �̹� ����� ��Ƽ���󿡵� �� ������ ����
            foreach (var renderer in maskWriters)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetInt("_StencilRef", stencilRef);
                }
            }

            foreach (var renderer in maskUsers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetInt("_StencilRef", stencilRef);
                }
            }
        }

        /// <summary>
        /// ���� ������ ���� ���ٽ� ����ũ �޽� ����
        /// </summary>
        public void CreateBoardStencilMask(Dictionary<(int x, int y), BoardBlockObject> boardBlockDic, float blockDistance)
        {
            if (boardBlockDic.Count == 0) return;

            // ���ٽ� ����ũ ������Ʈ ����
            GameObject maskObj = new GameObject("BoardStencilMask");
            maskObj.transform.SetParent(transform);

            MeshFilter meshFilter = maskObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = maskObj.AddComponent<MeshRenderer>();

            // ���ٽ� ����ũ ��Ƽ���� ����
            meshRenderer.material = stencilMaskMaterial;

            // ���� ���� �޽� ����
            Mesh mesh = CreateBoardMesh(boardBlockDic, blockDistance);
            meshFilter.mesh = mesh;

            // ����� ��� ����
            if (showDebugMesh)
            {
                meshRenderer.material = new Material(Shader.Find("Standard"));
                meshRenderer.material.color = new Color(1, 1, 0, 0.5f);
            }

            // ����ũ �ۼ��� ��Ͽ� �߰�
            maskWriters.Add(meshRenderer);
        }

        /// <summary>
        /// ���� ���� �޽� ����
        /// </summary>
        private Mesh CreateBoardMesh(Dictionary<(int x, int y), BoardBlockObject> boardBlockDic, float blockDistance)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            float yPos = 0.01f; // �ణ �÷��� ��ħ ����
            float halfSize = blockDistance * 0.5f;

            int vertIndex = 0;

            // �� ���� ��� ��ġ�� ���� ���� ����
            foreach (var pos in boardBlockDic.Keys)
            {
                float x = pos.x * blockDistance;
                float z = pos.y * blockDistance;

                // ���� ���� (�簢��)
                vertices.Add(new Vector3(x - halfSize, yPos, z - halfSize)); // ���� �Ʒ�
                vertices.Add(new Vector3(x + halfSize, yPos, z - halfSize)); // ������ �Ʒ�
                vertices.Add(new Vector3(x + halfSize, yPos, z + halfSize)); // ������ ��
                vertices.Add(new Vector3(x - halfSize, yPos, z + halfSize)); // ���� ��

                // ���� (���� ����)
                for (int i = 0; i < 4; i++)
                {
                    normals.Add(Vector3.up);
                }

                // �ﰢ�� (�� ���� �ﰢ������ ���� ����)
                triangles.Add(vertIndex);
                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex + 2);

                triangles.Add(vertIndex);
                triangles.Add(vertIndex + 2);
                triangles.Add(vertIndex + 3);

                vertIndex += 4;
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// �ڿ� ����
        /// </summary>
        private void OnDestroy()
        {
            ClearMaskingSetup();

            // ���ٽ� ��Ƽ���� ����
            if (stencilMaskMaterial != null)
            {
                Destroy(stencilMaskMaterial);
            }

            if (stencilUseMaterial != null)
            {
                Destroy(stencilUseMaterial);
            }
        }
    }
}