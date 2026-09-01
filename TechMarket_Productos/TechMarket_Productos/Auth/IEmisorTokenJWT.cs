namespace TechMarket_Productos.Auth
{
	public interface IEmisorTokenJWT
	{
		string GenerarToken(string usuario, string rol);
	}
}
