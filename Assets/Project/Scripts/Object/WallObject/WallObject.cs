using UnityEngine;
using Project.Scripts.View;

public class WallObject : MonoBehaviour
{
    [SerializeField] MeshRenderer wallRenderer;
    [SerializeField] private GameObject arrow;

    // 뷰 컴포넌트 참조
    private WallView wallView;

    private void Awake()
    {
        // 뷰 컴포넌트 참조 가져오기
        wallView = GetComponent<WallView>();
        if (wallView == null)
        {
            wallView = gameObject.AddComponent<WallView>();
        }
    }

    public void SetWall(Material material, bool isCuttingBox)
    {
        // 기본 렌더러 설정
        if (wallRenderer != null)
        {
            wallRenderer.material = material;
        }

        // 화살표 활성화 설정
        if (arrow != null)
        {
            arrow.SetActive(isCuttingBox);
        }

        // 뷰 컴포넌트를 통한 설정
        if (wallView != null)
        {
            wallView.SetMaterial(material, isCuttingBox);
        }
    }

    /// <summary>
    /// 벽이 파괴될 때 호출됩니다.
    /// </summary>
    public void DestroyWall()
    {
        if (wallView != null)
        {
            wallView.PlayDestroyAnimation(() => {
                Destroy(gameObject);
            });
        }
        else
        {
            // 뷰 컴포넌트가 없는 경우
            Destroy(gameObject);
        }
    }
}