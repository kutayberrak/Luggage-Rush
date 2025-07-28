public interface IPowerUp
{
    PowerUpType Type { get; }
    float Duration { get; }
    void Activate(object context = null);
    void Deactivate();
}