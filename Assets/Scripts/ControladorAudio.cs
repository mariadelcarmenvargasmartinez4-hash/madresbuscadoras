using UnityEngine;

/// <summary>
/// Controlador de Audio para el Nivel 2: "Las Puertas Cerradas de la Burocracia".
/// Administra la música ambiental melancólica y solemne en bucle (AudioSource)
/// y los efectos de sonido (SFX): pasos, saltos, recolección de hojas, daño, caída y victoria.
/// </summary>
public class ControladorAudio : MonoBehaviour
{
    public static ControladorAudio Instancia { get; private set; }

    [Header("Fuentes de Audio")]
    public AudioSource musicaSource;
    public AudioSource sfxSource;

    [Header("Música de Fondo (BGM)")]
    public AudioClip musicaFondoNivel2;
    [Range(0f, 1f)]
    public float volumenMusica = 0.45f;

    [Header("Efectos de Sonido (SFX)")]
    public AudioClip sfxRecogerHoja;
    public AudioClip sfxDanio;
    public AudioClip sfxSalto;
    public AudioClip sfxCaidaVacio;
    public AudioClip sfxPaso;
    public AudioClip sfxVictoria;
    public AudioClip sfxBeberAgua;
    [Range(0f, 1f)]
    public float volumenSFX = 0.85f;

    [Header("Estado de Audio")]
    [Tooltip("Indica si el sonido ha sido silenciado (quitar sonido)")]
    public bool sonidoSilenciado = false;

    private float tiempoUltimoPaso = 0f;

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

        CargarClipsFallback();
        ConfigurarFuentes();
    }

    private void CargarClipsFallback()
    {
        if (musicaFondoNivel2 == null) musicaFondoNivel2 = Resources.Load<AudioClip>("Audio/Musica/musica_fondo_nivel2");
        if (sfxRecogerHoja == null) sfxRecogerHoja = Resources.Load<AudioClip>("Audio/SFX/sfx_recoger_hoja");
        if (sfxDanio == null) sfxDanio = Resources.Load<AudioClip>("Audio/SFX/sfx_danio");
        if (sfxSalto == null) sfxSalto = Resources.Load<AudioClip>("Audio/SFX/sfx_salto");
        if (sfxCaidaVacio == null) sfxCaidaVacio = Resources.Load<AudioClip>("Audio/SFX/sfx_caida_vacio");
        if (sfxPaso == null) sfxPaso = Resources.Load<AudioClip>("Audio/SFX/sfx_paso");
        if (sfxVictoria == null) sfxVictoria = Resources.Load<AudioClip>("Audio/SFX/sfx_victoria_expediente");
        if (sfxBeberAgua == null) sfxBeberAgua = Resources.Load<AudioClip>("Audio/SFX/sfx_beber_agua");
    }

    void Start()
    {
        // Cargar preferencia guardada de sonido (0 = activo, 1 = silenciado)
        sonidoSilenciado = PlayerPrefs.GetInt("SonidoSilenciado", 0) == 1;
        SilenciarSonido(sonidoSilenciado);

        if (!sonidoSilenciado)
        {
            ReproducirMusicaFondo();
        }
    }

    /// <summary>
    /// Alterna el estado de sonido entre activado y silenciado (Quitar sonido).
    /// </summary>
    public void AlternarSonido()
    {
        SilenciarSonido(!sonidoSilenciado);
    }

    /// <summary>
    /// Silencia o activa todo el audio del juego (Música de fondo, efectos de sonido y pasos).
    /// </summary>
    public void SilenciarSonido(bool silenciar)
    {
        sonidoSilenciado = silenciar;
        PlayerPrefs.SetInt("SonidoSilenciado", silenciar ? 1 : 0);
        PlayerPrefs.Save();

        AudioListener.pause = silenciar;
        AudioListener.volume = silenciar ? 0f : 1f;

        if (musicaSource != null)
        {
            musicaSource.mute = silenciar;
            if (!silenciar && !musicaSource.isPlaying && musicaFondoNivel2 != null)
            {
                musicaSource.Play();
            }
        }

        if (sfxSource != null)
        {
            sfxSource.mute = silenciar;
        }

        Debug.Log($"[ControladorAudio] Sonido {(silenciar ? "SILENCIADO (Quitar Sonido)" : "ACTIVADO")}.");
    }

    private void ConfigurarFuentes()
    {
        if (musicaSource == null)
        {
            musicaSource = gameObject.AddComponent<AudioSource>();
        }
        musicaSource.loop = true;
        musicaSource.playOnAwake = false;
        musicaSource.volume = volumenMusica;
        musicaSource.spatialBlend = 0f; // 2D Sound
        musicaSource.mute = sonidoSilenciado;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = volumenSFX;
        sfxSource.spatialBlend = 0f; // 2D Sound
        sfxSource.mute = sonidoSilenciado;
    }

    public void ReproducirMusicaFondo()
    {
        if (musicaSource == null) return;
        if (sonidoSilenciado)
        {
            musicaSource.mute = true;
            return;
        }

        if (musicaFondoNivel2 != null && musicaSource.clip != musicaFondoNivel2)
        {
            musicaSource.clip = musicaFondoNivel2;
        }

        if (musicaSource.clip != null && !musicaSource.isPlaying)
        {
            musicaSource.volume = volumenMusica;
            musicaSource.mute = false;
            musicaSource.Play();
        }
    }

    public void PausarMusica()
    {
        if (musicaSource != null && musicaSource.isPlaying)
        {
            musicaSource.Pause();
        }
    }

    public void ReanudarMusica()
    {
        if (sonidoSilenciado) return;

        if (musicaSource != null && !musicaSource.isPlaying)
        {
            musicaSource.UnPause();
        }
    }

    public void DetenerMusica()
    {
        if (musicaSource != null)
        {
            musicaSource.Stop();
        }
    }

    public void ReproducirSFX(AudioClip clip, float escalaVolumen = 1f)
    {
        if (sonidoSilenciado) return;

        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumenSFX * escalaVolumen);
        }
    }

    public void ReproducirRecogerHoja()
    {
        ReproducirSFX(sfxRecogerHoja, 1.0f);
    }

    public void ReproducirDanio()
    {
        ReproducirSFX(sfxDanio, 1.1f);
    }

    public void ReproducirSalto()
    {
        ReproducirSFX(sfxSalto, 0.7f);
    }

    public void ReproducirCaidaVacio()
    {
        ReproducirSFX(sfxCaidaVacio, 1.0f);
    }

    public void ReproducirPaso()
    {
        if (Time.time - tiempoUltimoPaso > 0.28f)
        {
            tiempoUltimoPaso = Time.time;
            ReproducirSFX(sfxPaso, 0.35f);
        }
    }

    public void ReproducirVictoria()
    {
        DetenerMusica();
        ReproducirSFX(sfxVictoria, 1.2f);
    }

    public void ReproducirBeberAgua()
    {
        ReproducirSFX(sfxBeberAgua, 1.0f);
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);
        if (musicaSource != null)
        {
            musicaSource.volume = volumenMusica;
        }
    }

    public void CambiarVolumenSFX(float nuevoVolumen)
    {
        volumenSFX = Mathf.Clamp01(nuevoVolumen);
        if (sfxSource != null)
        {
            sfxSource.volume = volumenSFX;
        }
    }

    public static ControladorAudio AsegurarInstancia()
    {
        if (Instancia == null)
        {
            var go = GameObject.Find("ControladorAudio");
            if (go == null)
            {
                go = new GameObject("ControladorAudio");
            }
            Instancia = go.GetComponent<ControladorAudio>();
            if (Instancia == null)
            {
                Instancia = go.AddComponent<ControladorAudio>();
            }
        }
        return Instancia;
    }
}
