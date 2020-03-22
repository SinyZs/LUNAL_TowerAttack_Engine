using UnityEngine;

// Singleton classique
public class Singleton
{
    private static Singleton m_Instance;

    public static Singleton Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new Singleton();
            }
            return m_Instance;
        }
    }

    private Singleton()
    {

    }
}
