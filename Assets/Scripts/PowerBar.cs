using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBar : MonoBehaviour
{
    public LevelManager levelManager;
    private int playerPop;
    private int enemyPop;

    // PowerBar position. 
    private Vector3 startingPosition;    
    // Bars 
    private Transform playerBar;
    private Vector3 initialplayerBarScale;
    // Shake 
    private float shakeAmount = 20f;
    // Scale = playerPop/(playerPop + enemyPop)
    private float scale;

    // Start is called before the first frame update
    void Start()
    {   
        // Bars
        playerBar = transform.GetChild(0);
        initialplayerBarScale= playerBar.transform.localScale;
        // Shake Bar
        startingPosition = transform.position;
        // Shake Camera 
        //startingPosition= Camera.main.transform.position;

        
    }

    // Update is called once per frame
    void Update()
    {
        playerPop = levelManager.getPlayerPop();
        enemyPop = levelManager.getEnemyPop();

        scalePlayerBar();
        shakeBar();
    }

    
    
    // Change the playerBar sprite based on the population. 
    private void scalePlayerBar()
    {
        float totalPop = playerPop + enemyPop;
        scale = (float)(playerPop/ totalPop);
        playerBar.localScale = Vector3.Scale(initialplayerBarScale,new Vector3(scale, 1, 1));   
        
    }

    // Shake the bar if below a certain scale. 
    private void shakeBar() 
    {
        if (scale <= 0.2f)
        {
            Vector3 newPos = Random.insideUnitCircle * (Time.deltaTime * shakeAmount);
            // These axes don't shake. 
            // Bar
            newPos.x = transform.position.x;
            newPos.z = transform.position.z;
            //Camera
            //newPos.x = Camera.main.transform.position.x;    
            //newPos.z = Camera.main.transform.position.z;
            // Y-Axis shakes a bit from the original position.
            newPos.y += startingPosition.y;
            // Bar 
            transform.position = newPos;
            // Camera
            //Camera.main.transform.position = newPos;
        }
        // Return to original position. 
        else
        {
            // Bar
            transform.position = startingPosition;
            // Camera
            //Camera.main.transform.position = startingPosition;
        }
    }

}

