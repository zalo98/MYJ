using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFNodes : MonoBehaviour
{
    [SerializeField]
    private List<PFNodes> neighbors = new List<PFNodes>();
    [SerializeField] private int widthPos;
    [SerializeField] private int heightPos;
    [SerializeField] private float cost;
    [SerializeField] private bool blocked;
    private Renderer rend;

    public bool Blocked => blocked;
    public List<PFNodes> Neighbors
    {
        get { return neighbors; }
    }

    public Color Color
    {
        set { rend.material.color = value; }
    }
    public int x { get { return widthPos; } }
    public int y { get { return heightPos; } }
    public float Cost { get { return cost; } }

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetIndexes(int w, int h)
    {
        widthPos = w;
        heightPos = h;
    }
    public void SetNeighbors(List<PFNodes> neighbors)
    {
        this.neighbors = neighbors;
    }

    private void OnMouseDown()
    {
        PFManager.Instance.SetPath(this);
    }
}
