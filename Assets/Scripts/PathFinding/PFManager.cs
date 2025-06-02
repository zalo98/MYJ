using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

public enum PFAlgorithm
{
    BFS,
    DFS,
    Dijkstra,
    GreedyBestFirst,
    Astar,
    AstarPS,
    DijkstraSatisfy
}

public class PFManager : MonoBehaviour
{
    public static PFManager Instance { get; private set; }
    [SerializeField] PFEntity entity;
    [SerializeField] PFNodeGrid grid;
    [SerializeField] float distanceToTarget;
    [SerializeField] Color startColor, endColor;
    [SerializeField] PFAlgorithm algorithm;
    [SerializeField] LayerMask walls;

    private void Awake()
    {
        Instance = this;
    }
    
    public void SetPath(PFNodes end)
    {
        foreach (var node in grid.nodeGrid) { node.Color = Color.black; }
        var startNode = grid.nodeGrid.
            Where(x => (x.transform.position - entity.transform.position).sqrMagnitude <= entity.reachDistance * entity.reachDistance)
               .OrderBy(x => (x.transform.position - entity.transform.position).sqrMagnitude).FirstOrDefault();

        var path = new List<PFNodes>();
        switch (algorithm)
        {
            case PFAlgorithm.BFS:
                path = PathFinding.BFS(startNode, end);
                break;
            case PFAlgorithm.DFS:
                path = PathFinding.DFS(startNode, end);
                break;
            case PFAlgorithm.Dijkstra:
                path = PathFinding.Dijkstra(startNode, end);
                break;
            case PFAlgorithm.GreedyBestFirst:
                path = PathFinding.GreedyBestFirst(startNode, end);
                break;
            case PFAlgorithm.Astar:
                path = PathFinding.Astar(startNode, end, walls);
                break;
            case PFAlgorithm.AstarPS:
                path = PathFinding.AstarPS(startNode, end, walls);
                break;
            case PFAlgorithm.DijkstraSatisfy:
                path = PathFinding.Dijkstra(startNode,
                //    n => PathFinding.LineOfSight(n.transform.position, end.transform.position, walls)
                //&& Vector3.Distance(n.transform.position, end.transform.position) < distanceToTarget, 
                n => n.Cost > 4,
                walls);
                break;
        }
         
        for (int i = 0; i < path.Count; i++)
        {
            path[i].Color = Color.Lerp(startColor, endColor, i / (float)path.Count);
        }
        entity.SetPath = path;
    }

    
    

    void Imprimir(string x) => Debug.Log(x); 
}
