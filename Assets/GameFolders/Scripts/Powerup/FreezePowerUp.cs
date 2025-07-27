using GameFolders.Scripts;

public class FreezePowerUp : IPowerUp
{
    public PowerUpType Type => PowerUpType.Freeze;
    public float Duration { get; private set; }

    private readonly ConveyorBeltController conveyor;
    private float originalSpeed;
    private float freezeSpeed = 1.5f;

    public FreezePowerUp(float duration, ConveyorBeltController conveyor)
    {
        Duration = duration;
        this.conveyor = conveyor;
    }

    public void Activate(object context = null)
    {
        originalSpeed = conveyor.conveyorSpeed;
        conveyor.conveyorSpeed = freezeSpeed;
    }

    public void Deactivate()
    {
        conveyor.conveyorSpeed = originalSpeed;
    }
}
