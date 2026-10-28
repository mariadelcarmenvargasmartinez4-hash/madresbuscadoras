using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Control de movimiento para la protagonista Madre Buscadora.
/// Permite moverse con las flechas direccionales (o teclas A/D) y saltar con la tecla V (o Espacio).
/// Soporta tanto el Nuevo Input System de Unity como el sistema clásico.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovimientoMadre : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad horizontal de la protagonista")]
    public float velocidad = 6f;

    [Header("Salto (Tecla V o Espacio)")]
    [Tooltip("Fuerza de impulso vertical")]
    public float fuerzaSalto = 11.8f;
    [Tooltip("Capa de suelo opcional")]
    public LayerMask capaSuelo;
    [Tooltip("Distancia hacia abajo para detectar suelo")]
    public float distanciaSuelo = 0.35f;

    [Header("Sprites de Animación")]
    public Sprite spriteIdle;
    public Sprite spriteCaminar;
    public Sprite spriteSaltar;

    [Header("Opciones de Animación")]
    public float intervaloPaso = 0.18f;

    [Header("Estado en Tiempo Real (Depuración)")]
    public float inputActual = 0f;
    public bool enElSuelo = true;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;

    private float movimientoX = 0f;
    private bool debeSaltar = false;
    private float temporizadorAnim = 0f;
    private bool pasoAlterno = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (sr != null)
        {
            if (spriteIdle == null) spriteIdle = CargarSpriteRobusto("Madre_Idle");
            if (spriteCaminar == null) spriteCaminar = CargarSpriteRobusto("Madre_Caminar");
            if (spriteSaltar == null) spriteSaltar = CargarSpriteRobusto("Madre_Saltar");

            if (sr.sprite == null && spriteIdle != null)
            {
                sr.sprite = spriteIdle;
            }

            sr.sortingOrder = 25; // Por delante de plataformas y fondos
            sr.color = Color.white;
            var matUnlit = Shader.Find("Sprites/Default");
            if (matUnlit != null) sr.sharedMaterial = new Material(matUnlit);
        }

        if (transform.localScale.x > 0.5f)
        {
            transform.localScale = new Vector3(0.24f, 0.24f, 1f);
        }

        ControladorNivel2.AsegurarInstancia();
    }

    private Sprite CargarSpriteRobusto(string nombre)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(nombre);
        if (sprites != null && sprites.Length > 0) return sprites[0];
        Sprite sp = Resources.Load<Sprite>(nombre);
        if (sp != null) return sp;
        return null;
    }

    [Header("Punto de Reaparición")]
    [Tooltip("Posición de la primera plataforma donde inicia el nivel")]
    public Vector3 posicionPrimeraPlataforma = new Vector3(-74f, 6.2f, 0f);

    private bool recibiendoImpacto = false;

    void Start()
    {
        if (posicionPrimeraPlataforma == Vector3.zero)
        {
            posicionPrimeraPlataforma = transform.position;
        }
        ControladorNivel2.AsegurarInstancia();
    }

    void Update()
    {
        // Si el juego está en pausa, victoria o derrota, congelar movimiento
        if (ControladorNivel2.Instancia != null && ControladorNivel2.Instancia.estadoActual != ControladorNivel2.EstadoJuego.Jugando)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // Si cae al vacío por debajo del escenario: SIEMPRE regresa a la primera plataforma
        if (transform.position.y < -23f)
        {
            ReaparecerEnPrimeraPlataforma();
            return;
        }

        movimientoX = 0f;

        // 1. Detección con Nuevo Input System (Unity 6 por defecto)
        #if ENABLE_INPUT_SYSTEM
        var teclado = Keyboard.current;
        if (teclado != null)
        {
            // Flechas o teclas A/D
            if (teclado.leftArrowKey.isPressed || teclado.aKey.isPressed)
                movimientoX -= 1f;
            if (teclado.rightArrowKey.isPressed || teclado.dKey.isPressed)
                movimientoX += 1f;

            // Salto con letra V o barra espaciadora
            if (teclado.vKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame)
            {
                debeSaltar = true;
            }
        }
        #endif

        // 2. Detección con Sistema Clásico (fallback)
        try
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                movimientoX -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                movimientoX += 1f;

            if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.Space))
            {
                debeSaltar = true;
            }
        }
        catch
        {
            // Ignorar si el sistema clásico está inactivo en Unity
        }

        inputActual = movimientoX;

        // 3. Voltear sprite según dirección
        if (movimientoX > 0.01f)
        {
            if (sr != null) sr.flipX = false;
        }
        else if (movimientoX < -0.01f)
        {
            if (sr != null) sr.flipX = true;
        }

        // 4. Comprobar si está en el suelo
        VerificarSuelo();

        // 5. Salto
        if (debeSaltar)
        {
            if (enElSuelo)
            {
                RealizarSalto();
            }
            debeSaltar = false;
        }

        // 6. Animación de sprites
        ActualizarVisuales();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Despertar el rigidbody si está dormido
        if (rb.IsSleeping())
        {
            rb.WakeUp();
        }

        // Movimiento horizontal en física (Unity 6)
        if (!recibiendoImpacto)
        {
            Vector2 vel = rb.linearVelocity;
            vel.x = movimientoX * velocidad;
            rb.linearVelocity = vel;
        }
    }

    public void AplicarImpacto(Vector2 fuerza)
    {
        if (rb != null)
        {
            rb.linearVelocity = fuerza;
            recibiendoImpacto = true;
            CancelInvoke(nameof(FinImpacto));
            Invoke(nameof(FinImpacto), 0.25f);
        }

        StartCoroutine(ParpadeoDanio());
    }

    private void FinImpacto()
    {
        recibiendoImpacto = false;
    }

    /// <summary>
    /// Teletransporta a la protagonista de regreso a la primera plataforma cuando cae al vacío.
    /// Aplica penalización de salud y efecto visual.
    /// </summary>
    public void ReaparecerEnPrimeraPlataforma()
    {
        transform.position = posicionPrimeraPlataforma;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        recibiendoImpacto = false;
        if (ControladorNivel2.Instancia != null)
        {
            ControladorNivel2.Instancia.ModificarSalud(-20f);
        }
        ControladorAudio.Instancia?.ReproducirCaidaVacio();
        StartCoroutine(ParpadeoDanio());
    }

    private System.Collections.IEnumerator ParpadeoDanio()
    {
        if (sr == null) yield break;
        Color colorOriginal = Color.white;
        for (int i = 0; i < 3; i++)
        {
            sr.color = new Color(1f, 0.3f, 0.3f, 0.7f);
            yield return new WaitForSeconds(0.08f);
            sr.color = colorOriginal;
            yield return new WaitForSeconds(0.08f);
        }
        sr.color = colorOriginal;
    }

    private bool contactoFisicoSuelo = false;

    private void OnCollisionStay2D(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.GetContact(i);
            if (contact.normal.y > 0.5f)
            {
                contactoFisicoSuelo = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        contactoFisicoSuelo = false;
    }

    private void VerificarSuelo()
    {
        if (col != null)
        {
            // Raycast desde la base central del collider
            Vector2 centroBase = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.05f);
            
            // Si se definió capa de suelo
            if (capaSuelo.value != 0)
            {
                RaycastHit2D hit = Physics2D.Raycast(centroBase, Vector2.down, distanciaSuelo + 0.08f, capaSuelo);
                enElSuelo = hit.collider != null || contactoFisicoSuelo;
            }
            else
            {
                // Detección general con raycast filtrando el propio collider
                RaycastHit2D[] hits = Physics2D.RaycastAll(centroBase, Vector2.down, distanciaSuelo + 0.12f);
                bool pisoDetectado = false;
                foreach (var h in hits)
                {
                    if (h.collider != null && h.collider != col && !h.collider.isTrigger)
                    {
                        pisoDetectado = true;
                        break;
                    }
                }
                enElSuelo = pisoDetectado || contactoFisicoSuelo;
            }
        }
        else if (rb != null)
        {
            enElSuelo = contactoFisicoSuelo || (Mathf.Abs(rb.linearVelocity.y) < 0.05f);
        }
    }

    private void RealizarSalto()
    {
        if (rb == null) return;

        contactoFisicoSuelo = false;
        enElSuelo = false;

        Vector2 vel = rb.linearVelocity;
        vel.y = fuerzaSalto;
        rb.linearVelocity = vel;

        ControladorAudio.Instancia?.ReproducirSalto();

        if (anim != null)
        {
            anim.SetTrigger("Saltar");
        }
    }

    private void ActualizarVisuales()
    {
        if (anim != null)
        {
            anim.SetFloat("Velocidad", Mathf.Abs(movimientoX));
            anim.SetBool("EnSuelo", enElSuelo);
            if (enElSuelo && Mathf.Abs(movimientoX) > 0.01f)
            {
                ControladorAudio.Instancia?.ReproducirPaso();
            }
            return;
        }

        if (sr == null) return;

        // Pose de salto
        if (!enElSuelo)
        {
            if (spriteSaltar != null)
            {
                sr.sprite = spriteSaltar;
            }
        }
        // Pose de caminata
        else if (Mathf.Abs(movimientoX) > 0.01f)
        {
            temporizadorAnim += Time.deltaTime;
            if (temporizadorAnim >= intervaloPaso)
            {
                temporizadorAnim = 0f;
                pasoAlterno = !pasoAlterno;
                ControladorAudio.Instancia?.ReproducirPaso();
            }

            if (pasoAlterno && spriteCaminar != null)
            {
                sr.sprite = spriteCaminar;
            }
            else if (spriteIdle != null)
            {
                sr.sprite = spriteIdle;
            }
        }
        // Pose de reposo
        else
        {
            temporizadorAnim = 0f;
            if (spriteIdle != null)
            {
                sr.sprite = spriteIdle;
            }
        }
    }

    // Dibujar el rayo de detección de suelo en la escena de Unity
    private void OnDrawGizmosSelected()
    {
        if (col != null)
        {
            Gizmos.color = enElSuelo ? Color.green : Color.red;
            Vector2 centroBase = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.05f);
            Gizmos.DrawLine(centroBase, centroBase + Vector2.down * (distanciaSuelo + 0.1f));
        }
    }
}
