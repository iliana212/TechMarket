namespace TechMarket_Productos.Auth
{
	public record CredencialesDTO(string Usuario, string Password);
	public record TokenDTO(string AccessToken, string Rol, DateTime ExpiraUtc);
	
	public static class AuthEndpoints
	{
		private static readonly Dictionary<string, (string Password, string Rol)> Usuarios = new()
		{
			["admin"] = ("admin123", "Admin"),
			["vendedor"] = ("vendedor123", "User")
		};

		public static void MapAuthEndpoints(this WebApplication app)
		{
			app.MapPost("/api/auth/login", (CredencialesDTO credenciales, IEmisorTokenJWT emisor, IConfiguration config) =>
			{
				if (!Usuarios.TryGetValue(credenciales.Usuario, out var cuenta) || cuenta.Password != credenciales.Password)
					return Results.Unauthorized();

				var token = emisor.GenerarToken(credenciales.Usuario, cuenta.Rol);
				var minutos = int.Parse(config["Jwt:ExpiraMinutos"] ?? "60");

				return Results.Ok(new TokenDTO(token, cuenta.Rol, DateTime.UtcNow.AddMinutes(minutos)));
			})
			.WithTags("Auth")
			.WithName("Login")
			.WithSummary("Autentica y devuelve JWT. Usuario prueba: admin/admin123/vendedor/vendedor123")
			.AllowAnonymous();
		}
	}
}
