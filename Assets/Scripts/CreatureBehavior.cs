using System;
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
   
   [SerializeField] 
   private GameObject creaturePrefab;

   public GameObject deployedCreature;

   [SerializeField] private PlayerHand playerHand;
   public CardSpot cardSpot;
   public void InjectCreatureWithData(CreatureCard cardData, PlayerHand hand)
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

       //pass the prefab 
       creaturePrefab = cardData.creaturePrefab;

       playerHand = hand;
   }

   private void OnTriggerEnter(Collider other)
   {
       if (other.gameObject.tag == "Hand")
       {
           //select this card!
           playerHand.SelectActiveCard(this);
       }
   }

   public void ReturnToSpot()
   {
       //move card to card spot and look at camera 
       transform.parent = cardSpot.spot;
       transform.position = cardSpot.spot.position;
       gameObject.layer = 8;  //set playable layer 
       transform.LookAt(Camera.main.transform);
   }

   public override void ActivateCard(Vector3 worldPos)   
   {
       //deploy the card 
       GameObject creature = Instantiate(creaturePrefab, worldPos, Quaternion.identity);
       deployedCreature = creature;
   }
}
