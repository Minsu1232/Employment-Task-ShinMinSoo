using UnityEngine;

namespace Project.Scripts.Model
{
    [System.Serializable]
    public class WallData : PositionData, IColorableData
    {
        [SerializeField] private ObjectPropertiesEnum.WallDirection wallDirection;
        [SerializeField] private int length;
        [SerializeField] private ColorType wallColor;
        [SerializeField] private WallGimmickType wallGimmickType;

        public ObjectPropertiesEnum.WallDirection WallDirection => wallDirection;
        public int Length => length;
        public ColorType ColorType { get => wallColor; set => wallColor = value; }
        public WallGimmickType WallGimmickType => wallGimmickType;

        public WallData() : base() { }

        public WallData(int x, int y, ObjectPropertiesEnum.WallDirection direction,
                        int length, ColorType color, WallGimmickType gimmickType = WallGimmickType.None)
            : base(x, y)
        {
            this.wallDirection = direction;
            this.length = length;
            this.wallColor = color;
            this.wallGimmickType = gimmickType;
        }

        /// <summary>
        /// �� ���⿡ ���� ��ġ ���
        /// </summary>
        public Vector3 GetWallPosition(float gridSize = 0.79f)
        {
            Vector3 position = ToWorldPosition(gridSize);

            switch (wallDirection)
            {
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                    position.z += 0.5f * gridSize;
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Down:
                    position.z -= 0.5f * gridSize;
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Left:
                    position.x -= 0.5f * gridSize;
                    break;
                case ObjectPropertiesEnum.WallDirection.Single_Right:
                    position.x += 0.5f * gridSize;
                    break;
                    // ��Ÿ ���̽���
            }

            // ���̿� ���� ��ġ ����
            if (length > 1)
            {
                if (wallDirection == ObjectPropertiesEnum.WallDirection.Single_Up ||
                    wallDirection == ObjectPropertiesEnum.WallDirection.Single_Down)
                {
                    position.x += (length - 1) * gridSize * 0.5f;
                }
                else if (wallDirection == ObjectPropertiesEnum.WallDirection.Single_Left ||
                         wallDirection == ObjectPropertiesEnum.WallDirection.Single_Right)
                {
                    position.z += (length - 1) * gridSize * 0.5f;
                }
            }

            return position;
        }

        /// <summary>
        /// �� ���⿡ ���� ȸ�� ���
        /// </summary>
        public Quaternion GetWallRotation()
        {
            switch (wallDirection)
            {
                case ObjectPropertiesEnum.WallDirection.Single_Up:
                    return Quaternion.Euler(0f, 180f, 0f);
                case ObjectPropertiesEnum.WallDirection.Single_Down:
                    return Quaternion.identity;
                case ObjectPropertiesEnum.WallDirection.Single_Left:
                    return Quaternion.Euler(0f, 90f, 0f);
                case ObjectPropertiesEnum.WallDirection.Single_Right:
                    return Quaternion.Euler(0f, -90f, 0f);
                // ��Ÿ ���̽���
                default:
                    return Quaternion.identity;
            }
        }
    }
}