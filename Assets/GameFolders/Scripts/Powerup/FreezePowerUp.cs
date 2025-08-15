using GameFolders.Scripts;
using System.Collections.Generic;

public class FreezePowerUp : IPowerUp
{
    public PowerUpType Type => PowerUpType.Freeze;
    public float Duration { get; private set; }

    private readonly List<ConveyorBeltController> conveyors;
    private readonly Dictionary<ConveyorBeltController, float> originalSpeeds = new();
    private float originalSpeed;
    private float freezeSpeed = 0.5f;

    public FreezePowerUp(float duration, List<ConveyorBeltController> conveyors)
    {
        Duration = duration;
        this.conveyors = conveyors;
    }

    public void Activate(object context = null)
    {
        foreach (var c in conveyors)
        {
            if (c == null || c.gameObject == null || !c.gameObject.activeInHierarchy) continue;

            if (!originalSpeeds.ContainsKey(c))
                originalSpeeds[c] = c.conveyorSpeed;

            c.conveyorSpeed = freezeSpeed;
        }

        PowerUpInventory.Instance.DecreaseCount(PowerUpType.Freeze);

        PowerUpInventory.Instance.PlayFreezeEffectAtPosition();
        AudioManager.Instance?.PlaySFX("FreezeSFX");
        AudioManager.Instance?.TriggerMediumVibration();
    }

    public void Deactivate()
    {
        foreach (var kv in originalSpeeds)
        {
            var c = kv.Key;
            if (c != null)
                c.conveyorSpeed = kv.Value;
        }
        originalSpeeds.Clear();
    }
}
