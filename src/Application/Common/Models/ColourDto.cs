using FHIRAI.Domain.ValueObjects;

namespace FHIRAI.Application.Common.Models;

public class ColourDto
{
    public string Code { get; init; } = string.Empty;

    public static implicit operator Colour(ColourDto dto)
    {
        return Colour.From(dto.Code);
    }

    public static implicit operator ColourDto(Colour colour)
    {
        return new ColourDto { Code = colour.Code };
    }
}
