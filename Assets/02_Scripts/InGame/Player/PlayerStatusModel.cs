using System;
using UnityEngine;

public class PlayerStatusModel
{
    public float MaxHP { get; private set; }
    public float CurrentHP { get; private set; }

    public float MaxStamina { get; private set; }
    public float CurrentStamina { get; private set; }

    public event Action<float> HealthChanged;
    public event Action<float> StaminaChanged;

    public void InitPlayer(float maxHP, float maxStamina)
    {
        MaxHP = Mathf.Max(1f, maxHP);
        CurrentHP = MaxHP;

        MaxStamina = Mathf.Max(1f, maxStamina);
        CurrentStamina = MaxStamina;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
            return;

        float previousHP = CurrentHP;

        CurrentHP = Mathf.Clamp(
            CurrentHP - damage,
            0f,
            MaxHP
        );

        if (Mathf.Approximately(previousHP, CurrentHP))
            return;

        HealthChanged?.Invoke(CurrentHP);
    }

    public void UseStamina(float amount)
    {
        if (amount <= 0f)
            return;
        float previousStamina = CurrentStamina;
        CurrentStamina = Mathf.Clamp(
            CurrentStamina - amount,
            0f,
            MaxStamina
        );
        if (Mathf.Approximately(previousStamina, CurrentStamina))
            return;
        StaminaChanged?.Invoke(CurrentStamina);
    }

    public void RecoverStamina(float amount)
    {
        if (amount <= 0f)
            return;
        float previousStamina = CurrentStamina;
        CurrentStamina = Mathf.Clamp(
            CurrentStamina + amount,
            0f,
            MaxStamina
        );
        if (Mathf.Approximately(previousStamina, CurrentStamina))
            return;
        StaminaChanged?.Invoke(CurrentStamina);
    }
}
