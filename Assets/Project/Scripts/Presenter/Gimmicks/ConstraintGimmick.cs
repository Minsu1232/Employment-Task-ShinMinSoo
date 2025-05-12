using Project.Scripts.Controller;
using Project.Scripts.Model;
using UnityEngine;
namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 제약 기믹 프레젠터
    /// </summary>
    public class ConstraintGimmick : GimmickPresenter
    {
        private GimmickConstraintData constraintData;
        private bool isWidth; // 가로 방향인지 여부
        private GameObject indicatorObject;

        public override ObjectPropertiesEnum.BlockGimmickType GimmickType =>
            ObjectPropertiesEnum.BlockGimmickType.Constraint;

        public override void Initialize(GimmickData gimmickData, GameObject targetObject)
        {
            base.Initialize(gimmickData, targetObject);

            // 특정 기믹 데이터로 캐스팅
            if (gimmickData is GimmickConstraintData)
            {
                constraintData = gimmickData as GimmickConstraintData;
                isWidth = constraintData.IsWidth;

                // 시각적 표현 설정
                SetupVisuals();
            }
            else
            {
                Debug.LogError("ConstraintGimmick에 잘못된 데이터 타입 제공됨");
            }
        }

        private void SetupVisuals()
        {
            // 제약 방향 표시기 생성
            indicatorObject = new GameObject("ConstraintIndicator");
            indicatorObject.transform.SetParent(targetObject.transform);
            indicatorObject.transform.localPosition = new Vector3(0, 0.1f, 0);

            // 큐브 생성
            GameObject arrowObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowObj.transform.SetParent(indicatorObject.transform);

            // 방향에 따른 크기 설정
            if (isWidth)
            {
                // 가로 화살표
                arrowObj.transform.localScale = new Vector3(0.5f, 0.05f, 0.05f);
                arrowObj.transform.localPosition = Vector3.zero;
            }
            else
            {
                // 세로 화살표
                arrowObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.5f);
                arrowObj.transform.localPosition = Vector3.zero;
            }

            // 화살표 머티리얼 설정
            Renderer renderer = arrowObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
        }

        public override bool OnBlockMove(Vector3 position)
        {
            if (!isActive) return true;

            // 블록 그룹 핸들러 가져오기
            var handler = targetObject.GetComponent<BlockDragHandler>();
            if (handler == null) return true;

            // 블록 그룹의 이전 위치
            Vector3 previousPosition = handler.transform.position;

            // 이동 방향 제한 확인
            if (isWidth) // 가로 방향만 허용
            {
                // Z 축 이동 제한
                if (Mathf.Abs(position.z - previousPosition.z) > 0.1f)
                {
                    return false;
                }
            }
            else // 세로 방향만 허용
            {
                // X 축 이동 제한
                if (Mathf.Abs(position.x - previousPosition.x) > 0.1f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}