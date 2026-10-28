using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Script de Editor para Unity que configura automáticamente el Nivel 2.
/// - Abre la entrada subterránea al foso profundo tras el archivero.
/// - Coloca puentes y plataformas escalonadas (one-way) conectando todas las hojas (1 a 10).
/// - 10 Hojas de expediente 100% accesibles y libres de atascos.
/// - Obstáculos equilibrados y retorno a la 1ª plataforma al caer al vacío.
/// </summary>
[InitializeOnLoad]
public static class ConfigurarNivel2Automatico
{
    static ConfigurarNivel2Automatico()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Application.isPlaying)
            {
                ConfigurarTodoElNivel(false);
            }
        };
    }

    [MenuItem("Juego/Configurar Nivel 2 Completo")]
    public static void MenuConfigurar()
    {
        ConfigurarTodoElNivel(true);
    }

    public static void ConfigurarTodoElNivel(bool mostrarDialogo)
    {
        var escenaActiva = EditorSceneManager.GetActiveScene();
        if (!escenaActiva.IsValid() || !escenaActiva.isLoaded)
        {
            return;
        }

        // 0. LIMPIEZA DE OBJETOS ANTIGUOS Y DUPLICADOS
        string[] objetosAntiguos = new string[]
        {
            "Obstaculo_Puas_1_Camino", "Obstaculo_Puas_2_Bajada", "Obstaculo_Puas_3_Cueva",
            "Obstaculo_Puas_4_Descenso", "Obstaculo_Puas_5_Boveda",
            "Pista_Hoja_1_Cartelera", "Pista_Hoja_1_Superficie",
            "Pista_Hoja_2_Plaza", "Pista_Hoja_3_Subterraneo", "Pista_Hoja_4_Boveda_Final"
        };
        foreach (string nom in objetosAntiguos)
        {
            GameObject obj = GameObject.Find(nom);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        // 0.1 ABRIR LA ENTRADA SUBTERRÁNEA EN EL TILEMAP (Elimina el techo sólido en Y = -5)
        GameObject goTilemap = GameObject.Find("Tilemap");
        if (goTilemap != null)
        {
            Tilemap tm = goTilemap.GetComponent<Tilemap>();
            if (tm != null)
            {
                for (int x = -68; x <= -64; x++)
                {
                    tm.SetTile(new Vector3Int(x, -5, 0), null);
                }
            }
        }

        Material matDefault = AssetDatabase.LoadAssetAtPath<Material>("Assets/Cainos/Pixel Art Platformer - Village Props/Material/MT Village Props - Default.mat");
        if (matDefault == null)
        {
            matDefault = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        }

        // 1. CONTROLADOR DE NIVEL (10 HOJAS, 10 ACERTIJOS Y HUD)
        GameObject goControlador = GameObject.Find("ControladorNivel");
        if (goControlador == null)
        {
            goControlador = new GameObject("ControladorNivel");
        }
        var ctrl = goControlador.GetComponent<ControladorNivel2>();
        if (ctrl == null)
        {
            ctrl = goControlador.AddComponent<ControladorNivel2>();
        }
        ctrl.saludMaxima = 100f;
        ctrl.saludActual = 100f;
        ctrl.totalHojas = 10;

        // 1.1 CONTROLADOR DE AUDIO (MÚSICA AMBIENTAL Y EFECTOS SONOROS)
        GameObject goAudio = GameObject.Find("ControladorAudio");
        if (goAudio == null)
        {
            goAudio = new GameObject("ControladorAudio");
        }
        var ctrlAudio = goAudio.GetComponent<ControladorAudio>();
        if (ctrlAudio == null)
        {
            ctrlAudio = goAudio.AddComponent<ControladorAudio>();
        }
        ctrlAudio.musicaFondoNivel2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Musica/musica_fondo_nivel2.wav");
        ctrlAudio.sfxRecogerHoja = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_recoger_hoja.wav");
        ctrlAudio.sfxDanio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_danio.wav");
        ctrlAudio.sfxSalto = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_salto.wav");
        ctrlAudio.sfxCaidaVacio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_caida_vacio.wav");
        ctrlAudio.sfxPaso = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_paso.wav");
        ctrlAudio.sfxVictoria = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_victoria_expediente.wav");
        ctrlAudio.sfxBeberAgua = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/sfx_beber_agua.wav");

        // 2. CÁMARA (SEGUIMIENTO SUAVE CENTRADO)
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            var camSigue = cam.GetComponent<CamaraSigue>();
            if (camSigue == null)
            {
                camSigue = cam.gameObject.AddComponent<CamaraSigue>();
            }
            camSigue.usarLimites = false;
            camSigue.offset = new Vector3(0f, 1.2f, -10f);
            camSigue.suavizado = 0.12f;

            var jugador = GameObject.Find("MadreBuscadora");
            if (jugador != null)
            {
                camSigue.objetivo = jugador.transform;
            }
        }

        // 3. PROTAGONISTA (MADRE BUSCADORA) - Configuración y Reaparición en 1ª Plataforma
        GameObject goMadre = GameObject.Find("MadreBuscadora");
        Vector3 posPrimeraPlataforma = new Vector3(-74f, 6.2f, 0f);
        if (goMadre != null)
        {
            var srMadre = goMadre.GetComponent<SpriteRenderer>();
            if (srMadre != null && matDefault != null)
            {
                srMadre.sharedMaterial = matDefault;
                srMadre.sortingOrder = 15;
            }

            var movMadre = goMadre.GetComponent<MovimientoMadre>();
            if (movMadre != null)
            {
                movMadre.posicionPrimeraPlataforma = posPrimeraPlataforma;
                movMadre.fuerzaSalto = 11.8f;
            }
        }

        // 4. FONDO TEMÁTICO MULTICAPA CON PARALLAX (SAN LUIS DE LA PAZ Y BUROCRACIA)
        // Destruir objeto de fondo antiguo si existe
        GameObject goFondoAntiguo = GameObject.Find("FondoEscenario");
        if (goFondoAntiguo != null) Object.DestroyImmediate(goFondoAntiguo);

        Sprite spriteEscena = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/imagen/madre_buscadora_escena.png");
        Sprite spriteSanLuis = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/imagen/fondo_san_luis_de_la_paz.jpg");

        // 4.1 CAPA 1: CIELO Y HORIZONTE LEJANO (Desierto y colinas de Guanajuato)
        if (spriteEscena != null)
        {
            GameObject goCielo = GameObject.Find("Fondo_Cielo_Horizonte");
            if (goCielo == null) goCielo = new GameObject("Fondo_Cielo_Horizonte");
            goCielo.transform.position = new Vector3(-45f, 6.0f, 25f);
            goCielo.transform.localScale = new Vector3(28f, 18f, 1f);

            var srCielo = goCielo.GetComponent<SpriteRenderer>();
            if (srCielo == null) srCielo = goCielo.AddComponent<SpriteRenderer>();
            srCielo.sprite = spriteEscena;
            srCielo.sortingOrder = -100;
            if (matDefault != null) srCielo.sharedMaterial = matDefault;
            srCielo.color = new Color(0.58f, 0.58f, 0.68f, 0.85f);

            var parCielo = goCielo.GetComponent<FondoParallax>();
            if (parCielo == null) parCielo = goCielo.AddComponent<FondoParallax>();
            parCielo.factorX = 0.85f;
            parCielo.factorY = 0.75f;
            parCielo.ordenRenderizado = -100;
        }

        // 4.2 CAPA 2: EDIFICIOS INSTITUCIONALES DE SAN LUIS DE LA PAZ (Puertas cerradas y Registro)
        if (spriteSanLuis != null)
        {
            GameObject goSanLuis = GameObject.Find("Fondo_Burocracia_SanLuis");
            if (goSanLuis == null) goSanLuis = new GameObject("Fondo_Burocracia_SanLuis");
            goSanLuis.transform.position = new Vector3(-45f, -1.0f, 16f);
            goSanLuis.transform.localScale = new Vector3(26f, 15f, 1f);

            var srSanLuis = goSanLuis.GetComponent<SpriteRenderer>();
            if (srSanLuis == null) srSanLuis = goSanLuis.AddComponent<SpriteRenderer>();
            srSanLuis.sprite = spriteSanLuis;
            srSanLuis.sortingOrder = -80;
            if (matDefault != null) srSanLuis.sharedMaterial = matDefault;
            srSanLuis.color = new Color(0.85f, 0.85f, 0.90f, 0.95f);

            var parSanLuis = goSanLuis.GetComponent<FondoParallax>();
            if (parSanLuis == null) parSanLuis = goSanLuis.AddComponent<FondoParallax>();
            parSanLuis.factorX = 0.65f;
            parSanLuis.factorY = 0.45f;
            parSanLuis.ordenRenderizado = -80;
        }

        // 4.3 CAPA 3: FONDO PROFUNDO SUBTERRÁNEO (Archivos sepultados y bóveda)
        if (spriteEscena != null)
        {
            GameObject goCueva = GameObject.Find("Fondo_Cueva_Profunda");
            if (goCueva == null) goCueva = new GameObject("Fondo_Cueva_Profunda");
            goCueva.transform.position = new Vector3(-58f, -14.0f, 12f);
            goCueva.transform.localScale = new Vector3(26f, 16f, 1f);

            var srCueva = goCueva.GetComponent<SpriteRenderer>();
            if (srCueva == null) srCueva = goCueva.AddComponent<SpriteRenderer>();
            srCueva.sprite = spriteEscena;
            srCueva.sortingOrder = -70;
            if (matDefault != null) srCueva.sharedMaterial = matDefault;
            srCueva.color = new Color(0.18f, 0.20f, 0.26f, 0.92f);

            var parCueva = goCueva.GetComponent<FondoParallax>();
            if (parCueva == null) parCueva = goCueva.AddComponent<FondoParallax>();
            parCueva.factorX = 0.50f;
            parCueva.factorY = 0.35f;
            parCueva.ordenRenderizado = -70;
        }

        // 5. CARGAR SPRITES DE LAS HOJAS, PICOS, PLATAFORMAS Y ANTORCHAS
        Sprite[] spritesNoti = new Sprite[4];
        Sprite spritePua = null;
        Sprite spriteSpikeBall = null;
        Sprite spritePlataformaMadera = null;
        Sprite spriteAntorcha = null;

        Object[] propsAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Cainos/Pixel Art Platformer - Village Props/Texture/TX Village Props.png");
        foreach (var a in propsAssets)
        {
            if (a is Sprite sp)
            {
                if (sp.name.Contains("Billboard Noti 01")) spritesNoti[0] = sp;
                else if (sp.name.Contains("Billboard Noti 02")) spritesNoti[1] = sp;
                else if (sp.name.Contains("Billboard Noti 03")) spritesNoti[2] = sp;
                else if (sp.name.Contains("Billboard Noti 04")) spritesNoti[3] = sp;
                else if (sp.name.Contains("Spike Ball")) spriteSpikeBall = sp;
                else if (sp.name.Contains("Spike") && !sp.name.Contains("Ball")) spritePua = sp;
                else if (sp.name.Contains("Platform M") || sp.name.Contains("Platform S")) spritePlataformaMadera = sp;
                else if (sp.name.Contains("Torch") || sp.name.Contains("Road Sign")) spriteAntorcha = sp;
            }
        }

        // 6. CONFIGURAR LAS 10 HOJAS ESCONDIDAS (TODAS 100% ACCESIBLES)
        var hojasInfo = new[]
        {
            new { Nombre = "Pista_Hoja_1_Mercado", Pos = new Vector3(-47.5f, 7.2f, 0f), Num = 1, Titulo = "Ficha de Búsqueda 2011", Detalle = "Datos de los 22 migrantes y su salida de San Luis de la Paz." },
            new { Nombre = "Pista_Hoja_2_Parada", Pos = new Vector3(-27.5f, 6.2f, 0f), Num = 2, Titulo = "Talón del Boleto Roto", Detalle = "Horario de salida y registro del autobús en la terminal." },
            new { Nombre = "Pista_Hoja_3_Pozo", Pos = new Vector3(-42.8f, 1.4f, 0f), Num = 3, Titulo = "Testimonio de la Dependienta", Detalle = "Última conversación antes de abordar el transporte." },
            new { Nombre = "Pista_Hoja_4_Registro", Pos = new Vector3(-20.5f, 1.4f, 0f), Num = 4, Titulo = "Acta y Lista de Pasajeros", Detalle = "Nombres cotejados con el Registro Civil municipal." },
            new { Nombre = "Pista_Hoja_5_Puente", Pos = new Vector3(-33.0f, -5.2f, 0f), Num = 5, Titulo = "Bitácora de la Ruta Norte", Detalle = "Desviación no autorizada del autobús detectada." },
            new { Nombre = "Pista_Hoja_6_Estatua", Pos = new Vector3(-44.5f, -3.8f, 0f), Num = 6, Titulo = "Oficio Confidencial No. 22", Detalle = "Primeros indicios forenses vinculados a fosas del norte." },
            new { Nombre = "Pista_Hoja_7_Archivero", Pos = new Vector3(-62.0f, -4.8f, 0f), Num = 7, Titulo = "Carpeta de Denuncia Rechazada", Detalle = "Documentos institucionales que fueron archivados sin investigar." },
            new { Nombre = "Pista_Hoja_8_Andamio", Pos = new Vector3(-64.5f, -7.5f, 0f), Num = 8, Titulo = "Informe Preliminar de San Fernando", Detalle = "Reportes que confirman hallazgos en San Fernando, Tamaulipas." },
            new { Nombre = "Pista_Hoja_9_Cadenas", Pos = new Vector3(-54.0f, -15.5f, 0f), Num = 9, Titulo = "Ficha de Cotejo Forense del EAAF", Detalle = "Peritaje independiente de antropología forense." },
            new { Nombre = "Pista_Hoja_10_Boveda_Final", Pos = new Vector3(-73.8f, -19.2f, 0f), Num = 10, Titulo = "Expediente Completo con Sello Oficial", Detalle = "¡Expediente reunido al 100%! Datos listos para la búsqueda en campo (Nivel 3)." }
        };

        for (int i = 0; i < hojasInfo.Length; i++)
        {
            var h = hojasInfo[i];
            GameObject goHoja = GameObject.Find(h.Nombre);
            if (goHoja == null)
            {
                goHoja = new GameObject(h.Nombre);
            }
            goHoja.transform.position = h.Pos;
            goHoja.transform.localScale = (i == 9) ? new Vector3(2.2f, 2.2f, 1f) : new Vector3(1.8f, 1.8f, 1f);

            Transform rotuloAntiguo = goHoja.transform.Find("RotuloFlotante");
            if (rotuloAntiguo != null)
            {
                Object.DestroyImmediate(rotuloAntiguo.gameObject);
            }

            var srH = goHoja.GetComponent<SpriteRenderer>();
            if (srH == null) srH = goHoja.AddComponent<SpriteRenderer>();
            int spriteIdx = i % 4;
            if (spritesNoti[spriteIdx] != null) srH.sprite = spritesNoti[spriteIdx];
            if (matDefault != null) srH.sharedMaterial = matDefault;
            srH.sortingOrder = 18;

            var colH = goHoja.GetComponent<BoxCollider2D>();
            if (colH == null) colH = goHoja.AddComponent<BoxCollider2D>();
            colH.isTrigger = true;
            colH.size = new Vector2(0.6f, 0.8f);

            var pistaScript = goHoja.GetComponent<PistaHoja>();
            if (pistaScript == null) pistaScript = goHoja.AddComponent<PistaHoja>();
            pistaScript.numeroHoja = h.Num;
            pistaScript.titulo = h.Titulo;
            pistaScript.pistaDetalle = h.Detalle;
        }

        // 7. PLATAFORMAS ESCALONADAS Y PUENTES (ONE-WAY): TODA LA CUEVA CONECTADA
        var escalonesInfo = new[]
        {
            // 7.1 Salida del foso en la calle superior hacia arriba
            new { Nombre = "Plataforma_Escalon_Subida_1", Pos = new Vector3(-35.0f, 2.7f, 0f), Escala = new Vector2(2.4f, 0.35f) },
            new { Nombre = "Plataforma_Escalon_Subida_2", Pos = new Vector3(-33.0f, 4.4f, 0f), Escala = new Vector2(2.4f, 0.35f) },

            // 7.2 Conexión intermedia hacia el puente del descenso
            new { Nombre = "Plataforma_Escalon_Puente", Pos = new Vector3(-30.5f, -2.2f, 0f), Escala = new Vector2(3.0f, 0.35f) },

            // 7.3 NUEVA ENTRADA SUBTERRÁNEA: Descenso desde el piso de Archivos (Y = -5) hacia la Cueva (Y = -9.0)
            new { Nombre = "Plataforma_Entrada_Cueva_1", Pos = new Vector3(-66.5f, -6.6f, 0f), Escala = new Vector2(3.0f, 0.35f) },
            new { Nombre = "Plataforma_Entrada_Cueva_2", Pos = new Vector3(-64.8f, -8.2f, 0f), Escala = new Vector2(3.0f, 0.35f) },

            // 7.4 Descenso desde el andamio superior (Hoja 8) hacia el nivel medio de la cueva
            new { Nombre = "Plataforma_Cueva_Bajada_1", Pos = new Vector3(-58.5f, -11.5f, 0f), Escala = new Vector2(3.2f, 0.35f) },
            new { Nombre = "Plataforma_Cueva_Bajada_2", Pos = new Vector3(-55.5f, -13.5f, 0f), Escala = new Vector2(3.2f, 0.35f) },

            // 7.5 Puente intermedio conectando la colina de Hoja 9 con la isla central del cofre
            new { Nombre = "Plataforma_Cueva_Puente_Centro", Pos = new Vector3(-62.5f, -14.2f, 0f), Escala = new Vector2(3.8f, 0.35f) },

            // 7.6 Conexión desde la isla central hacia la Bóveda Final (Hoja 10)
            new { Nombre = "Plataforma_Cueva_Hacia_Boveda_1", Pos = new Vector3(-71.5f, -16.8f, 0f), Escala = new Vector2(3.2f, 0.35f) },
            new { Nombre = "Plataforma_Cueva_Hacia_Boveda_2", Pos = new Vector3(-74.0f, -18.5f, 0f), Escala = new Vector2(3.2f, 0.35f) }
        };

        foreach (var esc in escalonesInfo)
        {
            GameObject goEsc = GameObject.Find(esc.Nombre);
            if (goEsc == null)
            {
                goEsc = new GameObject(esc.Nombre);
            }
            goEsc.transform.position = esc.Pos;
            goEsc.transform.localScale = new Vector3(esc.Escala.x, esc.Escala.y, 1f);

            var srEsc = goEsc.GetComponent<SpriteRenderer>();
            if (srEsc == null) srEsc = goEsc.AddComponent<SpriteRenderer>();
            if (spritePlataformaMadera != null) srEsc.sprite = spritePlataformaMadera;
            if (matDefault != null) srEsc.sharedMaterial = matDefault;
            srEsc.color = new Color(0.85f, 0.75f, 0.65f, 1f);
            srEsc.sortingOrder = 6;

            var colEsc = goEsc.GetComponent<BoxCollider2D>();
            if (colEsc == null) colEsc = goEsc.AddComponent<BoxCollider2D>();
            colEsc.size = Vector2.one;
            colEsc.usedByEffector = true;

            var effector = goEsc.GetComponent<PlatformEffector2D>();
            if (effector == null) effector = goEsc.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 170f;
        }

        // Señal visual de la Entrada a la Cueva
        GameObject goSenal = GameObject.Find("Senal_Entrada_Subterraneo");
        if (goSenal == null)
        {
            goSenal = new GameObject("Senal_Entrada_Subterraneo");
        }
        goSenal.transform.position = new Vector3(-63.5f, -4.3f, 0f);
        goSenal.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        var srSenal = goSenal.GetComponent<SpriteRenderer>();
        if (srSenal == null) srSenal = goSenal.AddComponent<SpriteRenderer>();
        if (spriteAntorcha != null) srSenal.sprite = spriteAntorcha;
        if (matDefault != null) srSenal.sharedMaterial = matDefault;
        srSenal.sortingOrder = 10;

        // Plataforma principal del puente en el descenso
        GameObject goPuente = GameObject.Find("Plataforma_Puente_Descenso");
        if (goPuente == null)
        {
            goPuente = new GameObject("Plataforma_Puente_Descenso");
        }
        goPuente.transform.position = new Vector3(-35f, -5.6f, 0f);
        goPuente.transform.localScale = new Vector3(12f, 0.6f, 1f);

        var colPuente = goPuente.GetComponent<BoxCollider2D>();
        if (colPuente == null) colPuente = goPuente.AddComponent<BoxCollider2D>();
        colPuente.size = Vector2.one;

        var srPuente = goPuente.GetComponent<SpriteRenderer>();
        if (srPuente == null) srPuente = goPuente.AddComponent<SpriteRenderer>();
        if (matDefault != null) srPuente.sharedMaterial = matDefault;
        srPuente.color = new Color(0.4f, 0.3f, 0.2f, 0.8f);
        srPuente.sortingOrder = 2;

        // 8. OBSTÁCULOS MÓVILES (PATRULLAS QUE AMENAZAN LAS RUTAS)
        var movilesInfo = new[]
        {
            new { Nombre = "Obstaculo_Movil_1_Mercado", Pos = new Vector3(-55.0f, 6.8f, 0f), Tipo = ObstaculoMovil.TipoPatrulla.Horizontal, Dist = 3.5f, Vel = 2.2f },
            new { Nombre = "Obstaculo_Movil_2_Parada", Pos = new Vector3(-33.0f, 6.8f, 0f), Tipo = ObstaculoMovil.TipoPatrulla.Horizontal, Dist = 2.5f, Vel = 2.5f },
            new { Nombre = "Obstaculo_Movil_3_Plaza", Pos = new Vector3(-32.0f, 1.5f, 0f), Tipo = ObstaculoMovil.TipoPatrulla.Horizontal, Dist = 3.5f, Vel = 2.4f },
            new { Nombre = "Obstaculo_Movil_4_Andamio", Pos = new Vector3(-36.0f, -3.5f, 0f), Tipo = ObstaculoMovil.TipoPatrulla.Vertical, Dist = 2.0f, Vel = 2.0f },
            new { Nombre = "Obstaculo_Movil_5_Estatua", Pos = new Vector3(-50.0f, -4.5f, 0f), Tipo = ObstaculoMovil.TipoPatrulla.Horizontal, Dist = 3.0f, Vel = 2.6f },
            new { Nombre = "Obstaculo_Movil_6_Boveda", Pos = new Vector3(-68.0f, -18.2f, 0f), Tipo = ObstaculoMovil.TipoPatrulla.Horizontal, Dist = 2.5f, Vel = 2.4f }
        };

        for (int m = 0; m < movilesInfo.Length; m++)
        {
            var mov = movilesInfo[m];
            GameObject goMov = GameObject.Find(mov.Nombre);
            if (goMov == null)
            {
                goMov = new GameObject(mov.Nombre);
            }
            goMov.transform.position = mov.Pos;
            goMov.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

            var srMov = goMov.GetComponent<SpriteRenderer>();
            if (srMov == null) srMov = goMov.AddComponent<SpriteRenderer>();
            if (spriteSpikeBall != null) srMov.sprite = spriteSpikeBall;
            else if (spritePua != null) srMov.sprite = spritePua;
            if (matDefault != null) srMov.sharedMaterial = matDefault;
            srMov.sortingOrder = 9;

            var colMov = goMov.GetComponent<CircleCollider2D>();
            if (colMov == null) colMov = goMov.AddComponent<CircleCollider2D>();
            colMov.isTrigger = true;
            colMov.radius = 0.45f;

            var scriptMov = goMov.GetComponent<ObstaculoMovil>();
            if (scriptMov == null) scriptMov = goMov.AddComponent<ObstaculoMovil>();
            scriptMov.tipoMovimiento = mov.Tipo;
            scriptMov.distancia = mov.Dist;
            scriptMov.velocidad = mov.Vel;
            scriptMov.danio = 20f;
            scriptMov.fuerzaRebote = 5f;
        }

        // 9. OBSTÁCULOS ESTÁTICOS (12 TRAMPAS DE PÚAS SEPARADAS DE LAS HOJAS)
        Vector3[] posicionesPeligro = new Vector3[]
        {
            new Vector3(-65.0f, 6.0f, 0f),   // Puas 1: Camino superior inicial
            new Vector3(-52.5f, 6.0f, 0f),   // Puas 2: Antes del mercado
            new Vector3(-31.5f, 6.0f, 0f),   // Puas 3: Antes de la barricada
            new Vector3(-25.0f, 1.0f, 0f),   // Puas 4: Terraza del registro
            new Vector3(-37.0f, 1.0f, 0f),   // Puas 5: Plaza central
            new Vector3(-48.5f, 1.0f, 0f),   // Puas 6: Al oeste del pozo
            new Vector3(-39.0f, -5.0f, 0f),  // Puas 7: Paso al puente
            new Vector3(-54.0f, -5.0f, 0f),  // Puas 8: Camino a archivos
            new Vector3(-58.5f, -10.0f, 0f), // Puas 9: Descenso de andamios (lejos de Hoja 8)
            new Vector3(-70.0f, -14.0f, 0f), // Puas 10: Cornisa inferior
            new Vector3(-61.0f, -17.0f, 0f), // Puas 11: Fosas y túnel
            new Vector3(-70.5f, -20.0f, 0f)  // Puas 12: Entrada a la bóveda final
        };

        for (int p = 0; p < posicionesPeligro.Length; p++)
        {
            string nombreObs = $"Obstaculo_Puas_{p + 1}";
            GameObject goObs = GameObject.Find(nombreObs);
            if (goObs == null)
            {
                goObs = new GameObject(nombreObs);
            }
            goObs.transform.position = posicionesPeligro[p];

            var srObs = goObs.GetComponent<SpriteRenderer>();
            if (srObs == null) srObs = goObs.AddComponent<SpriteRenderer>();
            if (spritePua != null) srObs.sprite = spritePua;
            if (matDefault != null) srObs.sharedMaterial = matDefault;
            srObs.sortingOrder = 8;
            srObs.color = new Color(1f, 0.75f, 0.75f, 1f);

            var colObs = goObs.GetComponent<BoxCollider2D>();
            if (colObs == null) colObs = goObs.AddComponent<BoxCollider2D>();
            colObs.isTrigger = true;
            colObs.size = new Vector2(1.0f, 0.35f);
            colObs.offset = new Vector2(0f, 0.05f);

            var obsScript = goObs.GetComponent<ObstaculoPeligro>();
            if (obsScript == null) obsScript = goObs.AddComponent<ObstaculoPeligro>();
            obsScript.danio = 20f;
            obsScript.fuerzaRebote = 4.5f;
            obsScript.intervaloDanio = 1.2f;
        }

        // 10. ÍTEMS CONSUMIBLES: BOTELLAS DE AGUA PARA RECUPERAR SALUD/ÁNIMO (+25)
        Sprite spriteBotella = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/imagen/botella_agua.png");

        var botellasInfo = new[]
        {
            new { Nombre = "Botella_Agua_1_Superficie", Pos = new Vector3(-38.5f, 6.2f, 0f) },
            new { Nombre = "Botella_Agua_2_Registro", Pos = new Vector3(-28.0f, 1.4f, 0f) },
            new { Nombre = "Botella_Agua_3_Cueva", Pos = new Vector3(-59.0f, -14.2f, 0f) }
        };

        foreach (var b in botellasInfo)
        {
            GameObject goBotella = GameObject.Find(b.Nombre);
            if (goBotella == null)
            {
                goBotella = new GameObject(b.Nombre);
            }
            goBotella.transform.position = b.Pos;
            goBotella.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            var srBotella = goBotella.GetComponent<SpriteRenderer>();
            if (srBotella == null) srBotella = goBotella.AddComponent<SpriteRenderer>();
            if (spriteBotella != null) srBotella.sprite = spriteBotella;
            if (matDefault != null) srBotella.sharedMaterial = matDefault;
            srBotella.sortingOrder = 17;

            var colBotella = goBotella.GetComponent<BoxCollider2D>();
            if (colBotella == null) colBotella = goBotella.AddComponent<BoxCollider2D>();
            colBotella.isTrigger = true;
            colBotella.size = new Vector2(0.7f, 0.9f);

            var itemBotella = goBotella.GetComponent<ItemBotellaAgua>();
            if (itemBotella == null) itemBotella = goBotella.AddComponent<ItemBotellaAgua>();
            itemBotella.saludRestaurada = 25f;
            itemBotella.velocidadFlotacion = 2.4f;
            itemBotella.amplitudFlotacion = 0.12f;
        }

        EditorSceneManager.MarkSceneDirty(escenaActiva);
        EditorSceneManager.SaveScene(escenaActiva);

        Debug.Log("[Nivel 2] ¡Entrada subterránea abierta y puentes conectados para las 10 hojas!");

        if (mostrarDialogo)
        {
            EditorUtility.DisplayDialog("Nivel 2 - Entrada Abierta",
                "¡Se habilitó el acceso completo a la zona subterránea!\n\n" +
                "• Entrada abierta: Se retiró el techo bloqueado en el piso de archivos (Y = -5).\n" +
                "• Red de puentes y plataformas: Se instalaron plataformas escalonadas (one-way) para descender hacia la Hoja 8, cruzar a la Hoja 9 y llegar a la Bóveda Final (Hoja 10).\n" +
                "• Doble vía: Puedes subir y bajar libremente por toda la cueva sin quedar atrapado.\n" +
                "• Señalización y acertijos actualizados.\n\n" +
                "¡Presiona Play para explorar!", "Aceptar");
        }
    }
}
