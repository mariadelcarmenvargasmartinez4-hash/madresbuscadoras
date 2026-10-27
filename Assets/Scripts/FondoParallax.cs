using UnityEngine;

/// <summary>
/// Controla la imagen de fondo temática del juego para que acompañe a la cámara
/// con un efecto Parallax suave durante el descenso de la Madre Buscadora.
/// </summary>
public class FondoParallax : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform camara;

    [Header("Efecto Parallax")]
    [Tooltip("Grado de desplazamiento horizontal respecto a la cámara (0 = estático, 1 = se mueve idéntico)")]
    public float factorX = 0.65f;
    [Tooltip("Grado de desplazamiento vertical respecto a la cámara")]
    public float factorY = 0.45f;

    private Vector3 posCamaraAnterior;

    void Start()
    {
        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }

        if (camara != null)
        {
            posCamaraAnterior = camara.position;
        }

        // Asegurar que el fondo esté detrás de todo el mundo y props
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = -50;
        }
    }

    void LateUpdate()
    {
        if (camara == null)
        {
            if (Camera.main != null) camara = Camera.main.transform;
            return;
        }

        Vector3 delta = camara.position - posCamaraAnterior;
        transform.position += new Vector3(delta.x * factorX, delta.y * factorY, 0f);
        posCamaraAnterior = camara.position;
    }
}
