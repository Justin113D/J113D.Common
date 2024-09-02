using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J113D.Common
{
    public abstract class BaseSettings
    {
        private const string _filename = "settings.json";
        private const string _windowsPath = "%LOCALAPPDATA%\\{0}\\{1}\\" + _filename;
        private const string _macosPath = "~/Library/Application Support/{0}/{1}/" + _filename;
        private const string _linuxPath = "$HOME/.config/{0}/{1}/" + _filename;

        private readonly string _filePath;
        private readonly Dictionary<string, object> _values;
        private static readonly JsonSerializerOptions _serializerOptions;

        static BaseSettings()
        {
            _serializerOptions = new()
            {
                WriteIndented = true
            };

            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        protected object this[string name]
        {
            get => _values[name];
            set => _values[name] = value;
        }

        protected BaseSettings()
        {
            _values = [];

            string filepath;
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filepath = _windowsPath;
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                filepath = _linuxPath;
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                filepath = _macosPath;
            }
            else
            {
                filepath = ".";
            }

            Assembly assembly = Assembly.GetEntryAssembly()!;
            AssemblyCompanyAttribute company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()!;
            AssemblyProductAttribute product = assembly.GetCustomAttribute<AssemblyProductAttribute>()!;

            filepath = Environment.ExpandEnvironmentVariables(filepath);
            filepath = string.Format(filepath, company.Company, product.Product);
            _filePath = Path.GetFullPath(filepath);
        }

        public abstract void Reset();

        protected abstract object ConvertValue(string name, JsonElement value);

        public void Save()
        {
            string json = JsonSerializer.Serialize(_values, _serializerOptions);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }

        public void Load()
        {
            Reset();

            if(!File.Exists(_filePath))
            {
                return;
            }

            string json = File.ReadAllText(_filePath);
            Dictionary<string, object> values;

            try
            {
                values = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
            }
            catch
            {
                return;
            }

            foreach(KeyValuePair<string, object> pair in values)
            {
                object convertedValue;

                try
                {
                    convertedValue = ConvertValue(pair.Key, (JsonElement)pair.Value);
                }
                catch(InvalidDataException)
                {
                    continue;
                }

                _values[pair.Key] = convertedValue;
            }
        }
    }
}
