using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class InsaneSanityState : BaseState<SanityStateMachine.ESanityState>
    {
        protected PlayerContext _context;

        public InsaneSanityState(PlayerContext context, SanityStateMachine.ESanityState statekey) : base(statekey)
        {
            _context = context;
        }


        public override void EnterState()
        {
            VolumeManager.Instance?.ApplyInsaneEffects();
        }
        public override void UpdateState()
        {
            _context.Stats.Sanity -= 4.0f * Time.deltaTime;
        }
        public override void ExitState() { }

        public override SanityStateMachine.ESanityState GetNextState()
        {
            if (_context.Stats.Sanity > 25)
            {
                return SanityStateMachine.ESanityState.Unstable;
            }
            return StateKey;
        }
    }
}