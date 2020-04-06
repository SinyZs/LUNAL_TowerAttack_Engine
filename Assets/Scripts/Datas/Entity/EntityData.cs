using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityData", menuName = "TowerAttack/Entity/BasicEntity", order = 1)]
public class EntityData : ScriptableObject
{
    [Header("Debug Var")]
    public Color debugColor;

    [Header("Global Props")]
    public Alignment alignment;
    public int startLife = 1;
    public int popAmount = 1;
    public int nbrEntityAtPop = 1;

    [Header("Attack Props")]
    public bool isAttackEntity = true;
    public Alignment typeTarget;
    public int damageAttack = 1;
    public int rangeDetect = 1;

    [Header("Projectile Props")]
    public bool isProjectileAttack = false;
    public GameObject prefabProjectile;

    [Header("Time Next Attack")]
    [Range(0, 10)]
    public float timeWaitNextAttack = 1;

    [Header("Creator Props")]
    public bool isCreatorEntity;
    public GameObject toCreate;
    public int nbrToCreate = 1;
    public float timeWaitNextCreate = 1;
}
