namespace Zenatur.Tms.Application.State;

public class CiotStateContainer
{
    public EmissaoViewModel? EmissaoEmAndamento { get; private set; }
    public int FaseAtual { get; private set; }

    public event Action? OnChange;

    public void IniciarEmissao(EmissaoViewModel vm)
    {
        EmissaoEmAndamento = vm;
        FaseAtual = 1;
        NotifyStateChanged();
    }

    public void AvancarFase()
    {
        if (FaseAtual < 4) FaseAtual++;
        NotifyStateChanged();
    }

    public void RetrocederFase()
    {
        if (FaseAtual > 1) FaseAtual--;
        NotifyStateChanged();
    }

    public void Limpar()
    {
        EmissaoEmAndamento = null;
        FaseAtual = 0;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
