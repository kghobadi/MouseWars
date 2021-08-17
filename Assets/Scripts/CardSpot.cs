
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardSpot
{
    public Transform spot;
    public CreatureBehavior creature;
    public CreatureCard card;
    public bool occupied;
}