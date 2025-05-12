using UnityEngine;
using Project.Scripts.View;

public class WallObject : MonoBehaviour
{
    [SerializeField] MeshRenderer wallRenderer;
    [SerializeField] private GameObject arrow;

    // �� ������Ʈ ����
    private WallView wallView;

    private void Awake()
    {
        // �� ������Ʈ ���� ��������
        wallView = GetComponent<WallView>();
        if (wallView == null)
        {
            wallView = gameObject.AddComponent<WallView>();
        }
    }

    public void SetWall(Material material, bool isCuttingBox)
    {
        // �⺻ ������ ����
        if (wallRenderer != null)
        {
            wallRenderer.material = material;
        }

        // ȭ��ǥ Ȱ��ȭ ����
        if (arrow != null)
        {
            arrow.SetActive(isCuttingBox);
        }

        // �� ������Ʈ�� ���� ����
        if (wallView != null)
        {
            wallView.SetMaterial(material, isCuttingBox);
        }
    }

    /// <summary>
    /// ���� �ı��� �� ȣ��˴ϴ�.
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
            // �� ������Ʈ�� ���� ���
            Destroy(gameObject);
        }
    }
}