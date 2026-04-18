using UnityEngine;

[DisallowMultipleComponent]
public class GroundChunkDefinition : MonoBehaviour
{
    private const string EntryAnchorName = "EntryAnchor";
    private const string ExitAnchorName = "ExitAnchor";
    private const string PlayerSpawnName = "PlayerSpawn";

    [Header("Identity")]
    [SerializeField] private GroundChunkRole role = GroundChunkRole.Normal;
    [SerializeField] private GroundSocketType entrySocketType = GroundSocketType.Flat;
    [SerializeField] private GroundSocketType exitSocketType = GroundSocketType.Flat;
    [SerializeField] [Min(1)] private int selectionWeight = 1;

    [Header("Authoring")]
    [SerializeField] [Min(1f)] private float authoringWidth = 32f;
    [SerializeField] [Min(0.5f)] private float seamWidth = 4f;

    [Header("Markers")]
    [SerializeField] private Transform entryAnchor;
    [SerializeField] private Transform exitAnchor;
    [SerializeField] private Transform playerSpawn;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGizmos = true;

    public GroundChunkRole Role => role;
    public GroundSocketType EntrySocketType => entrySocketType;
    public GroundSocketType ExitSocketType => exitSocketType;
    public int SelectionWeight => Mathf.Max(1, selectionWeight);
    public float AuthoringWidth => authoringWidth;
    public float SeamWidth => seamWidth;
    public Transform EntryAnchor => entryAnchor;
    public Transform ExitAnchor => exitAnchor;
    public Transform PlayerSpawn => playerSpawn;

    public bool SupportsEntrySocket(GroundSocketType requiredSocket)
    {
        return entrySocketType == requiredSocket;
    }

    public bool IsConfigured(out string reason)
    {
        if (entryAnchor == null)
        {
            reason = "Missing EntryAnchor.";
            return false;
        }

        if (exitAnchor == null)
        {
            reason = "Missing ExitAnchor.";
            return false;
        }

        if (role == GroundChunkRole.Start && playerSpawn == null)
        {
            reason = "Start chunk is missing PlayerSpawn.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private void OnValidate()
    {
        selectionWeight = Mathf.Max(1, selectionWeight);
        authoringWidth = Mathf.Max(1f, authoringWidth);
        seamWidth = Mathf.Clamp(seamWidth, 0.5f, authoringWidth * 0.5f);

        if (entryAnchor == null)
            entryAnchor = FindChildRecursive(transform, EntryAnchorName);

        if (exitAnchor == null)
            exitAnchor = FindChildRecursive(transform, ExitAnchorName);

        if (role == GroundChunkRole.Start && playerSpawn == null)
            playerSpawn = FindChildRecursive(transform, PlayerSpawnName);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
            return;

        if (entryAnchor != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(entryAnchor.position, 0.2f);
        }

        if (exitAnchor != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(exitAnchor.position, 0.2f);
        }

        if (entryAnchor != null && exitAnchor != null)
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.9f);
            Gizmos.DrawLine(entryAnchor.position, exitAnchor.position);
        }

        if (playerSpawn != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(playerSpawn.position, 0.2f);
        }
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (root.name == childName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChildRecursive(root.GetChild(i), childName);
            if (result != null)
                return result;
        }

        return null;
    }
}
