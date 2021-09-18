using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreatureCardItem : Card
{
    private Camera mainCam;
    private CreatureCard myCardData;
    
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
    private TMP_Text cardAttackRadius;
    [SerializeField]
    private TMP_Text cardAlcololCost;
    [SerializeField]
    private TMP_Text cardSpecial;

   public GameObject deployedCreature;
   
   [SerializeField] private PlayerHand playerHand;
   public CardSpot cardSpot;

   private Vector3 origSpriteScale;
   private float scaleValue;

   [Header("Card Sounds")] 
   public AudioClip [] collectCards;
   public AudioClip [] activateCards;
   void Start()
   {
       mainCam = Camera.main;
       origSpriteScale = cardRenderer.transform.localScale;
   }
   
   public void InjectCreatureWithData(CreatureCard cardData, PlayerHand hand)
   {
       myCardData = cardData;
       
       if (cardRenderer)
       {
           cardRenderer.sprite = cardData.cardSprite;
       }
       
       //ResizeSpriteObject();

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
       if (cardAttackRadius)
       {
            cardAttackRadius.text = cardData.attackRadius.ToString();
       }

       if (cardAlcololCost)
       {
           cardAlcololCost.text = cardData.alcololAmount.ToString();
       }
       if (cardSpecial)
       {
            cardSpecial.text = cardData.special; 
       }

       //set player hand 
       playerHand = hand;

       //set sounds from data 
       if (cardData.collects.Length > 0 && cardData.collects[0] != null)
           collectCards = cardData.collects;

       if (cardData.activates.Length > 0 && cardData.activates[0] != null)
           activateCards = cardData.activates;
       
       //play collection sound
       PlayRandomSound(collectCards, 1f);
   }

   void ResizeSpriteObject()
   {
       cardRenderer.transform.localScale = origSpriteScale;
       float scaleMult = 1f;
       cardRenderer.transform.localScale *= scaleMult;
   }

   private void OnTriggerEnter(Collider other)
   {
       if (other.gameObject.CompareTag("Hand"))
       {
           //select this card!
           if (playerHand)
           {
               if(playerHand.canHoldCard) 
                   playerHand.SelectActiveCard(this);
           }
       }
   }

   public void ReturnToSpot()
   {
       //move card to card spot and look at camera 
       transform.parent = cardSpot.spot;
       transform.position = cardSpot.spot.position;
       gameObject.layer = 8;  //set playable layer 
       transform.LookAt(mainCam.transform);
   }

   //checks if alcolol is great enough
   public bool CheckCanPlayCard()
   {
       int newAlcolol = playerHand.myPlayer.currentAlcolol - myCardData.alcololAmount;
       if (newAlcolol >= 0)
       {
           return true;
       }
       else
       {
           return false;
       }
   }
   
   public override void ActivateCard(Vector3 worldPos, bool zFlipped)   
   {
       //deploy the card 
       deployedCreature = Instantiate(myCardData.creaturePrefab, worldPos, Quaternion.identity);
       //flip the creature 
       if (zFlipped)
       {
           deployedCreature.transform.Rotate(0f, 180f, 0f);
       }
       //get and set creature behavior data 
       CreatureBehavior creatureBehavior = deployedCreature.GetComponent<CreatureBehavior>();
       creatureBehavior.InjectCreatureData(myCardData, playerHand);
       
       //cleanse card spot
       cardSpot.creature = null;
       cardSpot.card = null;
       cardSpot.occupied = false;
       
       //reduce the alcolol amount
       playerHand.myPlayer.ReduceAlcolol(myCardData.alcololAmount);
       
       //play activate card on board sound 
       PlayRandomSound(activateCards, 1f);
       
       //destroy card
       Destroy(gameObject);
       
       //could instead parent card to creature it summons
       //only show card for info when player hand hovers on creature
       
       //destroy this card after sound
       //float delay = myAudioSource.clip.length;
       //StartCoroutine(WaitToDestroy(delay));
   }

   IEnumerator WaitToDestroy(float time)
   {
       yield return new WaitForSeconds(time);
       
       Destroy(gameObject);
   }
}
