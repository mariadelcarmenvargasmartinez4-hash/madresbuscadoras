using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Herramienta de Editor para generar la Tile Palette de Edificios Modulares de San Luis de la Paz,
/// crear los prefabs listos para arrastrar y decorar el fondo de la escena con arquitectura colonial auténtica.
/// </summary>
public static class CreadorPaletaEdificiosModulares
{
    private const string RUTA_SPRITESHEET = "Assets/imagen/edificios_modulares_san_luis.png";
    private const string CARPETA_PALETAS = "Assets/Paletas";
    private const string CARPETA_TILES = "Assets/Paletas/Tiles_Edificios";
    private const string CARPETA_PREFABS = "Assets/Prefabs/EdificiosModulares";
    private const string PREFAB_PALETA = "Assets/Paletas/Paleta_Edificios_Modulares.prefab";

    [MenuItem("Juego/3. Crear Tile Palette y Decorar con Edificios Modulares", false, 3)]
    public static void EjecutarCreacionCompleta()
    {
        Debug.Log("[Edificios Modulares] Iniciando creación de Tile Palette y decoración colonial...");

        // 1. Asegurar directorios
        if (!AssetDatabase.IsValidFolder("Assets/Paletas"))
        {
            AssetDatabase.CreateFolder("Assets", "Paletas");
        }
        if (!AssetDatabase.IsValidFolder(CARPETA_TILES))
        {
            AssetDatabase.CreateFolder("Assets/Paletas", "Tiles_Edificios");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(CARPETA_PREFABS))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "EdificiosModulares");
        }

        // 2. Cargar sprites generados de la hoja
        Object[] todosLosAssets = AssetDatabase.LoadAllAssetsAtPath(RUTA_SPRITESHEET);
        System.Collections.Generic.List<Sprite> listaSprites = new System.Collections.Generic.List<Sprite>();

        foreach (var obj in todosLosAssets)
        {
            if (obj is Sprite sp)
            {
                listaSprites.Add(sp);
            }
        }

        if (listaSprites.Count == 0)
        {
            Debug.LogError("[Edificios Modulares] No se encontraron sprites en " + RUTA_SPRITESHEET);
            return;
        }

        Debug.Log($"[Edificios Modulares] {listaSprites.Count} sprites encontrados en la hoja modular.");

        // 3. Crear o actualizar los assets Tile individuales
        System.Collections.Generic.Dictionary<string, Tile> mapaTiles = new System.Collections.Generic.Dictionary<string, Tile>();
        foreach (var sp in listaSprites)
        {
            string rutaTile = $"{CARPETA_TILES}/{sp.name}.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(rutaTile);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sp;
                tile.color = Color.white;
                tile.colliderType = Tile.ColliderType.None; // Fondo decorativo sin colisión bloqueadora
                AssetDatabase.CreateAsset(tile, rutaTile);
            }
            else
            {
                tile.sprite = sp;
                tile.colliderType = Tile.ColliderType.None;
                EditorUtility.SetDirty(tile);
            }
            mapaTiles[sp.name] = tile;
        }

        // 4. Crear la Paleta Prefab (Tile Palette) para la ventana 'Tile Palette' de Unity
        GameObject goPaleta = new GameObject("Paleta_Edificios_Modulares");
        Grid grid = goPaleta.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        GameObject goCapa = new GameObject("Layer1");
        goCapa.transform.SetParent(goPaleta.transform);
        Tilemap tm = goCapa.AddComponent<Tilemap>();
        TilemapRenderer tr = goCapa.AddComponent<TilemapRenderer>();
        tr.sortingOrder = -5;

        // Acomodar los tiles en una cuadrícula espaciosa y ordenada
        int columna = 0;
        int fila = 0;
        const int COLUMNAS_POR_FILA = 5;
        const int SEPARACION_X = 6;
        const int SEPARACION_Y = 6;

        foreach (var kvp in mapaTiles)
        {
            Vector3Int celda = new Vector3Int(columna * SEPARACION_X, -fila * SEPARACION_Y, 0);
            tm.SetTile(celda, kvp.Value);

            columna++;
            if (columna >= COLUMNAS_POR_FILA)
            {
                columna = 0;
                fila++;
            }
        }

        tm.CompressBounds();
        tm.RefreshAllTiles();

        // Guardar como Prefab de Paleta en Assets/Paletas/
        PrefabUtility.SaveAsPrefabAsset(goPaleta, PREFAB_PALETA);
        Object.DestroyImmediate(goPaleta);

        // 5. Crear Prefabs individuales en Assets/Prefabs/EdificiosModulares/ para arrastrar directamente
        Material matDefault = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        foreach (var sp in listaSprites)
        {
            string rutaPrefab = $"{CARPETA_PREFABS}/{sp.name}.prefab";
            GameObject goProp = new GameObject(sp.name);
            var sr = goProp.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = 0;
            if (matDefault != null) sr.sharedMaterial = matDefault;

            PrefabUtility.SaveAsPrefabAsset(goProp, rutaPrefab);
            Object.DestroyImmediate(goProp);
        }

        // 6. Decorar el fondo de la escena activa con edificios coloniales auténticos
        DecorarEscenaConEdificios();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Edificios Modulares] ¡Tile Palette, Tiles, Prefabs y Decoración de Fondo creados con éxito al 100%!");

        EditorUtility.DisplayDialog("Tile Palette y Edificios Modulares Creados",
            "¡Tile Palette de Edificios Modulares generada exitosamente!\n\n" +
            "• Carpeta de Paleta: Assets/Paletas/Paleta_Edificios_Modulares.prefab\n" +
            "• Carpeta de 26 Tiles: Assets/Paletas/Tiles_Edificios/\n" +
            "• Carpeta de 26 Prefabs: Assets/Prefabs/EdificiosModulares/\n" +
            "• El fondo del nivel ha sido enriquecido con fachadas, arcos coloniales, balcones y ventanas de San Luis de la Paz.\n\n" +
            "Para abrir la Paleta de Pintura en Unity:\n" +
            "1. Ve al menú: Window -> 2D -> Tile Palette\n" +
            "2. Selecciona 'Paleta_Edificios_Modulares'\n" +
            "3. ¡Pinta libremente en cualquier Tilemap o arrastra los prefabs!", "¡Entendido!");
    }

    public static void DecorarEscenaConEdificios()
    {
        var escenaActiva = EditorSceneManager.GetActiveScene();
        if (!escenaActiva.IsValid() || !escenaActiva.isLoaded) return;

        // Buscar o crear contenedor principal de decoración colonial de fondo
        GameObject goContenedor = GameObject.Find("Decoracion_Edificios_SanLuis");
        if (goContenedor == null)
        {
            goContenedor = new GameObject("Decoracion_Edificios_SanLuis");
        }

        // Limpiar elementos hijos antiguos si existieran para renovar con precisión
        for (int i = goContenedor.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(goContenedor.transform.GetChild(i).gameObject);
        }

        Object[] todosLosAssets = AssetDatabase.LoadAllAssetsAtPath(RUTA_SPRITESHEET);
        System.Collections.Generic.Dictionary<string, Sprite> sprites = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (var obj in todosLosAssets)
        {
            if (obj is Sprite sp) sprites[sp.name] = sp;
        }

        // Asegurar que el fondo gris neutro (Background) esté activo y detrás de todo
        GameObject goBackground = GameObject.Find("Background");
        if (goBackground != null)
        {
            goBackground.SetActive(true);
            var srBg = goBackground.GetComponent<SpriteRenderer>();
            if (srBg != null) srBg.sortingOrder = -100;
        }

        // Eliminar cualquier residuo del fondo de montañas (Fondo_Tematico_SanLuis / fondo_san_luis_de_la_paz)
        GameObject goFondoAntiguo = GameObject.Find("Fondo_Tematico_SanLuis");
        if (goFondoAntiguo != null)
        {
            Object.DestroyImmediate(goFondoAntiguo);
        }

        Material matDefault = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        // Configuraciones de edificios y elementos arquitectónicos de fondo
        // Z = 2.0 y sortingOrder = -5 (detalles en -4) para colocarse de fondo sobre Background (-100),
        // pero SIEMPRE detrás del camino (Ground en 0), del pasto (Grass en 0), props y la jugadora (10)
        var edificiosConfig = new[]
        {
            // =========================================================================
            // === PISO 1 (Calle Superior - Fachadas Coloniales Continuas de Pared a Pared) ===
            // 10 Casas completas continuas de X = -83.0 a X = +3.25, base en Y = 6.0 (pasto)
            // =========================================================================
            // Casa 1: X in [-83.00, -74.50]
            new { NombreGO = "Casa_01_Amarilla", SpriteNombre = "Fachada_Amarilla_Completa", Pos = new Vector3(-78.75f, 9.89f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Balcon_Casa_01", SpriteNombre = "Balcon_Cantera_Hierro", Pos = new Vector3(-78.75f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 2: X in [-74.50, -65.75] (Punto de inicio de Esperanza en X = -74.0)
            new { NombreGO = "Casa_02_Roja", SpriteNombre = "Fachada_Roja_Completa", Pos = new Vector3(-70.125f, 9.86f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Mirador_Casa_02", SpriteNombre = "Mirador_Balcon_Madera", Pos = new Vector3(-70.125f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 3: X in [-65.75, -57.25]
            new { NombreGO = "Casa_03_Amarilla", SpriteNombre = "Fachada_Amarilla_Completa", Pos = new Vector3(-61.50f, 9.89f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Balcon_Casa_03", SpriteNombre = "Balcon_Cantera_Hierro", Pos = new Vector3(-61.50f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },
            new { NombreGO = "Talavera_Casa_03", SpriteNombre = "Azulejos_Talavera_Guanajuato", Pos = new Vector3(-61.50f, 10.50f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 4: X in [-57.25, -48.50]
            new { NombreGO = "Casa_04_Roja", SpriteNombre = "Fachada_Roja_Completa", Pos = new Vector3(-52.875f, 9.86f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Mirador_Casa_04", SpriteNombre = "Mirador_Balcon_Madera", Pos = new Vector3(-52.875f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 5: X in [-48.50, -40.00] (Zona del Mercado Tradicional)
            new { NombreGO = "Casa_05_Amarilla", SpriteNombre = "Fachada_Amarilla_Completa", Pos = new Vector3(-44.25f, 9.89f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Balcon_Casa_05", SpriteNombre = "Balcon_Cantera_Hierro", Pos = new Vector3(-44.25f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },
            new { NombreGO = "Barandal_Casa_05", SpriteNombre = "Barandal_Hierro_Largo", Pos = new Vector3(-44.25f, 10.00f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 6: X in [-40.00, -31.25]
            new { NombreGO = "Casa_06_Roja", SpriteNombre = "Fachada_Roja_Completa", Pos = new Vector3(-35.625f, 9.86f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Mirador_Casa_06", SpriteNombre = "Mirador_Balcon_Madera", Pos = new Vector3(-35.625f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 7: X in [-31.25, -22.75]
            new { NombreGO = "Casa_07_Amarilla", SpriteNombre = "Fachada_Amarilla_Completa", Pos = new Vector3(-27.00f, 9.89f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Balcon_Casa_07", SpriteNombre = "Balcon_Cantera_Hierro", Pos = new Vector3(-27.00f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },
            new { NombreGO = "Talavera_Casa_07", SpriteNombre = "Azulejos_Talavera_Guanajuato", Pos = new Vector3(-27.00f, 10.50f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 8: X in [-22.75, -14.00]
            new { NombreGO = "Casa_08_Roja", SpriteNombre = "Fachada_Roja_Completa", Pos = new Vector3(-18.375f, 9.86f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Mirador_Casa_08", SpriteNombre = "Mirador_Balcon_Madera", Pos = new Vector3(-18.375f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 9: X in [-14.00, -5.50]
            new { NombreGO = "Casa_09_Amarilla", SpriteNombre = "Fachada_Amarilla_Completa", Pos = new Vector3(-9.75f, 9.89f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Balcon_Casa_09", SpriteNombre = "Balcon_Cantera_Hierro", Pos = new Vector3(-9.75f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },

            // Casa 10: X in [-5.50, +3.25]
            new { NombreGO = "Casa_10_Roja", SpriteNombre = "Fachada_Roja_Completa", Pos = new Vector3(-1.125f, 9.86f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Mirador_Casa_10", SpriteNombre = "Mirador_Balcon_Madera", Pos = new Vector3(-1.125f, 12.20f, 1.8f), Escala = Vector3.one, Order = -4 },

            // =========================================================================
            // === PISO 2 (Plaza Central y Registro Civil - Portales y Fachadas Municipales) ===
            // Base en Y = 1.0, techo en Y = 5.0 (luz vertical de 4.0 m)
            // =========================================================================
            new { NombreGO = "Arcada_Plaza_01", SpriteNombre = "Arco_Triple_Colonial", Pos = new Vector3(-72.0f, 3.0f, 2.0f), Escala = new Vector3(0.93f, 0.93f, 1f), Order = -5 },
            new { NombreGO = "Oficina_Plaza_01", SpriteNombre = "Fachada_Azul_Puerta", Pos = new Vector3(-65.0f, 3.0f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Arcada_Plaza_02", SpriteNombre = "Arco_Triple_Colonial", Pos = new Vector3(-58.0f, 3.0f, 2.0f), Escala = new Vector3(0.93f, 0.93f, 1f), Order = -5 },
            new { NombreGO = "Puerta_Registro_01", SpriteNombre = "Puerta_Doble_Tallada", Pos = new Vector3(-51.0f, 2.8f, 2.0f), Escala = new Vector3(0.85f, 0.85f, 1f), Order = -5 },
            new { NombreGO = "Ventana_Registro_01", SpriteNombre = "Ventana_Cantera_Vidrio", Pos = new Vector3(-48.0f, 3.0f, 1.8f), Escala = new Vector3(0.9f, 0.9f, 1f), Order = -4 },
            new { NombreGO = "Arcada_Plaza_03", SpriteNombre = "Arco_Triple_Colonial", Pos = new Vector3(-43.0f, 3.0f, 2.0f), Escala = new Vector3(0.93f, 0.93f, 1f), Order = -5 },
            new { NombreGO = "Oficina_RegistroCivil", SpriteNombre = "Fachada_Azul_Puerta", Pos = new Vector3(-36.0f, 3.0f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Arco_Piedra_Plaza", SpriteNombre = "Arco_Piedra_MedioPunto", Pos = new Vector3(-31.0f, 2.8f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Oficina_Plaza_03", SpriteNombre = "Fachada_Azul_Puerta", Pos = new Vector3(-26.0f, 3.0f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Puerta_Registro_02", SpriteNombre = "Puerta_Doble_Tallada", Pos = new Vector3(-21.0f, 2.8f, 2.0f), Escala = new Vector3(0.85f, 0.85f, 1f), Order = -5 },
            new { NombreGO = "Arcada_Plaza_04", SpriteNombre = "Arco_Triple_Colonial", Pos = new Vector3(-15.0f, 3.0f, 2.0f), Escala = new Vector3(0.93f, 0.93f, 1f), Order = -5 },
            new { NombreGO = "Oficina_Plaza_04", SpriteNombre = "Fachada_Azul_Puerta", Pos = new Vector3(-8.0f, 3.0f, 2.0f), Escala = Vector3.one, Order = -5 },

            // =========================================================================
            // === PISO 3 (Zona de Archivos Coloniales & Panteón Histórico) ===
            // Base en Y = -4.0, techo en Y = 0.0 (luz vertical de 4.0 m)
            // =========================================================================
            new { NombreGO = "Arco_Piedra_Archivos", SpriteNombre = "Arco_Piedra_MedioPunto", Pos = new Vector3(-72.0f, -2.2f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Arco_Archivos_01", SpriteNombre = "Arco_Cantera_Mixtilineo", Pos = new Vector3(-60.0f, -2.2f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Puerta_Archivos_01", SpriteNombre = "Puerta_Doble_Tallada", Pos = new Vector3(-53.0f, -2.2f, 2.0f), Escala = new Vector3(0.85f, 0.85f, 1f), Order = -5 },
            new { NombreGO = "Ventana_Archivos_01", SpriteNombre = "Ventana_Lattice_Plomada", Pos = new Vector3(-49.5f, -1.8f, 1.8f), Escala = Vector3.one, Order = -4 },
            new { NombreGO = "Arco_Archivos_02", SpriteNombre = "Arco_Cantera_Mixtilineo", Pos = new Vector3(-45.0f, -2.2f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Puerta_Archivos_02", SpriteNombre = "Puerta_Madera_Simple", Pos = new Vector3(-39.0f, -2.2f, 2.0f), Escala = new Vector3(0.85f, 0.85f, 1f), Order = -5 },
            new { NombreGO = "Rejas_Archivos_01", SpriteNombre = "Rejas_Hierro_Ventana", Pos = new Vector3(-35.5f, -2.0f, 1.8f), Escala = Vector3.one, Order = -4 },
            new { NombreGO = "Arco_Archivos_03", SpriteNombre = "Arco_Cantera_Mixtilineo", Pos = new Vector3(-32.0f, -2.2f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Puerta_Archivos_03", SpriteNombre = "Puerta_Doble_Tallada", Pos = new Vector3(-25.0f, -2.2f, 2.0f), Escala = new Vector3(0.85f, 0.85f, 1f), Order = -5 },
            new { NombreGO = "Ventana_Archivos_02", SpriteNombre = "Ventana_Madera_Cuadros", Pos = new Vector3(-21.5f, -2.0f, 1.8f), Escala = new Vector3(0.9f, 0.9f, 1f), Order = -4 },
            new { NombreGO = "Arco_Archivos_04", SpriteNombre = "Arco_Cantera_Mixtilineo", Pos = new Vector3(-18.0f, -2.2f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Puerta_Archivos_04", SpriteNombre = "Puerta_Madera_Simple", Pos = new Vector3(-11.0f, -2.2f, 2.0f), Escala = new Vector3(0.85f, 0.85f, 1f), Order = -5 },

            // =========================================================================
            // === CUEVA SUBTERRÁNEA (Descensos Mineros & Bóveda Final) ===
            // =========================================================================
            new { NombreGO = "Arco_Entrada_Cueva", SpriteNombre = "Arco_Piedra_MedioPunto", Pos = new Vector3(-66.0f, -7.5f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Arco_Pasaje_Cueva", SpriteNombre = "Arco_Piedra_MedioPunto", Pos = new Vector3(-58.0f, -11.5f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Arco_Puente_Cueva", SpriteNombre = "Arco_Cantera_Mixtilineo", Pos = new Vector3(-54.0f, -14.5f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Arco_Boveda_Final", SpriteNombre = "Arco_Triple_Colonial", Pos = new Vector3(-74.0f, -17.5f, 2.0f), Escala = Vector3.one, Order = -5 },
            new { NombreGO = "Rejas_Boveda_Final", SpriteNombre = "Rejas_Hierro_Ventana", Pos = new Vector3(-74.0f, -15.5f, 1.8f), Escala = Vector3.one, Order = -4 }
        };

        foreach (var conf in edificiosConfig)
        {
            if (sprites.TryGetValue(conf.SpriteNombre, out Sprite sp))
            {
                GameObject elem = new GameObject(conf.NombreGO);
                elem.transform.SetParent(goContenedor.transform);
                elem.transform.position = conf.Pos;
                elem.transform.localScale = conf.Escala;

                var sr = elem.AddComponent<SpriteRenderer>();
                sr.sprite = sp;
                sr.sortingOrder = conf.Order;
                if (matDefault != null) sr.sharedMaterial = matDefault;
                sr.color = Color.white;
            }
        }

        EditorSceneManager.MarkSceneDirty(escenaActiva);
        EditorSceneManager.SaveScene(escenaActiva);
        Debug.Log("[Edificios Modulares] Fachadas, arcos y balcones colocados exitosamente en la escena.");
    }
}
