using System;
using UnityEngine;

namespace DarkHome
{
    public abstract class BaseState<EState> : IState where EState : Enum
    {
        public BaseState(EState key)
        {
            StateKey = key;
        }

        public EState StateKey { get; private set; }
        #region Required State Methods
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract EState GetNextState();
        #endregion

        #region Optional State Methods
        public virtual void FixedUpdateState() { }
        public virtual void LateUpdateState() { }
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerStay(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }
        #endregion
    }
}