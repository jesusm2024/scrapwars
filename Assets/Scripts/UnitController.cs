using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UnitController : MonoBehaviour
{

    public float speed = 2f;
    public float damage = 1f;

    public int payload;
    private TMP_Text payloadText;
    private Node.Control source;

    private Node target;
    private Vector3 v;
    private Vector3 startPos;
    //private GameObject robot;

    private void Start()
    {
        // Aassumes structure is Node -> Canvas -> Text
        //payloadText = transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        //Robot = transform.GetChild(1).gameObject;
    }


    // Update is called once per frame
    void Update()
    {
        transform.position += speed * Time.deltaTime * v;

        float targetDist = (target.transform.position - startPos).magnitude;
        float progress = (transform.position - startPos).magnitude;
        if (progress >= targetDist)
        {
            target.add(this);
            Destroy(gameObject);
        }

        //payloadText.text = string.Format("{0}", payload);
    }

    public void setTarget(Node node, Node.Control c)
    {
        target = node;
        source = c;

        Vector3 diff = target.transform.position - transform.position;
        v = diff.normalized;
        startPos = transform.position;

        // looks better if unit starts just in front of node
        transform.position += v * 0.5f;
    }

    public Node.Control GetSource()
    {
        return source;
    }
}
