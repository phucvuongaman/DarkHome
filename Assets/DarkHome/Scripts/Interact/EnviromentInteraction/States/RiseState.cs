using UnityEngine;
namespace DarkHome
{
    public class RiseState : EnviromentInteractionState
    {
        float _elapsedTime = 0.0f;
        float _lerpDuration = 5.0f;
        float _riseWeight = 1.0f;
        Quaternion _expectedHandRotaion;
        float _maxDistance = 0.5f;
        protected LayerMask _interactableLayerMask = LayerMask.GetMask("Interactable");
        float _rotaionSpeed = 1000f;
        float _touchDistanceThreshold = 0.05f;
        float _touchTimeThreshold = 1f;
        public RiseState(EnviromentInteractionContext context
                , EnviromentInteractionStateMachine.EEnviromentInteractionState statekey) : base(context, statekey)
        {
            EnviromentInteractionContext Context = context;
        }

        public override void EnterState() { }
        public override void UpdateState()
        {

            CalculateExpectedHandRotation();

            Context.InteractionPointYOffset = Mathf.Lerp(Context.InteractionPointYOffset,
            Context.ClosestPointOnColliderFromShoulder.y, _elapsedTime / _lerpDuration);

            Context.CurrentIkContraint.weight = Mathf.Lerp(Context.CurrentIkContraint.weight
            , _riseWeight, _elapsedTime / _lerpDuration);

            Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(Context.CurrentMultiRotationConstraint.weight
            , _riseWeight, _elapsedTime / _lerpDuration);

            Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(Context.CurrentIkTargetTransform.rotation,
            _expectedHandRotaion, _rotaionSpeed * Time.deltaTime);

            _elapsedTime += Time.deltaTime;
        }
        private void CalculateExpectedHandRotation()
        {
            Vector3 startPos = Context.CurrentShoulderTransform.position;
            Vector3 endPos = Context.ClosestPointOnColliderFromShoulder;
            Vector3 direction = (endPos - startPos).normalized;

            RaycastHit hit;
            if (Physics.Raycast(startPos, direction, out hit, _maxDistance, _interactableLayerMask))
            {
                Vector3 surfaceNormal = hit.normal;
                Vector3 targetForward = -surfaceNormal;

                // _expectedHandRotaion = Quaternion.LookRotation(
                //  Context.CurrentIkContraint == Context.LeftIkContraint
                // ? targetForward : -targetForward
                // , Vector3.up);
                _expectedHandRotaion = Quaternion.LookRotation(targetForward, Vector3.up);
            }
        }
        public override void ExitState() { }
        public override EnviromentInteractionStateMachine.EEnviromentInteractionState GetNextState()
        {
            // Debug.Log("RiseState" + CheckShouldReset());
            if (CheckShouldReset())
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Reset;
            }

            if (Vector3.Distance(Context.CurrentIkTargetTransform.position, Context.ClosestPointOnColliderFromShoulder)
            < _touchDistanceThreshold && _elapsedTime >= _touchTimeThreshold)
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Touch;
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