namespace Zenatur.Tms.Application.LegacyBridge;

/// <summary>
/// De/Para TipoVeiculo legado (Bridge) → CategoriaVeiculo Pamcard.
/// Apenas mapeamentos de alta confiança e sem ambiguidade.
/// Códigos ausentes ficam para o usuário escolher manualmente.
/// </summary>
public static class TipoVeiculoLegadoMapper
{
    private static readonly IReadOnlyDictionary<int, string> _map = new Dictionary<int, string>
    {
        [5]  = "1", // Fiorino                  → Automóvel/camionete/furgão (2 eixos rod. simples)
        [6]  = "4", // TRUCK                    → Caminhão (3 eixos rod. dupla)
        [8]  = "2", // TOCO                     → Caminhão leve (2 eixos rod. dupla)
        [12] = "1", // VAN                      → Automóvel/camionete/furgão (2 eixos rod. simples)
        [13] = "9", // Moto                     → Motocicleta
        [14] = "2", // CAMINHÃO 3/4             → Caminhão leve (2 eixos rod. dupla)
        [20] = "2", // CAM 3/4                  → Caminhão leve (2 eixos rod. dupla)
        [23] = "1", // Fiorino / Veículo passeio → Automóvel/camionete/furgão (2 eixos rod. simples)
        [33] = "1", // FIORINO/DOBLO            → Automóvel/camionete/furgão (2 eixos rod. simples)
    };

    public static string? MapearParaPamcard(int legadoId)
        => _map.TryGetValue(legadoId, out var pamcardId) ? pamcardId : null;
}
