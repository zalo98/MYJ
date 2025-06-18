using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathFinding
{
    public static List<PFNodes> BFS(PFNodes start, PFNodes end)
    {
        var frontier = new Queue<PFNodes>();
        frontier.Enqueue(start);
        Dictionary<PFNodes, PFNodes> cameFrom = new() { { start, null } };

        while(frontier.Count > 0)
        {
            PFNodes currentInSearch = frontier.Dequeue();
            currentInSearch.Color = Color.gray;

            if(currentInSearch == end)
            {
                //Creación del Path
                PFNodes currentInPath = end;
                List<PFNodes> path = new();
                while (currentInPath != null) //Podemos preguntar que sea distinto de start para no agregarlo
                {
                    path.Add(currentInPath);
                    currentInPath = cameFrom[currentInPath];
                }
                path.Reverse();
                return path; 
            }

            for (int i = 0; i < currentInSearch.Neighbors.Count; i++)
            {
                //Podríamos hacer la salida anticipada en esta instancia donde se encuentra el final como vecino del actual
                var next = currentInSearch.Neighbors[i];
                if (next.Blocked) continue;
                if(!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom.Add(next, currentInSearch);
                }
            }
        }
        return new();
    }

    public static List<PFNodes> DFS(PFNodes start, PFNodes end)
    {
        var frontier = new Stack<PFNodes>();
        frontier.Push(start);
        Dictionary<PFNodes, PFNodes> cameFrom = new() { { start, null } };

        while (frontier.Count > 0)
        {
            PFNodes currentInSearch = frontier.Pop();
            currentInSearch.GetComponent<Renderer>().material.color = Color.cyan;
            if (currentInSearch == end)
            {
                //Creación del Path
                PFNodes currentInPath = end;
                List<PFNodes> path = new();
                while (currentInPath != null) //Podemos preguntar que sea distinto de start para no agregarlo
                {
                    path.Add(currentInPath);
                    currentInPath = cameFrom[currentInPath];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < currentInSearch.Neighbors.Count; i++)
            {
                var next = currentInSearch.Neighbors[i];
                if (next.Blocked) continue;

                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Push(next);
                    cameFrom.Add(next, currentInSearch);
                }
            }
        }

        return null;
    }

    public static List<PFNodes> Dijkstra(PFNodes start, PFNodes end)
    { 
        var frontier = new PriorityQueue<PFNodes>();
        frontier.Enqueue(start, 0);
        Dictionary<PFNodes, float> costSoFar = new() { { start, 0 } };
        Dictionary<PFNodes, PFNodes> cameFrom = new() { { start, null } };

        while (!frontier.IsEmpty)
        {
            PFNodes currentInSearch = frontier.Dequeue();
            currentInSearch.Color = Color.Lerp(Color.yellow, Color.red, currentInSearch.Cost/5);

            if (currentInSearch == end)
            {
                //Creación del Path
                PFNodes currentInPath = end;
                List<PFNodes> path = new();
                while (currentInPath != null) //Podemos preguntar que sea distinto de start para no agregarlo
                {
                    path.Add(currentInPath);
                    currentInPath = cameFrom[currentInPath];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < currentInSearch.Neighbors.Count; i++)
            {
                var next = currentInSearch.Neighbors[i];
                if (next.Blocked) continue;
                float newCost = costSoFar[currentInSearch] + next.Cost;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    frontier.Enqueue(next, newCost);
                    cameFrom[next] = currentInSearch;
                }
            }
        }
        return new();
    }

    private static float Heuristic(PFNodes a, PFNodes b)
    {
        return Mathf.Abs(a.x - b.x) +
             Mathf.Abs(a.y - b.y);
    }

    public static List<PFNodes> GreedyBestFirst(PFNodes start, PFNodes end)
    {
        var frontier = new PriorityQueue<PFNodes>();
        frontier.Enqueue(start, 0);
        Dictionary<PFNodes, PFNodes> cameFrom = new() { { start, null } };

        while (!frontier.IsEmpty)
        {
            PFNodes currentInSearch = frontier.Dequeue();
            currentInSearch.Color = Color.gray;

            if (currentInSearch == end)
            {
                //Creación del Path
                PFNodes currentInPath = end;
                List<PFNodes> path = new();
                while (currentInPath != null) //Podemos preguntar que sea distinto de start para no agregarlo
                {
                    path.Add(currentInPath);
                    currentInPath = cameFrom[currentInPath];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < currentInSearch.Neighbors.Count; i++)
            {
                var next = currentInSearch.Neighbors[i];
                if (next.Blocked) continue;
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next, Heuristic(end, next));
                    cameFrom.Add(next, currentInSearch);
                }
            }
        }
        return new();
    }
    public static List<PFNodes> Astar(PFNodes start, PFNodes end, LayerMask mask)
    {
        var frontier = new PriorityQueue<PFNodes>();
        frontier.Enqueue(start, 0);
        Dictionary<PFNodes, float> costSoFar = new() { { start, 0 } };
        Dictionary<PFNodes, PFNodes> cameFrom = new() { { start, null } };

        while (!frontier.IsEmpty)
        {
            PFNodes currentInSearch = frontier.Dequeue();
            currentInSearch.Color = Color.Lerp(Color.yellow, Color.red, currentInSearch.Cost / 5);

            if (currentInSearch == end)
            {
                //Creación del Path
                PFNodes currentInPath = end;
                List<PFNodes> path = new();
                while (currentInPath != null) //Podemos preguntar que sea distinto de start para no agregarlo
                {
                    path.Add(currentInPath);
                    currentInPath = cameFrom[currentInPath];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < currentInSearch.Neighbors.Count; i++)
            {
                var next = currentInSearch.Neighbors[i];
                if (next.Blocked || !LineOfSight(currentInSearch.transform.position, next.transform.position, mask)) continue;
                float newCost = costSoFar[currentInSearch] + next.Cost;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    frontier.Enqueue(next, newCost + Heuristic(end, next));
                    cameFrom[next] = currentInSearch;
                }
            }
        }
        return new();
    }
    /* private bool LineOfSight(PFNodes aNode, PFNodes bNode)
     {
         int ax = aNode.x;
         int ay = aNode.y;
         int bx = bNode.x;
         int by = bNode.y;

         var x0 = ax;
         var y0 = ay;
         var x1 = bx;
         var y1 = by;

         var dy = y1 - y0;
         var dx = x1 - x0;

         var f = 0;

         if (dy < 0) {
             dy = -dy;
             ay = -1;
         }
         else
             ay = 1;

         if (dx < 0)
         {
             dx = -dx;
             ax = -1;
         }
         else
             ax = 1;

         if (dx >= dy)
         {
             while (x0 != x1) 
             {
                 f = f + dy;
                 if (f >= dx) {
                     if grid(x0 + ((s.x - 1) / 2), y0 + ((s.y - 1) / 2)): 
                     return false;
                     y0 = y0 + ay;
                     f = f - dx;
                         }
                 if f != 0 AND grid(x0+((s.x - 1) / 2), y0 + ((s.y - 1) / 2)):
                 return false
                 if dy = 0 AND grid(x0 +((s.x - 1) / 2), y0) AND grid(x0 +((s.x - 1), 2), y0 - 1):
                 return false
                 x0 = x0 + s.x
             } 
         }
         else
         {
             while (y0 != y1):
             f = f + dx
             if (f >= dy)
                 if grid(x0 + ((s.x - 1) / 2), y0 + ((s.y - 1) / 2)) :
                     return false;
             x0 = x0 + s.x
                 f = f - dy
             if f != 0 AND grid(x0+((s.x - 1) / 2), y0 + ((s.y - 1) / 2)):
                 return false;
             if dx = 0 AND grid(x0, y0+((s.y - 1) / 2)) AND grid(x0-1 , y0 + ((s.y - 1) / 2)):
                 return false
             y0 = y0 + s.y
         }
     return true

     }*/
    public static bool LineOfSight(Vector3 nodeA, Vector3 nodeB, LayerMask mask)
    {
        return !Physics.Linecast(nodeA, nodeB, mask);
    }
    public static List<PFNodes> AstarPS(PFNodes start, PFNodes end, LayerMask walls)
    {
        var path = Astar(start, end, walls);
        int current = 0;
        while (current + 2 < path.Count)
        {
            Vector3 a = path[current].transform.position;
            Vector3 b = path[current + 2].transform.position;
            if (LineOfSight(a, b, walls))
                path.RemoveAt(current + 1);
            else
                current++;
        }
        return path;

    }

    public static List<PFNodes> Dijkstra(PFNodes start, Func<PFNodes, bool> Predicate, LayerMask mask)
    {
        var frontier = new PriorityQueue<PFNodes>();
        frontier.Enqueue(start, 0);
        Dictionary<PFNodes, float> costSoFar = new() { { start, 0 } };
        Dictionary<PFNodes, PFNodes> cameFrom = new() { { start, null } };

        while (!frontier.IsEmpty)
        {
            PFNodes currentInSearch = frontier.Dequeue();
            currentInSearch.Color = Color.Lerp(Color.yellow, Color.red, currentInSearch.Cost / 5);

            if (Predicate(currentInSearch))
            {
                //Creación del Path
                PFNodes currentInPath = currentInSearch;
                List<PFNodes> path = new();
                while (currentInPath != null) //Podemos preguntar que sea distinto de start para no agregarlo
                {
                    path.Add(currentInPath);
                    currentInPath = cameFrom[currentInPath];
                }
                path.Reverse();
                return path;
            }

            for (int i = 0; i < currentInSearch.Neighbors.Count; i++)
            {
                var next = currentInSearch.Neighbors[i];
                if (next.Blocked || !LineOfSight(currentInSearch.transform.position, next.transform.position, mask)) continue;
                float newCost = costSoFar[currentInSearch] + next.Cost;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    frontier.Enqueue(next, newCost);
                    cameFrom[next] = currentInSearch;
                }
            }
        }
        return new();
    }
}
