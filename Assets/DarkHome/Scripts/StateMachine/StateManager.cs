using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{
    public class StateManager<EState> : MonoBehaviour where EState : Enum
    {
        protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();

        protected BaseState<EState> CurrentState;
        protected bool IsTransitioningState = false;

        void Start()
        {
            CurrentState.EnterState();
        }

        void Update()
        {
            EState nextStateKey = CurrentState.GetNextState();

            if (!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
            {
                CurrentState.UpdateState();
            }
            else if (!IsTransitioningState)
            {
                TransitionToState(nextStateKey);
            }
        }
        void LateUpdate()
        {
            if (CurrentState != null)
            {
                CurrentState.LateUpdateState();
            }
        }
        void FixedUpdate() // HÀM MỚI
        {
            if (CurrentState != null)
            {
                CurrentState.FixedUpdateState();
            }
        }

        public void TransitionToState(EState stateKey)
        {
            IsTransitioningState = true;
            CurrentState.ExitState();
            CurrentState = States[stateKey];
            CurrentState.EnterState();
            IsTransitioningState = false;
        }

        void OnTriggerEnter(Collider other)
        {
            CurrentState.OnTriggerEnter(other);
        }
        void OnTriggerStay(Collider other)
        {
            CurrentState.OnTriggerStay(other);
        }

        void OnTriggerExit(Collider other)
        {
            CurrentState.OnTriggerExit(other);
        }
    }
}