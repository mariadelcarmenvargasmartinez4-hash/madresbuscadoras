using UnityEngine;

/// <summary>
/// Obstáculo móvil (bola de picos o trampa con movimiento) que patrulla
/// entre dos puntos para poner en riesgo al jugador mientras busca las hojas.
/// </summary>
public class ObstaculoMovil : MonoBehaviour
{
    public enum TipoPatrulla
    {
        Horizontal,
        Vertical
    }

    [Header("Movimiento")]
    [Tooltip("Eje de movimiento: Horizontal o Vertical")]
    public TipoPatrulla tipoMovimiento = TipoPatrulla.Horizontal;
    [Tooltip("Distancia que recorre hacia cada lado desde su origen")]
    public float distancia = 3.5f;
    [Tooltip("Velocidad de desplazamiento")]
    public float velocidad = 2.2f;
    [Tooltip("Velocidad de rotación visual")]
    public float velocidadRotacion = 180f;

    [Header("Daño")]
    public float danio = 20f;
    public float fuerzaRebote = 5f;
    public float intervaloDanio = 1.0f;

    private Vector3 puntoOrigen;
    private float ultimoTiempoDanio = -10f;

    void Start()
    {
        puntoOrigen = transform.position;

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 9;
        }
    }

    void Update()
    {
        // Movimiento de vaivén suave (PingPong)
        float factor = Mathf.PingPong(Time.time * velocidad, distancia * 2f) - distancia;

        if (tipoMovimiento == TipoPatrulla.Horizontal)
        {
            transform.position = new Vector3(puntoOrigen.x + factor, puntoOrigen.y, puntoOrigen.z);
        }
        else
        {
            transform.position = new Vector3(puntoOrigen.x, puntoOrigen.y + factor, puntoOrigen.z);
        }

        // Rotación continua de la bola de picos para peligro visual
        transform.Rotate(0f, 0f, -velocidadRotacion * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ProcesarContacto(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarContacto(collision.gameObject);
    }

    private void ProcesarContacto(GameObject obj)
    {
        if (Time.time - ultimoTiempoDanio < intervaloDanio) return;

        var madre = obj.GetComponent<MovimientoMadre>();
        if (madre != null || obj.name.Contains("Madre"))
        {
            ultimoTiempoDanio = Time.time;

            if (ControladorNivel2.Instancia != null)
            {
                ControladorNivel2.Instancia.ModificarSalud(-danio);
            }

            if (madre != null)
            {
                Vector2 direccionEmpuje = (madre.transform.position - transform.position).normalized;
                direccionEmpuje.y = Mathf.Max(direccionEmpuje.y, 0.5f);
                madre.AplicarImpacto(direccionEmpuje * fuerzaRebote);
            }
        }
    }
}
