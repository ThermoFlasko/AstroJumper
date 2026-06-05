using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class SpaceshipLaser : MonoBehaviour
{
    private const int DefaultProjectilePoolCapacity = 64;
    private const int MaxProjectilePoolSize = 512;
    private const int DefaultVfxPoolCapacity = 32;
    private const int MaxVfxPoolSize = 256;
    private const int MaxActiveVfxPerPrefab = 96;

    private static readonly Dictionary<GameObject, ObjectPool<GameObject>> projectilePools =
        new Dictionary<GameObject, ObjectPool<GameObject>>();
    private static readonly Dictionary<GameObject, bool> projectilePrefabHasLaser =
        new Dictionary<GameObject, bool>();
    private static readonly Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> vfxPools =
        new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();
    private static readonly Dictionary<ParticleSystem, float> vfxLifetimeCache =
        new Dictionary<ParticleSystem, float>();
    private static readonly Dictionary<GameObject, int> prewarmedProjectileCounts =
        new Dictionary<GameObject, int>();
    private static readonly Dictionary<ParticleSystem, int> prewarmedVfxCounts =
        new Dictionary<ParticleSystem, int>();
    private static readonly Dictionary<ParticleSystem, int> activeVfxCounts =
        new Dictionary<ParticleSystem, int>();
    private static readonly List<GameObject> projectilePrewarmBuffer = new List<GameObject>(MaxProjectilePoolSize);
    private static readonly List<ParticleSystem> vfxPrewarmBuffer = new List<ParticleSystem>(MaxVfxPoolSize);

    private static Transform projectilePoolRoot;
    private static Transform vfxPoolRoot;

    private Rigidbody2D rigidBody;
    private PooledSpaceProjectile owner;

    [Header("Movement")]
    [SerializeField] private float speed = 100f;
    [SerializeField] private float maxSpeed = 100f;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] public int teamId = -1; // must be set by the spawner

    [Header("VFX")]
    [Tooltip("Particle prefab spawned when we successfully damage something.")]
    [SerializeField] private ParticleSystem hitVfxPrefab;

    [Tooltip("Extra seconds added to the particle's duration before destroying it.")]
    [SerializeField] private float vfxDestroyPadding = 0.25f;

    //prevent double triggers
    private bool hasHitSomething = false;
    private bool launched;
    private float despawnTime;

    public float LifeTime => Mathf.Max(0.01f, lifeTime);

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, int teamId = -1)
    {
        if (prefab == null)
            return null;

        if (!PrefabHasLaser(prefab))
            return Instantiate(prefab, position, rotation);

        EnsurePoolRoots();
        ObjectPool<GameObject> pool = GetProjectilePool(prefab);
        GameObject projectile = pool.Get();
        projectile.transform.SetPositionAndRotation(position, rotation);

        PooledSpaceProjectile pooledProjectile = projectile.GetComponent<PooledSpaceProjectile>();
        if (pooledProjectile == null)
            pooledProjectile = projectile.AddComponent<PooledSpaceProjectile>();

        pooledProjectile.Launch(pool, teamId);
        return projectile;
    }

    public static void PrewarmProjectile(GameObject prefab, int projectileCount, int hitVfxCount)
    {
        if (prefab == null || projectileCount <= 0 || !PrefabHasLaser(prefab))
            return;

        EnsurePoolRoots();

        int clampedProjectileCount = Mathf.Clamp(projectileCount, 0, MaxProjectilePoolSize);
        int previousProjectileCount = prewarmedProjectileCounts.TryGetValue(prefab, out int previous)
            ? previous
            : 0;
        int extraProjectileCount = clampedProjectileCount - previousProjectileCount;

        if (extraProjectileCount > 0)
        {
            ObjectPool<GameObject> pool = GetProjectilePool(prefab);
            projectilePrewarmBuffer.Clear();

            for (int i = 0; i < extraProjectileCount; i++)
                projectilePrewarmBuffer.Add(pool.Get());

            for (int i = 0; i < projectilePrewarmBuffer.Count; i++)
                pool.Release(projectilePrewarmBuffer[i]);

            projectilePrewarmBuffer.Clear();
            prewarmedProjectileCounts[prefab] = clampedProjectileCount;
        }

        if (hitVfxCount <= 0)
            return;

        SpaceshipLaser[] lasers = prefab.GetComponentsInChildren<SpaceshipLaser>(true);
        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] != null && lasers[i].hitVfxPrefab != null)
                PrewarmVfx(lasers[i].hitVfxPrefab, hitVfxCount);
        }
    }

    public static void PrewarmVfx(ParticleSystem prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        EnsurePoolRoots();

        int clampedCount = Mathf.Clamp(count, 0, MaxVfxPoolSize);
        int previousCount = prewarmedVfxCounts.TryGetValue(prefab, out int previous)
            ? previous
            : 0;
        int extraCount = clampedCount - previousCount;
        if (extraCount <= 0)
            return;

        ObjectPool<ParticleSystem> pool = GetVfxPool(prefab);
        vfxPrewarmBuffer.Clear();

        for (int i = 0; i < extraCount; i++)
            vfxPrewarmBuffer.Add(pool.Get());

        for (int i = 0; i < vfxPrewarmBuffer.Count; i++)
            pool.Release(vfxPrewarmBuffer[i]);

        vfxPrewarmBuffer.Clear();
        prewarmedVfxCounts[prefab] = clampedCount;
    }

    public static ParticleSystem SpawnPooledVfx(
        ParticleSystem prefab,
        Vector3 position,
        Quaternion rotation,
        float destroyPadding = 0f)
    {
        if (prefab == null)
            return null;

        EnsurePoolRoots();
        int activeCount = activeVfxCounts.TryGetValue(prefab, out int active) ? active : 0;
        if (activeCount >= MaxActiveVfxPerPrefab)
            return null;

        ObjectPool<ParticleSystem> pool = GetVfxPool(prefab);
        ParticleSystem instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);

        PooledParticleEffect pooledEffect = instance.GetComponent<PooledParticleEffect>();
        if (pooledEffect == null)
            pooledEffect = instance.gameObject.AddComponent<PooledParticleEffect>();

        float lifetime = GetCachedParticleSystemTotalLifetime(prefab) + Mathf.Max(0f, destroyPadding);
        pooledEffect.Play(pool, prefab, instance, lifetime);
        return instance;
    }

    private void Awake()
    {
        CacheRigidbody();
    }

    private void Start()
    {
        // Fallback for any legacy code/prefabs that still instantiate a laser directly.
        if (!launched)
            Launch(null, teamId);
    }

    internal void Launch(PooledSpaceProjectile projectileOwner, int newTeamId)
    {
        CacheRigidbody();

        owner = projectileOwner;
        teamId = newTeamId;
        hasHitSomething = false;
        launched = true;
        despawnTime = Time.time + LifeTime;

        if (rigidBody == null)
            return;

        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.WakeUp();
        rigidBody.linearVelocity = (Vector2)transform.up * ResolveLaunchSpeed();
    }

    internal void DeactivateForPool()
    {
        launched = false;
        hasHitSomething = false;
        owner = null;

        if (rigidBody == null)
            CacheRigidbody();

        if (rigidBody == null)
            return;

        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitSomething) return;

        // dont desotry if on same team
        if (teamId != -1)
        {
            var otherTeamAgent = other.GetComponentInParent<TeamAgent>();
            if (otherTeamAgent != null && otherTeamAgent.TeamId == teamId)
            {
                return; // friendly -> ignore
            }
        }

        //find smth damageable
        var damageable = other.GetComponentInParent<ISpaceDamagable>();
        if (damageable == null)
        {
            // If you WANT lasers to die on walls/asteroids/etc that are not damageable,
            // you can destroy here. Otherwise, just ignore.
            // if (destroyOnHit) Destroy(gameObject);
            return;
        }

        // not on our team and can be damaged -> hit it!
        damageable.TakeDamage(damage);

        // spawn vfx
        SpawnHitVfx();

        // destroy self if we are supposed to
        if (destroyOnHit)
        {
            hasHitSomething = true;
            FinishLaser();
        }
    }

    private void Update()
    {
        if (launched && Time.time >= despawnTime)
            FinishLaser();
    }

    private void CacheRigidbody()
    {
        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody2D>();
    }

    private float ResolveLaunchSpeed()
    {
        if (maxSpeed > 0f)
            return maxSpeed;

        return Mathf.Max(0f, speed);
    }

    private void FinishLaser()
    {
        if (!launched && owner == null)
            return;

        launched = false;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
        }

        if (owner != null)
        {
            PooledSpaceProjectile currentOwner = owner;
            owner = null;
            currentOwner.NotifyLaserFinished(this);
            return;
        }

        Destroy(gameObject);
    }

    private void SpawnHitVfx()
    {
        if (hitVfxPrefab == null) return;

        //Spawn at laser position, not rotated (since most hit effects are just explosion bursts that look fine without rotation)
        SpawnPooledVfx(hitVfxPrefab, transform.position, Quaternion.identity, vfxDestroyPadding);

        // If you want it to align with the bullet direction:
        // SpawnPooledVfx(hitVfxPrefab, transform.position, transform.rotation, vfxDestroyPadding);
    }

    private static bool PrefabHasLaser(GameObject prefab)
    {
        if (projectilePrefabHasLaser.TryGetValue(prefab, out bool hasLaser))
            return hasLaser;

        hasLaser = prefab.GetComponentInChildren<SpaceshipLaser>(true) != null;
        projectilePrefabHasLaser[prefab] = hasLaser;
        return hasLaser;
    }

    private static ObjectPool<GameObject> GetProjectilePool(GameObject prefab)
    {
        if (projectilePools.TryGetValue(prefab, out ObjectPool<GameObject> existing))
            return existing;

        ObjectPool<GameObject> pool = null;
        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject instance = Instantiate(prefab, projectilePoolRoot);
                PooledSpaceProjectile pooledProjectile = instance.GetComponent<PooledSpaceProjectile>();
                if (pooledProjectile == null)
                    pooledProjectile = instance.AddComponent<PooledSpaceProjectile>();

                pooledProjectile.CacheLaserStates();
                instance.SetActive(false);
                return instance;
            },
            actionOnGet: instance =>
            {
                if (instance != null)
                    instance.SetActive(true);
            },
            actionOnRelease: instance =>
            {
                if (instance == null)
                    return;

                PooledSpaceProjectile pooledProjectile = instance.GetComponent<PooledSpaceProjectile>();
                if (pooledProjectile != null)
                    pooledProjectile.ResetForPool();

                instance.transform.SetParent(projectilePoolRoot, true);
                instance.SetActive(false);
            },
            actionOnDestroy: instance =>
            {
                if (instance != null)
                    Destroy(instance);
            },
            collectionCheck: false,
            defaultCapacity: DefaultProjectilePoolCapacity,
            maxSize: MaxProjectilePoolSize);

        projectilePools[prefab] = pool;
        return pool;
    }

    private static ObjectPool<ParticleSystem> GetVfxPool(ParticleSystem prefab)
    {
        if (vfxPools.TryGetValue(prefab, out ObjectPool<ParticleSystem> existing))
            return existing;

        ObjectPool<ParticleSystem> pool = null;
        pool = new ObjectPool<ParticleSystem>(
            createFunc: () =>
            {
                ParticleSystem instance = Instantiate(prefab, vfxPoolRoot);
                PooledParticleEffect pooledEffect = instance.GetComponent<PooledParticleEffect>();
                if (pooledEffect == null)
                    pooledEffect = instance.gameObject.AddComponent<PooledParticleEffect>();

                pooledEffect.CacheParticles(instance);
                instance.gameObject.SetActive(false);
                return instance;
            },
            actionOnGet: instance =>
            {
                if (instance != null)
                    instance.gameObject.SetActive(true);
            },
            actionOnRelease: instance =>
            {
                if (instance == null)
                    return;

                PooledParticleEffect pooledEffect = instance.GetComponent<PooledParticleEffect>();
                if (pooledEffect != null)
                    pooledEffect.ResetForPool();

                instance.transform.SetParent(vfxPoolRoot, true);
                instance.gameObject.SetActive(false);
            },
            actionOnDestroy: instance =>
            {
                if (instance != null)
                    Destroy(instance.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: DefaultVfxPoolCapacity,
            maxSize: MaxVfxPoolSize);

        vfxPools[prefab] = pool;
        return pool;
    }

    private static void EnsurePoolRoots()
    {
        if (projectilePoolRoot == null)
        {
            projectilePools.Clear();
            prewarmedProjectileCounts.Clear();
            GameObject root = new GameObject("SpaceProjectilePool");
            projectilePoolRoot = root.transform;
        }

        if (vfxPoolRoot == null)
        {
            vfxPools.Clear();
            prewarmedVfxCounts.Clear();
            activeVfxCounts.Clear();
            GameObject root = new GameObject("SpaceVfxPool");
            vfxPoolRoot = root.transform;
        }
    }

    internal static void NotifyPooledVfxStarted(ParticleSystem prefab)
    {
        if (prefab == null)
            return;

        int activeCount = activeVfxCounts.TryGetValue(prefab, out int active) ? active : 0;
        activeVfxCounts[prefab] = activeCount + 1;
    }

    internal static void NotifyPooledVfxReleased(ParticleSystem prefab)
    {
        if (prefab == null)
            return;

        int activeCount = activeVfxCounts.TryGetValue(prefab, out int active) ? active : 0;
        if (activeCount <= 1)
        {
            activeVfxCounts.Remove(prefab);
            return;
        }

        activeVfxCounts[prefab] = activeCount - 1;
    }

    private static float GetCachedParticleSystemTotalLifetime(ParticleSystem prefab)
    {
        if (vfxLifetimeCache.TryGetValue(prefab, out float lifetime))
            return lifetime;

        lifetime = GetParticleSystemTotalLifetime(prefab);
        vfxLifetimeCache[prefab] = lifetime;
        return lifetime;
    }

    private static float GetParticleSystemTotalLifetime(ParticleSystem ps)
    {
        // Total time = duration + startLifetime (max) + max child lifetimes
        var main = ps.main;

        float duration = main.duration;

        // startLifetime can be constant or range
        float startLifetimeMax = main.startLifetime.mode switch
        {
            ParticleSystemCurveMode.TwoConstants => main.startLifetime.constantMax,
            ParticleSystemCurveMode.Constant => main.startLifetime.constant,
            _ => main.startLifetime.constantMax // decent fallback
        };

        // Include child particle systems too (common for hit effects)
        float childMax = 0f;
        var children = ps.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == ps) continue;
            var cm = children[i].main;

            float d = cm.duration;
            float sl = cm.startLifetime.mode switch
            {
                ParticleSystemCurveMode.TwoConstants => cm.startLifetime.constantMax,
                ParticleSystemCurveMode.Constant => cm.startLifetime.constant,
                _ => cm.startLifetime.constantMax
            };

            childMax = Mathf.Max(childMax, d + sl);
        }

        return Mathf.Max(duration + startLifetimeMax, childMax);
    }
}

public class PooledSpaceProjectile : MonoBehaviour
{
    private struct LaserState
    {
        public SpaceshipLaser Laser;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
    }

    private IObjectPool<GameObject> pool;
    private LaserState[] laserStates;
    private int activeLaserCount;
    private bool releaseRequested;
    private float forceReleaseTime;

    internal void CacheLaserStates()
    {
        if (laserStates != null && laserStates.Length > 0)
            return;

        SpaceshipLaser[] lasers = GetComponentsInChildren<SpaceshipLaser>(true);
        laserStates = new LaserState[lasers.Length];

        for (int i = 0; i < lasers.Length; i++)
        {
            Transform laserTransform = lasers[i].transform;
            laserStates[i] = new LaserState
            {
                Laser = lasers[i],
                LocalPosition = laserTransform.localPosition,
                LocalRotation = laserTransform.localRotation,
                LocalScale = laserTransform.localScale
            };
        }
    }

    internal void Launch(IObjectPool<GameObject> ownerPool, int teamId)
    {
        pool = ownerPool;
        releaseRequested = false;
        activeLaserCount = 0;

        CacheLaserStates();

        float maxLife = 0.1f;
        for (int i = 0; i < laserStates.Length; i++)
        {
            SpaceshipLaser laser = laserStates[i].Laser;
            if (laser == null)
                continue;

            Transform laserTransform = laser.transform;
            if (laserTransform != transform)
            {
                laserTransform.localPosition = laserStates[i].LocalPosition;
                laserTransform.localRotation = laserStates[i].LocalRotation;
                laserTransform.localScale = laserStates[i].LocalScale;
            }

            laser.gameObject.SetActive(true);
            laser.Launch(this, teamId);
            activeLaserCount++;
            maxLife = Mathf.Max(maxLife, laser.LifeTime);
        }

        forceReleaseTime = Time.time + maxLife + 0.1f;
    }

    internal void NotifyLaserFinished(SpaceshipLaser laser)
    {
        if (releaseRequested)
            return;

        if (laser != null && laser.gameObject != gameObject)
            laser.gameObject.SetActive(false);

        activeLaserCount = Mathf.Max(0, activeLaserCount - 1);
        if (activeLaserCount <= 0)
            ReleaseToPool();
    }

    internal void ResetForPool()
    {
        releaseRequested = false;
        activeLaserCount = 0;

        if (laserStates == null)
            return;

        for (int i = 0; i < laserStates.Length; i++)
        {
            SpaceshipLaser laser = laserStates[i].Laser;
            if (laser == null)
                continue;

            laser.DeactivateForPool();

            Transform laserTransform = laser.transform;
            if (laserTransform != transform)
            {
                laserTransform.localPosition = laserStates[i].LocalPosition;
                laserTransform.localRotation = laserStates[i].LocalRotation;
                laserTransform.localScale = laserStates[i].LocalScale;
            }
        }
    }

    private void Update()
    {
        if (!releaseRequested && activeLaserCount > 0 && Time.time >= forceReleaseTime)
            ReleaseToPool();
    }

    private void ReleaseToPool()
    {
        if (releaseRequested)
            return;

        releaseRequested = true;

        if (pool != null)
        {
            pool.Release(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}

public class PooledParticleEffect : MonoBehaviour
{
    private IObjectPool<ParticleSystem> pool;
    private ParticleSystem sourcePrefab;
    private ParticleSystem rootParticle;
    private ParticleSystem[] particles;
    private float releaseTime;
    private bool playing;
    private bool releaseRequested;

    internal void CacheParticles(ParticleSystem root)
    {
        rootParticle = root;
        particles = rootParticle != null
            ? rootParticle.GetComponentsInChildren<ParticleSystem>(true)
            : new ParticleSystem[0];
    }

    internal void Play(
        IObjectPool<ParticleSystem> ownerPool,
        ParticleSystem prefab,
        ParticleSystem root,
        float lifetime)
    {
        pool = ownerPool;
        sourcePrefab = prefab;
        releaseRequested = false;
        if (rootParticle == null || particles == null || particles.Length == 0)
            CacheParticles(root);

        playing = true;
        releaseTime = Time.time + Mathf.Max(0.05f, lifetime);
        SpaceshipLaser.NotifyPooledVfxStarted(sourcePrefab);

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particle = particles[i];
            if (particle == null)
                continue;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);
        }
    }

    internal void ResetForPool()
    {
        if (playing)
            SpaceshipLaser.NotifyPooledVfxReleased(sourcePrefab);

        playing = false;
        releaseRequested = false;
        sourcePrefab = null;

        if (particles == null)
            return;

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particle = particles[i];
            if (particle != null)
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Update()
    {
        if (playing && Time.time >= releaseTime)
            ReleaseToPool();
    }

    private void ReleaseToPool()
    {
        if (releaseRequested)
            return;

        releaseRequested = true;
        if (playing)
            SpaceshipLaser.NotifyPooledVfxReleased(sourcePrefab);

        playing = false;

        if (pool != null && rootParticle != null)
        {
            pool.Release(rootParticle);
            return;
        }

        Destroy(gameObject);
    }
}
