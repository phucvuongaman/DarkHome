using UnityEngine;

namespace DarkHome
{
    public class NpcRunState : BaseState<NpcStateMachine.ENpcStates>
    {
        protected NpcContext _context;

        public NpcRunState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState() { }

        public override void ExitState() { }

        public override void UpdateState()
        {
            _context.NpcMovement.AnimatorRun();
        }

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            if (_context.NpcMovement.IsIdle)
            {
                return NpcStateMachine.ENpcStates.Idle;
            }
            return StateKey;
        }
    }
}