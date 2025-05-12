using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.View.Effects
{
    /// <summary>
    /// 스텐실 마스킹 효과를 관리하는 뷰 컴포넌트
    /// </summary>
    public class StencilMaskView : MonoBehaviour
    {
        [Header("스텐실 셰이더 참조")]
        [SerializeField] private Material stencilMaskMaterial;    // 마스크 작성 머티리얼
        [SerializeField] private Material stencilUseMaterial;     // 마스크 사용 머티리얼

        [Header("스텐실 설정")]
        [SerializeField] private int stencilRef = 1;              // 스텐실 참조값

        private List<Renderer> maskWriters = new List<Renderer>(); // 마스크를 쓰는 렌더러들
        private List<Renderer> maskUsers = new List<Renderer>();   // 마스크를 사용하는 렌더러들

        private void Awake()
        {
            // 스텐실 머티리얼이 없는 경우 기본 머티리얼 생성
            if (stencilMaskMaterial == null)
            {
                CreateDefaultStencilMaterials();
            }
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
            }

            // 마스크 사용 머티리얼 생성
            Shader useShader = Shader.Find("Custom/StencilUse");
            if (useShader != null)
            {
                stencilUseMaterial = new Material(useShader);
                stencilUseMaterial.SetInt("_StencilRef", stencilRef);
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
                            // 원본 머티리얼 저장 필요 시 여기에 코드 추가

                            // 스텐실 마스크 머티리얼 적용
                            renderer.material = stencilMaskMaterial;
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
                            // 원본 머티리얼 저장 필요 시 여기에 코드 추가

                            // 스텐실 사용 머티리얼 적용
                            renderer.material = stencilUseMaterial;
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
            // TODO: 원본 머티리얼로 복원하는 코드 필요 시 추가

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
        }
    }
}