using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class FoodObject : CellObject
{
    public int AmountGranted = 10;

    public override void PlayerEntered()
    {
        AudioManager.Instance.PlaySfx(SoundType.SFX_PickFood);

        Destroy(gameObject);

        //increase food
        GameManager.Instance.ChangeFood(AmountGranted);
    }

}
