// using UnityEngine;

// namespace DarkHome
// {
//     public abstract class EnviromentInteractionState : BaseState<EnviromentInteractionStateMachine.EEnviromentInteractionState>
//     {
//         private float _movingAwayOffset = 0.005f;
//         bool _shouldReset = false;

//         protected EnviromentInteractionContext Context;

//         public EnviromentInteractionState(EnviromentInteractionContext context
//         , EnviromentInteractionStateMachine.EEnviromentInteractionState statekey) : base(statekey)
//         {
//             Context = context;
//         }

//         protected bool CheckShouldReset()
//         {
//             if (_shouldReset)
//             {
//                 Context.LowestDistance = Mathf.Infinity;
//                 _shouldReset = false;
//                 // Debug.Log("_shouldReset");
//                 return true;
//             }

//             bool isPlayerStopped = Context.Rb.linearVelocity.magnitude < 0.05f;
//             bool isMovingAway = CheckIsMovingAway();
//             bool isBadAangle = CheckIsBadAngle();
//             // Debug.Log("isMovingAway" + isMovingAway);
//             // Debug.Log("isBadAangle" + isBadAangle);

//             if (isPlayerStopped || isMovingAway || isBadAangle)
//             // if (isMovingAway || isBadAangle)
//             // if (isMovingAway)
//             {
//                 Context.LowestDistance = Mathf.Infinity;
//                 return true;
//             }
//             return false;
//         }
//         protected bool CheckIsBadAngle()
//         {
//             if (Context.CurrentInterstingCollider == null)
//             {
//                 return false;
//             }

//             Vector3 targetDirection = Context.ClosestPointOnColliderFromShoulder - Context.CurrentShoulderTransform.position;
//             Vector3 shoulderDirection = Context.CurrentBodySide == EnviromentInteractionContext.EBodySide.Right
//             ? Context.RootTransform.right : -Context.RootTransform.right;

//             float dotProduct = Vector3.Dot(shoulderDirection, targetDirection.normalized);
//             bool isBadAangle = dotProduct < 0;

//             return isBadAangle;
//         }
//         protected bool CheckIsMovingAway()
//         {
//             float currentDistanceToTarget = Vector3.Distance(Context.RootTransform.position, Context.ClosestPointOnColliderFromShoulder);

//             bool isSearchingForNewInteraction = Context.CurrentInterstingCollider == null;
//             if (isSearchingForNewInteraction)
//             {
//                 // Debug.Log("isSearchingForNewInteraction" + isSearchingForNewInteraction);
//                 return false;
//             }

//             bool isGettingCloserToTarget = currentDistanceToTarget <= Context.LowestDistance;
//             if (isGettingCloserToTarget)
//             {
//                 Context.LowestDistance = currentDistanceToTarget;
//                 return false;
//             }

//             bool isMovingAwayFromTarget = currentDistanceToTarget > Context.LowestDistance + _movingAwayOffset;
//             if (isMovingAwayFromTarget)
//             {
//                 // Debug.Log("isMovingAwayFromTarget");
//                 Context.LowestDistance = Mathf.Infinity;
//                 return true;
//             }
//             return false;
//         }
//         private Vector3 GetClosestPointOnCollider(Collider intersectingCollider, Vector3 positionToCheck)
//         {
//             return intersectingCollider.ClosestPoint(positionToCheck);
//         }

//         protected void StartIkTargetPositionTracking(Collider intersectingCollider)
//         {
//             // Debug.Log("intersectingCollider.gameObject.layer: " + intersectingCollider.gameObject.layer + "   " + (intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && Context.CurrentInterstingCollider == null));
//             if (intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && Context.CurrentInterstingCollider == null)
//             {
//                 Context.CurrentInterstingCollider = intersectingCollider;
//                 Vector3 closestPointFromRoot = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
//                 Context.SetInteractionSide(closestPointFromRoot);
//                 // SetIkTargetPosition();
//                 SetIkTargetPosition(intersectingCollider);
//             }
//         }
//         protected void UpdateIkTargetPosition(Collider intersectingCollider)
//         {
//             if (intersectingCollider == Context.CurrentInterstingCollider)
//             {
//                 // SetIkTargetPosition();
//                 SetIkTargetPosition(intersectingCollider);
//             }
//         }
//         protected void ResetIkTargetPositionTracking(Collider intersectingCollider)
//         {
//             if (intersectingCollider == Context.CurrentInterstingCollider)
//             {
//                 Context.CurrentInterstingCollider = null;
//                 Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
//                 _shouldReset = true;
//             }
//         }

//         private void SetIkTargetPosition(Collider intersectingCollider)
//         {
//             // Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.CurrentInterstingCollider,
//             // new Vector3(Context.CurrentShoulderTransform.position.x, Context.CharacterShoulderHeight, Context.CurrentShoulderTransform.position.z));
//             Context.ClosestPointOnColliderFromShoulder = intersectingCollider.ClosestPoint(Context.CurrentShoulderTransform.position);

//             Vector3 rayDirection = Context.CurrentShoulderTransform.position - Context.ClosestPointOnColliderFromShoulder;
//             Vector3 normalizeRayDirection = rayDirection.normalized;
//             float offsetDistance = 0.05f;
//             Vector3 offset = normalizeRayDirection * offsetDistance;

//             Vector3 offsetPosition = Context.ClosestPointOnColliderFromShoulder + offset;
//             // Vector3 offsetPosition = Context.ClosestPointOnColliderFromShoulder;
//             Context.CurrentIkTargetTransform.position = new Vector3(offsetPosition.x, Context.InteractionPointYOffset, offsetPosition.z);
//         }
//     }
// }



using UnityEngine;

namespace DarkHome
{
    public abstract class EnviromentInteractionState : BaseState<EnviromentInteractionStateMachine.EEnviromentInteractionState>
    {
        private float _movingAwayOffset = 0.005f;
        bool _shouldReset = false;

        protected EnviromentInteractionContext Context;

        public EnviromentInteractionState(EnviromentInteractionContext context
        , EnviromentInteractionStateMachine.EEnviromentInteractionState statekey) : base(statekey)
        {
            Context = context;
        }

        protected bool CheckShouldReset()
        {
            if (_shouldReset)
            {
                Context.LowestDistance = Mathf.Infinity;
                _shouldReset = false;
                // Debug.Log("_shouldReset");
                return true;
            }

            bool isPlayerStopped = Context.Rb.linearVelocity.magnitude < 0.05f;
            bool isMovingAway = CheckIsMovingAway();
            bool isBadAangle = CheckIsBadAngle();
            // Debug.Log("isMovingAway" + isMovingAway);
            // Debug.Log("isBadAangle" + isBadAangle);

            if (isPlayerStopped || isMovingAway || isBadAangle)
            // if (isMovingAway || isBadAangle)
            // if (isMovingAway)
            {
                Context.LowestDistance = Mathf.Infinity;
                return true;
            }
            return false;
        }
        protected bool CheckIsBadAngle()
        {
            if (Context.CurrentInterstingCollider == null)
            {
                return false;
            }

            Vector3 targetDirection = Context.ClosestPointOnColliderFromShoulder - Context.CurrentShoulderTransform.position;
            Vector3 shoulderDirection = Context.CurrentBodySide == EnviromentInteractionContext.EBodySide.Right
            ? Context.RootTransform.right : -Context.RootTransform.right;

            float dotProduct = Vector3.Dot(shoulderDirection, targetDirection.normalized);
            bool isBadAangle = dotProduct < 0;

            return isBadAangle;
        }
        protected bool CheckIsMovingAway()
        {
            float currentDistanceToTarget = Vector3.Distance(Context.RootTransform.position, Context.ClosestPointOnColliderFromShoulder);

            bool isSearchingForNewInteraction = Context.CurrentInterstingCollider == null;
            if (isSearchingForNewInteraction)
            {
                // Debug.Log("isSearchingForNewInteraction" + isSearchingForNewInteraction);
                return false;
            }

            bool isGettingCloserToTarget = currentDistanceToTarget <= Context.LowestDistance;
            if (isGettingCloserToTarget)
            {
                Context.LowestDistance = currentDistanceToTarget;
                return false;
            }

            bool isMovingAwayFromTarget = currentDistanceToTarget > Context.LowestDistance + _movingAwayOffset;
            if (isMovingAwayFromTarget)
            {
                // Debug.Log("isMovingAwayFromTarget");
                Context.LowestDistance = Mathf.Infinity;
                return true;
            }
            return false;
        }
        private Vector3 GetClosestPointOnCollider(Collider intersectingCollider, Vector3 positionToCheck)
        {
            return intersectingCollider.ClosestPoint(positionToCheck);
        }

        protected void StartIkTargetPositionTracking(Collider intersectingCollider)
        {
            if (intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && Context.CurrentInterstingCollider == null)
            {
                Context.CurrentInterstingCollider = intersectingCollider;
                Vector3 closestPointFromRoot = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
                Context.SetInteractionSide(closestPointFromRoot);
                // Khởi tạo Y offset = shoulder height để target không bắt đầu ở Y=0 (sàn)
                Context.InteractionPointYOffset = Context.CharacterShoulderHeight;
                SetIkTargetPosition();
            }
        }
        protected void UpdateIkTargetPosition(Collider intersectingCollider)
        {
            if (intersectingCollider == Context.CurrentInterstingCollider)
            {
                // Cập nhật side realtime → cho phép switch tay phải/trái khi player di chuyển
                Vector3 closestFromRoot = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
                Context.SetInteractionSide(closestFromRoot);
                SetIkTargetPosition();
            }
        }
        protected void ResetIkTargetPositionTracking(Collider intersectingCollider)
        {
            if (intersectingCollider == Context.CurrentInterstingCollider)
            {
                Context.CurrentInterstingCollider = null;
                Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
                _shouldReset = true;
            }
        }

        private void SetIkTargetPosition()
        {
            // Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.CurrentInterstingCollider,
            // new Vector3(Context.CurrentShoulderTransform.position.x, Context.CharacterShoulderHeight, Context.CurrentShoulderTransform.position.z));
            Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.CurrentInterstingCollider,
            new Vector3(Context.CurrentShoulderTransform.position.x, Context.CharacterShoulderHeight, Context.CurrentShoulderTransform.position.z));

            Vector3 rayDirection = Context.CurrentShoulderTransform.position - Context.ClosestPointOnColliderFromShoulder;
            Vector3 normalizeRayDirection = rayDirection.normalized;
            float offsetDistance = 0.05f;
            Vector3 offset = normalizeRayDirection * offsetDistance;

            Vector3 offsetPosition = Context.ClosestPointOnColliderFromShoulder + offset;
            // Vector3 offsetPosition = Context.ClosestPointOnColliderFromShoulder;
            Context.CurrentIkTargetTransform.position = new Vector3(offsetPosition.x, Context.InteractionPointYOffset, offsetPosition.z);
        }
    }
}