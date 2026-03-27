using UnityEngine;

namespace DarkHome
{
    public class ResetState : EnviromentInteractionState
    {
        float _elapsedTime = 0.0f;
        float _resetDuration = 2.0f;
        float _lerpDuration = 10.0f;
        float _rotationSpeed = 500f;

        public ResetState(EnviromentInteractionContext context
        , EnviromentInteractionStateMachine.EEnviromentInteractionState statekey) : base(context, statekey)
        {
            EnviromentInteractionContext Context = context;
        }

        public override void EnterState()
        {
            _elapsedTime = 0.0f;
            Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
            Context.CurrentInterstingCollider = null;
        }
        public override void UpdateState()
        {
            _elapsedTime += Time.deltaTime;
            Context.InteractionPointYOffset = Mathf.Lerp(Context.InteractionPointYOffset,
            Context.ColliderCenterY, _elapsedTime / _lerpDuration);

            Context.CurrentIkContraint.weight = Mathf.Lerp(Context.CurrentIkContraint.weight, 0,
              _elapsedTime / _lerpDuration);

            Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(Context.CurrentMultiRotationConstraint.weight, 0,
            _elapsedTime / _lerpDuration);

            Context.CurrentIkTargetTransform.localPosition = Vector3.Lerp(Context.CurrentIkTargetTransform.localPosition,
            Context.CurrentOriginalTargetPosition, _elapsedTime / _lerpDuration);

            Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(Context.CurrentIkTargetTransform.rotation,
            Context.OriginalTargetRotation, _rotationSpeed * Time.deltaTime);
        }
        public override void ExitState() { }
        public override EnviromentInteractionStateMachine.EEnviromentInteractionState GetNextState()
        {
            // bool isMoving = Context.Rb.linearVelocity.magnitude < 0.05f;
            // if (_elapsedTime >= _resetDuration && isMoving)
            // if (isMoving)
            if (_elapsedTime >= _resetDuration)
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Search;
            }
            return StateKey;
        }
        public override void OnTriggerEnter(Collider other) { }
        public override void OnTriggerExit(Collider other) { }
        public override void OnTriggerStay(Collider other) { }
    }


}