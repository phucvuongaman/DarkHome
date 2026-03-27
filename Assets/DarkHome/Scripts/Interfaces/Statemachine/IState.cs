
namespace DarkHome
{
    public interface IState
    {
        void EnterState();
        void UpdateState();
        void ExitState();
    }
}