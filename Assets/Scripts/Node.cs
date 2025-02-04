using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    public enum Control { Neutral, Friendly, Enemy }

    [Header("Main Node State")]
    public Control state = Control.Neutral; // public for testing
    private SpriteRenderer nodeSprite;
    private Dictionary<Control, Color> colors;

    public int population; // public for testing
    private TMP_Text populationText;

    public int metal = 100;
    private TMP_Text metalText;


    [Header("Upgradable Attributes")]

    // controls how node population grows over time
    public float growthInterval = 1f;
    public int growthVal = 2;
    private float growthTimer = 0f;

    // how fast units move from node to node
    public float unitSpeed = 2f;
    // how many enemies killed per unit (attacking)
    public float unitDamage = 1f;
    // how many enemies killed per unit (defending)
    public float defense = 1f;

    public enum UpgradeTrack { None, Growth, Attack, Defense }
    [HideInInspector]
    public UpgradeTrack nodeType = UpgradeTrack.None;
    [HideInInspector]
    public int nodeLevel = 0;

    private float upgradeTimer = 0f;
    private float lastUpgradeCost = -1f;
    private UpgradeTrack lastUpgradeType = UpgradeTrack.None;
    private Image upgradeProgress;

    // Population and Scrap Metal Icons 
    private GameObject PopIcon;
    private GameObject MetalIcon;

    [Serializable]
    public struct AttackUpgrade
    {
        public float speed;
        public float damage;
        public int cost; // cost represents seconds to upgrade if population == 100
        public int req;
    }
    public List<AttackUpgrade> attackUpgrades;
    [Serializable]
    public struct DefenseUpgrade
    {
        public float value;
        public int cost;
        public int req;
    }
    public List<DefenseUpgrade> defUpgrades;
    [Serializable]
    public struct GrowthUpgrade
    {
        public int value;
        public int cost;
        public int req;
    }
    public List<GrowthUpgrade> growthUpgrades;

    private Button GrowthButton;
    private Button AttackButton;
    private Button DefenseButton;
    private DisplayPopUp popUp;

    [Header("Icons")]
    public List<Sprite> GrowthIcons;
    public List<Sprite> AttackIcons;
    public List<Sprite> DefenseIcons;
    private List<Sprite> currentIcons;
    private SpriteRenderer LevelIcon;

    // Last Text Positions
    private Vector3 popTextPos;
    private Vector3 metalTextPos;

  


    // adjacency list for a graph
    public List<Node> adjacent = new List<Node>();

    private bool selected = false;
    private SpriteRenderer selectIcon;
    private Transform moveArrow;

    // Start is called before the first frame update
    private void Start()
    {
        Setup();

        selectIcon.enabled = false;
        hideMoveArrow();
    }

    public void Setup()
    {
        Transform canvas = transform.Find("Canvas");

        // assumes structure is Node -> Canvas -> Text
        populationText = canvas.Find("Population Text").GetComponent<TMP_Text>();
        metalText = canvas.Find("Metal Text").GetComponent<TMP_Text>();
        //nodeSprite = GetComponent<SpriteRenderer>();
        nodeSprite = transform.Find("Building Background").GetComponent<SpriteRenderer>();
        selectIcon = transform.Find("Node Select Icon").GetComponent<SpriteRenderer>();
        moveArrow = transform.Find("Move Arrow");

        // Color based on state. 
        colors = new Dictionary<Control, Color>();
        colors.Add(Control.Friendly, Color.blue);
        colors.Add(Control.Enemy, Color.red);
        colors.Add(Control.Neutral, Color.grey);
        
        // Upgrade Buttons
        GrowthButton = canvas.GetChild(4).GetComponent<Button>();  
        GrowthButton.gameObject.SetActive(false);
        GrowthButton.onClick.AddListener(startGrowthUpgrade);

        AttackButton = canvas.GetChild(5).GetComponent<Button>();
        AttackButton.gameObject.SetActive(false);
        AttackButton.onClick.AddListener(startAttackUpgrade);

        // Setting up display panel for buttons
        popUp = GameObject.Find("Upgrade Description").GetComponent<DisplayPopUp>();
        EventTrigger growthTrigger = GrowthButton.GetComponent<EventTrigger>();
        EventTrigger.Entry showGrowthEntry = new EventTrigger.Entry();
        showGrowthEntry.eventID = EventTriggerType.PointerEnter;
        showGrowthEntry.callback.AddListener((eventData) => { popUp.DisplayGrowthInfo(this); });
        growthTrigger.triggers.Add(showGrowthEntry);

        EventTrigger attackTrigger = AttackButton.GetComponent<EventTrigger>();
        EventTrigger.Entry showAttackEntry = new EventTrigger.Entry();
        showAttackEntry.eventID = EventTriggerType.PointerEnter;
        showAttackEntry.callback.AddListener((eventData) => { popUp.DisplayAttackInfo(this); });
        attackTrigger.triggers.Add(showAttackEntry);

        EventTrigger.Entry hideEntry = new EventTrigger.Entry();
        hideEntry.eventID = EventTriggerType.PointerExit;
        hideEntry.callback.AddListener((eventData) => { popUp.HideDisplay(); });
        growthTrigger.triggers.Add(hideEntry);
        attackTrigger.triggers.Add(hideEntry);


        //defUpButton = transform.Find("Canvas").GetChild(4).GetComponent<Button>();
        //defUpButton.gameObject.SetActive(false);
        //defUpButton.onClick.AddListener(speedUpgrade);

        // Update Progress Icon
        upgradeProgress = transform.Find("Canvas").Find("Upgrade Progress").GetComponent<Image>();

        // Population and Scrap Metal Icons 
        PopIcon = transform.Find("Canvas").Find("Population Icon").gameObject;
        MetalIcon = transform.Find("Canvas").Find("Metal Icon").gameObject;
        PopIcon.SetActive(false);
        MetalIcon.SetActive(false);

        // Level Icon 
        LevelIcon = transform.Find("Level Icon").GetComponent<SpriteRenderer>();

        // Last Text Position
        popTextPos = populationText.transform.localPosition;
        metalTextPos = metalText.transform.localPosition;

    }

    // Update is called once per frame
    private void Update()
    {

        switch (state)
        {
            case Control.Neutral:
                nodeSprite.color = new Color(0,0,0,0);
                upgradeProgress.color = new Color(0, 0, 0, 0);
                break;

            case Control.Friendly:
                drawUpgradeButtons();
                populationText.color = Color.blue;
                upgradeProgress.color = Color.blue;
                tickGrowth();
                displayIcons();
                break;

            case Control.Enemy:
                deselect();
                nodeSprite.color = Color.red;
                populationText.color = Color.red;
                upgradeProgress.color = Color.red;
                tickGrowth();
                break;
        }

        Render();

    }

    public void aimMoveArrow(Vector3 target)
    {
        moveArrow.GetComponent<SpriteRenderer>().enabled = true;

        // moves arrow around node
        Vector3 dir = new Vector3(target.x - transform.position.x, target.y - transform.position.y, 0);
        moveArrow.localPosition = dir.normalized * 1.45f;

        // rotates arrow to point at mouse position
        float angle = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x) * Mathf.Rad2Deg;
        moveArrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }

    public void hideMoveArrow()
    {
        moveArrow.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void Render()
    {
        populationText.text = string.Format("{0}", population);
        // only show metal value if metal > 0
        metalText.text = metal == 0 ? "" : string.Format("{0}", metal);
        // Move text to center for enemy and neutral nodes. 
        if (state != Control.Friendly)
        {
            populationText.transform.localPosition = new Vector3(0, -120, 0);
            metalText.transform.localPosition = new Vector3(0, 110, 0);
        }
        // Move text back for friendly node. 
        else
        {
            populationText.transform.localPosition = popTextPos;
            metalText.transform.localPosition = metalTextPos;
        }
        nodeSprite.color = colors[state];
    }

    private void tickGrowth()
    {
        growthTimer += Time.deltaTime;
        if (growthTimer >= growthInterval && metal > 0)
        {
            int tick = Mathf.Min(metal, growthVal);
            population += tick;
            metal -= tick;
            growthTimer = 0f;
        }

        // tick for upgrades
        upgradeTimer -= Time.deltaTime * (population * 0.01f);
        // sets progress bar to correct proportion
        upgradeProgress.fillAmount = Mathf.Max(0, upgradeTimer) / lastUpgradeCost;

        if (upgradeTimer <= 0 && lastUpgradeCost > 0)
        {
            // upgrade
            switch (lastUpgradeType)
            {
                case UpgradeTrack.Attack:
                    attackUpgrade();
                    break;
                case UpgradeTrack.Defense:
                    
                    break;
                case UpgradeTrack.Growth:
                    growthUpgrade();
                    break;
                case UpgradeTrack.None:

                    break;
            }

            lastUpgradeType = UpgradeTrack.None;
            lastUpgradeCost = -1;

        }
    }

    private void displayIcons()
    {
        PopIcon.SetActive(true);
        MetalIcon.SetActive(true);
    }

    public bool IsCurrentlyUpgrading()
    {
        return upgradeTimer > 0;
    }

    public void add(UnitController enemy)
    {
        if (enemy.GetSource() == state)
        {
            population += enemy.payload;
            return;
        }

        float homePower = population * defense;
        float enemyPower = enemy.payload * enemy.damage;

        int start_pop = population;

        if (homePower >= enemyPower)
        {
            int lost = Mathf.FloorToInt((enemy.payload / defense) * enemy.damage);

            population -= lost;
            metal += enemy.payload + lost;
        }
        else
        {
            int lost = Mathf.FloorToInt((population / enemy.damage) * defense);

            state = enemy.GetSource();
            cancelUpgrade();

            population = enemy.payload - lost;
            metal += start_pop + lost;
        }
    }

    public void sub(int transfer)
    {
        population -= transfer;
    }

    public int enemyAdj()
    {
        int count = 0;
        foreach (Node adj in adjacent)
        {
            
            if (adj.state != this.state)
            {
                count += 1;
            }
        }
        return count;
    }

    // returns false if already selected
    public bool select()
    {
        //if (selected || state != Control.Friendly)
        if (selected)
        {
            return false;
        }

        selected = true;
        selectIcon.enabled = true;
        return true;
    }

    

    public void deselect()
    {
        selected = false;
        selectIcon.enabled = false;
        hideMoveArrow();
    }

    // draws upgrade buttons if upgrade is possible
    private void drawUpgradeButtons()
    {
        if (upgradeTimer > 0)
        {
            GrowthButton.gameObject.SetActive(false);
            AttackButton.gameObject.SetActive(false);
            return;
        }

        switch (nodeType)
        {
            case UpgradeTrack.None:
                GrowthButton.gameObject.SetActive(population >= growthUpgrades[0].req);
                AttackButton.gameObject.SetActive(population >= attackUpgrades[0].req);
                break;

            case UpgradeTrack.Growth:
                AttackButton.gameObject.SetActive(false);
                bool canUpgradeGrowth = nodeLevel < growthUpgrades.Count && population >= growthUpgrades[nodeLevel].req;
                GrowthButton.gameObject.SetActive(canUpgradeGrowth);
                break;

            case UpgradeTrack.Attack:
                GrowthButton.gameObject.SetActive(false);
                bool canUpgradeSpeed = nodeLevel < attackUpgrades.Count && population >= attackUpgrades[nodeLevel].req;
                AttackButton.gameObject.SetActive(canUpgradeSpeed);
                break;

            case UpgradeTrack.Defense:
                //defUpButton.gameObject.SetActive(false);
                bool canUpgradeDef = nodeLevel < defUpgrades.Count && population >= defUpgrades[nodeLevel].req;

                //defUpButton.gameObject.SetActive(canUpgradeDef);
                break;
        }
    }

    private void cancelUpgrade()
    {
        upgradeTimer = -1;
        lastUpgradeCost = -1;
        lastUpgradeType = UpgradeTrack.None;
    }

    public void startGrowthUpgrade()
    {
        if (IsCurrentlyUpgrading())
        {
            return;
        }
        if (nodeLevel > growthUpgrades.Count)
        {
            Debug.Log("Growth at max level, cannot upgrade.");
            return;
        }

        int index = Mathf.Max(0, nodeLevel - 1);

        if (population < growthUpgrades[index].req)
        {
            Debug.Log("Don't meet population requirement, cannot upgrade.");
            return;
        }

        upgradeTimer = growthUpgrades[index].cost;
        lastUpgradeCost = growthUpgrades[index].cost;
        lastUpgradeType = UpgradeTrack.Growth;

        popUp.HideDisplay();
    }
   

    // Upgrade growth rate of node.
    private void growthUpgrade()
    {
        // Level must be increased when button is pressed. 
        nodeLevel++;

        if (nodeLevel > growthUpgrades.Count)
        {
            Debug.Log("Growth at max level, cannot upgrade.");
            return;
        }


        if (nodeType == UpgradeTrack.None)
        {
            nodeType = UpgradeTrack.Growth;
            currentIcons = GrowthIcons;
        }
   

        growthVal += growthUpgrades[nodeLevel - 1].value;
        // population -= growthUpgrades[nodeLevel].cost;

        if (nodeLevel > 0 && nodeLevel <= currentIcons.Count)
        {
            LevelIcon.sprite = currentIcons[nodeLevel - 1];
        }
    }

    public void startAttackUpgrade()
    {
        if (IsCurrentlyUpgrading())
        {
            return;
        }
        if (nodeLevel > attackUpgrades.Count)
        {
            Debug.Log("Attack at max level, cannot upgrade.");
            return;
        }

        int index = Mathf.Max(0, nodeLevel - 1);

        if (population < attackUpgrades[index].req)
        {
            Debug.Log("Don't meet population requirement, cannot upgrade.");
            return;
        }

        upgradeTimer = attackUpgrades[index].cost;
        lastUpgradeCost = attackUpgrades[index].cost;
        lastUpgradeType = UpgradeTrack.Attack;

        popUp.HideDisplay();
    }

    private void attackUpgrade()
    {
        // Level must be increased when button is pressed. 
        nodeLevel++;

        if (nodeLevel > attackUpgrades.Count)
        {
            Debug.Log("Attack at max level, cannot upgrade.");
            return;
        }


        if (nodeType == UpgradeTrack.None)
        {
            nodeType = UpgradeTrack.Attack;
            currentIcons = AttackIcons;
        }


        unitSpeed += attackUpgrades[nodeLevel - 1].speed;
        unitDamage += attackUpgrades[nodeLevel - 1].damage;
        // population -= attackUpgrades[nodeLevel].cost;

        if (nodeLevel > 0 && nodeLevel <= currentIcons.Count)
        {
            LevelIcon.sprite = currentIcons[nodeLevel - 1];
        }

    }

   

}

