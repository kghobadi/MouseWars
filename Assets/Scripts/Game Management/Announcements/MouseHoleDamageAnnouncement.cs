using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseHoleDamageAnnouncement : Announcement
{
    public Transform dealerParent;
    public TMP_Text damageAmount;
    public Image playerImage;

    public void SetDamageAmount(int amount)
    {
        damageAmount.text = amount.ToString();
    }

    public void SetPlayerImage(Sprite pSprite)
    {
        playerImage.sprite = pSprite;
    }
}
