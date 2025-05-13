using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "VisualConfig", menuName = "Config/VisualConfig")]
    public class VisualConfig : ScriptableObject
    {
        [Header("파티클")]
        public ParticleSystem destroyParticlePrefab;

        [Header("버텍스 스텐실 디졸브 효과")]
        [Tooltip("디졸브 효과 에지 색상")]
        public Color dissolveEdgeColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);

        [Tooltip("디졸브 효과 에지 두께")]
        public float dissolveEdgeWidth = 0.05f;

        [Tooltip("디졸브 효과 기본 지속 시간")]
        public float dissolveDuration = 1.0f;

        [Tooltip("버텍스 스텐실 디졸브 셰이더 머티리얼")]
        public Material vertexStencilDissolveMaterial;

        [Tooltip("스텐실 버퍼 참조값")]
        public int stencilRefValue = 1;

        [Header("아웃라인 설정")]
        public Color outlineColor = Color.yellow;
        public float outlineWidth = 2f;

   
    }
}