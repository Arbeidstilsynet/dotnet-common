namespace Arbeidstilsynet.Common.Altinn.Model.Exceptions;

/// <summary>
/// The exception thrown when an Altinn event source cannot be parsed.
/// </summary>
/// <param name="message">The message that describes the error.</param>
/// <param name="innerException">The exception that caused the parsing failure.</param>
public class AltinnEventSourceParseException(string message, Exception innerException)
    : Exception(message, innerException) { }
