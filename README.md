# Hasta Encontrarles: El Camino de la Esperanza
### Nivel 2: "Las Puertas Cerradas de la Burocracia"

> **Serious Game 2D** basado en la memoria y lucha de las madres y familias buscadoras de México, inspirado en el caso real de los **22 migrantes desaparecidos en marzo de 2011** originarios de San Luis de la Paz, Guanajuato.
> 
> **Desarrollado por:** Carmen Vargas y Valeria Martínez.  
> **Motor:** Unity 6 (URP 2D)  
> **Repositorio:** `https://github.com/mariadelcarmenvargasmartinez4-hash/madresbuscadoras`

---

## 📖 Sinopsis del Nivel 2

Tras salir de la comunidad en búsqueda de sus seres queridos (Nivel 1), **Esperanza**, madre buscadora, se enfrenta a la primera gran muralla impuesta a las víctimas: **la indiferencia burocrática y el archivo sistemático de los expedientes oficiales**.

En este nivel de plataformas y exploración 2D, Esperanza debe recorrer oficinas gubernamentales, plazas públicas y túneles subterráneos para rescatar **10 fragmentos de expediente documental** que las autoridades han intentado ocultar, descifrando acertijos deductivos en su camino hacia la verdad (preparando la búsqueda en campo del Nivel 3).

---

## 🎮 Mecánicas y Características Implementadas

1. **Control de la Protagonista:**
   - Movimiento horizontal responsivo y salto ágil (`fuerzaSalto = 11.8f`).
   - Compatibilidad nativa tanto con el *Nuevo Input System* de Unity como con el sistema clásico.
   - Poses visuales animadas: reposo (*Idle*), caminata (*Walk*) y salto (*Jump*).
   - Sistema de impacto y parpadeo de daño temporal al chocar con peligros.

2. **Búsqueda de 10 Hojas de Expediente Documental:**
   - 10 fragmentos de expedientes oficiales distribuidos estratégicamente a lo largo de 5 alturas del nivel.
   - Brillo áureo y partículas de proximidad cuando la protagonista se encuentra a menos de 3.5 metros.
   - Sistema narrativo con títulos y descripciones de documentos forenses e institucionales.

3. **Sistema de Deducción y Acertijos en HUD:**
   - La interfaz guía al jugador mediante acertijos deductivos contextuales que cambian progresivamente tras encontrar cada hoja (sin flechas invasivas).
   - Contador dinámico en pantalla: `📋 Hojas: X de 10`.

4. **Barra de Ánimo / Salud y Peligros Equilibrados:**
   - Barra de vida visual con cálculo dinámico de daño.
   - 12 trampas de púas estáticas colocadas en pasillos de riesgo, respetando distancias seguras con respecto a las hojas.
   - 6 bolas de picos patrullando de forma móvil (horizontal y vertical).

5. **Nivel Vertical Interconectado:**
   - Plataformas escalonadas con física unidireccional (*PlatformEffector2D* tipo *one-way*), permitiendo atravesarlas desde abajo y pararse sobre ellas.
   - Apertura de túnel y entrada subterránea que conecta la planta de archivos con la fosa y bóveda final profunda.
   - Puentes de madera conectando los abismos subterráneos para garantizar que las 10 hojas sean 100% accesibles.

6. **Sistema de Seguridad y Reaparición en la Primera Plataforma:**
   - Cada vez que el personaje cae al vacío fuera del escenario (`Y < -23f`), reaparece automáticamente en la **primera plataforma de inicio** (`X: -74, Y: 6.2`), deteniendo su inercia y descontando salud.

---

## ⌨️ Controles

| Acción | Teclado Principal | Alternativa |
| :--- | :--- | :--- |
| **Moverse a la Izquierda** | `Flecha Izquierda` | Tecla `A` |
| **Moverse a la Derecha** | `Flecha Derecha` | Tecla `D` |
| **Saltar** | Tecla `V` | Barra Espaciadora (`Espacio`) |
| **Pausar / Reanudar** | Tecla `Escape` | Tecla `P` |

---

## 📋 Hojas de Expediente del Nivel 2

| # | Documento | Ubicación |
| :-: | :--- | :--- |
| **1** | Ficha de Búsqueda 2011 | Puesto del mercado superior |
| **2** | Talón del Boleto Roto | Parada solitaria de autobuses |
| **3** | Testimonio de la Dependienta | Cornisa del viejo pozo |
| **4** | Acta y Lista de Pasajeros | Rincón de la oficina del Registro |
| **5** | Bitácora de la Ruta Norte | Maderos del puente del descenso |
| **6** | Oficio Confidencial No. 22 | Pasadizo junto a la estatua de piedra |
| **7** | Carpeta de Denuncia Rechazada | Estante final de los archiveros |
| **8** | Informe Preliminar de San Fernando | Andamios bajo la entrada subterránea |
| **9** | Ficha de Cotejo Forense del EAAF | Colina de roca entre las cadenas |
| **10** | **Expediente Completo Oficial (100%)** | **Bóveda final subterránea** |

---

## 🛠️ Próximas Mejoras (Backlog / Tareas Pendientes)

- [ ] **Música y Diseño Sonoro:** Composición de música ambiental de tensión y melancolía, y efectos de pasos y recolección.
- [ ] **Fondo Visual Temático:** Ajuste del fondo con Parallax representando el paisaje de San Luis de la Paz y edificios institucionales.
- [ ] **Ítem de Recuperación (Botella de Agua):** Creación del consumible "Botella de Agua" para que Esperanza recupere vida (+25) durante su trayecto.
