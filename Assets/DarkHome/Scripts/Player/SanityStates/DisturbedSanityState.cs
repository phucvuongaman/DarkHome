using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace DarkHome
{
    public class DisturbedSanityState : BaseState<SanityStateMachine.ESanityState>
    {
        protected PlayerContext _context;

        public DisturbedSanityState(PlayerContext context, SanityStateMachine.ESanityState statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            VolumeManager.Instance?.ApplyDisturbedEffects();
        }
        public override void UpdateState()
        {
            _context.Stats.Sanity -= 1.0f * Time.deltaTime;
        }
        public override void ExitState() { }

        public override SanityStateMachine.ESanityState GetNextState()
        {
            float currentSanity = _context.Stats.Sanity;
            // Nếu sanity hồi phục trên 75, quay về Stable
            if (currentSanity > 75)
            {
                return SanityStateMachine.ESanityState.Stable;
            }
            // Nếu sanity giảm xuống dưới 50, chuyển sang Unstable
            else if (currentSanity <= 50)
            {
                return SanityStateMachine.ESanityState.Unstable;
            }
            return StateKey;
        }

    }
}