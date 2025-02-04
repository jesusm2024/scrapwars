using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{

    List<GameObject> selectButtons;

    // Start is called before the first frame update
    void Start()
    {
        // need to reset time scale after returning from level
        Time.timeScale = 1f;


        selectButtons = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main Menu");
        }
    }

    public void LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }
}
