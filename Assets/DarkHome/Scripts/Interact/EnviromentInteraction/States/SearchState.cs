using UnityEngine;

namespace DarkHome
{
    public class SearchState : EnviromentInteractionState
    {
        public float _approachDistanceTheshold = .5f;
        public SearchState(EnviromentInteractionContext context
                , EnviromentInteractionStateMachine.EEnviromentInteractionState statekey) : base(context, statekey)
        {
            EnviromentInteractionContext Context = context;
        }

        public override void EnterState() { }
        public override void UpdateState() { }
        public override void ExitState() { }
        public override EnviromentInteractionStateMachine.EEnviromentInteractionState GetNextState()
        {
            // Debug.Log("SearchState" + CheckShouldReset());
            if (CheckShouldReset())
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Reset;
            }
            // So sánh với vai (shoulder), không phải chân (root)
            // Vì ClosestPointOnColliderFromShoulder được tính từ vai nên phải dùng cùng tham chiếu
            bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder
            , Context.CurrentShoulderTransform.position) < _approachDistanceTheshold;

            bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;

            if (isClosestPointOnColliderValid && isCloseToTarget)
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Approach;
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