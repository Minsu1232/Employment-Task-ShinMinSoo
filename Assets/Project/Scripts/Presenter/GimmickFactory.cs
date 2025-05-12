using Project.Scripts.Model;
using UnityEngine;
using System.Collections.Generic;

namespace Project.Scripts.Presenter
{
    /// <summary>
    /// 기믹 생성을 담당하는 팩토리 클래스
    /// </summary>
    public class GimmickFactory : MonoBehaviour
    {
        private static GimmickFactory instance;

        // 싱글톤 패턴
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
        /// 기믹 데이터에 맞는 프레젠터 생성
        /// </summary>
        public IGimmickPresenter CreateGimmick(GimmickData gimmickData, GameObject targetObject)
        {
            if (gimmickData == null) return null;

            IGimmickPresenter presenter = null;

            // 기믹 타입에 따른 프레젠터 생성
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
                    // 알 수 없는 기믹 타입
                    Debug.LogWarning($"알 수 없는 기믹 타입: {gimmickData.GimmickType}");
                    return null;
            }

            // 프레젠터 초기화
            presenter.Initialize(gimmickData, targetObject);
            return presenter;
        }

        /// <summary>
        /// 여러 기믹 데이터에 맞는 프레젠터들 생성
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