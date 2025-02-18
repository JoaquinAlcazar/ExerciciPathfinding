using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public BoxCollider2D Panel;
    public GameObject token;
    public GameObject startToken;
    public GameObject endToken;
    public GameObject lookedToken;
    public GameObject visitedToken;
    protected float lessHeuristic;
    //private int[,] GameMatrix; //0 not chosen, 1 player, 2 enemy de momento no hago nada con esto
    private Node[,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;
    private Node actualNode;
    private Node startNode;
    private Node endNode;
    List<Node> nodesWay = new List<Node>();
    private String nodeList = "Optimal way: ";
    void Awake()
    {
        Instance = this;
        //GameMatrix = new int[Size, Size];
        Debug.Log(Panel);
        Debug.Log(Size);
        Calculs.CalculateDistances(Panel, Size);
    }
    private void Start()
    {
        /*for(int i = 0; i<Size; i++)
        {
            for (int j = 0; j< Size; j++)
            {
                GameMatrix[i, j] = 0;
            }
        }*/
        
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while(endPosx== startPosx || endPosy== startPosy);

        //GameMatrix[startPosx, startPosy] = 2;
        //GameMatrix[startPosx, startPosy] = 1;
        NodeMatrix = new Node[Size, Size];
        CreateNodes();

        startNode = NodeMatrix[startPosx, startPosy];
        Debug.Log($"Start node: {startNode.PositionX}, {startNode.PositionY}");
        actualNode = startNode;

        endNode = NodeMatrix[endPosx, endPosy];
        Debug.Log($"End node: {endNode.PositionX}, {endNode.PositionY}");

        Instantiate(startToken, NodeMatrix[startPosx, startPosy].RealPosition, Quaternion.identity);
        Instantiate(endToken, NodeMatrix[endPosx, endPosy].RealPosition, Quaternion.identity);

        StartCoroutine(searcher());

        StartCoroutine(WayPainterLoop(nodesWay, startNode, endNode));


    }
    public void CreateNodes()
    {
        for(int i=0; i<Size; i++)
        {
            for(int j=0; j<Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i,j));
                NodeMatrix[i,j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i,j],endPosx,endPosy);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }
        DebugMatrix();
    }
    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);
                Debug.Log("Element (" + j + ", " + i + ")");
                Debug.Log("Position " + NodeMatrix[i, j].RealPosition);
                Debug.Log("Heuristic " + NodeMatrix[i, j].Heuristic);
                Debug.Log("Ways: ");
                foreach (var way in NodeMatrix[i, j].WayList)
                {
                    Debug.Log(" (" + way.NodeDestiny.PositionX + ", " + way.NodeDestiny.PositionY + ")");
                }
            }
        }
    }
    public void SetWays(Node node, int x, int y)
    {
        node.WayList = new List<Way>();
        if (x>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(x<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(y>0)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y<Size-1)
        {
            node.WayList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x>0)
            {
                node.WayList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x<Size-1)
            {
                node.WayList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public Node step(Node actualNode)
    {
        lessHeuristic = 1000;
        Node nextNode = null;
        foreach (var node in actualNode.WayList)
        {

            Instantiate(lookedToken, NodeMatrix[node.NodeDestiny.PositionX, node.NodeDestiny.PositionY].RealPosition, Quaternion.identity);
            if (node.NodeDestiny.Heuristic < lessHeuristic)
            {
                lessHeuristic = node.NodeDestiny.Heuristic;
                nextNode = node.NodeDestiny;
            }
        }

        Debug.Log($"Next node: {nextNode.PositionX}, {nextNode.PositionY}");
        return nextNode;
    }

    IEnumerator WayPainterLoop(List<Node> way, Node start, Node finish)
    {
        while (way.Count > 0 && way[way.Count - 1] != finish) // Se repite mientras no haya llegado al final
        {
            yield return StartCoroutine(WayPainter(way, start, finish));
            yield return new WaitForSeconds(1f);
        }
        foreach (var item in nodesWay)
        {
            nodeList += $"[{item.PositionX},{item.PositionY}], ";
        }
        Debug.Log(nodeList);
        yield break;
    }
    IEnumerator WayPainter(List<Node>way, Node start, Node finish)
    {

        Instantiate(startToken, NodeMatrix[startPosx, startPosy].RealPosition, Quaternion.identity);
        Instantiate(endToken, NodeMatrix[endPosx, endPosy].RealPosition, Quaternion.identity);
        foreach (Node node in way)
        {
            if (node.RealPosition != finish.RealPosition)
            Instantiate(visitedToken, NodeMatrix[node.PositionX, node.PositionY].RealPosition, Quaternion.identity);
        }

        yield return null;
    }

    IEnumerator searcher()
    {
        while (true) // Se repetirá indefinidamente
        {
            actualNode = step(actualNode);
            nodesWay.Add(actualNode);

            if (actualNode == endNode)
            { 
                StartCoroutine(WayPainter(nodesWay, startNode, endNode));
                yield break; // Detiene la corrutina cuando llega al nodo final
            }

            yield return new WaitForSeconds(1f);
        }
    }
}
