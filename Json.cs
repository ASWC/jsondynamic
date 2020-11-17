

using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Dynamic;
using System.Collections;
using System;

namespace JsonDynamic
{
    public class Json : DynamicObject
    {
        protected static int PRETTY_INDENT = 2;
        protected static int INDENT_LEVEL = 0;
        protected static JavaScriptSerializer serializer = new JavaScriptSerializer();
        protected Dictionary<string, object> _dynamicmembers;
        protected bool isJsonObject;
        public bool includeNullvalues { get; set; }

        public Json(string json = null)
        {
            includeNullvalues = true;
            isJsonObject = true;
            _dynamicmembers = new Dictionary<string, object>();
            if (json != null)
            {
                Dictionary<string, object> projectdata = getObjectValues(json);
                if (projectdata == null)
                {
                    List<Dictionary<string, object>> projectarray = getObjectArray(json);
                    if (projectarray != null)
                    {
                        isJsonObject = false;
                        int index = 0;
                        foreach (Dictionary<string, object> item in projectarray)
                        {
                            this.setDynamicValue(index.ToString(), item);
                            index++;
                        }
                    }
                }
                else
                {
                    assignValues(this, projectdata);
                }
            }
        }

        public Boolean HasValue(string key)
        {
            if (_dynamicmembers.ContainsKey(key))
            {
                object data = new object();
                _dynamicmembers.TryGetValue(key, out data);
                if (data == DBNull.Value)
                {
                    return false;
                }
                if (data != null)
                {
                    if (data.GetType() == typeof(ArrayList))
                    {
                        ArrayList value = (ArrayList)data;
                        if (value.Count == 0)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public object GetValue(string key)
        {
            if (_dynamicmembers.ContainsKey(key))
            {
                object data = new object();
                _dynamicmembers.TryGetValue(key, out data);
                if (data != null || data != DBNull.Value)
                {
                    return data;
                }
            }
            return null;
        }

        public override string ToString()
        {
            String builder = "{";
            if (!isJsonObject)
            {
                builder = "[";
            }
            if (!includeNullvalues)
            {
                _dynamicmembers = removeNullvalues(_dynamicmembers);
            }
            if (_dynamicmembers.Count > 0)
            {
                foreach (KeyValuePair<string, object> entry in _dynamicmembers)
                {
                    if (entry.Value == null)
                    {
                        builder += "\"" + entry.Key + "\" : " + null + ",";
                        continue;
                    }
                    string value = entry.Value.ToString();
                    if (entry.Value.GetType() == typeof(string))
                    {
                        value = "\"" + entry.Value.ToString() + "\"";
                    }
                    else if (entry.Value.GetType() == typeof(bool))
                    {
                        value = value.ToLower();
                    }
                    else if (entry.Value.GetType() == typeof(ArrayList))
                    {
                        ArrayList array = (ArrayList)entry.Value;
                        value = getArrayJson(array);
                    }
                    if (isJsonObject)
                    {
                        builder += "\"" + entry.Key + "\" : " + value + ",";
                    }
                    else
                    {
                        builder += value + ",";
                    }
                }
                builder = builder.Remove(builder.Length - 1, 1);
            }
            if (!isJsonObject)
            {
                builder += "]";
            }
            else
            {
                builder += "}";
            }
            return builder;
        }

        public List<String> getKeys()
        {
            List<String> keys = new List<string>();
            foreach (KeyValuePair<string, object> entry in _dynamicmembers)
            {
                keys.Add(entry.Key);
            }
            return keys;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dynamicmembers[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dynamicmembers.TryGetValue(binder.Name, out result);
        }

        public void setDynamicValue(string key, object value)
        {
            if (_dynamicmembers.ContainsKey(key))
            {
                _dynamicmembers.Remove(key);
            }
            if (value == null)
            {
                _dynamicmembers.Add(key, value);
                return;
            }
            else if (value == DBNull.Value)
            {
                _dynamicmembers.Add(key, null);
                return;
            }
            Type type = value.GetType();
            if (type == typeof(string))
            {
                _dynamicmembers.Add(key, value);
            }
            else if (type == typeof(ArrayList))
            {
                ArrayList list = (ArrayList)value;
                ArrayList copy = new ArrayList();
                _dynamicmembers.Add(key, copy);
                if (list.Count > 0)
                {
                    foreach (object data in list)
                    {
                        object listvalue = GetObjectValue(data);
                        copy.Add(listvalue);
                    }
                }
            }
            else if (type == typeof(Dictionary<string, object>))
            {
                string json = serializer.Serialize(value);
                Json parseddata = new Json(json);
                _dynamicmembers.Add(key, parseddata);
            }
            else if (type == typeof(bool))
            {
                _dynamicmembers.Add(key, value);
            }
            else if (type == typeof(int))
            {
                _dynamicmembers.Add(key, value);
            }
            else
            {
                _dynamicmembers.Add(key, value);
            }
        }

        public dynamic dynamic
        {
            get
            {
                return this;
            }
        }

        public string ToPrettyJson()
        {
            return ToIndentedJson(1, false, true);
        }

        public long Count
        {
            get
            {
                if (!isJsonObject)
                {
                    return _dynamicmembers.Count;
                }
                return 0;
            }
        }

        protected string ToIndentedJson(int spaces = 1, bool inner = false, bool isroot = false)
        {
            string lb = Environment.NewLine;
            String builder = lb + getSpaces((spaces - 1) * PRETTY_INDENT) + "{" + lb;
            if (isroot && !isJsonObject)
            {
                builder = lb + getSpaces((spaces - 1) * PRETTY_INDENT) + "[" + lb;
            }
            if (!includeNullvalues)
            {
                _dynamicmembers = removeNullvalues(_dynamicmembers);
            }
            if (_dynamicmembers.Count > 0)
            {
                foreach (KeyValuePair<string, object> entry in _dynamicmembers)
                {
                    string value = "";
                    if (entry.Value == null)
                    {
                        value = "null";
                    }
                    else
                    {
                        value = entry.Value.ToString();
                        if (entry.Value.GetType() == typeof(string))
                        {
                            value = "\"" + entry.Value.ToString() + "\"";
                        }
                        else if (entry.Value.GetType() == typeof(ArrayList))
                        {
                            ArrayList array = (ArrayList)entry.Value;
                            value = getPrettyArrayJson(array, spaces + 1, true);
                        }
                        else if (entry.Value.GetType() == typeof(Json))
                        {
                            Json castvalue = (Json)entry.Value;
                            value = castvalue.ToIndentedJson(spaces + 1, true);
                        }
                    }
                    if (isJsonObject)
                    {
                        builder += getSpaces(spaces * PRETTY_INDENT) + "\"" + entry.Key + "\" : " + value + "," + lb;
                    }
                    else
                    {
                        builder += getSpaces(spaces * PRETTY_INDENT) + value + "," + lb;
                    }
                }
                builder = builder.Remove(builder.Length - 3) + lb;
            }
            if (isroot && !isJsonObject)
            {
                builder += getSpaces((spaces - 1) * PRETTY_INDENT) + "]";
            }
            else
            {
                builder += getSpaces((spaces - 1) * PRETTY_INDENT) + "}";
            }
            return builder;
        }

        protected static void assignValues(Json target, Dictionary<string, object> values)
        {
            foreach (KeyValuePair<string, object> entry in values)
            {
                target.setDynamicValue(entry.Key, entry.Value);
            }
        }

        protected object GetObjectValue(object data)
        {
            if (data == null)
            {
                return null;
            }
            else if (data == DBNull.Value)
            {
                return null;
            }
            if (data.GetType() == typeof(Dictionary<string, object>))
            {
                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(data);
                Json parseddata = new Json(json);
                return parseddata;
            }
            return data;
        }

        protected Dictionary<string, object> getObjectValues(string json)
        {
            try
            {
                return serializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch
            {
                return null;
            }
        }

        protected List<Dictionary<string, object>> getObjectArray(string json)
        {
            try
            {
                return serializer.Deserialize<List<Dictionary<string, object>>>(json);
            }
            catch
            {
                return null;
            }
        }

        protected static Dictionary<string, object> removeNullvalues(Dictionary<string, object> dic)
        {
            Dictionary<string, object> copy = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> values in dic)
            {
                if (values.Value != null)
                {
                    copy.Add(values.Key, values.Value);
                }
            }
            return copy;
        }

        protected string getSpaces(int spaces)
        {
            return new String(' ', spaces);
        }

        protected string getPrettyArrayJson(ArrayList list, int spaces = 1, bool inner = false)
        {
            string lb = Environment.NewLine;
            string value = lb + getSpaces((spaces - 1) * PRETTY_INDENT) + "[" + lb;
            if (list.Count == 0)
            {
                return "[]";
            }
            foreach (object arrayvalue in list)
            {
                if (arrayvalue.GetType() == typeof(string))
                {
                    value += getSpaces(spaces + 3) + "\"" + arrayvalue.ToString() + "\"";
                }
                else if (arrayvalue.GetType() == typeof(ArrayList))
                {
                    ArrayList innerlist = (ArrayList)arrayvalue;
                    value += getSpaces(spaces + 1) + getPrettyArrayJson(innerlist, spaces + 1, true);
                }
                else if (arrayvalue.GetType() == typeof(Json))
                {
                    Json castvalue = (Json)arrayvalue;
                    value += getSpaces(spaces + 1) + castvalue.ToIndentedJson(spaces + 1, true);
                }
                else
                {
                    value += getSpaces(spaces + 2) + arrayvalue.ToString();
                }
                value += "," + lb;
            }
            value = value.Remove(value.Length - 3);
            value += lb + getSpaces((spaces - 1) * PRETTY_INDENT) + "]";
            return value;
        }

        protected string getArrayJson(ArrayList list)
        {
            string value = "[";
            if (list.Count == 0)
            {
                return "[]";
            }
            foreach (object arrayvalue in list)
            {
                if (arrayvalue.GetType() == typeof(string))
                {
                    value += "\"" + arrayvalue.ToString() + "\"";
                }
                else if (arrayvalue.GetType() == typeof(ArrayList))
                {
                    ArrayList innerlist = (ArrayList)arrayvalue;
                    value += getArrayJson(innerlist);
                }
                else
                {
                    value += arrayvalue.ToString();
                }
                value += ",";
            }
            value = value.Remove(value.Length - 1, 1);
            value += "]";
            return value;
        }
    }
}
