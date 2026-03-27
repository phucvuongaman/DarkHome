using UnityEngine;

namespace DarkHome
{
    public class TouchState : EnviromentInteractionState
    {
        float _elapsedTime = 0.0f;
        float _resetThreshold = 1.5f;
        public TouchState(EnviromentInteractionContext context
                , EnviromentInteractionStateMachine.EEnviromentInteractionState statekey) : base(context, statekey)
        {
            Context = context;
        }

        public override void EnterState()
        {
            // Debug.Log("Enter TouchState");
            _elapsedTime = 0.0f;
        }
        public override void UpdateState()
        {
            _elapsedTime += Time.deltaTime;
        }
        public override void ExitState() { }
        public override EnviromentInteractionStateMachine.EEnviromentInteractionState GetNextState()
        {
            // Debug.Log("TouchState" + CheckShouldReset());
            if (_elapsedTime > _resetThreshold || CheckShouldReset())
            // if (CheckShouldReset())
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Reset;
            }
            return StateKey;
        }
        public override void OnTriggerEnter(Collider other)
        {
            StartIkTargetPositionTracking(other);
        }
        public override void OnTriggerStay(Collider other)
        {
            UpdateIkTargetPosition(other);
        }
        public override void OnTriggerExit(Collider other)
        {
            ResetIkTargetPositionTracking(other);
        }

    }
}