using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PFNodeGrid : MonoBehaviour
{
    public PFNodes prefab;
    public PFNodes[] nodeGrid;
    public int width;
    public int height;
    public float distance;

    [ContextMenu("Crear Nodos")]
    public void SetNodeGrid()
    {
        nodeGrid = new PFNodes[width * height];
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                PFNodes newNode = Instantiate(prefab, transform.position + 
                    new Vector3(w * distance, 0, h * distance), Quaternion.identity, transform);
                newNode.SetIndexes(w, h);
                nodeGrid[w + h * width] = newNode;
            }
        }
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                List<PFNodes> neigh = new();
                if (w > 0) neigh.Add(nodeGrid[(w - 1) + h * width]);
                if (w < width - 1) neigh.Add(nodeGrid[(w + 1) + h * width]);
                if (h > 0) neigh.Add(nodeGrid[w + (h - 1) * width]);
                if (h < height - 1) neigh.Add(nodeGrid[w + (h + 1) * width]);
                nodeGrid[w + h * width].SetNeighbors(neigh);
            }
        }
    }
    [ContextMenu("Eliminar Nodos")]
    public void DeleteNodes()
    {
        for (int i = 0; i < nodeGrid.Length; i++)
        {
            DestroyImmediate(nodeGrid[i].gameObject);
        }
        nodeGrid = new PFNodes[0];
    }
}
