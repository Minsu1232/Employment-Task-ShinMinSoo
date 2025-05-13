using UnityEngine;
using System.Collections.Generic;
using Project.Scripts.Controller;

namespace Project.Scripts.View
{
    /// <summary>
    /// 버텍스 기반 스텐실 효과를 관리하는 클래스
    /// </summary>
    public class VertexStencilEffectView
    {
        // 셰이더 머티리얼
        private Material wallStencilWriterMaterial;
        private Material blockStencilReaderMaterial;

        // 스텐실 설정
        private int stencilRefValue = 1;

        // 원본 머티리얼 저장
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        // 현재 효과 적용중인 블록
        private Dictionary<GameObject, ClipData> clippedBlocks = new Dictionary<GameObject, ClipData>();

        // 클리핑 데이터 클래스
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
        /// 초기화
        /// </summary>
        public void Initialize(Material wallWriter, Material blockReader)
        {
            wallStencilWriterMaterial = wallWriter;
            blockStencilReaderMaterial = blockReader;

            // 머티리얼이 없는 경우 기본 셰이더로 생성
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

            // 스텐실 참조값 설정
            SetStencilRef(1);
        }

        /// <summary>
        /// 스텐실 참조값 설정
        /// </summary>
        public void SetStencilRef(int value)
        {
            stencilRefValue = value;

            // 머티리얼에 참조값 설정
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
        /// 스텐실 마스킹 설정
        /// </summary>
       
        public void SetupStencilMasking(List<GameObject> walls, List<GameObject> blocks)
        {
            // 기존 설정 정리
            CleanupMaterials(false);

            // 벽 오브젝트에 스텐실 Writer 적용
            foreach (var wall in walls)
            {
                ApplyStencilWriterToObject(wall);
            }

            // 블록에 스텐실 Reader 설정 적용
            foreach (var block in blocks)
            {
                PrepareBlockForStencilReading(block);
            }
        }
        /// <summary>
        /// 블록에 스텐실 읽기 설정 적용
        /// </summary>
        public void PrepareBlockForStencilReading(GameObject blockObj)
        {
            if (blockObj == null || blockStencilReaderMaterial == null) return;

            // 블록의 모든 렌더러 찾기
            Renderer[] renderers = blockObj.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // 원본 머티리얼에 이미 스텐실 설정이 적용된 경우 스킵
                if (renderer.material.shader.name.Contains("BlockStencilReader"))
                    continue;

                // 원본 머티리얼 저장
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials.Add(renderer, renderer.material);
                }

                // 스텐실 리더 머티리얼 생성 및 적용
                Material stencilMaterial = new Material(blockStencilReaderMaterial);

                // 스텐실 참조값 설정
                stencilMaterial.SetInt("_StencilRef", stencilRefValue);

                // 렌더러에 머티리얼 적용
                renderer.material = stencilMaterial;
            }
        }
        /// <summary>
        /// 벽 오브젝트에 스텐실 Writer 적용
        /// </summary>
        private void ApplyStencilWriterToObject(GameObject obj)
        {
            if (wallStencilWriterMaterial == null) return;

            // Board 레이어인 경우 스킵
            if (obj.layer == LayerMask.NameToLayer("Board"))
                return;

            // 메인 벽 Renderer만 찾기
            Renderer renderer = obj.GetComponent<Renderer>();

            // 렌더러가 없으면 WallObject에서 찾기
            if (renderer == null)
            {
                WallObject wallObj = obj.GetComponent<WallObject>();
                if (wallObj != null)
                {
                    // WallObject에서 메인 렌더러 참조 가져오기
                    renderer = wallObj.GetMainRenderer();
                }
            }

            if (renderer != null)
            {
                // 원본 머티리얼 저장
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials.Add(renderer, renderer.material);
                }

                // 스텐실 Writer 머티리얼 인스턴스 생성
                Material material = new Material(wallStencilWriterMaterial);

                // 원본 텍스처와 색상 복사
                if (originalMaterials[renderer].mainTexture != null)
                {
                    material.mainTexture = originalMaterials[renderer].mainTexture;
                }
                material.color = originalMaterials[renderer].color;

                // 머티리얼 적용
                renderer.material = material;
            }
        }

        /// <summary>
        /// 블록에 클리핑 효과 적용
        /// </summary>
        public void ApplyClippingToBlock(GameObject blockGroup, Vector3 wallPosition, Vector3 wallNormal)
        {
            if (blockGroup == null || blockStencilReaderMaterial == null) return;

            // 이미 클리핑 중인 블록이면 업데이트
            if (clippedBlocks.ContainsKey(blockGroup))
            {
                ClipData clipData = clippedBlocks[blockGroup];
                clipData.planePosition = wallPosition;
                clipData.planeNormal = wallNormal;
                // 모든 렌더러에 업데이트된 클리핑 설정 적용
                UpdateBlockClipping(blockGroup, clipData);
                return;
            }

            // 클리핑 데이터 생성
            ClipData newClipData = new ClipData(blockGroup, wallPosition, wallNormal);
            clippedBlocks.Add(blockGroup, newClipData);

            // 블록의 모든 렌더러 찾기
            Renderer[] renderers = blockGroup.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // 원본 머티리얼 저장
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials.Add(renderer, renderer.material);
                }

                // 원본 머티리얼 참조
                Material originalMat = originalMaterials[renderer];

                // 클리핑 머티리얼 생성
                Material clipMaterial = new Material(blockStencilReaderMaterial);

                // ----- 필수 속성만 복사 -----

                // 기본 텍스처 복사 (URP는 _BaseMap 사용)
                if (originalMat.HasProperty("_BaseMap"))
                {
                    clipMaterial.SetTexture("_BaseMap", originalMat.GetTexture("_BaseMap"));
                }

                // 색상 복사 (URP는 _BaseColor 사용)
                if (originalMat.HasProperty("_BaseColor"))
                {
                    clipMaterial.SetColor("_BaseColor", originalMat.GetColor("_BaseColor"));
                }

                // 메탈릭 값 복사
                if (originalMat.HasProperty("_Metallic"))
                {
                    clipMaterial.SetFloat("_Metallic", originalMat.GetFloat("_Metallic"));
                }

                // 스무스니스 값 복사
                if (originalMat.HasProperty("_Smoothness"))
                {
                    clipMaterial.SetFloat("_Smoothness", originalMat.GetFloat("_Smoothness"));
                }

                // 투명도 설정 (Surface Type)
                if (originalMat.HasProperty("_Surface"))
                {
                    float surfaceType = originalMat.GetFloat("_Surface");
                    clipMaterial.SetFloat("_Surface", surfaceType);

                    // 투명도 관련 속성들 설정
                    if (surfaceType > 0.5f)  // Surface Type이 Transparent
                    {
                        clipMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

                        // 블렌딩 설정 복사
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

                            // 알파 프리멀티플라이 설정
                            if (blendMode == 1)  // 1은 Premultiply
                            {
                                clipMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            }
                        }
                    }
                }

                // 렌더링 큐 설정
                clipMaterial.renderQueue = originalMat.renderQueue;

                // ----- 클리핑 관련 속성 설정 -----

                // 클리핑 파라미터 설정
                clipMaterial.SetVector("_ClipPlanePos", wallPosition);
                clipMaterial.SetVector("_ClipPlaneNormal", wallNormal);

                // 머티리얼 적용
                renderer.material = clipMaterial;
            }
        }

        /// <summary>
        /// 블록 클리핑 설정 업데이트
        /// </summary>
        private void UpdateBlockClipping(GameObject blockObj, ClipData clipData)
        {
            Renderer[] renderers = blockObj.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // 클리핑 파라미터 업데이트
                renderer.material.SetVector("_ClipPlanePos", clipData.planePosition);
                renderer.material.SetVector("_ClipPlaneNormal", clipData.planeNormal);
            }
        }

        /// <summary>
        /// 방향에 따른 클리핑 방향 계산
        /// </summary>
        // 방향 계산 함수 수정
        public Vector3 CalculateClipNormal(LaunchDirection direction)
        {
            switch (direction)
            {
                case LaunchDirection.Up:
                    return Vector3.back;     // -Z 방향 (벽이 위쪽에 있으면 아래로)
                case LaunchDirection.Down:
                    return Vector3.forward;  // +Z 방향 (벽이 아래쪽에 있으면 위로)
                case LaunchDirection.Left:
                    return Vector3.right;    // +X 방향 (벽이 왼쪽에 있으면 오른쪽으로)
                case LaunchDirection.Right:
                    return Vector3.left;     // -X 방향 (벽이 오른쪽에 있으면 왼쪽으로)
                default:
                    return Vector3.up;       // 기본값
            }
        }

        /// <summary>
        /// 머티리얼 정리
        /// </summary>
        public void CleanupMaterials(bool includeClipping = true)
        {
            List<Renderer> restoreList = new List<Renderer>();

            foreach (var pair in originalMaterials)
            {
                if (pair.Key == null) continue;

                bool shouldRestore = true;

                // 클리핑 중인 오브젝트는 제외
                if (!includeClipping)
                {
                    GameObject rendererObj = pair.Key.gameObject;
                    Transform parent = rendererObj.transform;

                    // 부모 계층 확인
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

            // 복원된 렌더러 제거
            foreach (var renderer in restoreList)
            {
                originalMaterials.Remove(renderer);
            }

            // 클리핑 객체 정리
            if (includeClipping)
            {
                clippedBlocks.Clear();
            }
        }

        /// <summary>
        /// 블록에서 클리핑 제거
        /// </summary>
        public void RemoveClippingFromBlock(GameObject blockObj)
        {
            if (!clippedBlocks.ContainsKey(blockObj)) return;

            // 블록의 모든 렌더러 복원
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

            // 클리핑 리스트에서 제거
            clippedBlocks.Remove(blockObj);
        }

        /// <summary>
        /// 완전히 정리
        /// </summary>
        public void Cleanup()
        {
            CleanupMaterials(true);
        }
    }
}