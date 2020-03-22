using UnityEngine;

// On force le fait que l'entity ai un collider
[RequireComponent(typeof(CapsuleCollider))]
public class Entity : MonoBehaviour
{
    [Header("Props")]
    public Alignment alignment;

    public int startLife = 1;
    [SerializeField]
    private int m_CurrentLife = 1;

    public int popAmount = 1;

    [Header("AttackProps")]
    public GameObject attackContainer;
    public int damageAttack = -1;
    public int rangeDetect = 1;

    [Header("Time Next Attack")]
    [Range(0, 10)]
    public float timeWaitNextAttack = 1;
    private float m_CurrentTimeBeforeNextAttack = 0;
    private bool m_CanAttack = true;

    public static Vector3 myPoint = Vector3.zero;


    // Variable perso
    public float m_targetTime = 0.0f;
    public GameObject monsterToInstantiate;
    public GameObject globalSpawnTarget;

    public void Awake()
    {
        InitEntity();
    }

    // Initialisation - Construction de l'entité
    public virtual void InitEntity()
    {

    }

    // Set de l'entité lorsqu'elle est activée
    // Elle est reset à ses valeurs de depart
    public virtual void RestartEntity()
    {
        CapsuleCollider colliderAttack;
        colliderAttack = attackContainer.GetComponent<CapsuleCollider>();
        colliderAttack.radius = rangeDetect;
        
        m_CurrentLife = startLife;
    }

    public virtual void Update()
    {        
        if(!m_CanAttack)
        {
            if(m_CurrentTimeBeforeNextAttack < timeWaitNextAttack)
            {
                m_CurrentTimeBeforeNextAttack += Time.deltaTime;
            }
            else
            {
                m_CanAttack = true;
            }
        }

        if (GameObject.FindWithTag("Tower"))
        {
            m_targetTime += Time.deltaTime;

            if (m_targetTime >= 4.0f)
            {
                SpawnEntity();
            }
        }
    }

    // Life
    private void SetLife(int amountLife)
    {
        m_CurrentLife = amountLife;
    }

    private void DamageEntity(int damage)
    {
        m_CurrentLife -= damage;
        if(m_CurrentLife <= 0)
        {
            // Entity Die
            //GameObject.Destroy(gameObject);
            PoolManager.Instance.PoolElement(gameObject);
        }
    }

    private bool IsValidEntity()
    {
        return gameObject.activeSelf && m_CurrentLife > 0;
    }

    // Attack
    private void OnTriggerStay(Collider other)
    {
        if (m_CanAttack)
        {
            //Debug.Log($"Ontrigger {name}: ", other.gameObject);
            DetectTarget(other.gameObject);
        }
    }

    private void DetectTarget(GameObject target)
    {
        // Verification si bon layer
        if(target.gameObject.layer == LayerMask.NameToLayer("Damageable"))
        {
            // Recuperation de l'entity pour tester l'alignement
            Entity entity = target.GetComponent<Entity>();
            if (entity && entity.alignment != alignment)
            {
                //Debug.Log("Can Hit This");
                DoAttack(entity);
            }
            else if(entity && entity.alignment == alignment)
            {

            }
        }
    }

    protected virtual bool DoAttack(Entity targetEntity)
    {
        // On verifie si l'entity est valide
        if(targetEntity.IsValidEntity())
        {
            // On applique les degats
            targetEntity.DamageEntity(damageAttack);

            // On set les variables pour l'attente de l'attaque
            m_CanAttack = false;
            m_CurrentTimeBeforeNextAttack = 0;

            SoundManager.Instance.PlayOneShotGlobalSound();
            return true;
        }
        return false;
    }

    private void SpawnEntity()
    {
        GameObject instantiated = PoolManager.Instance.GetElement(monsterToInstantiate);
        instantiated.transform.position = transform.position;
        instantiated.SetActive(true);

        m_targetTime = 0.0f;

        Entity entity = instantiated.GetComponent<Entity>();
        if (entity)
        {
            if (entity is EntityMoveable moveable)
            {
                moveable.SetGlobalTarget(globalSpawnTarget);
            }
            entity.RestartEntity();
        }
    }
}
