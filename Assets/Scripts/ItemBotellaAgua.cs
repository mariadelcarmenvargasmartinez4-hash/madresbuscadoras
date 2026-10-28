using UnityEngine;

/// <summary>
/// Ítem recolectable de soporte vital: Botella de Agua.
/// Restaura +25 puntos a la salud/ánimo de la protagonista (hasta el máximo de 100).
/// Posee animación suave de flotación vertical, destello visual y efecto sonoro reconfortante.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemBotellaAgua : MonoBehaviour
{
    [Header("Configuración de Curación")]
    [Tooltip("Cantidad de salud/ánimo restaurada")]
    public float saludRestaurada = 25f;

    [Header("Animación de Flotación")]
    public float velocidadFlotacion = 2.4f;
    public float amplitudFlotacion = 0.12f;

    [Header("Efecto de Brillo")]
    public bool emitirPulsacion = true;

    private Vector3 posicionInicial;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool recolectado = false;
    private Transform transformJugador;

    void Start()
    {
        posicionInicial = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (sr != null)
        {
            sr.sortingOrder = 17;
        }

        var mb = FindFirstObjectByType<MovimientoMadre>();
        if (mb != null) transformJugador = mb.transform;
    }

    void Update()
    {
        if (recolectado) return;

        // 1. Flotación suave
        float nuevoY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(posicionInicial.x, nuevoY, posicionInicial.z);

        // 2. Destello pulsante y detección de proximidad
        if (sr != null && emitirPulsacion)
        {
            float pulso = 0.85f + 0.15f * Mathf.Sin(Time.time * 5f);
            if (transformJugador != null && Vector2.Distance(transform.position, transformJugador.position) < 3.0f)
            {
                // Destello más intenso cuando Esperanza está cerca
                sr.color = new Color(0.7f, 0.95f, 1f, 1f);
            }
            else
            {
                sr.color = new Color(pulso, pulso * 0.98f, 1f, 0.95f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectado) return;

        if (other.CompareTag("Player") || other.GetComponent<MovimientoMadre>() != null || other.name.Contains("Madre"))
        {
            recolectar();
        }
    }

    private void recolectar()
    {
        recolectado = true;

        if (ControladorNivel2.Instancia != null)
        {
            ControladorNivel2.Instancia.ConsumirBotellaAgua(saludRestaurada);
        }

        if (col != null) col.enabled = false;

        StartCoroutine(EfectoConsumo());
    }

    private System.Collections.IEnumerator EfectoConsumo()
    {
        Vector3 escalaInicial = transform.localScale;
        float tiempo = 0f;
        float duracion = 0.35f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            // Escala crece y se desvanece suavemente
            transform.localScale = escalaInicial * (1f + 0.45f * t);
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
