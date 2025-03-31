namespace PocCQRS.Application.Commands;

public class CreateOrder
{
    public record Command(string ProductName, int Quantity);
}