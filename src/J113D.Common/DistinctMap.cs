using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace J113D.Common
{
	/// <summary>
	/// Base interface for distinct map classes
	/// </summary>
	public interface IDistinctMap
	{
		/// <summary>
		/// Collection of unique values
		/// </summary>
		public IList Values { get; }

		/// <summary>
		/// Index to value mapping. Null if values are already distinct
		/// </summary>
		public int[]? Map { get; }

		/// <summary>
		/// Returns an indexes remapped index.
		/// </summary>
		/// <param name="absoluteIndex">Absolute index</param>
		/// <returns>Mapped index</returns>
		public int this[int absoluteIndex] { get; }

		/// <summary>
		/// Returns an indexes remapped index.
		/// </summary>
		/// <param name="absoluteIndex">Absolute index</param>
		/// <returns>Mapped index</returns>
		public uint this[uint absoluteIndex] { get; }

		/// <summary>
		/// Returns an indexes remapped index.
		/// </summary>
		/// <param name="absoluteIndex">Absolute index</param>
		/// <returns>Mapped index</returns>
		public short this[short absoluteIndex] { get; }

		/// <summary>
		/// Returns an indexes remapped index.
		/// </summary>
		/// <param name="absoluteIndex">Absolute index</param>
		/// <returns>Mapped index</returns>
		public ushort this[ushort absoluteIndex] { get; }

		/// <summary>
		/// Gets a value from an original absolute index
		/// </summary>
		/// <param name="absoluteIndex"></param>
		/// <returns></returns>
		public object? GetValue(int absoluteIndex);
	}

	/// <summary>
	/// A distinct index to value mapping.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class DistinctMap<T> : IDistinctMap
	{
		private readonly IList _interfaceValues;

		IList IDistinctMap.Values => _interfaceValues;

		/// <summary>
		/// Collection of unique values
		/// </summary>
		public IList<T> Values { get; }

		/// <summary>
		/// Values as an array
		/// </summary>
		public T[] ValueArray => Values is T[] array ? array : [.. Values];

		/// <inheritdoc/>
		public int[]? Map { get; }

		/// <inheritdoc/>
		public int this[int absoluteIndex]
			=> Map == null ? absoluteIndex : Map[absoluteIndex];

		/// <inheritdoc/>
		public uint this[uint absoluteIndex]
			=> Map == null ? absoluteIndex : (uint)Map[absoluteIndex];

		/// <inheritdoc/>
		public short this[short absoluteIndex]
			=> Map == null ? absoluteIndex : (short)Map[absoluteIndex];

		/// <inheritdoc/>
		public ushort this[ushort absoluteIndex]
			=> Map == null ? absoluteIndex : (ushort)Map[absoluteIndex];

		/// <inheritdoc/>
		object? IDistinctMap.GetValue(int absoluteIndex)
		{
			return GetValue(absoluteIndex);
		}

		/// <summary>
		/// Gets a value from an original absolute index
		/// </summary>
		/// <param name="absoluteIndex"></param>
		/// <returns></returns>
		public T GetValue(int absoluteIndex)
		{
			return Values[this[absoluteIndex]];
		}


		/// <summary>
		/// Creates a new distinct map
		/// </summary>
		/// <param name="values">Distinct values</param>
		/// <param name="map">Index mapping</param>
		public DistinctMap(IList<T> values, int[]? map)
		{
			Values = values;
			Map = map;
			_interfaceValues = new ReadOnlyCollection<T>(values);
		}

		/// <summary>
		/// Tries to create a distinct mapping for a collection of values.
		/// </summary>
		/// <param name="collection">The collection to create a mapping for.</param>
		/// <param name="comparer">Comparer to use when looking for distinct values.</param>
		/// <param name="map">Resulting mapping.</param>
		/// <returns>Whether not all values were distinct.</returns>
		public static bool TryCreateDistinctMap(IList<T> collection, EqualityComparer<T> comparer, out DistinctMap<T> map)
		{
			int[] resultMap = new int[collection.Count];
			T[] resultDistinct = new T[collection.Count];
			int distinctCount = 0;

			for(int i = 0; i < resultMap.Length; i++)
			{
				T c = collection[i];

				for(int j = 0; j < distinctCount; j++)
				{
					if(comparer.Equals(resultDistinct[j], c))
					{
						resultMap[i] = j;
						goto found;
					}
				}

				resultMap[i] = distinctCount;
				resultDistinct[distinctCount] = c;
				distinctCount++;

				found:
				;
			}

			if(distinctCount == resultMap.Length)
			{
				map = new(collection, null);
				return false;
			}

			T[] distinct = new T[distinctCount];
			Array.Copy(resultDistinct, distinct, distinctCount);
			map = new(distinct, resultMap);

			return true;
		}

		/// <summary>
		/// Tries to create a distinct mapping for a collection of values.
		/// Utilizes a "Sort, Sweep and Prune" algorithm to reduce time spent comparing items
		/// </summary>
		/// <param name="collection">The collection to create a mapping for.</param>
		/// <param name="getSSPValue">Function to retrieve the SSP value.</param>
		/// <param name="sspDelta">The SSP distance after which to stop comparing values.</param>
		/// <param name="comparer">Comparer to use when looking for distinct values.</param>
		/// <param name="map">Resulting mapping.</param>
		/// <returns>Whether not all values were distinct.</returns>
		public static bool TryCreateDistinctMapSSP<S>(IList<T> collection, Func<T, S> getSSPValue, S sspDelta, EqualityComparer<T> comparer, out DistinctMap<T> map) where S : INumber<S>
		{
			int[] resultMap = new int[collection.Count];

			T[] resultDistinct = new T[resultMap.Length];
			S[] resultDistinctSSP = new S[resultMap.Length];
			int distinctCount = 0;

			(int index, S ssp)[] sspLUT = [
				.. collection
				.Select<T, (int index, S ssp)>((x, i) => (i, getSSPValue(x)))
				.OrderBy(x => x.ssp)
			];

			for(int i = 0; i < resultMap.Length; i++)
			{
				(int index, S ssp) = sspLUT[i];
				T c = collection[index];

				for(int j = distinctCount - 1; j >= 0; j--)
				{
					if(ssp - resultDistinctSSP[j] > sspDelta)
					{
						break;
					}

					if(comparer.Equals(resultDistinct[j], c))
					{
						resultMap[index] = j;
						goto found;
					}
				}

				resultMap[index] = distinctCount;
				resultDistinct[distinctCount] = c;
				resultDistinctSSP[distinctCount] = ssp;
				distinctCount++;

				found:
				;
			}

			if(distinctCount == resultMap.Length)
			{
				map = new(collection, null);
				return false;
			}

			T[] distinct = new T[distinctCount];
			Array.Copy(resultDistinct, distinct, distinctCount);
			map = new(distinct, resultMap);

			return true;
		}
	}

	/// <summary>
	/// Extensions variants for the create methods of <see cref="DistinctMap{T}"/>
	/// </summary>
	public static class DistinctMapExtensions
	{
		/// <summary>
		/// Tries to create a distinct mapping for a collection of values.
		/// </summary>
		/// <param name="collection">The collection to create a mapping for.</param>
		/// <param name="comparer">Comparer to use when looking for distinct values.</param>
		/// <param name="map">Resulting mapping.</param>
		/// <returns>Whether not all values were distinct.</returns>
		public static bool TryCreateDistinctMap<T>(this IList<T> collection, EqualityComparer<T> comparer, out DistinctMap<T> map)
		{
			return DistinctMap<T>.TryCreateDistinctMap(collection, comparer, out map);
		}

		/// <summary>
		/// Tries to create a distinct mapping for a collection of values.
		/// </summary>
		/// <param name="collection">The collection to create a mapping for.</param>
		/// <param name="map">Resulting mapping.</param>
		/// <returns>Whether not all values were distinct.</returns>
		public static bool TryCreateDistinctMap<T>(this IList<T> collection, out DistinctMap<T> map) where T : IEquatable<T>
		{
			return TryCreateDistinctMap(collection, EqualityComparer<T>.Default, out map);
		}

		/// <summary>
		/// Creates a distinct map for a collection.
		/// </summary>
		/// <param name="collection">Collection to create the distinct map for</param>
		/// <param name="comparer">Comparer to use when looking for distinct values.</param>
		/// <returns>Created mapping.</returns>
		public static DistinctMap<T> CreateDistinctMap<T>(this IList<T> collection, EqualityComparer<T> comparer)
		{
			TryCreateDistinctMap(collection, comparer, out DistinctMap<T> result);
			return result;
		}

		/// <summary>
		/// Creates a distinct map for a collection.
		/// </summary>
		/// <param name="collection">Collection to create the distinct map for</param>
		/// <returns>Created mapping.</returns>
		public static DistinctMap<T> CreateDistinctMap<T>(this IList<T> collection) where T : IEquatable<T>
		{
			TryCreateDistinctMap(collection, out DistinctMap<T> result);
			return result;
		}


		/// <summary>
		/// Tries to create a distinct mapping for a collection of values.
		/// </summary>
		/// <param name="collection">The collection to create a mapping for.</param>
		/// <param name="getSSPValue">Function to retrieve the SSP value.</param>
		/// <param name="sspDelta">The SSP distance after which to stop comparing values.</param>
		/// <param name="comparer">Comparer to use when looking for distinct values.</param>
		/// <param name="map">Resulting mapping.</param>
		/// <returns>Whether not all values were distinct.</returns>
		public static bool TryCreateDistinctMapSSP<T, S>(this IList<T> collection, Func<T, S> getSSPValue, S sspDelta, EqualityComparer<T> comparer, out DistinctMap<T> map) where S : INumber<S>
		{
			return DistinctMap<T>.TryCreateDistinctMapSSP(collection, getSSPValue, sspDelta, comparer, out map);
		}

		/// <summary>
		/// Tries to create a distinct mapping for a collection of values.
		/// </summary>
		/// <param name="collection">The collection to create a mapping for.</param>
		/// <param name="getSSPValue">Function to retrieve the SSP value.</param>
		/// <param name="sspDelta">The SSP distance after which to stop comparing values.</param>
		/// <param name="map">Resulting mapping.</param>
		/// <returns>Whether not all values were distinct.</returns>
		public static bool TryCreateDistinctMapSSP<T, S>(this IList<T> collection, Func<T, S> getSSPValue, S sspDelta, out DistinctMap<T> map) where T : IEquatable<T> where S : INumber<S>
		{
			return TryCreateDistinctMapSSP(collection, getSSPValue, sspDelta, EqualityComparer<T>.Default, out map);
		}

		/// <summary>
		/// Creates a distinct map for a collection.
		/// </summary>
		/// <param name="collection">Collection to create the distinct map for</param>
		/// <param name="getSSPValue">Function to retrieve the SSP value.</param>
		/// <param name="sspDelta">The SSP distance after which to stop comparing values.</param>
		/// <param name="comparer">Comparer to use when looking for distinct values.</param>
		/// <returns>Created mapping.</returns>
		public static DistinctMap<T> CreateDistinctMapSSP<T, S>(this IList<T> collection, Func<T, S> getSSPValue, S sspDelta, EqualityComparer<T> comparer) where S : INumber<S>
		{
			TryCreateDistinctMapSSP(collection, getSSPValue, sspDelta, comparer, out DistinctMap<T> result);
			return result;
		}

		/// <summary>
		/// Creates a distinct map for a collection.
		/// </summary>
		/// <param name="collection">Collection to create the distinct map for</param>
		/// <param name="getSSPValue">Function to retrieve the SSP value.</param>
		/// <param name="sspDelta">The SSP distance after which to stop comparing values.</param>
		/// <returns>Created mapping.</returns>
		public static DistinctMap<T> CreateDistinctMapSSP<T, S>(this IList<T> collection, Func<T, S> getSSPValue, S sspDelta) where T : IEquatable<T> where S : INumber<S>
		{
			TryCreateDistinctMapSSP(collection, getSSPValue, sspDelta, out DistinctMap<T> result);
			return result;
		}
	}
}
