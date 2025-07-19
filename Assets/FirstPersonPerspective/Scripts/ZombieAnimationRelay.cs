using UnityEngine;

public class ZombieAnimationRelay : MonoBehaviour
{
    private Zombie zombie;

    private void Awake()
    {
        zombie = GetComponentInParent<Zombie>();
        if(zombie == null)
        {
            Debug.LogError("Zombie Parent Not Found");
        }
    }

    // Relay the event to the parent's Zombie script
    public void OnAttackEnd()
    {
        zombie.OnAttackEnd();
    }

    // You can add additional relays if needed
    public void OnAttackEffect()
    {
        zombie.OnAttackEffect();
    }
}