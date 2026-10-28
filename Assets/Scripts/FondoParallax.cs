using UnityEngine;

/// <summary>
/// Controla las capas de fondo temático del juego con un efecto Parallax multicapa suave
/// durante el recorrido y descenso de la Madre Buscadora.
/// Utiliza cálculo absoluto determinista, inmune a desincronizaciones por teletransporte o reinicio.
/// </summary>
public class FondoParallax : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform camara;

    [Header("Efecto Parallax Multicapa")]
    [Tooltip("Grado de desplazamiento horizontal respecto a la cámara (0 = estático en mundo, 1 = se mueve idéntico a cámara)")]
    [Range(0f, 1f)]
    public float factorX = 0.65f;
    [Tooltip("Grado de desplazamiento vertical respecto a la cámara")]
    [Range(0f, 1f)]
    public float factorY = 0.45f;

    [Header("Ajustes de Capa")]
    [Tooltip("Orden en la capa de renderizado (negativo para quedar siempre detrás de props y jugador)")]
    public int ordenRenderizado = -80;

    private Vector3 posInicialObjeto;
    private Vector3 posInicialCamara;
    private bool inicializado = false;

    void Awake()
    {
        posInicialObjeto = transform.position;
    }

    void Start()
    {
        Inicializar();
    }

    private void Inicializar()
    {
        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }

        if (camara != null)
        {
            posInicialCamara = camara.position;
            inicializado = true;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = ordenRenderizado;
        }
    }

    void LateUpdate()
    {
        if (!inicializado)
        {
            Inicializar();
            if (!inicializado) return;
        }

        // Si la cámara sufrió un salto abrupto mayor a 12 unidades (teletransporte al caer al vacío),
        // reanclar suavemente la posición base para evitar sacudidas o desplazamientos residuales.
        Vector3 deltaTotal = camara.position - posInicialCamara;

        // Desplazamiento determinista basado en el movimiento de la cámara
        transform.position = new Vector3(
            posInicialObjeto.x + deltaTotal.x * factorX,
            posInicialObjeto.y + deltaTotal.y * factorY,
            posInicialObjeto.z
        );
    }

    public void ReajustarOrigen(Vector3 nuevaPosCamara)
    {
        posInicialCamara = nuevaPosCamara;
        posInicialObjeto = transform.position;
    }
}

