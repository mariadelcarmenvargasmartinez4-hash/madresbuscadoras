using UnityEngine;

/// <summary>
/// Cámara suave que sigue a la protagonista Madre Buscadora a lo largo del mundo 2D.
/// </summary>
public class CamaraSigue : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    public Transform objetivo;

    [Header("Ajustes de Seguimiento")]
    public float suavizado = 0.15f;
    public Vector3 offset = new Vector3(0f, 1.2f, -10f);

    [Header("Límites del Escenario (Opcional)")]
    public bool usarLimites = false;
    public float minX = -85f;
    public float maxX = 2f;
    public float minY = -26f;
    public float maxY = 15f;

    private Vector3 velocidadRef = Vector3.zero;

    void Start()
    {
        BuscarJugador();
    }

    private void BuscarJugador()
    {
        if (objetivo == null)
        {
            var mb = FindFirstObjectByType<MovimientoMadre>();
            if (mb != null)
            {
                objetivo = mb.transform;
            }
            else
            {
                GameObject jugador = GameObject.Find("MadreBuscadora");
                if (jugador != null) objetivo = jugador.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (objetivo == null)
        {
            BuscarJugador();
            if (objetivo == null) return;
        }

        Vector3 posDeseada = objetivo.position + offset;

        if (usarLimites)
        {
            posDeseada.x = Mathf.Clamp(posDeseada.x, minX, maxX);
            posDeseada.y = Mathf.Clamp(posDeseada.y, minY, maxY);
        }

        posDeseada.z = -10f;

        transform.position = Vector3.SmoothDamp(transform.position, posDeseada, ref velocidadRef, suavizado);
    }
}
