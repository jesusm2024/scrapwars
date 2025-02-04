using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    //making graph
    public Dictionary<Node, List<Node>> AdjList = new Dictionary<Node, List<Node>>();

    private List<Node> nodes = new List<Node>();
    private List<Node> selected = new List<Node>();

    private Dictionary<Node.Control, Color> colors;

    [Header("Unity Setup (don't change)")]
    //public GameObject unitPrefab;
    public GameObject friendlyRobotPrefab;
    public GameObject enemyRobotPrefab;
    private GameObject prefab;
    public Transform unitParent;


    // Start is called before the first frame update
    void Start()
    {

        for (int i = 0; i < transform.childCount; i++)
        {
            nodes.Add(transform.GetChild(i).GetComponent<Node>());
        }

        foreach (Node node in nodes)
        {
            AdjList.Add(node, node.adjacent);
        }

        colors = new Dictionary<Node.Control, Color>();
        colors.Add(Node.Control.Friendly, Color.blue);
        colors.Add(Node.Control.Enemy, Color.red);
        colors.Add(Node.Control.Neutral, Color.grey);
    }

    // Update is called once per frame
    void Update()
    {

        // if left mouse button, raycast and add hit nodes into list of selected
        // on mouse button up, raycast
        //      if over node, move from all selected nodes to end node
        //      else: deselect all nodes

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            // cast ray from mouse pos, if we hit a node add it to selected
            RaycastHit2D rayHit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (rayHit.collider != null)
            {
                Node hitNode = rayHit.collider.gameObject.GetComponent<Node>();
                if (hitNode.select())
                {
                    selected.Add(hitNode);
                }
            }

            // determine which node mouse is aiming at from each node in selected
            foreach (Node origin in selected)
            {
                RaycastHit2D aim = raycast2Mouse(origin, mousePos);

                if (aim.collider == null)
                {
                    origin.hideMoveArrow();
                    continue;
                }

                Node target = aim.collider.gameObject.GetComponent<Node>();

                if (!origin.adjacent.Contains(target)) 
                {
                    origin.hideMoveArrow();
                    continue;
                }

                origin.aimMoveArrow(target.transform.position);
                   
            }

           
            
        }

        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit2D rayHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (rayHit.collider != null)
            {
                Node end = rayHit.collider.gameObject.GetComponent<Node>();
                foreach (Node start in selected)
                {
                    // if state changed while node was selected, we ignore it
                    if (start.state != Node.Control.Friendly)
                    {
                        continue;
                    }

                    // if holding shift then send all
                    float fraction = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 1.0f : 0.5f;
                    startMove(start, end, fraction);

                }
            }
            // special case for movement, move if pointed towards valid node
            else if (selected.Count == 1)
            {
                RaycastHit2D aim = raycast2Mouse(selected[0], mousePos);
                if (aim.collider != null)
                {
                    Node target = aim.collider.gameObject.GetComponent<Node>();
                    if (selected[0].adjacent.Contains(target))
                    {
                        // if holding shift then send all
                        float fraction = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 1.0f : 0.5f;
                        startMove(selected[0], target, fraction);
                    }
                }
            }

            foreach (Node n in selected)
            {
                n.deselect();
            }
            selected.Clear();
        }

    }

    private RaycastHit2D raycast2Mouse(Node origin, Vector3 mousePos)
    {
        Vector3 dir = mousePos - origin.transform.position;
        dir.z = 0;
        dir = dir.normalized;

        // ray needs to start outside node's own collider
        Vector3 rayStart = origin.transform.position + dir * (origin.GetComponent<CircleCollider2D>().radius + 0.1f);
        return Physics2D.Raycast(rayStart, dir);
    }


    public void startMove(Node start, Node end, float percent = 0.5f)
    {
        if (start == end)
        {
            return;
        }

        if (!start.adjacent.Contains(end))
        {
            return;
        }

        if (start.IsCurrentlyUpgrading())
        {
            return;
        }

        int transfer = percent == 1f ? start.population - 1 : (int)(start.population * percent);        

        if (transfer <= 0)
        {
            return;
        }

        start.sub(transfer);

        // Prefab based on the state of the start node. 
        if (start.state == Node.Control.Friendly)
        {
            prefab = friendlyRobotPrefab;
        }
        else
        {
            prefab = enemyRobotPrefab;
        }

        GameObject unitObj = Instantiate(prefab, start.transform.position, Quaternion.identity, unitParent);
        // Flip prefab if end node to the left of start node. 
        if (end.transform.position.x < start.transform.position.x)
        {
            unitObj.transform.localScale = new Vector3(-1, 1, 1);
        }
        UnitController unit = unitObj.GetComponent<UnitController>();
        unit.setTarget(end, start.state);
        //unitObj.GetComponent<SpriteRenderer>().color = getNodeColor(start.state);

        // setting unit attributes
        unit.payload = transfer;
        unit.damage = start.unitDamage;
        unit.speed = start.unitSpeed;

    }

    public Color getNodeColor(Node.Control state)
    {
        return colors[state];
    }

}
