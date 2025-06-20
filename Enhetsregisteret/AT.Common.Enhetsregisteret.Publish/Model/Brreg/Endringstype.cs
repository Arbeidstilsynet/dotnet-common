namespace Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

/// <summary>
/// Hvilken type endring det gjelder.
/// </summary>
public enum Endringstype
{
    /// <summary>
    /// Ukjent type endring. Ofte fordi endringen har skjedd før endringstype ble innført.
    /// </summary>
    Ukjent,

    /// <summary>
    /// Enheten har blitt lagt til i Enhetsregisteret
    /// </summary>
    Ny,

    /// <summary>
    /// Enheten har blitt endret i Enhetsregisteret
    /// </summary>
    Endring,

    /// <summary>
    /// Enheten har blitt slettet fra Enhetsregisteret
    /// </summary>
    Sletting,

    /// <summary>
    /// Enheten har blitt fjernet fra Åpne Data. Eventuelle kopier skal også fjerne enheten.
    /// </summary>
    Fjernet,
}
