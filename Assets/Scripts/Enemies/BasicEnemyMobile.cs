using Unity.FPS.Game;
using UnityEngine;
using System.Collections;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class BasicEnemyMobile : MonoBehaviour
    {
        private WeaponController weaponController;
        public enum AIState
        {
            Patrol,
            Follow,
            Attack,
        }


        public Animator Animator;

        [Tooltip("Fraction of the enemy's attack range at which it will stop moving towards target while attacking")]
        [Range(0f, 1f)]
        public float AttackStopDistanceRatio = 0.5f;
        public bool is_distance_attack = false;
        public GameObject ammo_prefab;
        public float AttackDamage = 40.0f;
        public float AttackRate = 1.0f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        [Header("Sound")] public AudioClip MovementSound;
        public MinMaxFloat PitchDistortionMovementSpeed;

        public AIState AiState { get; private set; }
        EnemyController m_EnemyController;
        AudioSource m_AudioSource;

        public bool damage_stun = true;

        const string k_AnimMoveSpeedParameter = "speed";
        const string k_AnimAttackParameter = "attack";
        // const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamagedParameter = "damage";

        float m_lastTimeAttacked = float.NegativeInfinity;



        void Start()
        {
            weaponController = GetComponent<WeaponController>();
            if (weaponController == null)
                weaponController = GetComponentInChildren<WeaponController>();
            if(weaponController != null)
                weaponController.Owner = gameObject;

            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, BasicEnemyMobile>(m_EnemyController, this,
                gameObject);

            m_EnemyController.onAttack += OnAttack;
            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;
            m_EnemyController.SetPathDestinationToClosestNode();
            m_EnemyController.onDamaged += OnDamaged;

            // Start patrolling
            AiState = AIState.Patrol;

            // adding a audio source to play the movement sound on it
            m_AudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, BasicEnemyMobile>(m_AudioSource, this, gameObject);
            m_AudioSource.clip = MovementSound;
            m_AudioSource.Play();

        }

        void Update()
        {
            UpdateAiStateTransitions();
            UpdateCurrentAiState();

            float moveSpeed = m_EnemyController.NavMeshAgent.velocity.magnitude;

            // Update animator speed parameter
            Animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);

            // changing the pitch of the movement sound depending on the movement speed
            m_AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
                moveSpeed / m_EnemyController.NavMeshAgent.speed);
        }


        void UpdateAiStateTransitions()
        {
            // Handle transitions 
            switch (AiState)
            {
                case AIState.Follow:
                    // Transition to attack when there is a line of sight to the target
                    if (m_EnemyController.IsSeeingTarget && m_EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        m_EnemyController.SetNavDestination(transform.position);
                    }
                    break;
                case AIState.Attack:
                    // Transition to follow when no longer a target in attack range
                    if (!m_EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Follow;
                    }
                    break;
            }
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Patrol:
                    m_EnemyController.UpdatePathDestination();
                    m_EnemyController.SetNavDestination(m_EnemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);

                    break;
                case AIState.Attack:
                    if (Vector3.Distance(m_EnemyController.KnownDetectedTarget.transform.position,
                            m_EnemyController.DetectionModule.DetectionSourcePoint.position)
                        >= (AttackStopDistanceRatio * m_EnemyController.DetectionModule.AttackRange))
                    {
                        m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
                    }
                    else
                    {
                        m_EnemyController.SetNavDestination(transform.position);
                    }

                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    
                    
                    if (Time.time - m_lastTimeAttacked >= AttackRate || float.IsNegativeInfinity(m_lastTimeAttacked))
                    {
                        m_EnemyController.TryAttack(m_EnemyController.KnownDetectedTarget.transform.position);
                        m_lastTimeAttacked = Time.time;
                    }
                    
                    break;
            }
        }

        void OnAttack()
        {
            GameObject obj = GameObject.FindWithTag("Player");
            Damageable damageable = obj.GetComponent<Collider>().GetComponent<Damageable>();
            if (damageable)
            {
                if(!is_distance_attack)
                {
                    damageable.InflictDamage(AttackDamage, false, gameObject);
                    return;
                }

                Vector3 offset = new Vector3(0f,1f,0f);
                // throw projectile
                Vector3 v = ( obj.transform.position - transform.position+offset ).normalized;
                Ray r = new Ray(transform.position, v);
                GameObject bullet = Instantiate(ammo_prefab, transform.position, Quaternion.LookRotation(v));

                bullet.GetComponent<ProjectileBase>().Shoot(weaponController);
                bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 1.5f;

                //Destroy(bullet, 3);
            }
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Patrol)
            {
                AiState = AIState.Follow;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }

            // Animator.SetBool(k_AnimAlertedParameter, true);
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Follow || AiState == AIState.Attack)
            {
                AiState = AIState.Patrol;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Stop();
            }

            // Animator.SetBool(k_AnimAlertedParameter, false);
        }

        void OnDamaged()
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            if(damage_stun) 
                Animator.SetTrigger(k_AnimOnDamagedParameter);
        }
    }
}