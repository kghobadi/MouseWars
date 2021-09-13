using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to create data base of cards.
/// Injected into cards when they are spawned by pulling them from the Deck. 
/// </summary>
[CreateAssetMenu(fileName = "CreatureCardData", menuName = "ScriptableObjects/CreatureCardScriptable", order = 1)]
public class CreatureCard : ScriptableObject
{
    public string cardName;
    public string cardDescription;
    public int health;
    public float moveSpeed;
    public float moveRadius;
    public int damage;
    public float attackRadius;
    public int alcololAmount = 1;
    public string special;
    public Texture cardTexture;
    public Sprite cardSprite;
    public GameObject creaturePrefab;
    public GameObject creatureModelPrefab;
    public float tailLength;
    
    [Header("Card Sounds")] 
    public AudioClip[] collects;
    public AudioClip[] activates;
}
