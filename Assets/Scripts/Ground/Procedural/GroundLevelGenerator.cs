using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent]
public class GroundLevelGenerator : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool clearExistingChunksOnGenerate = true;
    [SerializeField] private bool randomizeSeedOnPlay = true;
    [SerializeField] private bool avoidImmediateRepeats = true;
    [SerializeField] [Min(0)] private int normalChunkCount = 4;
    [SerializeField] [Min(1f)] private float expectedChunkWidth = 32f;
    [SerializeField] private int seed = 12345;
    [SerializeField] private bool logGeneration = true;

    [Header("Scene References")]
    [SerializeField] private Transform generatedChunkParent;
    [SerializeField] private Transform playerOverride;
    [SerializeField] private bool snapPlayerToStartChunk = true;
    [SerializeField] private bool movePlayerDuringEditorPreview = false;

    [Header("Chunk Pools")]
    [SerializeField] private GroundChunkDefinition[] startChunks;
    [SerializeField] private GroundChunkDefinition[] normalChunks;
    [SerializeField] private GroundChunkDefinition[] endChunks;

    [Header("Debug")]
    [SerializeField] private bool enableRegenerateHotkey = true;
    [SerializeField] private KeyCode regenerateKey = KeyCode.G;

    private readonly List<GroundChunkDefinition> spawnedChunks = new List<GroundChunkDefinition>(16);
    private System.Random random;
    private int lastRuntimeSeed;

    public int LastRuntimeSeed => lastRuntimeSeed;
    public IReadOnlyList<GroundChunkDefinition> SpawnedChunks => spawnedChunks;

    private void Start()
    {
        if (generateOnStart)
            GenerateLevel();
    }

    private void Update()
    {
        if (enableRegenerateHotkey && Input.GetKeyDown(regenerateKey))
            GenerateLevel();
    }

    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        if (!EnsureChunkParent())
            return;

        if (clearExistingChunksOnGenerate)
            ClearGeneratedChunks();

        lastRuntimeSeed = ResolveRuntimeSeed();
        random = new System.Random(lastRuntimeSeed);
        print($"last run time seed {lastRuntimeSeed}");

        Transform playerTransform = ResolvePlayerTransform();
        GroundChunkDefinition previousChunk = null;
        GroundSocketType requiredSocket = GroundSocketType.Flat;
        Vector3 nextEntryPosition = transform.position;

        GroundChunkDefinition startChunk = SpawnChunk(
            startChunks,
            GroundChunkRole.Start,
            requiredSocket,
            previousChunk,
            ref nextEntryPosition);

        if (startChunk == null)
            return;

        previousChunk = startChunk;
        requiredSocket = startChunk.ExitSocketType;

        if (snapPlayerToStartChunk)
            SnapPlayerToChunkStart(playerTransform, startChunk);

        for (int i = 0; i < normalChunkCount; i++)
        {
            GroundChunkDefinition normalChunk = SpawnChunk(
                normalChunks,
                GroundChunkRole.Normal,
                requiredSocket,
                previousChunk,
                ref nextEntryPosition);

            if (normalChunk == null)
                return;

            previousChunk = normalChunk;
            requiredSocket = normalChunk.ExitSocketType;
        }

        SpawnChunk(
            endChunks,
            GroundChunkRole.End,
            requiredSocket,
            previousChunk,
            ref nextEntryPosition);

        if (logGeneration)
            Debug.Log($"[GroundLevelGenerator] Generated {spawnedChunks.Count} chunks using seed {lastRuntimeSeed}.", this);

        MarkSceneDirty();
    }

    [ContextMenu("Clear Generated Chunks")]
    public void ClearGeneratedChunks()
    {
        if (generatedChunkParent == null)
            return;

        for (int i = generatedChunkParent.childCount - 1; i >= 0; i--)
        {
            Transform child = generatedChunkParent.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(child.gameObject);
#else
                DestroyImmediate(child.gameObject);
#endif
            }
        }

        spawnedChunks.Clear();
        MarkSceneDirty();
    }

    private GroundChunkDefinition SpawnChunk(
        GroundChunkDefinition[] sourcePool,
        GroundChunkRole expectedRole,
        GroundSocketType requiredSocket,
        GroundChunkDefinition previousChunk,
        ref Vector3 nextEntryPosition)
    {
        GroundChunkDefinition prefab = SelectChunkPrefab(sourcePool, expectedRole, requiredSocket, previousChunk);
        if (prefab == null)
            return null;

        GroundChunkDefinition instance = InstantiateChunkPrefab(prefab);
        Vector3 offset = nextEntryPosition - instance.EntryAnchor.position;
        instance.transform.position += offset;

        if (Mathf.Abs(instance.AuthoringWidth - expectedChunkWidth) > 0.01f)
        {
            Debug.LogWarning(
                $"[GroundLevelGenerator] Chunk '{instance.name}' has authoring width {instance.AuthoringWidth}, but the generator expects {expectedChunkWidth}.",
                instance);
        }

        nextEntryPosition = instance.ExitAnchor.position;
        spawnedChunks.Add(instance);
        return instance;
    }

    private GroundChunkDefinition SelectChunkPrefab(
        GroundChunkDefinition[] sourcePool,
        GroundChunkRole expectedRole,
        GroundSocketType requiredSocket,
        GroundChunkDefinition previousChunk)
    {
        if (sourcePool == null || sourcePool.Length == 0)
        {
            Debug.LogError($"[GroundLevelGenerator] No chunks assigned for role {expectedRole}.", this);
            return null;
        }

        List<GroundChunkDefinition> candidates = new List<GroundChunkDefinition>(sourcePool.Length);
        int totalWeight = 0;

        for (int i = 0; i < sourcePool.Length; i++)
        {
            GroundChunkDefinition chunk = sourcePool[i];
            if (chunk == null)
                continue;

            if (chunk.Role != expectedRole)
            {
                Debug.LogWarning($"[GroundLevelGenerator] Chunk '{chunk.name}' is in the {expectedRole} pool but is marked as {chunk.Role}.", chunk);
                continue;
            }

            if (!chunk.SupportsEntrySocket(requiredSocket))
                continue;

            if (!chunk.IsConfigured(out string reason))
            {
                Debug.LogWarning($"[GroundLevelGenerator] Chunk '{chunk.name}' is not configured: {reason}", chunk);
                continue;
            }

            if (avoidImmediateRepeats &&
                expectedRole == GroundChunkRole.Normal &&
                previousChunk != null &&
                string.Equals(GetChunkBaseName(chunk), GetChunkBaseName(previousChunk), StringComparison.Ordinal) &&
                sourcePool.Length > 1)
            {
                continue;
            }

            candidates.Add(chunk);
            totalWeight += chunk.SelectionWeight;
        }

        if (candidates.Count == 0)
        {
            Debug.LogError($"[GroundLevelGenerator] Could not find a valid {expectedRole} chunk for socket {requiredSocket}.", this);
            return null;
        }

        int roll = random.Next(0, totalWeight);
        for (int i = 0; i < candidates.Count; i++)
        {
            GroundChunkDefinition candidate = candidates[i];
            roll -= candidate.SelectionWeight;
            if (roll < 0)
                return candidate;
        }

        return candidates[candidates.Count - 1];
    }

    //make sure we have a parent for the chunk
    private bool EnsureChunkParent()
    {
        if (generatedChunkParent != null)
            return true;

        Transform existing = transform.Find("GeneratedChunks");
        if (existing != null)
        {
            generatedChunkParent = existing;
            return true;
        }

        GameObject root = new GameObject("GeneratedChunks");
        root.transform.SetParent(transform, false);
        generatedChunkParent = root.transform;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            Undo.RegisterCreatedObjectUndo(root, "Create Generated Chunk Root");
#endif

        return true;
    }

    private int ResolveRuntimeSeed()
    {
        if (SaveManager.instance.CurrentLevelSaveData.planetLevelData.PCGSeed != 0)
        {
            print("giving previous seed");
            return SaveManager.instance.CurrentLevelSaveData.planetLevelData.PCGSeed;
        }
        return randomizeSeedOnPlay && Application.isPlaying ? Environment.TickCount : seed;
    }

    private Transform ResolvePlayerTransform()
    {
        if (playerOverride != null)
            return playerOverride;

        Player player = FindFirstObjectByType<Player>();
        return player != null ? player.transform : null;
    }

    private void SnapPlayerToChunkStart(Transform playerTransform, GroundChunkDefinition startChunk)
    {
        if (playerTransform == null || startChunk == null)
            return;

        if (!Application.isPlaying && !movePlayerDuringEditorPreview)
            return;

        Transform spawn = startChunk.PlayerSpawn != null ? startChunk.PlayerSpawn : startChunk.EntryAnchor;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            Undo.RecordObject(playerTransform, "Move Player To Chunk Start");
#endif

        playerTransform.position = spawn.position;

        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        MarkSceneDirty();
    }

    private static string GetChunkBaseName(GroundChunkDefinition chunk)
    {
        if (chunk == null)
            return string.Empty;

        return chunk.name.Replace("(Clone)", string.Empty).Trim();
    }

    public int GetSeed()
    {
        return lastRuntimeSeed;
    }

    private GroundChunkDefinition InstantiateChunkPrefab(GroundChunkDefinition prefab)
    {
        if (Application.isPlaying)
            return Instantiate(prefab, generatedChunkParent);

#if UNITY_EDITOR
        GameObject instanceObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, generatedChunkParent);
        Undo.RegisterCreatedObjectUndo(instanceObject, "Generate Ground Chunk Preview");
        return instanceObject.GetComponent<GroundChunkDefinition>();
#else
        return Instantiate(prefab, generatedChunkParent);
#endif
    }

    private void MarkSceneDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }
}
