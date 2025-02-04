using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{

    // parent object of all units
    private Transform unitParent;

    private int playerPop;
    private int enemyPop;


    public GameObject screenOverlay;
    private TMP_Text endgameText;
    private Button endgameButton;
    private TMP_Text endgameButtonText;
    private Button backButton;

    private Dictionary<string, string> levelMap;


    // Start is called before the first frame update
    void Start()
    {
        // need to reset time scale after changing levels
        Time.timeScale = 1f;

        unitParent = GetComponent<MoveManager>().unitParent;

        endgameText = screenOverlay.transform.GetChild(0).GetComponent<TMP_Text>();
        endgameButton = screenOverlay.transform.GetChild(1).GetComponent<Button>();
        endgameButtonText = endgameButton.transform.GetChild(0).GetComponent<TMP_Text>();
        backButton = screenOverlay.transform.GetChild(2).GetComponent<Button>();
        backButton.onClick.AddListener(ExitLevel);

        levelMap = new Dictionary<string, string>();
        levelMap.Add("Tutorial 1", "Tutorial 2");
        levelMap.Add("Tutorial 2", "Tutorial 3");
        levelMap.Add("Tutorial 3", "Level 1");
        levelMap.Add("Level 1", "Level 2");
        levelMap.Add("Level 2", "Level 3");
        levelMap.Add("Level 3", "Level 4");
        levelMap.Add("Level 4", "Level Select");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitLevel();
            return;
        }

        // means endgame screen is already engaged
        if (screenOverlay.activeSelf)
        {
            return;
        }



        calculatePop();

        // means enemy won
        if (playerPop == 0)
        {
            Time.timeScale = 0;

            screenOverlay.GetComponent<Image>().color = new Color(1, 0, 0, 0.5f); // red
            endgameText.text = "DEFEAT";
            endgameButton.onClick.AddListener(RestartLevel);
            endgameButtonText.text = "Retry Level";

            screenOverlay.SetActive(true);

            return;
        }
        // means player won
        else if (enemyPop == 0)
        {
            Time.timeScale = 0;
            
            screenOverlay.GetComponent<Image>().color = new Color(0, 0, 1, 0.5f); // blue
            endgameText.text = "VICTORY!!";
            endgameButton.onClick.AddListener(NextLevel);
            endgameButtonText.text = "Next Level";

            screenOverlay.SetActive(true);

            return;
        }
    }

    private void ExitLevel()
    {
        SceneManager.LoadScene("Level Select");
    }

    public void NextLevel()
    {
        string current = SceneManager.GetActiveScene().name;
        if (levelMap.ContainsKey(current))
        {
            SceneManager.LoadScene(levelMap[current]);
        }
        else
        {
            Debug.Log("No next level mapping for this scene");
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Calculate the player and enemy populations. 
    private void calculatePop()
    {
        playerPop = 0;
        enemyPop = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Node curr = transform.GetChild(i).GetComponent<Node>();
            if (curr.state == Node.Control.Friendly)
            {
                playerPop += curr.population;
            }
            else if (curr.state == Node.Control.Enemy)
            {
                enemyPop += curr.population;
            }
        }
        for (int i = 0; i < unitParent.childCount; i++)
        {
            UnitController curr = unitParent.GetChild(i).GetComponent<UnitController>();
            if (curr.GetSource() == Node.Control.Friendly)
            {
                playerPop += curr.payload;
            }
            else if (curr.GetSource() == Node.Control.Enemy)
            {
                enemyPop += curr.payload;
            }
        }
    }

    public int getPlayerPop()
    {
        return playerPop;
    }

    public int getEnemyPop()
    {
        return enemyPop;
    }
}
