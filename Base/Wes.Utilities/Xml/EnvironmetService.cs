using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Wes.Desktop.Windows.ViewModel;

namespace Wes.Utilities.Xml
{
    public class EnvironmetService
    {
        private static readonly string _runFile = Path.Combine(AppPath.DataPath, "wes.db");

        #region Xml
        private static string GetFile(string filename)
        {
            string file = string.Empty;
            if (String.IsNullOrWhiteSpace(filename))
            {
                file = _runFile;
            }
            else
            {
                file = filename;
            }
            return file;
        }

        private static XDocument Load(string filename)
        {
            string file = GetFile(filename);

            if (File.Exists(file))
            {
                try
                {
                    return XDocument.Load(file);
                }
                catch (System.Exception e)
                {
                    LoggingService.Error(e);
                }
                return Create();
            }
            else
                return Create();
        }

        private static XDocument Create()
        {
            return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("WES"));
        }

        private static void AddXElement(XElement ele, string name, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var elements = ele.Elements().Where(e => e.Name == name);
            if (elements.Count() > 0)
            {
                foreach (var item in elements)
                    item.Remove();
            }
            XElement element = new XElement(name);
            element.Value = value;
            ele.Add(element);
        }

        private static void AppendElement(XElement ele, string name, XmlDataViewModel xmlDataViewModel)
        {
            if (xmlDataViewModel == null || string.IsNullOrEmpty(xmlDataViewModel.Value)) return;

            XElement element = new XElement(name);
            element.Value = xmlDataViewModel.Value;
            element.SetAttributeValue("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
            element.SetAttributeValue("flowID", xmlDataViewModel.FlowID);
            ele.Add(element);
        }

        private static void AddXElement(XElement ele, string element, string name, string value, bool isAppend = false, XmlDataViewModel historyViewModel = null)
        {
            var elements = ele.Elements().Where(e => e.Name == element);
            XElement child = null;
            if (elements.Count() > 0)
            {
                child = elements.FirstOrDefault();
            }
            else
            {
                child = new XElement(element);
                ele.Add(child);
            }

            if (isAppend)
                AppendElement(child, name, historyViewModel);
            else
                AddXElement(child, name, value);
        }

        private static string GetElementValue(XElement ele, string element, string name)
        {
            var nodes = ele.Elements(element);
            if (nodes.Count() > 0)
            {
                var children = nodes.FirstOrDefault().Elements(name);
                if (children.Count() > 0)
                    return children.FirstOrDefault().Value;
            }
            return null;
        }

        private static List<string> GetElementValues(XElement ele, string element)
        {
            List<string> values = new List<string>();
            var nodes = ele.Elements().Where(e => e.Name.ToString().Contains(element));
            if (nodes.Count() > 0)
            {
                var children = nodes.FirstOrDefault().Elements();
                if (children.Count() > 0)
                {
                    foreach (var item in children)
                    {
                        values.Add(item.Value);
                    }
                }
            }
            return values;
        }
        private static List<XmlDataViewModel> GetElementEntityValues(XElement ele, string element)
        {
            List<XmlDataViewModel> values = new List<XmlDataViewModel>();
            var nodes = ele.Elements().Where(e => e.Name.ToString().Contains(element));
            if (nodes.Count() > 0)
            {
                var children = nodes.FirstOrDefault().Elements();
                if (children.Count() > 0)
                {
                    foreach (var item in children)
                    {
                        XmlDataViewModel historyViewModel = new XmlDataViewModel();
                        historyViewModel.Value = item.Value;
                        historyViewModel.Time = item.Attribute("time").Value;
                        historyViewModel.FlowID = item.Attribute("flowID") == null ? "" : item.Attribute("flowID").Value;
                        values.Add(historyViewModel);
                    }
                }
            }
            return values;
        }

        private static Dictionary<string, object> GetElementKeyValues(XElement ele, string element)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            var nodes = ele.Elements().Where(e => e.Name.ToString().Contains(element));
            if (nodes.Count() > 0)
            {
                var children = nodes.FirstOrDefault().Elements();
                if (children.Count() > 0)
                {
                    foreach (var item in children)
                        keyValues[item.Name.ToString()] = item.Value;
                }
            }
            return keyValues;
        }

        private static Dictionary<string, Dictionary<string, object>> GetElements(XElement ele, string element)
        {
            Dictionary<string, Dictionary<string, object>> elements = new Dictionary<string, Dictionary<string, object>>();
            var nodes = ele.Elements().Where(e => e.Name.ToString().Contains(element));
            if (nodes.Count() > 0)
            {
                foreach (var item in nodes)
                {
                    Dictionary<string, object> keyValues = new Dictionary<string, object>();
                    foreach (var children in item.Elements())
                    {
                        keyValues[children.Name.ToString()] = children.Value;
                    }
                    elements[item.Name.ToString()] = keyValues;
                }
            }
            return elements;
        }

        private static void DeleteElements(XElement ele, string element)
        {
            var nodes = ele.Elements().Where(e => e.Name.ToString().Contains(element));
            if (nodes.Count() > 0)
            {
                for (int i = nodes.Count() - 1; i >= 0; i--)
                {
                    nodes.ElementAt(i).Remove();
                }
            }
        }

        #endregion

        #region Public

        public static bool AddValue(string element, string name, string value, string filename = null)
        {
            try
            {
                string file = GetFile(filename);
                XDocument doc = Load(file);
                AddXElement(doc.Root, element, name, value);
                doc.Save(file);
                return true;
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }

        public static bool AddKeyValues(string element, Dictionary<string, object> keyValues, string filename = null)
        {
            try
            {
                if (keyValues == null) return true;
                string file = GetFile(filename);
                XDocument doc = Load(file);
                foreach (var item in keyValues)
                {
                    AddXElement(doc.Root, element, item.Key, item.Value.ToString());
                }
                doc.Save(file);
                return true;
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }

        public static bool RemoveFirstValue(string element, string name, string filename)
        {
            try
            {
                string file = GetFile(filename);
                XDocument doc = Load(file);
                var elements = doc.Root.Elements().Where(e => e.Name == element);
                elements.First().Elements().First().Remove();
                doc.Save(file);
                return true;
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }


        public static bool AppendValue(string element, string name, XmlDataViewModel xmlDataViewModel, string filename = null)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlDataViewModel.Value)) return true;
                string file = GetFile(filename);
                XDocument doc = Load(file);
                AddXElement(doc.Root, element, name, null, true, xmlDataViewModel);
                doc.Save(file);
                return true;
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }

        public static bool AddValues(string element, string name, List<string> values, string filename = null)
        {
            try
            {
                if (values == null) return true;
                string file = GetFile(filename);
                XDocument doc = Load(file);
                var elements = doc.Root.Elements().Where(e => e.Name == element);
                if (elements.Count() > 0) elements.FirstOrDefault().RemoveAll();
                foreach (var item in values)
                {
                    AddXElement(doc.Root, element, name, item, true);
                }
                doc.Save(file);
                return true;
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }

        public static void AddWesValue(string name, string value, string filename = null)
        {
            AddValue("Environment", name, value, filename);
        }

        public static string GetWesValue(string name, string filename = null)
        {
            return GetValue("Environment", name, filename);
        }

        public static string GetValue(string element, string name, string filename = null, string defaultvalue = null)
        {
            XDocument doc = Load(filename);
            string value = GetElementValue(doc.Root, element, name);
            if (string.IsNullOrEmpty(value))
            {
                value = defaultvalue;
            }
            return value;
        }

        public static int GetIntValue(string element, string name, string filename = null)
        {
            string value = GetValue(element, name, filename);
            if (!string.IsNullOrEmpty(value))
            {
                int intValue = 0;
                if (Int32.TryParse(value, out intValue))
                    return intValue;
            }
            return 0;
        }

        public static bool GetBooleanValue(string element, string name, string filename = null)
        {
            string value = GetValue(element, name, filename);
            if (!string.IsNullOrEmpty(value))
            {
                bool boolValue = false;
                if (Boolean.TryParse(value, out boolValue))
                    return boolValue;
            }
            return false;
        }

        public static List<string> GetValues(string element, string filename = null)
        {
            XDocument doc = Load(filename);
            return GetElementValues(doc.Root, element);
        }

        public static List<XmlDataViewModel> GetEntityValues(string element, string filename = null)
        {
            XDocument doc = Load(filename);
            return GetElementEntityValues(doc.Root, element);
        }

        public static Dictionary<string, object> GetKeyValues(string element, string filename = null)
        {
            XDocument doc = Load(filename);
            return GetElementKeyValues(doc.Root, element);
        }

        public static Dictionary<string, Dictionary<string, object>> GetElements(string element, string filename = null)
        {
            XDocument doc = Load(filename);
            return GetElements(doc.Root, element);
        }

        public static void DeleteElement(string element, string filename = null)
        {
            try
            {
                string file = GetFile(filename);
                XDocument doc = Load(file);
                DeleteElements(doc.Root, element);
                doc.Save(file);
            }
            catch (System.Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        #endregion
    }
}
