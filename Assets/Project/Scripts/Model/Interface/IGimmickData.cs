namespace Project.Scripts.Model
{   
    /// <summary>
    /// 기믹 정보를 가진 게임 데이터 인터페이스
    /// </summary>
    public interface IGimmickData : IGameData
    {
        string GimmickType { get; set; }
        ObjectPropertiesEnum.BlockGimmickType GetGimmickEnum();
    }
}

