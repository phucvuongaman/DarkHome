using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Assertions;

namespace DarkHome
{
    public class EnviromentInteractionStateMachine : StateManager<EnviromentInteractionStateMachine.EEnviromentInteractionState>
    {
        public enum EEnviromentInteractionState
        {
            Search,
            Approach,
            Rise,
            Touch,
            Reset,
        }

        private EnviromentInteractionContext _context;

        [SerializeField] private TwoBoneIKConstraint _leftIkContraint;
        [SerializeField] private TwoBoneIKConstraint _rightIkContraint;
        [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
        [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CapsuleCollider _rootCollider;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (_context != null && _context.ClosestPointOnColliderFromShoulder != null)
            {
                Gizmos.DrawSphere(_context.ClosestPointOnColliderFromShoulder, 0.03f);
            }
        }

        private void Awake()
        {
            ValidateContraints();

            _context = new EnviromentInteractionContext(_leftIkContraint, _rightIkContraint, _leftMultiRotationConstraint
            , _rightMultiRotationConstraint, _rb, _rootCollider, transform.root);

            ContructEnviromentDetectionCollider();
            InitializeStates();
        }

        private void ValidateContraints()
        {
            Assert.IsNotNull(_leftIkContraint, "Left Ik contraint is not assigned");
            Assert.IsNotNull(_rightIkContraint, "Right Ik contraint is not assigned");
            Assert.IsNotNull(_leftMultiRotationConstraint, "Left multi_rotaion contraint is not assigned");
            Assert.IsNotNull(_rightMultiRotationConstraint, "Left Imulti_rotaionk contraint is not assigned");
            Assert.IsNotNull(_rb, "Rigidbody use to control chararter is not assigned");
            Assert.IsNotNull(_rootCollider, "RootCollider attached to chararter  is not assigned");
        }

        private void InitializeStates()
        {
            States.Add(EEnviromentInteractionState.Reset, new ResetState(_context, EEnviromentInteractionState.Reset));
            States.Add(EEnviromentInteractionState.Search, new SearchState(_context, EEnviromentInteractionState.Search));
            States.Add(EEnviromentInteractionState.Approach, new ApproachState(_context, EEnviromentInteractionState.Approach));
            States.Add(EEnviromentInteractionState.Rise, new RiseState(_context, EEnviromentInteractionState.Rise));
            States.Add(EEnviromentInteractionState.Touch, new TouchState(_context, EEnviromentInteractionState.Touch));
            CurrentState = States[EEnviromentInteractionState.Reset];
        }

        private void ContructEnviromentDetectionCollider()
        {
            float wingspan = _rootCollider.height;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(wingspan, wingspan, wingspan);

            boxCollider.center = new Vector3(_rootCollider.center.x
            , _rootCollider.center.y + (0.25f * wingspan)
            , _rootCollider.center.z + (0.5f * wingspan));

            boxCollider.isTrigger = true;

            _context.ColliderCenterY = _rootCollider.center.y;
        }
    }

}