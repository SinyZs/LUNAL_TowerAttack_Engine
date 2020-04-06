using UnityEngine;

// On force le fait que l'entity ai un collider
[RequireComponent(typeof(CapsuleCollider))]
public class Entity : MonoBehaviour
{
    [Header("Global Props")]
    public EntityData entityData;

    [SerializeField]
    private int m_CurrentLife = 1;

    [Header("Attack Props")]
    public GameObject attackContainer;

    private float m_CurrentTimeBeforeNextAttack = 0;
    private bool m_CanAttack = true;

    private float m_CurrentTimeBeforeNextCreate = 0;

    public void Awake()
    {
        InitEntity();

        /*if(isProjectileAttack && prefabProjectile != null)
        {
            Debug.LogError("Error No Prefab Projectile");
        }*/
    }

    // Initialisation - Construction de l'entité
    public virtual void InitEntity()
    {
        RestartEntity();
    }

    // Set de l'entité lorsqu'elle est activée
    // Elle est reset à ses valeurs de depart
    public virtual void RestartEntity()
    {
        if (entityData.isAttackEntity && attackContainer)
        {
            CapsuleCollider colliderAttack;
            colliderAttack = attackContainer.GetComponent<CapsuleCollider>();
            colliderAttack.radius = entityData.rangeDetect;
        }

        m_CurrentLife = entityData.startLife;
    }

    public virtual void Update()
    {
        UpdateAttack();

        UpdateCreator();
    }

    #region LIFE
    // Life
    private void SetLife(int amountLife)
    {
        m_CurrentLife = amountLife;
    }

    public void DamageEntity(int damage)
    {
        m_CurrentLife -= damage;
        if (m_CurrentLife <= 0)
        {
            // Entity Die
            EntityManager.Instance.PoolElement(gameObject);
        }
    }

    public bool IsValidEntity()
    {
        return gameObject.activeSelf && m_CurrentLife > 0;
    }
    #endregion LIFE

    #region ATTACK
    // Attack
    private void UpdateAttack()
    {
        if (entityData.isAttackEntity)
        {
            if (!m_CanAttack)
            {
                if (m_CurrentTimeBeforeNextAttack < entityData.timeWaitNextAttack)
                {
                    m_CurrentTimeBeforeNextAttack += Time.deltaTime;
                }
                else
                {
                    m_CanAttack = true;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (entityData.isAttackEntity)
        {
            if (m_CanAttack)
            {
                //Debug.Log($"Ontrigger {name}: ", other.gameObject);
                DetectTarget(other.gameObject);
            }
        }
    }

    private void DetectTarget(GameObject target)
    {
        // Verification si bon layer
        if (target.gameObject.layer == LayerMask.NameToLayer("Damageable"))
        {
            // Recuperation de l'entity pour tester l'alignement
            Entity entity = target.GetComponent<Entity>();
            if (entity && entity.entityData.alignment == entityData.typeTarget)
            {
                //Debug.Log("Can Hit This");
                DoAttack(entity);
            }
        }
    }

    protected virtual bool DoAttack(Entity targetEntity)
    {
        // On verifie si l'entity est valide
        if (targetEntity.IsValidEntity())
        {
            if (entityData.isProjectileAttack)
            {
                GameObject projectile = PoolManager.Instance.GetElement(entityData.prefabProjectile);
                Projectile projectileCompo = projectile.GetComponent<Projectile>();
                projectile.transform.position = attackContainer.transform.position;
                projectileCompo.InitTarget(targetEntity);
                projectileCompo.damage = entityData.damageAttack;
                projectile.SetActive(true);
            }
            else
            {
                // On applique les degats
                targetEntity.DamageEntity(entityData.damageAttack);
            }

            // On set les variables pour l'attente de l'attaque
            m_CanAttack = false;
            m_CurrentTimeBeforeNextAttack = 0;

            SoundManager.Instance.PlayOneShotGlobalSound();
            return true;
        }
        return false;
    }
    #endregion ATTACK

    #region CREATOR
    // Creator 
    private void UpdateCreator()
    {
        if (entityData.isCreatorEntity)
        {
            if (m_CurrentTimeBeforeNextCreate < entityData.timeWaitNextCreate)
            {
                m_CurrentTimeBeforeNextCreate += Time.deltaTime;
            }
            else
            {
                CreateNewEntity();
            }
        }
    }

    private void CreateNewEntity()
    {
        if (entityData.toCreate != null)
        {
            for (int i = 0; i < entityData.nbrToCreate; i++)
            {
                EntityManager.Instance.PopElementFromPrefab(entityData.toCreate, transform.position);
            }
            m_CurrentTimeBeforeNextCreate = 0;
        }
        else
        {
            Debug.LogError("NO PREFAB SETTED !", gameObject);
        }
    }
    #endregion CREATOR
}
