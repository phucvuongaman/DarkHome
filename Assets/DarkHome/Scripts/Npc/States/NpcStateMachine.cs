using UnityEngine;

namespace DarkHome
{
    public class NpcStateMachine : StateManager<NpcStateMachine.ENpcStates>
    {

        public enum ENpcStates
        {
            Idle,
            Confuse,
            Disapear,
            Move,
            Run,
            Talk,
            Rage,
            GuardZone,
            ChoosePatrolZone,
            Wait,
        }

        NpcContext _npcContext;
        // Các State của NPC
        public Vector3 MoveTargetPosition { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; set; }
        public void SetMoveTarget(Vector3 newTarget)
        {
            MoveTargetPosition = newTarget;
        }
        private void Awake()
        {
            _npcContext = GetComponent<NpcContext>();
            InitializeStates();
            CurrentState = States[ENpcStates.Idle];
        }

        private void InitializeStates()
        {
            States.Add(ENpcStates.Idle, new NpcIdleState(_npcContext, ENpcStates.Idle));
            States.Add(ENpcStates.Confuse, new NpcConfuseState(_npcContext, ENpcStates.Confuse));
            States.Add(ENpcStates.Move, new NpcMoveState(_npcContext, ENpcStates.Move));
            States.Add(ENpcStates.Run, new NpcRunState(_npcContext, ENpcStates.Run));
            States.Add(ENpcStates.Talk, new NpcTalkState(_npcContext, ENpcStates.Talk));
            States.Add(ENpcStates.Disapear, new NpcDisapearState(_npcContext, ENpcStates.Disapear));
            States.Add(ENpcStates.Rage, new NpcRageState(_npcContext, ENpcStates.Rage));
            States.Add(ENpcStates.GuardZone, new NpcGuardZoneState(_npcContext, ENpcStates.GuardZone));
            States.Add(ENpcStates.ChoosePatrolZone, new NpcChoosePatrolZoneState(_npcContext, ENpcStates.ChoosePatrolZone));
            States.Add(ENpcStates.Wait, new NpcWaitState(_npcContext, ENpcStates.Wait));
        }

    }
}