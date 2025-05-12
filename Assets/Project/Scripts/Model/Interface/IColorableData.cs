namespace Project.Scripts.Model
{
    /// <summary>
    /// 색상 정보를 가진 게임 데이터 인터페이스
    /// </summary>
    public interface IColorableData : IGameData
    {
        ColorType ColorType { get; set; }
    }
}


