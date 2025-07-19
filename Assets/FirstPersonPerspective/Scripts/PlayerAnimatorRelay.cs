using UnityEngine;

public class PlayerAnimationRelay : MonoBehaviour
{
    private Player_Zombie player;

    private void Awake()
    {
        player = GetComponentInParent<Player_Zombie>();
        if (player == null)
        {
            Debug.LogError("player Parent Not Found");
        }
    }

    // Relay the event to the parent's player script
    public void OnAttackEnd()
    {
        player.OnAttackEnd();
    }

    // You can add additional relays if needed
    public void OnAttackEffect()
    {
        player.OnAttackEffect();
    }
}