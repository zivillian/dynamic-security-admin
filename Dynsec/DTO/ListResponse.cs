namespace Dynsec.DTO;

public record ListResponse<T>
{
    public T[] Items { get; init; }

    public int Total { get; init; }
}