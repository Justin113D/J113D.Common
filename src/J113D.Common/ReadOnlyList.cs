using System.Collections;
using System.Collections.Generic;

namespace J113D.Common
{
	/// <summary>
	/// Readonly list wrapper
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReadOnlyList<T> : IReadOnlyList<T>
	{
		private readonly IReadOnlyList<T> _list;

		/// <inheritdoc/>
		public T this[int index] => _list[index];

		/// <inheritdoc/>
		public int Count => _list.Count;

		/// <summary>
		/// Creates a new readonly list wrapper
		/// </summary>
		/// <param name="list">List to wrap around</param>
		public ReadOnlyList(IReadOnlyList<T> list)
		{
			_list = list;
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}
		/// <inheritdoc/>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
