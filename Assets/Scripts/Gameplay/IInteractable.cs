namespace EcoDoTempo
{
    public interface IInteractable
    {
        string Prompt(Era era);
        void Interact(Era era);
    }
}
