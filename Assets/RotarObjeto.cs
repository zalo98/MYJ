using UnityEngine;

public class RotarObjeto : MonoBehaviour
{
    [Header("Rotación")]
    public Vector3 velocidadRotacion = new Vector3(0, 50, 0);

    [Header("Flotación")]
    public float amplitudFlotacion = 0.5f; // Qué tan alto/bajo flota
    public float velocidadFlotacion = 1f;   // Qué tan rápido flota

    private Vector3 posicionInicial;

    void Start()
    {
        // Guardamos la posición inicial del objeto
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Rotación continua
        transform.Rotate(velocidadRotacion * Time.deltaTime);

        // Flotación usando seno
        float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(posicionInicial.x, nuevaY, posicionInicial.z);
    }
}
