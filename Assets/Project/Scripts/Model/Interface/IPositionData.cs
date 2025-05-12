namespace Project.Scripts.Model
{
    /// <summary>
    /// 위치 정보를 가진 게임 데이터 인터페이스
    /// </summary>
    public interface IPositionData : IGameData
    {
        int X { get; set; }
        int Y { get; set; }
    }
}