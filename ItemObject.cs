using UnityEngine;

public class ItemObject : CellObject
{
    public ItemType Type;
    public float Duration = 5f; // số lượt tồn tại mặc định

    public override void PlayerEntered()
    {
        AudioManager.Instance.PlaySfx(SoundType.SFX_PickItem);

        GameManager.Instance.PlayerController.CollectItem(Type, Duration);
        Destroy(gameObject);
    }
}
