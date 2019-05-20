using System.Collections.Generic;
using System.Reflection;

namespace Wes.Flow
{
    public class WesFlow
    {
        private FieldInfo[] _fieldInfos = null;

        private static WesFlow _wesFlow;

        private static readonly object _locker = new object();

        public static WesFlow Instance
        {
            get
            {
                if (_wesFlow == null)
                {
                    lock (_locker)
                    {
                        if (_wesFlow == null)
                        {
                            _wesFlow = new WesFlow();
                        }
                    }
                }
                return _wesFlow;
            }
        }

        private WesFlow()
        {
            InitFields();
        }

        private void InitFields()
        {
            var wesFlow = typeof(WesFlowID);
            _fieldInfos = wesFlow.GetFields();
        }

        /// <summary>
        /// 獲取指定類型的所有流程
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public Dictionary<int, string> GetFlows(int mask)
        {
            Dictionary<int, string> flowItems = new Dictionary<int, string>();
            foreach (var fieldInfo in _fieldInfos)
            {
                int value = (int)fieldInfo.GetValue(null);
                if (value > 0 && (value & mask) == mask && value != mask)
                {
                    flowItems[value] = fieldInfo.Name;
                }
            }
            return flowItems;
        }

        /// <summary>
        /// 根據流程id獲取名稱
        /// </summary>
        /// <param name="flowId"></param>
        /// <returns></returns>
        public string GetFlowName(int flowId)
        {
            foreach (var fieldInfo in _fieldInfos)
            {
                if (fieldInfo.FieldType == typeof(int)) continue;
                int value = (int)fieldInfo.GetValue(null);
                if (flowId == value)
                {
                    return fieldInfo.Name;
                }
            }
            return null;
        }

        /// <summary>
        /// 根據流程名稱獲取id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetFlowID(string name)
        {
            foreach (var fieldInfo in _fieldInfos)
            {
                if (fieldInfo.Name == name)
                {
                    return (int)fieldInfo.GetValue(null);
                }
            }
            return (int)WesFlowID.UNKNOW;
        }

        /// <summary>
        /// 根據流程名稱獲取id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public WesFlowID GetFlow(string name)
        {
            foreach (var fieldInfo in _fieldInfos)
            {
                if (fieldInfo.Name == name)
                {
                    return (WesFlowID)fieldInfo.GetValue(null);
                }
            }
            return WesFlowID.UNKNOW;
        }

        /// <summary>
        /// 根據流程名稱獲取id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public WesFlowID GetFlow(int flowId)
        {
            foreach (var fieldInfo in _fieldInfos)
            {
                if (fieldInfo.FieldType == typeof(int)) continue;
                int value = (int)fieldInfo.GetValue(null);
                if (flowId == value)
                {
                    return (WesFlowID)fieldInfo.GetValue(null);
                }
            }
            return WesFlowID.UNKNOW;
        }

        /// <summary>
        /// 獲取流程類型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetFlowMask(string name)
        {
            return GetFlowMask(GetFlowID(name));
        }

        /// <summary>
        /// 獲取流程類型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetFlowMask(int flowId)
        {
            return flowId & 0xFFF0000;
        }
    }
}

