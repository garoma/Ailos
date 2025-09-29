// Application/Requests/LoginRequest.cs
namespace Application.Requests;

public class LoginRequest
{
    public string Cpf { get; set; }
    public string Senha { get; set; }
}
