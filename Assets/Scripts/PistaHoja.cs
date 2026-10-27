using UnityEngine;

/// <summary>
/// Representa un fragmento o parte de hoja del expediente escondido en el escenario.
/// El jugador debe deducir su ubicación a través de los acertijos del HUD.
/// Al acercarse, la hoja emite un destello dorado sutil.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PistaHoja : MonoBehaviour
{
    [Header("Datos del Documento")]
    [Tooltip("Número de fragmento (1 a 4)")]
    public int numeroHoja = 1;
    [Tooltip("Título del fragmento")]
    public string titulo = "Ficha de Búsqueda";
    [TextArea(2, 4)]
    [Tooltip("Descripción / Acertijo / Pista que revela este fragmento")]
    public string pistaDetalle = "Datos de los 22 migrantes y su salida de San Luis de la Paz.";

    [Header("Efecto Visual")]
    public float velocidadFlotacion = 2.0f;
    public float amplitudFlotacion = 0.08f;

    private Vector3 posicionInicial;
    private Vector3 escalaBase;
    private bool recolectado = false;
    private SpriteRenderer sr;
    private Collider2D col;
    private Transform transformJugador;

    void Start()
    {
        posicionInicial = transform.position;
        escalaBase = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (sr != null)
        {
            sr.sortingOrder = 18;
            sr.color = new Color(1f, 0.95f, 0.8f, 0.95f);
        }

        var mb = FindFirstObjectByType<MovimientoMadre>();
        if (mb != null) transformJugador = mb.transform;
    }

    void Update()
    {
        if (recolectado) return;

        // Flotación sutil para que parezca un papel arrastrado por el viento
        float nuevoY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(posicionInicial.x, nuevoY, posicionInicial.z);

        // Si el jugador está cerca (menos de 3 metros), emitir un suave destello dorado
        if (sr != null && transformJugador != null)
        {
            float dist = Vector2.Distance(transform.position, transformJugador.position);
            if (dist < 3.5f)
            {
                float brillo = 0.85f + 0.15f * Mathf.Sin(Time.time * 6f);
                sr.color = new Color(1f, brillo, 0.4f, 1f);
            }
            else
            {
                sr.color = new Color(1f, 0.95f, 0.8f, 0.9f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectado) return;

        if (other.CompareTag("Player") || other.GetComponent<MovimientoMadre>() != null || other.name.Contains("Madre"))
        {
            recolectado = true;

            if (ControladorNivel2.Instancia != null)
            {
                ControladorNivel2.Instancia.RecolectarHoja(numeroHoja, titulo, pistaDetalle);
            }

            if (col != null) col.enabled = false;
            if (sr != null) sr.enabled = false;

            Destroy(gameObject, 0.5f);
        }
    }
}
