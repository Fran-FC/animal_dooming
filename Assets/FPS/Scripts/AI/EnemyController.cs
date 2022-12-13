using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        public UnityEvent onDeathEvent = new UnityEvent();

        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer Renderer;
            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                Renderer = renderer;
                MaterialIndex = index;
            }
        }

        public EnemyType enemyType;

        [Header("Parameters")]
        [Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
        public float SelfDestructYHeight = -20f;

        [Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
        public float PathReachingRadius = 2f;

        [Tooltip("The speed at which the enemy rotates")]
        public float OrientationSpeed = 10f;

        [Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
        public float DeathDuration = 0f;


        [Header("Flash on hit")] [Tooltip("The material used for the body of the hoverbot")]
        public Material BodyMaterial;

        [Tooltip("The gradient representing the color of the flash on hit")] [GradientUsageAttribute(true)]
        public Gradient OnHitBodyGradient;

        [Tooltip("The duration of the flash on hit")]
        public float FlashOnHitDuration = 0.5f;

        [Header("Sounds")] [Tooltip("Sound played when recieving damages")]
        public AudioClip DamageTick;

        [Header("VFX")] [Tooltip("The VFX prefab spawned when the enemy dies")]
        public GameObject DeathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform DeathVfxSpawnPoint;

        [Header("Loot")] [Tooltip("The object this enemy can drop when dying")]
        public GameObject LootPrefab;

        [Tooltip("The chance the object has to drop")] [Range(0, 1)]
        public float DropRate = 1f;

        [Header("Debug Display")] [Tooltip("Color of the sphere gizmo representing the path reaching range")]
        public Color PathReachingRangeColor = Color.yellow;

        [Tooltip("Color of the sphere gizmo representing the attack range")]
        public Color AttackRangeColor = Color.red;

        [Tooltip("Color of the sphere gizmo representing the detection range")]
        public Color DetectionRangeColor = Color.blue;

        public UnityAction onAttack;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;
        public UnityAction onDamaged;

        List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();
        MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;
        float m_LastTimeDamaged = float.NegativeInfinity;


        public PatrolPath PatrolPath { get; set; }
        public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
        public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
        public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
        public bool HadKnownTarget => DetectionModule.HadKnownTarget;
        public NavMeshAgent NavMeshAgent { get; private set; }
        public DetectionModule DetectionModule { get; private set; }

        int m_PathDestinationNodeIndex;
        EnemyManager m_EnemyManager;
        ActorsManager m_ActorsManager;
        Health m_Health;
        Actor m_Actor;
        Collider[] m_SelfColliders;
        GameFlowManager m_GameFlowManager;
        bool m_WasDamagedThisFrame;
        NavigationModule m_NavigationModule;

        void Start()
        {
            m_EnemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(m_EnemyManager, this);

            m_ActorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(m_ActorsManager, this);

            m_EnemyManager.RegisterEnemy(this);

            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(m_Health, this, gameObject);

            m_Actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(m_Actor, this, gameObject);

            NavMeshAgent = GetComponent<NavMeshAgent>();
            m_SelfColliders = GetComponentsInChildren<Collider>();

            m_GameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(m_GameFlowManager, this);

            // Subscribe to damage & death actions
            m_Health.OnDie += OnDie;
            m_Health.OnDamaged += OnDamaged;


            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this,
                gameObject);
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
                this, gameObject);
            // Initialize detection module
            DetectionModule = detectionModules[0];
            DetectionModule.onDetectedTarget += OnDetectedTarget;
            DetectionModule.onLostTarget += OnLostTarget;
            onAttack += DetectionModule.OnAttack;

            var navigationModules = GetComponentsInChildren<NavigationModule>();
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
                this, gameObject);
            // Override navmesh agent data
            if (navigationModules.Length > 0)
            {
                m_NavigationModule = navigationModules[0];
                NavMeshAgent.speed = m_NavigationModule.MoveSpeed;
                NavMeshAgent.angularSpeed = m_NavigationModule.AngularSpeed;
                NavMeshAgent.acceleration = m_NavigationModule.Acceleration;
            }

            m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

        }

        void Update()
        {
            EnsureIsWithinLevelBounds();

            DetectionModule.HandleTargetDetection(m_Actor, m_SelfColliders);

            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / FlashOnHitDuration);
            m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in m_BodyRenderers)
            {
                data.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.MaterialIndex);
            }

            m_WasDamagedThisFrame = false;
        }

        void EnsureIsWithinLevelBounds()
        {
            // at every frame, this tests for conditions to kill the enemy
            if (transform.position.y < SelfDestructYHeight)
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnLostTarget()
        {
            onLostTarget.Invoke();

        }

        void OnDetectedTarget()
        {
            onDetectedTarget.Invoke();

        }

        public void OrientTowards(Vector3 lookPosition)
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
            }
        }

        bool IsPathValid()
        {
            return PatrolPath && PatrolPath.PathNodes.Count > 0;
        }

        public void ResetPathDestination()
        {
            m_PathDestinationNodeIndex = 0;
        }

        public void SetPathDestinationToClosestNode()
        {
            if (IsPathValid())
            {
                int closestPathNodeIndex = 0;
                for (int i = 0; i < PatrolPath.PathNodes.Count; i++)
                {
                    float distanceToPathNode = PatrolPath.GetDistanceToNode(transform.position, i);
                    if (distanceToPathNode < PatrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                    {
                        closestPathNodeIndex = i;
                    }
                }

                m_PathDestinationNodeIndex = closestPathNodeIndex;
            }
            else
            {
                m_PathDestinationNodeIndex = 0;
            }
        }

        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid())
            {
                return PatrolPath.GetPositionOfPathNode(m_PathDestinationNodeIndex);
            }
            else
            {
                return transform.position;
            }
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (NavMeshAgent)
            {
                NavMeshAgent.SetDestination(destination);
            }
        }

        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid())
            {
                // Check if reached the path destination
                if ((transform.position - GetDestinationOnPath()).magnitude <= PathReachingRadius)
                {
                    // increment path destination index
                    m_PathDestinationNodeIndex =
                        inverseOrder ? (m_PathDestinationNodeIndex - 1) : (m_PathDestinationNodeIndex + 1);
                    if (m_PathDestinationNodeIndex < 0)
                    {
                        m_PathDestinationNodeIndex += PatrolPath.PathNodes.Count;
                    }

                    if (m_PathDestinationNodeIndex >= PatrolPath.PathNodes.Count)
                    {
                        m_PathDestinationNodeIndex -= PatrolPath.PathNodes.Count;
                    }
                }
            }
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            // test if the damage source is the player
            if (damageSource && !damageSource.GetComponent<EnemyController>())
            {
                // pursue the player
                DetectionModule.OnDamaged(damageSource);
                
                onDamaged?.Invoke();
                m_LastTimeDamaged = Time.time;
            
                // play the damage tick sound
                if (DamageTick && !m_WasDamagedThisFrame)
                    AudioUtility.CreateSFX(DamageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
            
                m_WasDamagedThisFrame = true;
            }
        }

        void OnDie()
        {
            onDeathEvent.Invoke();
            // spawn a particle system when dying
            var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
            Destroy(vfx, 5f);

            // tells the game flow manager to handle the enemy destuction
            m_EnemyManager.UnregisterEnemy(this);

            // loot an object
            if (TryDropItem())
            {
                Instantiate(LootPrefab, transform.position, Quaternion.identity);
            }

            // this will call the OnDestroy function
            Destroy(gameObject, DeathDuration);
        }

        void OnDrawGizmosSelected()
        {
            // Path reaching range
            Gizmos.color = PathReachingRangeColor;
            Gizmos.DrawWireSphere(transform.position, PathReachingRadius);

            if (DetectionModule != null)
            {
                // Detection range
                Gizmos.color = DetectionRangeColor;
                Gizmos.DrawWireSphere(transform.position, DetectionModule.DetectionRange);

                // Attack range
                Gizmos.color = AttackRangeColor;
                Gizmos.DrawWireSphere(transform.position, DetectionModule.AttackRange);
            }
        }


        public bool TryAtack(Vector3 enemyPosition)
        {
            if (m_GameFlowManager.GameIsEnding)
                return false;


            Debug.Log("Trying attack");
            // Shoot the weapon

            if(onAttack != null)
            {
                onAttack.Invoke();
            }

            return true;
        }

        public bool TryDropItem()
        {
            if (DropRate == 0 || LootPrefab == null)
                return false;
            else if (DropRate == 1)
                return true;
            else
                return (Random.value <= DropRate);
        }
    }
}