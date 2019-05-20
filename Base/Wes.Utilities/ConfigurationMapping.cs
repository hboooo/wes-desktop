using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Wes.Utilities.Exception;

namespace Wes.Utilities
{
    public class ConfigurationMapping
    {
        public static readonly ConfigurationMapping Instance = new ConfigurationMapping();

        private dynamic _conf;
        public dynamic GlobalConf
        {
            get { return _conf; }
        }

        private ConfigurationMapping()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            try
            {
                string filename = Path.Combine(AppPath.BasePath, "Wes.Desktop.addin");
                if (!File.Exists(filename)) return;

                var data = DynamicJson.SerializeXNode(XDocument.Load(filename).Root, true);
                _conf = DynamicJson.DeserializeObject<object>(data);
            }
            catch (System.Exception ex)
            {
                LoggingService.Fatal("Initialize addin mapping error", ex);
                throw new WesException("加载AddIn[映射]失败,请联系管理员");
            }
        }

        #region addIn

        public dynamic GetAddInByIndex(int index)
        {
            dynamic addIn = _conf.addin;
            if (addIn == null || addIn.mapping == null || addIn.mapping.Count == 0)
                throw new WesException("加载AddIn[映射]失败,请联系管理员");
            int i = 0;
            foreach (var item in addIn.mapping)
            {
                if (i == index) return item;
                i++;
            }
            return null;
        }

        public dynamic GetAddInByName(string name)
        {
            dynamic addIn = _conf.addin;
            if (addIn == null || addIn.mapping == null || addIn.mapping.Count == 0)
                throw new WesException("加载AddIn[映射]失败,请联系管理员");

            foreach (var item in addIn.mapping)
            {
                if (string.Compare(item.name.ToString(), name, true) == 0)
                    return item;
            }
            return null;
        }

        public dynamic GetAddInByCode(string code)
        {
            dynamic addIn = _conf.addin;
            if (addIn == null || addIn.mapping == null || addIn.mapping.Count == 0)
                throw new WesException("加载AddIn[映射]失败,请联系管理员");

            foreach (var item in addIn.mapping)
            {
                if (item.endCustomer != null)
                {
                    bool isJArray = DynamicJson.IsJArray(item.endCustomer.code);
                    if (isJArray)
                    {
                        foreach (var c in item.endCustomer.code)
                        {
                            if (string.Compare(c.ToString(), code, true) == 0)
                                return item;
                        }
                    }
                    else
                    {
                        if (string.Compare(item.endCustomer.code.ToString(), code, true) == 0)
                            return item;
                    }
                }
            }
            return null;
        }

        public string ConvertToAddInCode(dynamic value)
        {
            string code = string.Empty;
            bool isJArray = DynamicJson.IsJArray(value);
            if (isJArray)
            {
                foreach (var item in value)
                {
                    if (!string.IsNullOrEmpty(item.ToString()))
                    {
                        code = item.ToString();
                        break;
                    }
                }
            }
            else
            {
                code = value.ToString();
            }
            return code;
        }

        #endregion

        #region shortcut
        private Dictionary<int, string> _shortcut;
        public Dictionary<int, string> Shortcut
        {
            get
            {
                if (_shortcut == null) BuildShortcut();
                return _shortcut;
            }
        }

        private void BuildShortcut()
        {
            if (_shortcut == null)
                _shortcut = new Dictionary<int, string>();

            dynamic shortcut = _conf.shortcut;
            foreach (var item in shortcut.keyboard)
            {
                _shortcut[Convert.ToInt32(item.key)] = item.command.ToString();
            }
        }
        #endregion
    }
}
