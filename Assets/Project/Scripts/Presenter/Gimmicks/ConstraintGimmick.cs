using Project.Scripts.Controller;
using Project.Scripts.Model;
using UnityEngine;
namespace Project.Scripts.Presenter
{
    /// <summary>
    /// ���� ��� ��������
    /// </summary>
    public class ConstraintGimmick : GimmickPresenter
    {
        private GimmickConstraintData constraintData;
        private bool isWidth; // ���� �������� ����
        private GameObject indicatorObject;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Constraint;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // Ư�� ��� �����ͷ� ĳ����
            if (gimmickData is GimmickConstraintData)
            {
                constraintData = gimmickData as GimmickConstraintData;
                isWidth = constraintData.IsWidth;

                // �ð��� ǥ�� ����
                SetupVisuals();
            }
            else
            {
                Debug.LogError("ConstraintGimmick�� �߸��� ������ Ÿ�� ������");
            }
        }

        private void SetupVisuals()
        {
            // ���� ���� ǥ�ñ� ����
            indicatorObject = new GameObject("ConstraintIndicator");
            indicatorObject.transform.SetParent(targetObject.transform);
            indicatorObject.transform.localPosition = new Vector3(0, 0.1f, 0);

            // ť�� ����
            GameObject arrowObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowObj.transform.SetParent(indicatorObject.transform);

            // ���⿡ ���� ũ�� ����
            if (isWidth)
            {
                // ���� ȭ��ǥ
                arrowObj.transform.localScale = new Vector3(0.5f, 0.05f, 0.05f);
                arrowObj.transform.localPosition = Vector3.zero;
            }
            else
            {
                // ���� ȭ��ǥ
                arrowObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.5f);
                arrowObj.transform.localPosition = Vector3.zero;
            }

            // ȭ��ǥ ��Ƽ���� ����
            Renderer renderer = arrowObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
        }

        public override bool OnBlockMove(Vector3 position)
        {
            if (!isActive) return true;

            // ��� �׷� �ڵ鷯 ��������
            var handler = targetObject.GetComponent<BlockDragHandler>();
            if (handler == null) return true;

            // ��� �׷��� ���� ��ġ
            Vector3 previousPosition = handler.transform.position;

            // �̵� ���� ���� Ȯ��
            if (isWidth) // ���� ���⸸ ���
            {
                // Z �� �̵� ����
                if (Mathf.Abs(position.z - previousPosition.z) > 0.1f)
                {
                    return false;
                }
            }
            else // ���� ���⸸ ���
            {
                // X �� �̵� ����
                if (Mathf.Abs(position.x - previousPosition.x) > 0.1f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}