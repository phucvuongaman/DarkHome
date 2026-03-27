using UnityEngine;

namespace DarkHome
{
    public class NpcDisapearState : BaseState<NpcStateMachine.ENpcStates>
    {
        protected NpcContext _context;

        public NpcDisapearState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState() { }
        public override void ExitState() { }
        public override void UpdateState() { }

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            return StateKey;
        }

    }

}