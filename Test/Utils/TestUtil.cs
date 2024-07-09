using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Test.Utils
{
	public class TestUtil
	{
		/// <summary>
		/// コンテナから特定のIDのオブジェクトを取得する
		/// </summary>
		/// <typeparam name="T">Dataクラス</typeparam>
		/// <param name="container">DBコンテナ</param>
		/// <param name="objectId">オブジェクトのID</param>
		/// <returns>DBから取得したデータオブジェクト</returns>
		public static async Task<T> GetObjectByIdAsync<T>(Container container, string objectId) where T : class {
			var parameter = Expression.Parameter(typeof(T), "object");
			var property = Expression.Property(parameter, "Id");
			var constant = Expression.Constant(objectId);
			var equality = Expression.Equal(property, constant);
			var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
			var queryable = container.GetItemLinqQueryable<T>().Where(lambda);
			var iterator = queryable.ToFeedIterator();
			while (iterator.HasMoreResults) {
				var response = await iterator.ReadNextAsync();
				if (response.Any()) {
					return response.FirstOrDefault();
				}
			}
			return null;
		}
	}
}
