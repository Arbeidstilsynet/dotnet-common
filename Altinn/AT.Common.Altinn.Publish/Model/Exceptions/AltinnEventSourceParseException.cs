namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

public class AltinnEventSourceParseException(string message, Exception innerException)
    : Exception(message, innerException) { }
