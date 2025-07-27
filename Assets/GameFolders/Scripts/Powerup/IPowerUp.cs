public interface IPowerUp
{
    PowerUpType Type { get; }
    float Duration { get; }
    void Activate(object context = null);
    void Deactivate();
}
public enum PowerUpType
{
    Freeze,
    SlotBomb,
    // ileride ekleyece�in di�er tipler�
}