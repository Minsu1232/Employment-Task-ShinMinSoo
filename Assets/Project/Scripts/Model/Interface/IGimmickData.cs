namespace Project.Scripts.Model
{   
    /// <summary>
    /// ��� ������ ���� ���� ������ �������̽�
    /// </summary>
    public interface IGimmickData : IGameData
    {
        string GimmickType { get; set; }
        ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum();
    }
}

