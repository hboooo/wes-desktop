using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Windows.Data;

namespace Wes.Utilities
{
    public class GetDynamicMemberBinder : GetMemberBinder
    {
        public GetDynamicMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase)
        {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            return target;
        }
    }

    public class SetDynamicMemberBinder : SetMemberBinder
    {
        public SetDynamicMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase)
        {

        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            return target;
        }
    }

    public class DynamicObjectClass : DynamicObject, INotifyPropertyChanged
    {
        public dynamic Self
        {
            get { return this; }
        }

        #region DynamicObject overrides

        public DynamicObjectClass()
        {
        }

        public object GetMember(string memberName)
        {
            object result;
            this.TryGetMember(new GetDynamicMemberBinder(memberName, false), out result);
            return result;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!members.ContainsKey(binder.Name))
                members[binder.Name] = null;
            return members.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value != null && value.GetType() == typeof(JArray))
                //
                // 说明:
                //     转换成List,否则DataGrid无法排序
                members[binder.Name] = DynamicJson.DeserializeObject<List<object>>(DynamicJson.SerializeObject(value));
            else
                members[binder.Name] = value;
            OnPropertyChanged(binder.Name);
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return members.Keys;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int index = (int)indexes[0];
            try
            {
                result = itemsCollection[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                result = null;
                return false;
            }

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            int index = (int)indexes[0];
            itemsCollection[index] = value;
            OnPropertyChanged(Binding.IndexerName);
            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (members.ContainsKey(binder.Name))
            {
                members.Remove(binder.Name);
                return true;
            }

            return false;
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            int index = (int)indexes[0];
            itemsCollection.RemoveAt(index);
            return true;
        }

        #endregion DynamicObject overrides

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        #region Public methods

        public DynamicObjectClass AddItem(object item)
        {
            itemsCollection.Add(item);
            OnPropertyChanged(Binding.IndexerName);
            return this;
        }

        #endregion Public methods

        #region Private data

        Dictionary<string, object> members = new Dictionary<string, object>();
        ObservableCollection<object> itemsCollection = new ObservableCollection<object>();

        #endregion Private data
    }
}