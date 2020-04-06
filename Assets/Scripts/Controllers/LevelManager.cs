using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelManager : MonoBehaviour
{
    public float timer = 40f;
    public int timerInt;
    
    public void Start()
    {
        EntityManager entityManager = FindObjectOfType<EntityManager>();
        if(entityManager != null)
        {
            entityManager.OnTowerDestroy += EndGame;
        }
    }
    private void Update()
    {
        timerInt = Mathf.RoundToInt(timer);
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            
            
            EndGame(Alignment.Player);
        }
    }

    private void EndGame(Alignment alignment)
    {
        switch(alignment)
        {
            case Alignment.Player:
                Debug.Log("LOOOOOOOOOOSE ! GAME OVER !");
                break;
            case Alignment.IA:
                Debug.Log("WIN ! YOU'RE THE BEST");
                break;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
}
