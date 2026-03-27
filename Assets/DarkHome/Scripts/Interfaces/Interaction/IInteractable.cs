


namespace DarkHome
{
    public interface IInteractable
    {
        // Thay thế hàm Interact() chung chung bằng các hàm cụ thể
        void OnInteractPress(Interactor interactor);
        void OnInteractHold(Interactor interactor);
        void OnInteractRelease(Interactor interactor);

    }
}