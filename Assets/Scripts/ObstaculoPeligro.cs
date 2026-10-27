using UnityEngine;

/// <summary>
/// Obstáculo del escenario (espinas, barricadas, escombros burocráticos)
/// que resta Ánimo / Salud a la protagonista al entrar en contacto.
/// </summary>
public class ObstaculoPeligro : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Cantidad de Ánimo/Vida que resta al tocarlo")]
    public float danio = 20f;
    [Tooltip("Fuerza de empuje hacia atrás")]
    public float fuerzaRebote = 4f;
    [Tooltip("Tiempo mínimo entre impactos sucesivos")]
    public float intervaloDanio = 1.2f;

    private float ultimoTiempoDanio = -10f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarContacto(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ProcesarContacto(other.gameObject);
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
                direccionEmpuje.y = Mathf.Max(direccionEmpuje.y, 0.4f); // Asegurar empuje hacia arriba
                madre.AplicarImpacto(direccionEmpuje * fuerzaRebote);
            }
        }
    }
}
