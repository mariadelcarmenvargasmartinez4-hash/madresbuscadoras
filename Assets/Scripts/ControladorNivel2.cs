using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controlador principal del Nivel 2: "Las Puertas Cerradas de la Burocracia".
/// Gestiona la barra de Ánimo/Vida, el expediente con las 10 hojas coleccionables,
/// el sistema de 10 acertijos deductivos, los obstáculos móviles y los 4 estados del juego.
/// </summary>
public class ControladorNivel2 : MonoBehaviour
{
    public static ControladorNivel2 Instancia { get; private set; }

    public enum EstadoJuego
    {
        Jugando,
        Pausado,
        Victoria,
        Derrota
    }

    [Header("Estado del Nivel")]
    public EstadoJuego estadoActual = EstadoJuego.Jugando;

    [Header("Salud y Ánimo")]
    [Tooltip("Ánimo o vida máxima de la Madre Buscadora")]
    public float saludMaxima = 100f;
    public float saludActual = 100f;

    [Header("Hojas y Expediente (10 Hojas)")]
    [Tooltip("Cantidad total de fragmentos de hoja por encontrar")]
    public int totalHojas = 10;
    public int hojasRecolectadas = 0;

    [Header("Tiempo")]
    public float tiempoTranscurrido = 0f;

    // 10 ACERTIJOS DEDUCTIVOS QUE GUIAN LA BÚSQUEDA DEL EXPEDIENTE COMPLETO
    private readonly string[] titulosAcertijos = new string[]
    {
        "🔍 ACERTIJO 1 (Superficie / Mercado)",
        "🔍 ACERTIJO 2 (Superficie / Parada)",
        "🔍 ACERTIJO 3 (Nivel Medio / Pozo)",
        "🔍 ACERTIJO 4 (Nivel Medio / Registro)",
        "🔍 ACERTIJO 5 (Puente de Descenso)",
        "🔍 ACERTIJO 6 (Sótano / La Estatua)",
        "🔍 ACERTIJO 7 (Túnel de Archiveros)",
        "🔍 ACERTIJO 8 (Abismo / Andamios)",
        "🔍 ACERTIJO 9 (Cornisa de Cadenas)",
        "🔍 ACERTIJO 10 FINAL (La Bóveda)"
    };

    private readonly string[] textosAcertijos = new string[]
    {
        "\"El rastro comienza donde comerciaban los viajeros: busca en lo alto, entre las cajas de madera sobre el tejado del mercado.\"",
        "\"El autobús partió de la parada solitaria al borde del pueblo... busca donde termina la calle alta, tras el poste de madera.\"",
        "\"Donde las ropas cuelgan al viento y el agua del viejo pozo guarda silencio... busca en la cornisa resguardada tras la piedra.\"",
        "\"En la mesa de los sellos oficiales y las salas de espera frías... busca en el rincón entre los barriles de la oficina del registro.\"",
        "\"El camino desciende a la oscuridad... busca bajo los maderos del andamio que une la plaza con el túnel subterráneo.\"",
        "\"La burocracia sepultó las quejas bajo tierra... busca la efigie de piedra que reza solitaria en el pasadizo subterráneo.\"",
        "\"Las respuestas oficiales siempre fueron evasivas... busca en lo más profundo del corredor izquierdo, tras el cofre oxidado.\"",
        "\"El frío aumenta conforme se desciende a la fosa... Tras revisar el archivero, baja por la entrada abierta en el suelo y desciende por los andamios de madera hacia la cueva subterránea.\"",
        "\"En las profundidades de la cueva, cruza los puentes de madera hacia la derecha... la hoja reposa sobre la colina de roca junto a las viejas cadenas.\"",
        "\"En el fondo de la gran bóveda final, cruzando hacia el extremo izquierdo... el expediente sellado aguarda para completar la verdad e iniciar la búsqueda en campo (Nivel 3).\""
    };

    // Notificación en pantalla al recoger una hoja
    private string mensajePistaActual = "";
    private float temporizadorMensaje = 0f;
    private const float DURACION_MENSAJE = 6.5f;

    // Texturas para la interfaz gráfica
    private Texture2D texFondoHUD;
    private Texture2D texBarraVida;
    private Texture2D texBarraFondo;
    private Texture2D texPanel;

    public static void AsegurarInstancia()
    {
        if (Instancia == null)
        {
            var existente = FindFirstObjectByType<ControladorNivel2>();
            if (existente != null)
            {
                Instancia = existente;
            }
            else
            {
                GameObject go = new GameObject("ControladorNivel");
                Instancia = go.AddComponent<ControladorNivel2>();
            }
        }
    }

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else if (Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        saludActual = saludMaxima;
        Time.timeScale = 1f;
        CrearTexturasUI();
    }

    void Update()
    {
        // 1. Control de Pausa (Tecla Escape o tecla P)
        VerificarEntradaPausa();

        // 2. Si estamos en juego, contar tiempo
        if (estadoActual == EstadoJuego.Jugando)
        {
            tiempoTranscurrido += Time.deltaTime;

            if (temporizadorMensaje > 0f)
            {
                temporizadorMensaje -= Time.deltaTime;
                if (temporizadorMensaje <= 0f)
                {
                    mensajePistaActual = "";
                }
            }
        }
    }

    private void VerificarEntradaPausa()
    {
        bool presionoPausa = false;

#if ENABLE_INPUT_SYSTEM
        var teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.escapeKey.wasPressedThisFrame || teclado.pKey.wasPressedThisFrame)
            {
                presionoPausa = true;
            }
        }
#endif
        if (!presionoPausa)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    presionoPausa = true;
                }
            }
            catch { }
        }

        if (presionoPausa)
        {
            if (estadoActual == EstadoJuego.Jugando)
            {
                PausarJuego();
            }
            else if (estadoActual == EstadoJuego.Pausado)
            {
                ReanudarJuego();
            }
        }
    }

    public void ModificarSalud(float cantidad)
    {
        if (estadoActual != EstadoJuego.Jugando) return;

        saludActual = Mathf.Clamp(saludActual + cantidad, 0f, saludMaxima);

        if (cantidad < 0f)
        {
            ControladorAudio.Instancia?.ReproducirDanio();
        }

        if (saludActual <= 0f)
        {
            ActivarGameOver();
        }
    }

    public void RecolectarHoja(int numero, string titulo, string detalle)
    {
        if (estadoActual != EstadoJuego.Jugando) return;

        hojasRecolectadas = Mathf.Clamp(hojasRecolectadas + 1, 0, totalHojas);
        mensajePistaActual = $"¡HOJA {hojasRecolectadas}/{totalHojas} ENCONTRADA!\n{titulo}\n\"{detalle}\"";
        temporizadorMensaje = DURACION_MENSAJE;

        // Efecto sonoro de recolección de evidencia
        ControladorAudio.Instancia?.ReproducirRecogerHoja();

        // Si ya recogió las 10 hojas
        if (hojasRecolectadas >= totalHojas)
        {
            Invoke(nameof(CompletarNivel), 2.2f);
        }
    }

    public void PausarJuego()
    {
        if (estadoActual == EstadoJuego.Jugando)
        {
            estadoActual = EstadoJuego.Pausado;
            Time.timeScale = 0f;
            ControladorAudio.Instancia?.PausarMusica();
        }
    }

    public void ReanudarJuego()
    {
        if (estadoActual == EstadoJuego.Pausado)
        {
            estadoActual = EstadoJuego.Jugando;
            Time.timeScale = 1f;
            ControladorAudio.Instancia?.ReanudarMusica();
        }
    }

    public void ActivarGameOver()
    {
        estadoActual = EstadoJuego.Derrota;
        Time.timeScale = 0f;
        ControladorAudio.Instancia?.DetenerMusica();
    }

    public void CompletarNivel()
    {
        estadoActual = EstadoJuego.Victoria;
        Time.timeScale = 0f;
        ControladorAudio.Instancia?.ReproducirVictoria();
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        if (SceneManager.sceneCountInBuildSettings > 1)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            ReiniciarNivel();
        }
    }

    // ==========================================
    // RENDERIZADO DEL HUD CON 10 ACERTIJOS
    // ==========================================
    private void CrearTexturasUI()
    {
        if (texFondoHUD == null) texFondoHUD = CrearTexturaColor(new Color(0.08f, 0.08f, 0.14f, 0.92f));
        if (texBarraFondo == null) texBarraFondo = CrearTexturaColor(new Color(0.2f, 0.2f, 0.25f, 0.95f));
        if (texBarraVida == null) texBarraVida = CrearTexturaColor(new Color(0.25f, 0.85f, 0.35f, 1f));
        if (texPanel == null) texPanel = CrearTexturaColor(new Color(0.06f, 0.06f, 0.10f, 0.96f));
    }

    private Texture2D CrearTexturaColor(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    void OnGUI()
    {
        CrearTexturasUI();

        // 1. HUD Superior (Salud, Conteo de 10 Hojas, Acertijo Deductivo y Tiempo)
        DibujarHUD();

        // 2. Banner de confirmación cuando se encuentra una hoja
        if (!string.IsNullOrEmpty(mensajePistaActual) && estadoActual == EstadoJuego.Jugando)
        {
            DibujarBannerPista();
        }

        // 3. Ventanas de estados (Pausa, Victoria, Game Over)
        switch (estadoActual)
        {
            case EstadoJuego.Pausado:
                DibujarMenuPausa();
                break;
            case EstadoJuego.Victoria:
                DibujarPantallaVictoria();
                break;
            case EstadoJuego.Derrota:
                DibujarPantallaDerrota();
                break;
        }
    }

    private void DibujarHUD()
    {
        GUIStyle estiloTexto = new GUIStyle(GUI.skin.label);
        estiloTexto.fontSize = 13;
        estiloTexto.fontStyle = FontStyle.Bold;
        estiloTexto.normal.textColor = Color.white;

        // PANEL 1: Superior Izquierdo (Barra de Ánimo / Vida)
        GUI.DrawTexture(new Rect(15, 15, 230, 72), texFondoHUD);
        float porcentaje = Mathf.Clamp01(saludActual / saludMaxima);
        GUI.Label(new Rect(25, 20, 210, 20), $"❤️ Ánimo / Vida: {Mathf.RoundToInt(saludActual)}%", estiloTexto);

        // Fondo de la barra
        GUI.DrawTexture(new Rect(25, 45, 210, 22), texBarraFondo);

        // Barra con color dinámico
        Color colorVida = porcentaje > 0.5f ? Color.Lerp(Color.yellow, new Color(0.2f, 0.9f, 0.3f), (porcentaje - 0.5f) * 2f)
                                           : Color.Lerp(new Color(0.95f, 0.2f, 0.2f), Color.yellow, porcentaje * 2f);
        GUI.color = colorVida;
        GUI.DrawTexture(new Rect(27, 47, 206 * porcentaje, 18), texBarraVida);
        GUI.color = Color.white;

        // PANEL 2: Superior Central (Conteo de 10 Hojas y ACERTIJO DE PISTA ACTIVO)
        float anchoCentral = 560f;
        float xCentral = (Screen.width - anchoCentral) / 2f;
        GUI.DrawTexture(new Rect(xCentral, 15, anchoCentral, 75), texFondoHUD);

        // Conteo de hojas
        GUIStyle estiloHojas = new GUIStyle(estiloTexto);
        estiloHojas.fontSize = 14;
        estiloHojas.alignment = TextAnchor.MiddleCenter;
        estiloHojas.normal.textColor = new Color(1f, 0.88f, 0.35f);
        GUI.Label(new Rect(xCentral, 18, anchoCentral, 20), $"📄 EXPEDIENTE: {hojasRecolectadas} de {totalHojas} Hojas Reunidas", estiloHojas);

        // Acertijo para deducir la siguiente hoja
        int idxAcertijo = Mathf.Clamp(hojasRecolectadas, 0, totalHojas - 1);
        string titulo = (hojasRecolectadas >= totalHojas) ? "⭐ EXPEDIENTE COMPLETO" : titulosAcertijos[idxAcertijo];
        string pista = (hojasRecolectadas >= totalHojas)
            ? "¡Has reunido las 10 partes del expediente! Las pruebas están listas para la brigada de búsqueda (Nivel 3)."
            : textosAcertijos[idxAcertijo];

        GUIStyle estiloAcertijo = new GUIStyle(GUI.skin.label);
        estiloAcertijo.fontSize = 11;
        estiloAcertijo.fontStyle = FontStyle.Italic;
        estiloAcertijo.alignment = TextAnchor.MiddleCenter;
        estiloAcertijo.normal.textColor = new Color(0.92f, 0.95f, 1f);
        estiloAcertijo.wordWrap = true;

        GUI.Label(new Rect(xCentral + 15, 38, anchoCentral - 30, 48), $"{titulo}: {pista}", estiloAcertijo);

        // PANEL 3: Superior Derecho (Tiempo y Pausa)
        float xDer = Screen.width - 180;
        GUI.DrawTexture(new Rect(xDer, 15, 165, 72), texFondoHUD);
        int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60F);
        int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60F);
        GUI.Label(new Rect(xDer + 15, 20, 140, 20), $"⏱️ Tiempo: {minutos:00}:{segundos:00}", estiloTexto);

        GUIStyle estiloMini = new GUIStyle(GUI.skin.label);
        estiloMini.fontSize = 11;
        estiloMini.normal.textColor = new Color(0.8f, 0.8f, 0.9f);
        GUI.Label(new Rect(xDer + 15, 45, 140, 20), "Tecla [P] Pausar", estiloMini);
    }

    private void DibujarBannerPista()
    {
        float ancho = 640f;
        float alto = 85f;
        float x = (Screen.width - ancho) / 2f;
        float y = Screen.height - alto - 25f;

        GUI.DrawTexture(new Rect(x, y, ancho, alto), texPanel);

        GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label);
        estiloTitulo.fontSize = 13;
        estiloTitulo.fontStyle = FontStyle.Bold;
        estiloTitulo.alignment = TextAnchor.MiddleCenter;
        estiloTitulo.normal.textColor = new Color(1f, 0.92f, 0.4f);
        estiloTitulo.wordWrap = true;

        GUI.Label(new Rect(x + 15, y + 10, ancho - 30, alto - 20), mensajePistaActual, estiloTitulo);
    }

    private void DibujarMenuPausa()
    {
        float ancho = 380f;
        float alto = 320f;
        float x = (Screen.width - ancho) / 2f;
        float y = (Screen.height - alto) / 2f;

        GUI.DrawTexture(new Rect(x, y, ancho, alto), texPanel);

        GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label);
        estiloTitulo.fontSize = 24;
        estiloTitulo.fontStyle = FontStyle.Bold;
        estiloTitulo.alignment = TextAnchor.MiddleCenter;
        estiloTitulo.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + 25, ancho, 40), "JUEGO PAUSADO", estiloTitulo);

        GUIStyle estiloBtn = new GUIStyle(GUI.skin.button);
        estiloBtn.fontSize = 15;
        estiloBtn.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + 50, y + 90, ancho - 100, 42), "Reanudar", estiloBtn))
        {
            ReanudarJuego();
        }

        if (GUI.Button(new Rect(x + 50, y + 145, ancho - 100, 42), "Reiniciar Nivel", estiloBtn))
        {
            ReiniciarNivel();
        }

        if (GUI.Button(new Rect(x + 50, y + 200, ancho - 100, 42), "Salir", estiloBtn))
        {
            SalirAlMenu();
        }
    }

    private void DibujarPantallaVictoria()
    {
        float ancho = 520f;
        float alto = 390f;
        float x = (Screen.width - ancho) / 2f;
        float y = (Screen.height - alto) / 2f;

        GUI.DrawTexture(new Rect(x, y, ancho, alto), texPanel);

        GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label);
        estiloTitulo.fontSize = 22;
        estiloTitulo.fontStyle = FontStyle.Bold;
        estiloTitulo.alignment = TextAnchor.MiddleCenter;
        estiloTitulo.normal.textColor = new Color(0.4f, 1f, 0.5f);
        GUI.Label(new Rect(x, y + 25, ancho, 35), "¡EXPEDIENTE REUNIDO AL 100%!", estiloTitulo);

        GUIStyle estiloTexto = new GUIStyle(GUI.skin.label);
        estiloTexto.fontSize = 13;
        estiloTexto.alignment = TextAnchor.MiddleCenter;
        estiloTexto.normal.textColor = Color.white;
        estiloTexto.wordWrap = true;

        int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60F);
        int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60F);
        string textoVictoria = $"¡Increíble labor de investigación!\nHas descifrado los 10 acertijos y recuperado todos los documentos oficiales sorteando las trampas móviles.\n\n" +
                               $"Hojas reunidas: {hojasRecolectadas} de {totalHojas}\n" +
                               $"Tiempo final: {minutos:00}:{segundos:00}\n" +
                               $"Ánimo restante: {Mathf.RoundToInt(saludActual)}%";

        GUI.Label(new Rect(x + 25, y + 75, ancho - 50, 120), textoVictoria, estiloTexto);

        GUIStyle estiloBtn = new GUIStyle(GUI.skin.button);
        estiloBtn.fontSize = 14;
        estiloBtn.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + 80, y + 225, ancho - 160, 42), "Rejugar Nivel", estiloBtn))
        {
            ReiniciarNivel();
        }

        if (GUI.Button(new Rect(x + 80, y + 280, ancho - 160, 42), "Continuar al Nivel 3", estiloBtn))
        {
            ReiniciarNivel();
        }
    }

    private void DibujarPantallaDerrota()
    {
        float ancho = 440f;
        float alto = 320f;
        float x = (Screen.width - ancho) / 2f;
        float y = (Screen.height - alto) / 2f;

        GUI.DrawTexture(new Rect(x, y, ancho, alto), texPanel);

        GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label);
        estiloTitulo.fontSize = 24;
        estiloTitulo.fontStyle = FontStyle.Bold;
        estiloTitulo.alignment = TextAnchor.MiddleCenter;
        estiloTitulo.normal.textColor = new Color(1f, 0.35f, 0.35f);
        GUI.Label(new Rect(x, y + 25, ancho, 35), "Ánimo Agotado", estiloTitulo);

        GUIStyle estiloTexto = new GUIStyle(GUI.skin.label);
        estiloTexto.fontSize = 13;
        estiloTexto.alignment = TextAnchor.MiddleCenter;
        estiloTexto.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        estiloTexto.wordWrap = true;

        string textoDerrota = "Los obstáculos móviles y el desgaste burocrático detuvieron la búsqueda.\n" +
                              "Una madre nunca se rinde: recupera fuerzas y vuelve a intentarlo.";

        GUI.Label(new Rect(x + 25, y + 75, ancho - 50, 80), textoDerrota, estiloTexto);

        GUIStyle estiloBtn = new GUIStyle(GUI.skin.button);
        estiloBtn.fontSize = 15;
        estiloBtn.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(x + 60, y + 175, ancho - 120, 45), "Reintentar", estiloBtn))
        {
            ReiniciarNivel();
        }

        if (GUI.Button(new Rect(x + 60, y + 235, ancho - 120, 42), "Salir", estiloBtn))
        {
            SalirAlMenu();
        }
    }
}
