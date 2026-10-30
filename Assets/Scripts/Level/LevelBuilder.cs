using UnityEngine;

namespace MarioBros.Level
{
    /// <summary>
    /// Genera el nivel 1-1 de Mario usando prefabs posicionados por código.
    /// Adjuntar a un GameObject vacío "LevelBuilder" en la escena.
    /// Asignar los prefabs desde el Inspector.
    /// </summary>
    public class LevelBuilder : MonoBehaviour
{
    [Header("Prefabs de Terreno")]
    [SerializeField] private GameObject groundBlockPrefab;
    [SerializeField] private GameObject brickBlockPrefab;
    [SerializeField] private GameObject questionBlockPrefab;
    [SerializeField] private GameObject hardBlockPrefab; // Bloque irrompible (escaleras)
    [SerializeField] private GameObject pipePrefab;      // Tubería (2x de ancho, variable alto)

    [Header("Prefabs de Elementos")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject goombaPrefab;
    [SerializeField] private GameObject koopaPrefab;
    [SerializeField] private GameObject flagPolePrefab;
    [SerializeField] private GameObject castlePrefab;

    [Header("Prefabs de Decoración")]
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private GameObject bushPrefab;
    [SerializeField] private GameObject hillPrefab;

    [Header("Configuración")]
    [SerializeField] private Transform levelParent;
    [SerializeField] private bool buildOnStart = true;

    private Transform groundParent;
    private Transform blocksParent;
    private Transform enemiesParent;
    private Transform decoParent;
    private Transform itemsParent;

    void Start()
    {
        if (buildOnStart)
        {
            BuildLevel1_1();
        }
    }

    /// <summary>
    /// Construye el nivel 1-1 completo
    /// </summary>
    public void BuildLevel1_1()
    {
        // Crear padres organizadores
        CreateParents();

        // Construir suelo (floor) - con huecos
        BuildGround();

        // Bloques ? y ladrillos
        BuildBlocks();

        // Tuberías
        BuildPipes();

        // Escaleras finales
        BuildStairs();

        // Enemigos
        PlaceEnemies();

        // Flag pole y castillo
        PlaceGoal();

        // Kill Zone
        CreateKillZone();

        Debug.Log("Nivel 1-1 construido exitosamente!");
    }

    private void CreateParents()
    {
        Transform parent = levelParent != null ? levelParent : transform;

        groundParent = new GameObject("--- Ground ---").transform;
        groundParent.SetParent(parent);

        blocksParent = new GameObject("--- Blocks ---").transform;
        blocksParent.SetParent(parent);

        enemiesParent = new GameObject("--- Enemies ---").transform;
        enemiesParent.SetParent(parent);

        decoParent = new GameObject("--- Decoration ---").transform;
        decoParent.SetParent(parent);

        itemsParent = new GameObject("--- Items ---").transform;
        itemsParent.SetParent(parent);
    }

    #region Ground Building

    private void BuildGround()
    {
        if (groundBlockPrefab == null) return;

        // Nivel 1-1: Suelo es 2 bloques de alto (Y=0 y Y=1, o Y=-1 y Y=0)
        // Usaremos Y=0 y Y=-1 para las 2 filas de suelo

        // Secciones de suelo (inicio, fin) - con gaps (huecos)
        int[][] groundSections = new int[][]
        {
            new int[] { 0, 69 },     // Sección 1: inicio hasta primer hueco
            new int[] { 71, 86 },    // Sección 2: después del primer hueco
            new int[] { 89, 152 },   // Sección 3: sección larga
            new int[] { 155, 212 }   // Sección 4: hasta el final
        };

        foreach (var section in groundSections)
        {
            for (int x = section[0]; x <= section[1]; x++)
            {
                // Fila superior del suelo (Y = 0)
                PlaceBlock(groundBlockPrefab, x, 0, groundParent);
                // Fila inferior del suelo (Y = -1)
                PlaceBlock(groundBlockPrefab, x, -1, groundParent);
            }
        }
    }

    #endregion

    #region Blocks Building

    private void BuildBlocks()
    {
        if (questionBlockPrefab == null && brickBlockPrefab == null) return;

        // === Fila de bloques baja (Y = 4, a 4 bloques sobre el suelo) ===
        // Bloque ? suelto (moneda)
        PlaceBlock(questionBlockPrefab, 16, 4, blocksParent);

        // Grupo: ladrillo - ? - ladrillo - ? - ladrillo - ? - ladrillo
        PlaceBlock(brickBlockPrefab, 20, 4, blocksParent);
        PlaceBlock(questionBlockPrefab, 21, 4, blocksParent);  // Mushroom/grow
        PlaceBlock(brickBlockPrefab, 22, 4, blocksParent);
        PlaceBlock(questionBlockPrefab, 23, 4, blocksParent);  // Coin
        PlaceBlock(brickBlockPrefab, 24, 4, blocksParent);

        // Bloque ? alto (Y = 8) - bloque oculto arriba
        PlaceBlock(questionBlockPrefab, 22, 8, blocksParent);

        // Bloques después de la primera tubería
        PlaceBlock(brickBlockPrefab, 77, 4, blocksParent);
        PlaceBlock(questionBlockPrefab, 78, 4, blocksParent);  // Mushroom
        PlaceBlock(brickBlockPrefab, 79, 4, blocksParent);

        // Fila alta de ladrillos
        PlaceBlock(brickBlockPrefab, 80, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 81, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 82, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 83, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 84, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 85, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 86, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 87, 8, blocksParent);

        // Fila alta separada (después del gap)
        PlaceBlock(brickBlockPrefab, 91, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 92, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 93, 8, blocksParent);

        // Bloques ? sobre plataforma baja
        PlaceBlock(questionBlockPrefab, 94, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 94, 4, blocksParent);

        // Más bloques zona media
        PlaceBlock(brickBlockPrefab, 100, 4, blocksParent);
        PlaceBlock(brickBlockPrefab, 101, 4, blocksParent);
        PlaceBlock(questionBlockPrefab, 106, 4, blocksParent);
        PlaceBlock(questionBlockPrefab, 109, 4, blocksParent);
        PlaceBlock(questionBlockPrefab, 109, 8, blocksParent);
        PlaceBlock(questionBlockPrefab, 112, 4, blocksParent);

        // Ladrillos zona final
        PlaceBlock(brickBlockPrefab, 118, 4, blocksParent);
        PlaceBlock(brickBlockPrefab, 121, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 122, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 123, 8, blocksParent);

        PlaceBlock(brickBlockPrefab, 128, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 129, 8, blocksParent);
        PlaceBlock(brickBlockPrefab, 130, 8, blocksParent);
        PlaceBlock(questionBlockPrefab, 129, 8, blocksParent);

        PlaceBlock(brickBlockPrefab, 129, 4, blocksParent);
        PlaceBlock(brickBlockPrefab, 130, 4, blocksParent);

        PlaceBlock(questionBlockPrefab, 170, 4, blocksParent);
        PlaceBlock(brickBlockPrefab, 171, 4, blocksParent);
        PlaceBlock(brickBlockPrefab, 172, 4, blocksParent);
    }

    #endregion

    #region Pipes Building

    private void BuildPipes()
    {
        if (pipePrefab == null) return;

        // Tuberías del nivel 1-1 (x, alturaBloques)
        // Tubería = 2 bloques de ancho, variable de alto
        PlacePipe(28, 2);  // Tubería pequeña
        PlacePipe(38, 3);  // Tubería mediana
        PlacePipe(46, 4);  // Tubería grande
        PlacePipe(57, 4);  // Tubería grande
        PlacePipe(163, 2); // Tubería pequeña al final
        PlacePipe(179, 2); // Tubería zona final
    }

    private void PlacePipe(int x, int height)
    {
        if (pipePrefab == null) return;

        // El prefab ya mide dos unidades: la escala es relativa a ese tamaño.
        // El suelo superior está centrado en Y=0, por lo que su superficie es Y=0.5.
        GameObject pipe = Instantiate(pipePrefab, new Vector3(x + 0.5f, 0.5f + height * 0.5f, 0), Quaternion.identity, groundParent);
        pipe.name = $"Pipe_{x}";
        pipe.transform.localScale = new Vector3(1, height / 2f, 1);
    }

    #endregion

    #region Stairs Building

    private void BuildStairs()
    {
        if (hardBlockPrefab == null) return;

        // Escalera 1 (subida) - antes del hueco final
        BuildStaircase(134, 1, 4, true);

        // Escalera 2 (bajada y subida)
        BuildStaircase(140, 1, 4, false);
        BuildStaircase(148, 1, 4, true);

        // Escalera 3 (subida)
        BuildStaircase(155, 1, 4, true);

        // Escalera final grande antes de la bandera
        BuildStaircase(181, 1, 8, true);
    }

    /// <summary>
    /// Construye una escalera de bloques
    /// </summary>
    /// <param name="startX">Posición X inicial</param>
    /// <param name="startY">Posición Y base (sobre el suelo)</param>
    /// <param name="maxHeight">Altura máxima en bloques</param>
    /// <param name="ascending">True = sube a la derecha, False = baja a la derecha</param>
    private void BuildStaircase(int startX, int startY, int maxHeight, bool ascending)
    {
        for (int step = 0; step < maxHeight; step++)
        {
            int x = ascending ? startX + step : startX + (maxHeight - 1 - step);
            for (int y = startY; y <= startY + step; y++)
            {
                PlaceBlock(hardBlockPrefab, x, y, groundParent);
            }
        }
    }

    #endregion

    #region Enemies Placement

    private void PlaceEnemies()
    {
        if (goombaPrefab == null) return;

        // Goombas del nivel 1-1 (posiciones aproximadas)
        int[] goombaPositions = new int[]
        {
            22, 40, 51, 52, 80, 82, 97, 98,
            114, 115, 124, 125, 128, 129, 174, 175
        };

        foreach (int x in goombaPositions)
        {
            PlaceBlock(goombaPrefab, x, 1, enemiesParent);
        }

        // Koopa Troopa
        if (koopaPrefab != null)
        {
            PlaceBlock(koopaPrefab, 107, 1, enemiesParent);
        }
    }

    #endregion

    #region Goal

    private void PlaceGoal()
    {
        // Flag Pole
        if (flagPolePrefab != null)
        {
            Instantiate(flagPolePrefab, new Vector3(198.5f, .5f, 0), Quaternion.identity, itemsParent);
        }

        // Castle
        if (castlePrefab != null)
        {
            Instantiate(castlePrefab, new Vector3(204.5f, .5f, 0), Quaternion.identity, decoParent);
        }
    }

    #endregion

    #region Kill Zone

    private void CreateKillZone()
    {
        GameObject killZone = new GameObject("KillZone");
        killZone.transform.SetParent(transform);
        killZone.transform.position = new Vector3(100, -5, 0);

        BoxCollider2D col = killZone.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(250, 2);

        killZone.AddComponent<KillZone>();
        killZone.layer = LayerMask.NameToLayer("Default");
    }

    #endregion

    #region Helpers

    private void PlaceBlock(GameObject prefab, int x, int y, Transform parent)
    {
        if (prefab == null) return;
        GameObject block = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity, parent);
        block.name = $"{prefab.name}_{x}_{y}";
    }

    #endregion
}
}
