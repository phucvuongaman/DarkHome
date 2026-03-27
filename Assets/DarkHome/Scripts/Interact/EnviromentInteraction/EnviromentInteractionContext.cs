/*
* Thay vì tạo một đống getter ở EnviromentInteractionStateMachine thì chỉ cần tạo 1 class context để sử dụng đỡ rối state machine
*/
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace DarkHome
{
    public class EnviromentInteractionContext
    {
        public enum EBodySide
        {
            Left,
            Right,
            Front,
        }

        private TwoBoneIKConstraint _leftIkContraint;
        private TwoBoneIKConstraint _rightIkContraint;
        private MultiRotationConstraint _leftMultiRotationConstraint;
        private MultiRotationConstraint _rightMultiRotationConstraint;
        private Rigidbody _rb;
        private CapsuleCollider _rootCollider;
        private Transform _rootTransform;
        private Vector3 _leftOriginalTargetPosition;
        private Vector3 _rightOriginalTargetPosition;


        public EnviromentInteractionContext(TwoBoneIKConstraint leftIkContraint, TwoBoneIKConstraint rightIkContraint
        , MultiRotationConstraint leftMultiRotationConstraint, MultiRotationConstraint rightMultiRotationConstraint
        , Rigidbody rb, CapsuleCollider rootCollider, Transform rootTransform)
        {
            _leftIkContraint = leftIkContraint;
            _rightIkContraint = rightIkContraint;
            _leftMultiRotationConstraint = leftMultiRotationConstraint;
            _rightMultiRotationConstraint = rightMultiRotationConstraint;
            _rb = rb;
            _rootCollider = rootCollider;
            _rootTransform = rootTransform;
            _leftOriginalTargetPosition = _leftIkContraint.data.target.transform.localPosition;
            _rightOriginalTargetPosition = _rightIkContraint.data.target.transform.localPosition;
            OriginalTargetRotation = _leftIkContraint.data.target.rotation;


            CharacterShoulderHeight = leftIkContraint.data.root.position.y;
            SetInteractionSide(Vector3.positiveInfinity);
        }

        public TwoBoneIKConstraint LeftIkContraint => _leftIkContraint;
        public TwoBoneIKConstraint RightIkContraint => _rightIkContraint;
        public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
        public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
        public Rigidbody Rb => _rb;
        public CapsuleCollider RootCollider => _rootCollider;
        public Transform RootTransform => _rootTransform;

        public float CharacterShoulderHeight { get; private set; }

        public Collider CurrentInterstingCollider { get; set; }
        public TwoBoneIKConstraint CurrentIkContraint { get; private set; }
        public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
        public Transform CurrentIkTargetTransform { get; private set; }
        public Transform CurrentShoulderTransform { get; private set; }
        public EBodySide CurrentBodySide { get; private set; }
        public Vector3 ClosestPointOnColliderFromShoulder { get; set; } = Vector3.positiveInfinity;
        public float InteractionPointYOffset { get; set; } = 0;
        public float ColliderCenterY { get; set; }
        public Vector3 CurrentOriginalTargetPosition { get; private set; }
        public Quaternion OriginalTargetRotation { get; private set; }
        public float LowestDistance { get; set; } = Mathf.Infinity;

        public void SetInteractionSide(Vector3 positionToCheck)
        {
            Vector3 leftShoulder = _leftIkContraint.data.root.transform.position;
            Vector3 rightShoulder = _rightIkContraint.data.root.transform.position;

            Vector3 toTarget = (positionToCheck - _rootTransform.position).normalized;
            float dot = Vector3.Dot(toTarget, _rootTransform.forward);
            bool isLeftCloser = Vector3.Distance(positionToCheck, leftShoulder) < Vector3.Distance(positionToCheck, rightShoulder);

            // if (dot > 0.7f)
            // {
            //     // Debug.Log("Front");
            //     CurrentBodySide = EBodySide.Front;
            //     CurrentIkContraint = _leftIkContraint; // hoặc tay cố định
            //     CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
            //     CurrentOriginalTargetPosition = _leftOriginalTargetPosition;
            // }
            // else if (isLeftCloser)
            if (isLeftCloser)
            {
                // Debug.Log("Left is closer");
                CurrentBodySide = EBodySide.Left;
                CurrentIkContraint = _leftIkContraint;
                CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
                CurrentOriginalTargetPosition = _leftOriginalTargetPosition;
            }
            else
            {
                // Debug.Log("Right is closer");
                CurrentBodySide = EBodySide.Right;
                CurrentIkContraint = _rightIkContraint;
                CurrentMultiRotationConstraint = _rightMultiRotationConstraint;
                CurrentOriginalTargetPosition = _rightOriginalTargetPosition;
            }
            CurrentShoulderTransform = CurrentIkContraint.data.root.transform;
            CurrentIkTargetTransform = CurrentIkContraint.data.target.transform;
        }

    }
}