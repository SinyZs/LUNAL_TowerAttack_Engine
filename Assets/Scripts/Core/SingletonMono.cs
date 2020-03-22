using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_Instance;

    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                // Verifier si une instance existe
                // Et recupérer si existante
                m_Instance = FindObjectOfType<T>();

                // Sinon créer la nouvelle
                if(m_Instance == null)
                {
                    GameObject newInstanceGO = new GameObject("[Singleton]" + typeof(T));
                    m_Instance = newInstanceGO.AddComponent<T>();
                }
            }

            // Retourner l'instance unique
            return m_Instance;
        }
    }
}
