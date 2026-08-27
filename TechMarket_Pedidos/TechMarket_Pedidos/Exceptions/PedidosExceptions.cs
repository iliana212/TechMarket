namespace TechMarket_Pedidos.Exceptions
{
	public class RecursoNoEncontradoException : Exception
	{
		public RecursoNoEncontradoException(string mensaje):base(mensaje){ }

	}

	public class StockInsuficienteException: Exception
	{
		public StockInsuficienteException(string mensaje) : base(mensaje) { }
	}

	public class ServiceProductosNoDisponibleException : Exception
	{
		public ServiceProductosNoDisponibleException(string mensaje, Exception? inner = null):base(mensaje, inner){ }
}
}
