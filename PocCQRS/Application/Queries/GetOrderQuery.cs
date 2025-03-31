using MediatR;
using PocCQRS.Domain.Entities;

namespace PocCQRS.Application.Queries;

public static class GetOrderQuery
{
    public record Query(Guid OrderId);
    
    public record Response(Guid Id, string ProductName, int Quantity);
}