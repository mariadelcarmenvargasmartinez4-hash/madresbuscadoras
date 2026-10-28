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
        Material matDefault = AssetDatabase.LoadAssetAtPath<Material>("Assets/Cainos/Pixel Art Platformer - Village Props/Material/MT Village Props - Default.mat");
        if (matDefault == null)
        {
            matDefault = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        }

        foreach (var sp in listaSprites)
        {
            string rutaPrefab = $"{CARPETA_PREFABS}/{sp.name}.prefab";
            GameObject goProp = new GameObject(sp.name);
            var sr = goProp.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = -5;
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

        Material matDefault = AssetDatabase.LoadAssetAtPath<Material>("Assets/Cainos/Pixel Art Platformer - Village Props/Material/MT Village Props - Default.mat");
        if (matDefault == null) matDefault = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        // Configuraciones de edificios y elementos arquitectónicos de fondo
        // Z = 2.0 y sortingOrder = -5 para colocarse detrás del jugador y de las plataformas, pero delante del cielo/montañas
        var edificiosConfig = new[]
        {
            // === PISO 1 (Calle Superior - Inicio y Mercado) ===
            new { SpriteNombre = "Fachada_Amarilla_Completa", Pos = new Vector3(-68.5f, 8.8f, 2.0f), Escala = new Vector3(1.1f, 1.1f, 1f), Order = -5 },
            new { SpriteNombre = "Balcon_Cantera_Hierro", Pos = new Vector3(-68.5f, 12.2f, 1.8f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -4 },
            new { SpriteNombre = "Arco_Triple_Colonial", Pos = new Vector3(-58.0f, 7.8f, 2.0f), Escala = new Vector3(1.1f, 1.1f, 1f), Order = -5 },
            new { SpriteNombre = "Azulejos_Talavera_Guanajuato", Pos = new Vector3(-58.0f, 10.2f, 1.8f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -4 },
            new { SpriteNombre = "Fachada_Roja_Completa", Pos = new Vector3(-44.0f, 8.8f, 2.0f), Escala = new Vector3(1.1f, 1.1f, 1f), Order = -5 },
            new { SpriteNombre = "Mirador_Balcon_Madera", Pos = new Vector3(-44.0f, 12.2f, 1.8f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -4 },
            new { SpriteNombre = "Fachada_Azul_Puerta", Pos = new Vector3(-33.0f, 7.5f, 2.0f), Escala = new Vector3(1.1f, 1.1f, 1f), Order = -5 },
            new { SpriteNombre = "Barandal_Hierro_Largo", Pos = new Vector3(-33.0f, 10.0f, 1.8f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -4 },

            // === PISO 2 (Plaza Central y Registro Civil) ===
            new { SpriteNombre = "Arco_Piedra_MedioPunto", Pos = new Vector3(-40.0f, 3.2f, 2.0f), Escala = new Vector3(1.1f, 1.1f, 1f), Order = -5 },
            new { SpriteNombre = "Fachada_Roja_Completa", Pos = new Vector3(-30.5f, 3.8f, 2.0f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -5 },
            new { SpriteNombre = "Ventana_Cantera_Vidrio", Pos = new Vector3(-30.5f, 6.8f, 1.8f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -4 },
            new { SpriteNombre = "Fachada_Amarilla_Completa", Pos = new Vector3(-19.5f, 3.8f, 2.0f), Escala = new Vector3(1.05f, 1.05f, 1f), Order = -5 },
            new { SpriteNombre = "Mirador_Madera_Panel", Pos = new Vector3(-19.5f, 7.2f, 1.8f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -4 },

            // === PISO 3 (Zona de Archivos y Pasos Coloniales) ===
            new { SpriteNombre = "Arco_Cantera_Mixtilineo", Pos = new Vector3(-55.0f, -3.2f, 2.0f), Escala = new Vector3(1.1f, 1.1f, 1f), Order = -5 },
            new { SpriteNombre = "Puerta_Doble_Tallada", Pos = new Vector3(-45.0f, -3.2f, 2.0f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -5 },
            new { SpriteNombre = "Ventana_Lattice_Plomada", Pos = new Vector3(-45.0f, -1.0f, 1.8f), Escala = new Vector3(1.0f, 1.0f, 1f), Order = -4 }
        };

        foreach (var conf in edificiosConfig)
        {
            if (sprites.TryGetValue(conf.SpriteNombre, out Sprite sp))
            {
                GameObject elem = new GameObject(conf.SpriteNombre);
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
