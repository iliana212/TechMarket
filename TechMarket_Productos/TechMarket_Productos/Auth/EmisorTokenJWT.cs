using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace TechMarket_Productos.Auth
{
	public class EmisorTokenJWT(IConfiguration _configuration) : IEmisorTokenJWT
	{
		public string GenerarToken(string usuario, string rol)
		{
			var claves = _configuration.GetSection("Jwt");
			var claveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claves["Key"]!));
			var credenciales = new SigningCredentials(claveSimetrica, SecurityAlgorithms.HmacSha256);
			var claims = new List<Claim> {
				new(JwtRegisteredClaimNames.Sub, usuario),
				new(ClaimTypes.Role, rol),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var minutos = int.Parse(claves["ExpiraMinutos"] ?? "60");

			var token = new JwtSecurityToken(
				issuer: claves["Issuer"],
				audience: claves["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(minutos),
				signingCredentials: credenciales);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
