using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Zenatur.Tms.Web.Localization;

public sealed class PtBrMudLocalizer : MudLocalizer
{
    private static readonly Dictionary<string, string> _pt = new()
    {
        ["MudStepper_Next"]     = "Próximo",
        ["MudStepper_Previous"] = "Anterior",
        ["MudStepper_Skip"]     = "Pular",
        ["MudStepper_Reset"]    = "Reiniciar",
        ["MudStepper_Complete"] = "Concluir",

        // DataGrid (caso seja usado depois)
        ["MudDataGrid_Apply"]   = "Aplicar",
        ["MudDataGrid_Cancel"]  = "Cancelar",
        ["MudDataGrid_Clear"]   = "Limpar",

        // Dialog
        ["MudMessageBox_Confirm"] = "Confirmar",
        ["MudMessageBox_Cancel"]  = "Cancelar",
        ["MudMessageBox_No"]      = "Não",
        ["MudMessageBox_Yes"]     = "Sim",
        ["MudMessageBox_Ok"]      = "OK",
    };

    public override LocalizedString this[string key] =>
        _pt.TryGetValue(key, out var value)
            ? new LocalizedString(key, value, resourceNotFound: false)
            : new LocalizedString(key, key, resourceNotFound: true);
}
