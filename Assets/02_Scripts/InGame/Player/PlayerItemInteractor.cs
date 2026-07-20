using UnityEngine;

public class PlayerItemInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler InputHandler;
    [SerializeField] private PlayerSight PlayerSight;
    [SerializeField, Min(0f)] private float InteractionDistance = 3f;
    [SerializeField] private LayerMask ItemLayerMask;

    private void OnEnable()
    {
        InputHandler.GetItemPerformed += TryPickupLookedAtItem;
    }

    private void OnDisable()
    {
        InputHandler.GetItemPerformed -= TryPickupLookedAtItem;
    }

    private void TryPickupLookedAtItem()
    {
        Transform sight = PlayerSight.GetPlayerSightTransform();

        if (!Physics.Raycast(
                sight.position,
                sight.forward,
                out RaycastHit hit,
                InteractionDistance,
                ItemLayerMask,
                QueryTriggerInteraction.Collide))
        {
            return;
        }

        FieldItem fieldItem = hit.collider.GetComponentInParent<FieldItem>();

        if (fieldItem == null)
            return;

        fieldItem.TryPickup();
    }
}
