using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class SanityStateMachine : StateManager<SanityStateMachine.ESanityState>
    {
        public enum ESanityState
        {
            Stable,
            Disturbed,
            Unstable,
            Insane
        }
        private PlayerContext _context;

        void Awake()
        {
            _context = GetComponent<PlayerContext>();
            InitializeStates();
            CurrentState = States[ESanityState.Stable];
        }

        private void InitializeStates()
        {
            States.Add(ESanityState.Stable, new StableSanityState(_context, ESanityState.Stable));
            States.Add(ESanityState.Disturbed, new DisturbedSanityState(_context, ESanityState.Disturbed));
            States.Add(ESanityState.Unstable, new UnstableSanityState(_context, ESanityState.Unstable));
            States.Add(ESanityState.Insane, new InsaneSanityState(_context, ESanityState.Insane));
        }
    }
}