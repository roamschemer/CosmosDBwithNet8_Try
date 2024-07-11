using Microsoft.Azure.Cosmos;

namespace Api.Utils
{
	public interface ICompanyContainer
	{
		Container Container { get; set; }
	}

	public class CosmosContainerWrapper(Container container) : ICompanyContainer
	{
		public Container Container { get; set; } = container;

	}
}
