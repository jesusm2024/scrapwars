using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    private List<Node> EnemyNodes = new List<Node>();
    private List<Node> DstNodes = new List<Node>();

    public float moveInterval = 2f;
    public float startDelay = 1f;
    private float moveTimer = 0f; // starts negative so there's an initial delay before enemy moves

    public MoveManager moveManager;
    public int difficulty = 2;

    private void Start()
    {
        moveTimer -= startDelay;
    }

    // Update is called once per frame
    void Update()
    {
        moveTimer += Time.deltaTime;

        if (moveTimer > moveInterval)
        {
            moveTimer = 0f;

            updateNodeLists();
            if (difficulty > 1)
            {
                foreach (Node src in EnemyNodes)
                {
                    float rand = Random.Range(0, 1f);

                    if (rand < 0.7f)
                    {
                        tryAttack(src);
                    }
                    else if (rand < 0.88f && rand >= 0.7f)
                    {
                        tryReinforce(src);
                    }
                    else
                    {
                        tryUpgrade(src);
                    }

                }
            }

        }
    }

    private void tryAttack(Node src)
    {   
        // Won't crash if enenmy wins. 
        if (DstNodes == new List<Node>())
        {
            return;
        }

        Node dst = src;
        // finds the thing that is easiest to attack
        foreach (Node adj in src.adjacent)
        {
            if (adj.population < dst.population && adj.state != src.state)
            {
                dst = adj;
            }
        }

        // chance that it goes all out vs just sending half
        float rand = Random.Range(0, 1f);
        if (rand < 0.8f)
        {
            if (src.population > (dst.population + 1) * 2)
            {
                moveManager.startMove(src, dst);
            }
        }
        else
        {
            if (src.population > (dst.population + 10))
            {

                moveManager.startMove(src, dst, 1);
            }
        }
        
    }

    

    private void tryReinforce(Node src)
    {
        Node mostInNeed = src;
        // finds the thing that needs the most reinforcing
        foreach (Node adj in src.adjacent)
        {
            if (adj.enemyAdj() > src.enemyAdj() && adj.state == src.state && adj.metal > 0)
            {
                mostInNeed = adj;
            }
        }

        if (mostInNeed == src)
        {
            int dstIndex = Random.Range(0, EnemyNodes.Count);
            if (EnemyNodes[dstIndex].metal > 0)
            {
                mostInNeed = EnemyNodes[dstIndex];
            }

        }

        if (src.metal <= 0)
        {
            moveManager.startMove(src, mostInNeed, 1);
            //(src.population - 1)/(src.population)
        }
        else
        {
            float rand = Random.Range(0, 1f);
            if (rand < 0.5f)
            {
                moveManager.startMove(src, mostInNeed, 0.25f);
            }
            else if (src.population > mostInNeed.population)
            {
                moveManager.startMove(src, mostInNeed);
            }
        }
    }

    private void tryUpgrade(Node src)
    {
        switch (src.nodeType)
        {
            case Node.UpgradeTrack.None:
                //if metal > population go growth
                if (src.metal > src.population*1.5 && src.metal > 30)
                {
                    src.startGrowthUpgrade();
                }
                else
                {
                    float rand = Random.Range(0, 1f);
                    if (rand < 0.5f)
                    {
                        src.startAttackUpgrade();
                    }
                    else
                    {
                        src.startAttackUpgrade();
                    }
                }
                //else 50/50 atk/def
                break;

            case Node.UpgradeTrack.Growth:
                if (src.population < 30 && src.metal > 60)
                {
                    src.startGrowthUpgrade();
                }
                    
                break;

            case Node.UpgradeTrack.Attack:

                if (src.population > 40)
                {
                    src.startAttackUpgrade();
                }
                break;

            case Node.UpgradeTrack.Defense:

                if (src.population < 30)
                {
                    //src.startDefenseUpgrade();
                }
                //
                break;
        }
    }

    private void updateNodeLists()
    {
        EnemyNodes.Clear();
        DstNodes.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Node.Control state = transform.GetChild(i).GetComponent<Node>().state;
            if (state == Node.Control.Enemy)
            {
                EnemyNodes.Add(transform.GetChild(i).GetComponent<Node>());
            }
            else
            {
                DstNodes.Add(transform.GetChild(i).GetComponent<Node>());
            }
        }
    }
}
