namespace Zad2.Core.DTOs;

public record TopPairDto(
    CharacterBaseDto Character1,
    CharacterBaseDto Character2,
    int Episodes
);