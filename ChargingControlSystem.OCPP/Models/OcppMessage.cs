namespace ChargingControlSystem.OCPP.Models;

/// <summary>
/// OCPP Message Type Identifier
/// [MessageTypeId, UniqueId, Action, Payload]
/// </summary>
public enum MessageType
{
    CALL = 2,        // Request
    CALLRESULT = 3,  // Response
    CALLERROR = 4    // Error
}

/// <summary>
/// Base class for OCPP messages
/// </summary>
public class OcppMessage
{
    public MessageType MessageType { get; set; }
    public string UniqueId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public object? Payload { get; set; }
}

/// <summary>
/// OCPP Error codes
/// </summary>
public class OcppError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorDescription { get; set; } = string.Empty;
    public object? ErrorDetails { get; set; }
}

/// <summary>
/// Standard OCPP Error Codes
/// </summary>
public static class OcppErrorCode
{
    public const string NotImplemented = "NotImplemented";
    public const string NotSupported = "NotSupported";
    public const string InternalError = "InternalError";
    public const string ProtocolError = "ProtocolError";
    public const string SecurityError = "SecurityError";
    public const string FormationViolation = "FormationViolation";
    public const string PropertyConstraintViolation = "PropertyConstraintViolation";
    public const string OccurenceConstraintViolation = "OccurenceConstraintViolation";
    public const string TypeConstraintViolation = "TypeConstraintViolation";
    public const string GenericError = "GenericError";
}


