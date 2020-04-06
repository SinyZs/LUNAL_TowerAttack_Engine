using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDeck", menuName = "TowerAttack/Deck", order = 1)]
public class Deck : ScriptableObject
{
    public List<EntityData> allEntities;
}
