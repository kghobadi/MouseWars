using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreatureBehavior : Card
{
   [SerializeField] 
   private SpriteRenderer cardRenderer;
   [SerializeField] 
   private TMP_Text cardName;
   [SerializeField] 
   private TMP_Text cardDescription;
   [SerializeField] 
   private TMP_Text cardAttack;
   [SerializeField] 
   private TMP_Text cardHP;
   [SerializeField] 
   private TMP_Text cardMove;

   public void InjectCreatureWithData(CreatureCard cardData)
   {
       if (cardRenderer)
       {
           cardRenderer.sprite = cardData.cardSprite;
       }

       if (cardName)
       {
           cardName.text = cardData.cardName;
       }

       if (cardDescription)
       {
           cardDescription.text = cardData.cardDescription;
       }

       if (cardAttack)
       {
           cardAttack.text = cardData.damage.ToString();
       }

       if (cardHP)
       {
           cardHP.text = cardData.health.ToString();
       }

       if (cardMove)
       {
           cardMove.text = cardData.moveSpeed.ToString();
       }
   }
}
