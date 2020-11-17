

using System.Collections.Generic;

namespace JsonDynamic
{
    public class JsonArray : Json
    {
        protected int index;

        public JsonArray() : base()
        {
            index = 0;
            isJsonObject = false;
        }

        public int Add(object value)
        {
            setDynamicValue(index.ToString(), value);
            int objectindex = index;
            index++;
            return objectindex;
        }

        public void Remove(object value)
        {
            bool arraychanged = false;
            foreach (KeyValuePair<string, object> values in _dynamicmembers)
            {
                if (values.Value == value)
                {
                    arraychanged = true;
                    _dynamicmembers.Remove(values.Key);
                    break;
                }
            }
            if (arraychanged)
            {
                index = 0;
                Dictionary<string, object> copy = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> values in _dynamicmembers)
                {
                    object savedobject = values.Value;
                    copy.Add(index.ToString(), savedobject);
                    index++;
                }
                _dynamicmembers = copy;
            }
        }
    }
}
