using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J113D.Common.Json
{
	/// <summary>
	/// Json reader instance.
	/// </summary>
	public readonly struct JsonObjectReaderInstance
	{
		private readonly Dictionary<string, object?> _values;
		private readonly Dictionary<string, PropertyDefinition> _currentPropertyDefinitions;

		/// <summary>
		/// Current property definitions.
		/// </summary>
		public ReadOnlyDictionary<string, PropertyDefinition> CurrentPropertyDefinitions { get; }

		/// <summary>
		/// Values read so far.
		/// </summary>
		public ReadOnlyDictionary<string, object?> Values { get; }

		/// <summary>
		/// Creates a new reader instance.
		/// </summary>
		/// <param name="propertyDefinitions"></param>
		/// <param name="values">Value storage</param>
		public JsonObjectReaderInstance(ReadOnlyDictionary<string, PropertyDefinition> propertyDefinitions, Dictionary<string, object?> values)
		{
			_currentPropertyDefinitions = [];
			CurrentPropertyDefinitions = new(_currentPropertyDefinitions);
			_values = values;
			Values = new(_values);

			AddPropertyDefinitions(propertyDefinitions);
		}

		/// <summary>
		/// Adds property definitions and their default values.
		/// </summary>
		/// <param name="propertyDefinitions">The property definitions to add</param>
		public readonly void AddPropertyDefinitions(ReadOnlyDictionary<string, PropertyDefinition> propertyDefinitions)
		{
			foreach(KeyValuePair<string, PropertyDefinition> item in propertyDefinitions)
			{
				_currentPropertyDefinitions.Add(item.Key, item.Value);
				_values.Add(item.Key, item.Value.Default);
			}
		}
	}
}
