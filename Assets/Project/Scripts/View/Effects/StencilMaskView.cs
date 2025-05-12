using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.View;
using Project.Scripts.Controller;

namespace Project.Scripts.View.Effects
{
    /// <summary>
    /// 스텐실 마스킹 효과를 관리하는 뷰 컴포넌트
    /// </summary>
    public class StencilMaskView : MonoBehaviour
    {
        [Header("스텐실 쉐이더 참조")]
        [SerializeField] private Material stencilMaskMaterial;    // 마스크 작성 머티리얼
        [SerializeField] private Material stencilUseMaterial;     // 마스크 사용 머티리얼

        [Header("스텐실 설정")]
        [SerializeField] private int stencilRef = 1;              // 스텐실 참조값
        [SerializeField] private float vertexOffset = 0.01f;      // 버텍스 확장 값

        [Header("디버그 설정")]
        [SerializeField] private bool showDebugMesh = false;      // 디버그 메시 표시 여부

        private List<Renderer> maskWriters = new List<Renderer>(); // 마스크를 쓰는 렌더러들
        private List<Renderer> maskUsers = new List<Renderer>();   // 마스크를 사용하는 렌더러들

        // 원본 머티리얼 저장용 딕셔너리
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        private void Awake()
        {
            // 스텐실 머티리얼이 없는 경우 기본 머티리얼 생성
            if (stencilMaskMaterial == null || stencilUseMaterial == null)
            {
                CreateDefaultStencilMaterials();
            }
        }
        /// <summary>
        /// 뷰 초기화
        /// </summary>
        /// <param name="camera">메인 카메라</param>
        /// <param name="material">스텐실 마스크 재질</param>
        public void Initialize(Camera camera, Material material)
        {
            // 카메라와 머티리얼 설정
            this.stencilMaskMaterial = material;

            // 쿼드 메시 생성
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // 메시 생성 및 설정
            Mesh quadMesh = CreateQuadMesh();
            meshFilter.mesh = quadMesh;

            // 스텐실 마스크 머티리얼 설정
            meshRenderer.material = this.stencilMaskMaterial;

            // 마스크 작성자 목록에 추가
            maskWriters.Add(meshRenderer);

            // 머티리얼에 스텐실 참조값 설정
            if (this.stencilMaskMaterial != null)
            {
                this.stencilMaskMaterial.SetInt("_StencilRef", stencilRef);
            }
        }

        /// <summary>
        /// 쿼드 메시 생성
        /// </summary>
        private Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            float size = 0.79f / 2; // 블록 크기의 절반

            // 정점 (사각형)
            Vector3[] vertices = new Vector3[4]
            {
        new Vector3(-size, 0, -size), // 왼쪽 아래
        new Vector3(size, 0, -size),  // 오른쪽 아래
        new Vector3(size, 0, size),   // 오른쪽 위
        new Vector3(-size, 0, size)   // 왼쪽 위
            };

            // 삼각형 (두 개의 삼각형으로 쿼드 구성)
            int[] triangles = new int[6]
            {
        0, 1, 2,
        0, 2, 3
            };

            // 법선 (위쪽 방향)
            Vector3[] normals = new Vector3[4]
            {
        Vector3.up,
        Vector3.up,
        Vector3.up,
        Vector3.up
            };

            // UV 좌표
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
        /// 기본 스텐실 머티리얼 생성
        /// </summary>
        private void CreateDefaultStencilMaterials()
        {
            // 마스크 작성 머티리얼 생성
            Shader maskShader = Shader.Find("Custom/StencilMask");
            if (maskShader != null)
            {
                stencilMaskMaterial = new Material(maskShader);
                stencilMaskMaterial.SetInt("_StencilRef", stencilRef);
                stencilMaskMaterial.SetFloat("_VertexOffset", vertexOffset);
            }
            else
            {
                Debug.LogError("스텐실 마스크 쉐이더를 찾을 수 없습니다.");
            }

            // 마스크 사용 머티리얼 생성
            Shader useShader = Shader.Find("Custom/StencilUse");
            if (useShader != null)
            {
                stencilUseMaterial = new Material(useShader);
                stencilUseMaterial.SetInt("_StencilRef", stencilRef);
            }
            else
            {
                Debug.LogError("스텐실 사용 쉐이더를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 스텐실 마스킹 설정
        /// </summary>
        public void SetupStencilMasking(List<GameObject> maskWriterObjects, List<GameObject> maskUserObjects)
        {
            // 기존 설정 초기화
            ClearMaskingSetup();

            // 마스크 작성 오브젝트 설정
            if (maskWriterObjects != null)
            {
                foreach (var obj in maskWriterObjects)
                {
                    if (obj != null)
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // 원본 머티리얼 저장
                            if (!originalMaterials.ContainsKey(renderer))
                            {
                                originalMaterials.Add(renderer, renderer.material);
                            }

                            // 스텐실 마스크 머티리얼 적용
                            renderer.material = new Material(stencilMaskMaterial);

                            // 원본 텍스처 및 색상 복사
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

            // 마스크 사용 오브젝트 설정
            if (maskUserObjects != null)
            {
                foreach (var obj in maskUserObjects)
                {
                    if (obj != null)
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // 원본 머티리얼 저장
                            if (!originalMaterials.ContainsKey(renderer))
                            {
                                originalMaterials.Add(renderer, renderer.material);
                            }

                            // 투명도 처리 여부에 따라 다른 쉐이더 사용
                            Material newMaterial;
                            if (originalMaterials[renderer].HasProperty("_Mode") &&
                                originalMaterials[renderer].GetFloat("_Mode") > 0) // 투명 재질
                            {
                                newMaterial = new Material(Shader.Find("Custom/StencilUseTransparent"));
                            }
                            else // 불투명 재질
                            {
                                newMaterial = new Material(stencilUseMaterial);
                            }

                            // 원본 텍스처 및 색상 복사
                            if (originalMaterials[renderer].mainTexture != null)
                            {
                                newMaterial.mainTexture = originalMaterials[renderer].mainTexture;
                            }
                            newMaterial.color = originalMaterials[renderer].color;
                            newMaterial.SetInt("_StencilRef", stencilRef);

                            // 스텐실 사용 머티리얼 적용
                            renderer.material = newMaterial;
                            maskUsers.Add(renderer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 마스킹 설정 초기화
        /// </summary>
        public void ClearMaskingSetup()
        {
            // 원본 머티리얼로 복원
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
        /// 스텐실 참조값 변경
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

            // 이미 적용된 머티리얼에도 새 참조값 적용
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
        /// 보드 영역에 대한 스텐실 마스크 메시 생성
        /// </summary>
        public void CreateBoardStencilMask(Dictionary<(int x, int y), BoardBlockObject> boardBlockDic, float blockDistance)
        {
            if (boardBlockDic.Count == 0) return;

            // 스텐실 마스크 오브젝트 생성
            GameObject maskObj = new GameObject("BoardStencilMask");
            maskObj.transform.SetParent(transform);

            MeshFilter meshFilter = maskObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = maskObj.AddComponent<MeshRenderer>();

            // 스텐실 마스크 머티리얼 설정
            meshRenderer.material = stencilMaskMaterial;

            // 보드 영역 메시 생성
            Mesh mesh = CreateBoardMesh(boardBlockDic, blockDistance);
            meshFilter.mesh = mesh;

            // 디버그 모드 설정
            if (showDebugMesh)
            {
                meshRenderer.material = new Material(Shader.Find("Standard"));
                meshRenderer.material.color = new Color(1, 1, 0, 0.5f);
            }

            // 마스크 작성자 목록에 추가
            maskWriters.Add(meshRenderer);
        }

        /// <summary>
        /// 보드 영역 메시 생성
        /// </summary>
        private Mesh CreateBoardMesh(Dictionary<(int x, int y), BoardBlockObject> boardBlockDic, float blockDistance)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            float yPos = 0.01f; // 약간 올려서 겹침 방지
            float halfSize = blockDistance * 0.5f;

            int vertIndex = 0;

            // 각 보드 블록 위치에 대해 쿼드 생성
            foreach (var pos in boardBlockDic.Keys)
            {
                float x = pos.x * blockDistance;
                float z = pos.y * blockDistance;

                // 쿼드 정점 (사각형)
                vertices.Add(new Vector3(x - halfSize, yPos, z - halfSize)); // 왼쪽 아래
                vertices.Add(new Vector3(x + halfSize, yPos, z - halfSize)); // 오른쪽 아래
                vertices.Add(new Vector3(x + halfSize, yPos, z + halfSize)); // 오른쪽 위
                vertices.Add(new Vector3(x - halfSize, yPos, z + halfSize)); // 왼쪽 위

                // 법선 (위쪽 방향)
                for (int i = 0; i < 4; i++)
                {
                    normals.Add(Vector3.up);
                }

                // 삼각형 (두 개의 삼각형으로 쿼드 구성)
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
        /// 자원 정리
        /// </summary>
        private void OnDestroy()
        {
            ClearMaskingSetup();

            // 스텐실 머티리얼 정리
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