using System.Collections;
using System.Collections.Generic;

namespace J113D.Common
{
	/// <summary>
	/// Readonly set wrapper
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReadOnlySet<T> : IReadOnlySet<T>
	{
		private readonly IReadOnlySet<T> _set;

		/// <inheritdoc/>
		public int Count => _set.Count;

		/// <summary>
		/// Creates a new readonly set wrapper
		/// </summary>
		/// <param name="set">Set to wrap around</param>
		public ReadOnlySet(IReadOnlySet<T> set)
		{
			_set = set;
		}

		/// <inheritdoc/>
		public bool Contains(T item)
		{
			return _set.Contains(item);
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return _set.GetEnumerator();
		}

		/// <inheritdoc/>
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			return _set.IsProperSubsetOf(other);
		}

		/// <inheritdoc/>
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			return _set.IsProperSupersetOf(other);
		}

		/// <inheritdoc/>
		public bool IsSubsetOf(IEnumerable<T> other)
		{
			return _set.IsSubsetOf(other);
		}

		/// <inheritdoc/>
		public bool IsSupersetOf(IEnumerable<T> other)
		{
			return _set.IsSupersetOf(other);
		}

		/// <inheritdoc/>
		public bool Overlaps(IEnumerable<T> other)
		{
			return _set.Overlaps(other);
		}

		/// <inheritdoc/>
		public bool SetEquals(IEnumerable<T> other)
		{
			return _set.SetEquals(other);
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
