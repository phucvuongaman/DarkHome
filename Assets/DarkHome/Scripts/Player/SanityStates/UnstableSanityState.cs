using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace DarkHome
{
    public class UnstableSanityState : BaseState<SanityStateMachine.ESanityState>
    {
        protected PlayerContext _context;

        public UnstableSanityState(PlayerContext context, SanityStateMachine.ESanityState statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            VolumeManager.Instance?.ApplyUnstableEffects();
        }
        public override void UpdateState()
        {
            _context.Stats.Sanity -= 2.0f * Time.deltaTime;
        }
        public override void ExitState() { }

        public override SanityStateMachine.ESanityState GetNextState()
        {
            float currentSanity = _context.Stats.Sanity;
            if (currentSanity > 50)
            {
                return SanityStateMachine.ESanityState.Disturbed;
            }
            else if (currentSanity <= 25)
            {
                return SanityStateMachine.ESanityState.Insane;
            }
            return StateKey;
        }

    }
}