using UnityEngine;
using System;

public class FireHealth : MonoBehaviour
{
    public float maxHealth = 4f;
    public ParticleSystem[] fxToStop;

    float health;
    bool outed;

    public static event Action OnAnyFireExtinguished;

    void Awake() => health = maxHealth;

    public void ExtinguishFire(float amount)
    {
        if (outed) return;

        health -= amount;
        if (health <= 0f)
        {
            outed = true;

            foreach (var ps in fxToStop)
                if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            OnAnyFireExtinguished?.Invoke();
            gameObject.SetActive(false);
        }
    }
}