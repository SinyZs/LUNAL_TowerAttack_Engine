using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonMono<PoolManager>
{
    [Range(1, 100)]
    public int nbrPopulateAtStart = 10;

    public List<GameObject> prefabsToPool;

    private Dictionary<GameObject, Pool> m_AllPools;

    public void Awake()
    {
        m_AllPools = new Dictionary<GameObject, Pool>();
        foreach(GameObject prefab in prefabsToPool)
        {
            if (!m_AllPools.ContainsKey(prefab))
            {
                m_AllPools.Add(prefab, new Pool(gameObject, prefab, nbrPopulateAtStart));
            }
            else
            {
                Debug.LogWarning("Prefab Key already Exist !", prefab);
            }
        }
    }

    public GameObject GetElement(GameObject prefabKey)
    {
        if(m_AllPools.ContainsKey(prefabKey))
        {
            return m_AllPools[prefabKey].GetAvailable();
        }
        Debug.LogError("NO POOL FOR : " + prefabKey.name);
        return null;
    }

    public void PoolElement(GameObject toPool)
    {
        Poolable poolable = toPool.GetComponent<Poolable>();
        if(poolable != null)
        {
            if (m_AllPools.ContainsKey(poolable.keyPrefab))
            {
                m_AllPools[poolable.keyPrefab].SetAvailable(toPool);
            }
            else
            {
                Debug.LogWarning("Try To Pool No Prefab Key available => Destroy !");
                Destroy(toPool);
            }
        }
        else
        {
            Debug.LogWarning("Try To Pool but no Poolable !");
            Destroy(toPool);
        }
    }
}

public class Pool
{
    private GameObject m_PrefabPool = null;

    private GameObject m_Container = null;

    private List<GameObject> m_AvailableElements = null;

    public Pool(GameObject container, GameObject prefab, int nbrPooledElement)
    {
        m_Container = container;
        m_PrefabPool = prefab;
        m_AvailableElements = new List<GameObject>();

        // On remplie la piscine => Populate Pool
        Populate(nbrPooledElement);
    }

    public GameObject GetAvailable()
    {
        if(m_AvailableElements.Count == 0)
        {
            Populate(5);
        }
        GameObject toGet = m_AvailableElements[0];
        m_AvailableElements.Remove(toGet);
        return toGet;
    }

    public void SetAvailable(GameObject toSetAvailable)
    {
        toSetAvailable.SetActive(false);
        m_AvailableElements.Add(toSetAvailable);
    }

    // On remplie la piscine => Populate Pool
    private void Populate(int nbrPooledElement)
    {
        for (int i = 0; i < nbrPooledElement; i++)
        {
            GameObject newInstantiate = GameObject.Instantiate(m_PrefabPool, m_Container.transform);

            // Verification du set de Poolable et de la clef
            Poolable poolable = newInstantiate.GetComponent<Poolable>();
            if(!poolable)
            {
                poolable = newInstantiate.AddComponent<Poolable>();
            }
            poolable.keyPrefab = m_PrefabPool;

            m_AvailableElements.Add(newInstantiate);
            newInstantiate.SetActive(false);
        }
    }
}
