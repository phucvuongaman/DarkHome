using UnityEngine;

namespace DarkHome
{

    public class ApproachState : EnviromentInteractionState
    {
        float _elapsedTime = 0.0f;
        float _lerpDuration = 5.0f;
        float _apprroachDuration = 2.0f;
        float _approachWeight = 0.5f;
        float _approachRotaionWeight = 0.75f;
        float _rotaionSpeed = 500f;
        float _riseDistanceTheshold = 0.5f;

        public ApproachState(EnviromentInteractionContext context
        , EnviromentInteractionStateMachine.EEnviromentInteractionState statekey) : base(context, statekey)
        {
            EnviromentInteractionContext Context = context;
        }

        public override void EnterState()
        {
            _elapsedTime = 0.0f;
        }
        public override void UpdateState()
        {
            // TODO: cần chỉnh lại góc
            // Tạo một Quaternion với trục Z hướng xuống dưới đất
            // Quaternion expectedGroundRotaion = Context.CurrentIkContraint == Context.LeftIkContraint
            //     ? Quaternion.LookRotation(-Vector3.up, Context.RootTransform.forward)
            //     : Quaternion.LookRotation(Vector3.up, Context.RootTransform.forward);
            // _elapsedTime += Time.deltaTime;

            Quaternion expectedGroundRotaion = Quaternion.LookRotation(-Vector3.up, Context.RootTransform.forward);
            _elapsedTime += Time.deltaTime;

            Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(Context.CurrentIkTargetTransform.rotation, expectedGroundRotaion,
            _rotaionSpeed * Time.deltaTime);

            Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(Context.CurrentMultiRotationConstraint.weight, _approachRotaionWeight,
            _elapsedTime / _lerpDuration);

            Context.CurrentIkContraint.weight = Mathf.Lerp(Context.CurrentIkContraint.weight, _approachWeight,
            _elapsedTime / _lerpDuration);
        }
        public override void ExitState() { }
        public override EnviromentInteractionStateMachine.EEnviromentInteractionState GetNextState()
        {
            // Debug.Log("ApproachState" + CheckShouldReset());
            bool isOverStateLifeDuration = _elapsedTime >= _apprroachDuration;
            if (isOverStateLifeDuration || CheckShouldReset())
            // if (isOverStateLifeDuration)
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Reset;
            }

            bool isWithinArmsReach = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder,
            Context.CurrentShoulderTransform.position) < _riseDistanceTheshold;
            bool isClosestPointOnColliderReal = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;

            if (isClosestPointOnColliderReal && isWithinArmsReach)
            {
                return EnviromentInteractionStateMachine.EEnviromentInteractionState.Rise;
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