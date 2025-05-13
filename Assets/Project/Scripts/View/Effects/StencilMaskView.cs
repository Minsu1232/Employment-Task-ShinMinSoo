using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.View
{
    /// <summary>
    /// 스텐실 버퍼 기반 효과를 관리하는 클래스
    /// </summary>
    public class StencilMaskView
    {
        // 셰이더 머티리얼
        private Material wallStencilWriterMaterial;
        private Material blockSlicingMaterial;

        // 스텐실 설정
        private int stencilRefValue = 1;

        // 원본 머티리얼 저장
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        // 슬라이싱 데이터
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

        // 슬라이싱 중인 오브젝트 추적
        private Dictionary<GameObject, SlicingData> slicingObjects = new Dictionary<GameObject, SlicingData>();

        // MonoBehaviour 참조 (코루틴 실행용)
        private MonoBehaviour coroutineHost;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(MonoBehaviour host, Material wallWriter, Material blockSlicing = null)
        {
            coroutineHost = host;
            wallStencilWriterMaterial = wallWriter;
            blockSlicingMaterial = blockSlicing;

            // 머티리얼이 없는 경우 기본 셰이더로 생성
            if (blockSlicingMaterial == null)
            {
                Shader slicingShader = Shader.Find("Custom/BlockSlicingShader");
                if (slicingShader != null)
                {
                    blockSlicingMaterial = new Material(slicingShader);
                }
            }

            // 스텐실 참조값 설정
            SetStencilRef(1);
        }

        /// <summary>
        /// 스텐실 참조값 설정
        /// </summary>
        // 스텐실 참조값 설정 메서드 수정
        public void SetStencilRef(int value)
        {
            stencilRefValue = value;

            // 머티리얼에 참조값 설정
            if (wallStencilWriterMaterial != null)
            {
                wallStencilWriterMaterial.SetInt("_StencilRef", stencilRefValue);
            }

            if (blockSlicingMaterial != null)
            {
                blockSlicingMaterial.SetInt("_StencilRef", stencilRefValue);
            }
        }

        // 스텐실 마스킹 설정 메서드 수정
        public void SetupStencilMasking(List<GameObject> walls, List<GameObject> blocks)
        {
            // 기존 설정 정리 (슬라이싱 중인 블록 제외)
            CleanupMaterials(false);

            // 벽 오브젝트에 스텐실 Writer 적용
            foreach (var wall in walls)
            {
                ApplyStencilWriterToObject(wall);
            }

            // 블록에는 스텐실 효과를 적용하지 않음 (슬라이싱 시에만 적용)
        }

        /// <summary>
        /// 벽 오브젝트에 스텐실 Writer 적용
        /// </summary>
        private void ApplyStencilWriterToObject(GameObject obj)
        {
            if (wallStencilWriterMaterial == null) return;

            // 메인 벽 Renderer만 찾기 (자식 오브젝트 제외)
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
        /// 블록 슬라이싱 효과 시작
        /// </summary>
        public void StartBlockSlicing(GameObject blockObj, Vector3 sliceDirection, float speed = 1.0f, System.Action onComplete = null)
        {
            if (blockObj == null || blockSlicingMaterial == null || coroutineHost == null) return;

            // 이미 슬라이싱 중인 블록이면 중단
            if (slicingObjects.ContainsKey(blockObj))
            {
                return;
            }

            // 슬라이싱 데이터 생성
            SlicingData sliceData = new SlicingData(blockObj, speed, onComplete);
            slicingObjects.Add(blockObj, sliceData);

            // 블록에 슬라이싱 머티리얼 적용
            ApplySlicingMaterialToBlock(blockObj, sliceDirection, sliceData);

            // 슬라이싱 애니메이션 시작
            coroutineHost.StartCoroutine(AnimateSlicing(blockObj));
        }
        /// <summary>
        /// 블록에 슬라이싱 머티리얼 적용
        /// </summary>
        private void ApplySlicingMaterialToBlock(GameObject blockObj, Vector3 sliceDirection, SlicingData sliceData)
        {
            // 블록의 모든 렌더러 찾기
            Renderer[] renderers = blockObj.GetComponentsInChildren<Renderer>();

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

                // 슬라이싱 머티리얼 인스턴스 생성
                Material sliceMaterial = new Material(blockSlicingMaterial);

                // 원본 텍스처 복사
                if (originalMat.HasProperty("_MainTex") && originalMat.mainTexture != null)
                {
                    sliceMaterial.SetTexture("_MainTex", originalMat.mainTexture);
                }

                // URP 텍스처 복사 (필요한 경우)
                if (originalMat.HasProperty("_BaseMap") && originalMat.GetTexture("_BaseMap") != null)
                {
                    sliceMaterial.SetTexture("_MainTex", originalMat.GetTexture("_BaseMap"));
                }

                // 원본 색상 복사
                if (originalMat.HasProperty("_Color"))
                {
                    sliceMaterial.SetColor("_Color", originalMat.GetColor("_Color"));
                }

                // URP 색상 복사 (필요한 경우)
                if (originalMat.HasProperty("_BaseColor"))
                {
                    sliceMaterial.SetColor("_Color", originalMat.GetColor("_BaseColor"));
                }

                // 슬라이싱 파라미터 설정
                sliceMaterial.SetVector("_SliceDirection", new Vector4(sliceDirection.x, sliceDirection.y, sliceDirection.z, 0));
                sliceMaterial.SetFloat("_SliceAmount", 0f);

                // 머티리얼 적용
                renderer.material = sliceMaterial;
                sliceData.materials.Add(sliceMaterial);
            }
        }
        /// <summary>
        /// 슬라이싱 애니메이션
        /// </summary>
        private IEnumerator AnimateSlicing(GameObject blockObj)
        {
            if (!slicingObjects.ContainsKey(blockObj))
            {
                yield break;
            }

            SlicingData sliceData = slicingObjects[blockObj];

            // 슬라이싱 애니메이션
            while (sliceData.amount < 1.0f)
            {
                // 블록이 파괴된 경우 중단
                if (blockObj == null)
                {
                    slicingObjects.Remove(blockObj);
                    yield break;
                }

                // 슬라이싱 진행
                sliceData.amount += Time.deltaTime * sliceData.speed;
                sliceData.amount = Mathf.Clamp01(sliceData.amount);

                // 모든 머티리얼 업데이트
                foreach (var material in sliceData.materials)
                {
                    if (material != null)
                    {
                        material.SetFloat("_SliceAmount", sliceData.amount);
                    }
                }

                yield return null;
            }

            // 애니메이션 완료 후 콜백 호출
            if (sliceData.onComplete != null)
            {
                sliceData.onComplete.Invoke();
            }

            // 슬라이싱 목록에서 제거
            slicingObjects.Remove(blockObj);
        }

        /// <summary>
        /// 방향에 따른 슬라이싱 방향 계산
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
        /// 머티리얼 정리
        /// </summary>
        public void CleanupMaterials(bool includeSlicing = true)
        {
            List<Renderer> restoreList = new List<Renderer>();

            foreach (var pair in originalMaterials)
            {
                if (pair.Key == null) continue;

                bool shouldRestore = true;

                // 슬라이싱 중인 오브젝트는 제외
                if (!includeSlicing)
                {
                    GameObject rendererObj = pair.Key.gameObject;
                    Transform parent = rendererObj.transform;

                    // 부모 계층 확인
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

            // 복원된 렌더러 제거
            foreach (var renderer in restoreList)
            {
                originalMaterials.Remove(renderer);
            }

            // 슬라이싱 객체 정리
            if (includeSlicing)
            {
                slicingObjects.Clear();
            }
        }

        /// <summary>
        /// 완전히 정리
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