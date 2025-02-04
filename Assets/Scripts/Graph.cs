using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    //public Dictionary<Node, List<Node>> AdjList = new Dictionary<Node, List<Node>>();
    public List<Node> nodes;
   // public List<Node[]> edges;


    public GameObject linePrefab;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            nodes.Add(transform.GetChild(i).GetComponent<Node>());
        }

        List<Node> done = new List<Node> ();
        foreach (Node node in nodes)
        {
            //AdjList.Add(node, node.adjacent);
            foreach (Node adj in node.adjacent)
            {
                if (!adj.adjacent.Contains(node))
                {
                    adj.adjacent.Add(node);
                    if (done.Contains(adj))
                    {
                        GameObject line = Instantiate(linePrefab);
                        LineRenderer lrend = line.GetComponent<LineRenderer>();
                        lrend.SetPosition(0, node.transform.position);
                        lrend.SetPosition(1, adj.transform.position);
                    }
                    
                }
                if (!done.Contains(adj))
                {
                    GameObject line = Instantiate(linePrefab);
                    LineRenderer lrend = line.GetComponent<LineRenderer>();
                    lrend.SetPosition(0, node.transform.position);
                    lrend.SetPosition(1, adj.transform.position);
                }
            }
            done.Add(node);

        }
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DrawLine(Node src, Node dst)
    {
        return;
    }
}
