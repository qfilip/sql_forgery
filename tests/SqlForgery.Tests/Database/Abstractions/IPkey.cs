namespace SqlForgery.Tests.Database.Abstractions;

internal interface IPkey<T> where T : struct
{
    public T Id { get; set; }
}
