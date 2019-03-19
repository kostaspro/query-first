using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TinyIoC;

namespace QueryFirst
{
    public class ConfigFileReader : IConfigFileReader
    {
        /// <summary>
        /// Returns the string contents of the first qfconfig.json file found,
        /// starting in the directory of the path supplied and going up.
        /// </summary>
        /// <param name="filePath">Full path name of the query file</param>
        /// <returns></returns>
        public string GetConfigFile(string filePath)
        {
            filePath = Path.GetDirectoryName(filePath);
            while (filePath != null)
            {
                if (File.Exists(filePath + "\\qfconfig.json"))
                {
                    return File.ReadAllText(filePath + "\\qfconfig.json");
                }
                filePath = Directory.GetParent(filePath)?.FullName;
            }
            return null;
        }
    }
    public class ConfigResolver : IConfigResolver
    {
        private IConfigFileReader _configFileReader;
        public ConfigResolver(IConfigFileReader configFileReader)
        {
            _configFileReader = configFileReader;
        }
        /// <summary>
        /// Returns the QueryFirst config for a given query file. Values specified directly in 
        /// the query file will trump values specified in the qfconfig.json file.
        /// We look for a qfconfig.json file beside the query file. If none found, we look in the parent directory,
        /// and so on up to the root directory. 
        /// 
        /// If the query specifies a QfDefaultConnection but no QfDefaultConnectionProviderName, "System.Data.SqlClient"
        /// will be assumed.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="queryText"></param>
        /// <returns></returns>
        
        public IQFConfigModel GetConfig(string filePath, string queryText)
        {
            IQFConfigModel config = TinyIoCContainer.Current.Resolve<IQFConfigModel>();
            var configFileContents = _configFileReader.GetConfigFile(filePath);
            if (!string.IsNullOrEmpty(configFileContents))
            {
                config = (IQFConfigModel)JsonConvert.DeserializeObject(configFileContents,config.GetType());
                if (string.IsNullOrEmpty(config.Provider))
                {
                    config.Provider = "System.Data.SqlClient";
                }
            }
            // if the query defines a QfDefaultConnection, use it.
            var match = Regex.Match(queryText, "^--QfDefaultConnection(=|:)(?<cstr>[^\r\n]*)", RegexOptions.Multiline);
            if (match.Success)
            {
                config.DefaultConnection = match.Groups["cstr"].Value;
                var matchProviderName = Regex.Match(queryText, "^--QfDefaultConnectionProviderName(=|:)(?<pn>[^\r\n]*)", RegexOptions.Multiline);
                if (matchProviderName.Success)
                {
                    config.Provider = matchProviderName.Groups["pn"].Value;
                }
                else
                {
                    config.Provider = "System.Data.SqlClient";
                }

            }
            return config;
        }
    }

    public interface IQFConfigModel
    {
        string DefaultConnection { get; set; }
        string Provider { get; set; }
        string HelperAssembly { get; set; }
        bool MakeSelfTest { get; set; }
    }
    public class QFConfigModel : IQFConfigModel
    {
        public string DefaultConnection { get; set; }
        public string Provider { get; set; }
        public string HelperAssembly { get; set; }
        public bool MakeSelfTest { get; set; }
    }
}
