using UnityEngine;

namespace Project.Scripts.Config
{
    [CreateAssetMenu(fileName = "VisualConfig", menuName = "Config/VisualConfig")]
    public class VisualConfig : ScriptableObject
    {
        [Header("��ƼŬ")]
        public ParticleSystem destroyParticlePrefab;

        [Header("���ؽ� ���ٽ� ������ ȿ��")]
        [Tooltip("������ ȿ�� ���� ����")]
        public Color dissolveEdgeColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);

        [Tooltip("������ ȿ�� ���� �β�")]
        public float dissolveEdgeWidth = 0.05f;

        [Tooltip("������ ȿ�� �⺻ ���� �ð�")]
        public float dissolveDuration = 1.0f;

        [Tooltip("���ؽ� ���ٽ� ������ ���̴� ��Ƽ����")]
        public Material vertexStencilDissolveMaterial;

        [Tooltip("���ٽ� ���� ������")]
        public int stencilRefValue = 1;

        [Header("�ƿ����� ����")]
        public Color outlineColor = Color.yellow;
        public float outlineWidth = 2f;

   
    }
}