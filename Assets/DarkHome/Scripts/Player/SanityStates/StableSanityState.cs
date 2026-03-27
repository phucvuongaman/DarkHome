using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace DarkHome
{
    public class StableSanityState : BaseState<SanityStateMachine.ESanityState>
    {
        protected PlayerContext _context;

        public StableSanityState(PlayerContext context, SanityStateMachine.ESanityState statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            VolumeManager.Instance?.ApplyStableEffects();
        }
        public override void UpdateState()
        {
            _context.Stats.Sanity += 0.5f * Time.deltaTime;
        }
        public override void ExitState() { }

        public override SanityStateMachine.ESanityState GetNextState()
        {
            // Nếu sanity giảm xuống dưới 75, chuyển sang trạng thái Disturbed
            if (_context.Stats.Sanity <= 75)
            {
                return SanityStateMachine.ESanityState.Disturbed;
            }
            return StateKey;
        }


    }
}