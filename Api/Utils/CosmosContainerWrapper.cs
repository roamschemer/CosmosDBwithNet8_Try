using Microsoft.Azure.Cosmos;

namespace Api.Utils
{
	public interface ICompanyContainer
	{
		Container Container { get; set; }
	}

	// public interface I〇〇Container ← コンテナーが増えるたびにこれを書くのかよ・・・
	// {
	// 	Container Container { get; set; }
	// }

	public class CosmosContainerWrapper(Container container) : ICompanyContainer //,I〇〇Container これも・・・
	{
		public Container Container { get; set; } = container;

	}
}