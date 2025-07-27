using System.Collections.Generic;
using UnityEngine;

public class PowerUpScheduler : MonoBehaviour
{
    public static PowerUpScheduler Instance { get; private set; }

    private struct Entry
    {
        public IPowerUp PowerUp;
        public float Remaining;

        public Entry(IPowerUp powerUp, float remaining)
        {
            PowerUp = powerUp;
            Remaining = remaining;
        }
    }

    private readonly List<Entry> active = new List<Entry>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void Schedule(IPowerUp powerUp, float duration)
    {
        // Anlýk power-ups (duration <= 0) sadece Activate edilir
        if (duration <= 0f)
        {
            powerUp.Activate();
            return;
        }

        // Aktif listede ayný türden varsa, iþlem yapma
        for (int i = 0; i < active.Count; i++)
        {
            if (active[i].PowerUp.Type == powerUp.Type)
            {
                return;
            }
        }

        powerUp.Activate();
        active.Add(new Entry(powerUp, duration));
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var entry = active[i];
            entry.Remaining -= delta;

            if (entry.Remaining <= 0f)
            {
                entry.PowerUp.Deactivate();
                active.RemoveAt(i);
            }
            else
            {
                active[i] = entry;
            }
        }
    }
}
