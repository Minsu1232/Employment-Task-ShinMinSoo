namespace Project.Scripts.Model
{
    /// <summary>
    /// ��ġ ������ ���� ���� ������ �������̽�
    /// </summary>
    public interface IPositionData : IGameData
    {
        int X { get; set; }
        int Y { get; set; }
    }
}