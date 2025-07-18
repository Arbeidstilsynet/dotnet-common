namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

public class MappedQueryParameterAttribute : Attribute
{
    public string QueryParameterName { get; set; }
}

public class MappedRequestHeaderParameterAttribute : Attribute
{
    public string HeaderParameterName { get; set; }
}
