using Project.Scripts.Model;
using UnityEngine;
using System.Collections.Generic;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// ��� ������ ����ϴ� ���丮 Ŭ����
    /// </summary>
    public class GimmickFactory : MonoBehaviour
    {
        private static GimmickFactory instance;

        // �̱��� ����
        public static GimmickFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("GimmickFactory");
                    instance = go.AddComponent<GimmickFactory>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// ��� �����Ϳ� �´� �������� ����
        /// </summary>
        public IGimmickPresenter CreateGimmick(GimmickData gimmickData, GameObject targetObject)
        {
            if (gimmickData == null) return null;

            IGimmickPresenter presenter = null;

            // ��� Ÿ�Կ� ���� �������� ����
            switch (gimmickData.GetGimmickEnum())
            {
                case ObjectPropertiesEnum.BlockGimmickType.Constraint:
                    presenter = targetObject.AddComponent<ConstraintGimmick>();
                    break;

                case ObjectPropertiesEnum.BlockGimmickType.Multiple:
                    presenter = targetObject.AddComponent<MultipleGimmick>();
                    break;

                case ObjectPropertiesEnum.BlockGimmickType.Frozen:
                    presenter = targetObject.AddComponent<IceGimmick>();
                    break;

                case ObjectPropertiesEnum.BlockGimmickType.Key:
                    presenter = targetObject.AddComponent<KeyGimmick>();
                    break;

                case ObjectPropertiesEnum.BlockGimmickType.Lock:
                    presenter = targetObject.AddComponent<LockGimmick>();
                    break;

                case ObjectPropertiesEnum.BlockGimmickType.Star:
                    presenter = targetObject.AddComponent<StarGimmick>();
                    break;

                default:
                    // �� �� ���� ��� Ÿ��
                    Debug.LogWarning($"�� �� ���� ��� Ÿ��: {gimmickData.GimmickType}");
                    return null;
            }

            // �������� �ʱ�ȭ
            presenter.Initialize(gimmickData, targetObject);
            return presenter;
        }

        /// <summary>
        /// ���� ��� �����Ϳ� �´� �������͵� ����
        /// </summary>
        public List<IGimmickPresenter> CreateGimmicks(List<GimmickData> gimmickDataList, GameObject targetObject)
        {
            List<IGimmickPresenter> presenters = new List<IGimmickPresenter>();

            if (gimmickDataList == null || gimmickDataList.Count == 0)
                return presenters;

            foreach (var gimmickData in gimmickDataList)
            {
                var presenter = CreateGimmick(gimmickData, targetObject);
                if (presenter != null)
                {
                    presenters.Add(presenter);
                }
            }

            return presenters;
        }
    }
}