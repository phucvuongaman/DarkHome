// Nếu là NPC bình thường thôi thì dùng NpcContext
// Còn là Enemy thì dùng EnemyContext.


using System;
using UnityEngine;
using UnityEngine.AI;

namespace DarkHome
{
    [RequireComponent(typeof(NpcStateMachine), typeof(NpcMovement), typeof(Npc))]
    [RequireComponent(typeof(HeadLook), typeof(ScannerTarget))]
    [RequireComponent(typeof(Rigidbody), typeof(NavMeshAgent))]
    // Animator KHÔNG RequireComponent vì nằm trên VRoid child model

    public class NpcContext : MonoBehaviour
    {

        public NpcStateMachine StateMachine { get; private set; }
        public NpcMovement NpcMovement { get; private set; }
        public Animator Animator { get; private set; }
        public Npc Npc { get; private set; }
        public Rigidbody Rb { get; set; }
        public NavMeshAgent Agent { get; set; }
        public HeadLook HeadLook { get; set; }
        public ScannerTarget ScannerTarget { get; private set; }
        public NpcAnchor CurrentAnchor; // Điểm neo hiện tại NPC đang hướng tới hoặc đang đứng

        protected virtual void Awake()
        {
            StateMachine = GetComponent<NpcStateMachine>();
            NpcMovement = GetComponent<NpcMovement>();
            Animator = GetComponentInChildren<Animator>();
            Npc = GetComponent<Npc>();
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
            HeadLook = GetComponent<HeadLook>();
            ScannerTarget = GetComponent<ScannerTarget>();

        }

        protected virtual void Start()
        {
            NpcManager.Instance.Register(this);
        }

        protected virtual void OnPlayerSpawned(Transform player)
        {
            HeadLook.Target = player;
            ScannerTarget.Target = player;
        }

        public NpcData GetCurrentNpcData()
        {
            return new NpcData
            {
                npcId = Npc.Id,
                position = transform.position,
                rotation = transform.rotation,
                isActive = gameObject.activeSelf,
            };
        }



        public void LoadData(NpcData data)
        {
            if (data == null) return;
            Agent.Warp(data.position);
            Agent.transform.rotation = data.rotation;
            gameObject.SetActive(data.isActive);
        }


        private void OnEnable()
        {
            EventManager.AddObserver<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, OnPlayerSpawned);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, OnPlayerSpawned);
        }


    }
}