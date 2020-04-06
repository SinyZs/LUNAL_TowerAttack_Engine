using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityMoveableData", menuName = "TowerAttack/Entity/MoveEntity", order = 1)]
public class EntityMoveableData : EntityData
{
    [Header("Move Props")]
    [Range(1, 50)]
    public float moveSpeed = 1;

    [Header("Stop Time")]
    // Variable de temps d'arret
    public float timeWaitBeforeMove = 1;

    [Header("Go To Target")]
    public int rangeToDoAttack = 1;
}
