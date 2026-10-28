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
    private static Sprite spriteCompartido = null;

    void Awake()
    {
        transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        AsegurarVisuales();
    }

    private void AsegurarVisuales()
    {
        if (sr != null && sr.sprite == null)
        {
            if (spriteCompartido != null)
            {
                sr.sprite = spriteCompartido;
                return;
            }

            // 1. Cargar desde Resources
            sr.sprite = Resources.Load<Sprite>("botella_agua");
            if (sr.sprite != null)
            {
                spriteCompartido = sr.sprite;
                return;
            }

            // 2. Generación procedural garantizada de emergencia
            spriteCompartido = GenerarSpriteBotellaProcedural();
            sr.sprite = spriteCompartido;
        }

        if (sr != null)
        {
            sr.sortingOrder = 17;
            sr.color = Color.white;
        }
    }

    private static Sprite GenerarSpriteBotellaProcedural()
    {
        int W = 32, H = 32;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color transp = new Color(0, 0, 0, 0);
        Color outline = new Color(0.08f, 0.14f, 0.25f, 1f);
        Color cap = new Color(0.92f, 0.96f, 1f, 1f);
        Color capShade = new Color(0.70f, 0.82f, 0.95f, 1f);
        Color water = new Color(0.22f, 0.74f, 0.97f, 0.95f);
        Color waterDeep = new Color(0.01f, 0.52f, 0.78f, 1f);
        Color highlight = new Color(0.94f, 0.98f, 1f, 0.95f);

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                tex.SetPixel(x, y, transp);
            }
        }

        // Tapa (y: 23 a 27, x: 13 a 18)
        for (int y = 23; y <= 27; y++)
        {
            for (int x = 13; x <= 18; x++)
            {
                tex.SetPixel(x, y, (x < 16) ? cap : capShade);
            }
            tex.SetPixel(12, y, outline);
            tex.SetPixel(19, y, outline);
        }
        for (int x = 12; x <= 19; x++)
        {
            tex.SetPixel(x, 27, outline);
            tex.SetPixel(x, 22, outline);
        }

        // Cuello (y: 19 a 22, x: 14 a 17)
        for (int y = 19; y <= 22; y++)
        {
            for (int x = 14; x <= 17; x++)
            {
                tex.SetPixel(x, y, (x == 14) ? highlight : water);
            }
            tex.SetPixel(13, y, outline);
            tex.SetPixel(18, y, outline);
        }

        // Hombros (y: 16 a 18)
        for (int x = 12; x <= 19; x++) tex.SetPixel(x, 18, (x <= 13) ? highlight : water);
        tex.SetPixel(11, 18, outline); tex.SetPixel(20, 18, outline);
        for (int x = 10; x <= 21; x++) tex.SetPixel(x, 17, (x <= 12) ? highlight : water);
        tex.SetPixel(9, 17, outline); tex.SetPixel(22, 17, outline);

        // Cuerpo (y: 4 a 16, x: 9 a 22)
        for (int y = 5; y <= 16; y++)
        {
            tex.SetPixel(9, y, outline);
            tex.SetPixel(22, y, outline);
            tex.SetPixel(10, y, highlight);
            tex.SetPixel(11, y, highlight);
            for (int x = 12; x <= 18; x++) tex.SetPixel(x, y, water);
            for (int x = 19; x <= 21; x++) tex.SetPixel(x, y, waterDeep);
        }

        // Base (y: 3 a 4)
        for (int x = 11; x <= 20; x++) tex.SetPixel(x, 4, outline);
        tex.SetPixel(10, 4, outline); tex.SetPixel(21, 4, outline);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 24f);
    }

    void Start()
    {
        transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        posicionInicial = transform.position;
        AsegurarVisuales();

        var mb = FindFirstObjectByType<MovimientoMadre>();
        if (mb != null) transformJugador = mb.transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
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
