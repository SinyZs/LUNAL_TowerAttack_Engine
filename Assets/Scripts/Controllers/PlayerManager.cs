using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private Camera m_CurrentCamera;
    private MapManager m_MapManager = null;
    private EntityManager m_EntityManager = null;
   

    [Header("Props Player Data")]
    public Deck deck;

    [Header("Props FeedBack Pos Drop")]
    public GameObject feedbackPosDrop;

    // Index courant dans le deck de l'entité qu'on peut dropper.
    // Si index == -1 on est dans l'etat => pas de drop possible.
    private int m_CurrentIndex = -1;

    // Stamina
    private const int m_MaxStamina = 9;
    [Header("Stamina Props")]
    [SerializeField]
    private float m_CurrentStamina = 0;
    public float speedStamina = 1;

    private void Awake()
    {
       
        m_MapManager = FindObjectOfType<MapManager>();
        m_EntityManager = FindObjectOfType<EntityManager>();
        m_CurrentCamera = FindObjectOfType<Camera>();
    }

    private void Start()
    {
        if (m_MapManager)
        {
            m_MapManager.DisplayDropFeedBack(false);
        }
    }

    private void Update()
    {
        UpdateStamina();
    }

    #region STAMINA
    public float GetCurrentStamina()
    {
        return m_CurrentStamina;
    }

    private void UpdateStamina()
    {
        if (m_CurrentStamina < m_MaxStamina)
        {
            AddStamina(Time.deltaTime * speedStamina);
        }
    }

    private void AddStamina(float amount)
    {
        m_CurrentStamina += amount;
        if(m_CurrentStamina < 0)
        {
            m_CurrentStamina = 0;
        }
        if(m_CurrentStamina > m_MaxStamina)
        {
            m_CurrentStamina = m_MaxStamina;
        }

    }
    #endregion STAMINA

    #region DROP ENTITY
    private void SetCurrentIndex(int index = -1)
    {
        // Increment Index poru passer à l'entité suivante
        m_CurrentIndex = index;

        // si l'index est superieur aux nombres d'entité présentent dans le deck
        if (m_CurrentIndex >= deck.allEntities.Count)
        {
            // On le remet à -1
            m_CurrentIndex = -1;
        }

        // Affichage de la zone de drop via la map en fonction de l'index
        if (m_MapManager)
        {
            m_MapManager.DisplayDropFeedBack(m_CurrentIndex != -1);
        }

        // Si pas de drop on desaffiche la feedback de drop
        if(m_CurrentIndex == -1)
        {
            UnDisplayFBDrop();
        }
    }

    private void DisplayFBDrop(Vector3 pos, Color colorFB)
    {
        feedbackPosDrop.SetActive(true);
        feedbackPosDrop.GetComponent<MeshRenderer>().material.color = colorFB;  ;
        feedbackPosDrop.transform.position = pos; ;
    }

    private void UnDisplayFBDrop()
    {
        feedbackPosDrop.SetActive(false);
    }
    #endregion DROP ENTITY

    #region PLAYER INPUT
    // Fonction de debug pour lancer les entités du players
    private void PopPlayerEntity(bool endDrop)
    {
        if(m_CurrentIndex != -1)
        {
            // Creation d'un Ray à partir de la camera
            Ray ray = m_CurrentCamera.ScreenPointToRay(Input.mousePosition);
            float mult = 1000;
            Debug.DrawRay(ray.origin, ray.direction * mult, Color.green);

            if (Physics.Raycast(ray, out RaycastHit hit, mult, LayerMask.GetMask("Default")))
            {
                // On verifie si on peut dropper à cette position de la map
                bool canDrop = m_MapManager.TestIfCanDropAtPos(hit.point);

                // On set le feedback en fonction de can drop
                DisplayFBDrop(new Vector3(hit.point.x, 0.4f, hit.point.z), canDrop ? Color.blue : Color.red);

                // Recuperation du bouton droit de la souris.
                if (canDrop && endDrop)
                {
                    // On recupère un élement depuis le poolmanager
                    m_EntityManager.PopElementFromData(deck.allEntities[m_CurrentIndex], hit.point);

                    AddStamina(-deck.allEntities[m_CurrentIndex].popAmount);
                }
            }
            else
            {
                // On desactive le feedback si on est pas sur la map
                UnDisplayFBDrop();
            }
        }
    }
    #endregion PLAYER INPUT

    #region DROP EVENT
    public void OnStartDrag(int index)
    {
        SetCurrentIndex(index);
    }

    public void OnDrag(int index)
    {
        PopPlayerEntity(false);
    }

    public void OnDrop(int index)
    {
        PopPlayerEntity(true);
        SetCurrentIndex();
    }
    #endregion DROP EVENT
}
