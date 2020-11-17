# Json and JsonArray

Provide a fast and easy way to parse and stringify json data

# Example, create json data

    Json jsonobject = new Json();
    jsonobject.dynamic.user = "Tom"; // dynamic access available
    jsonobject.dynamic.userData = new Json(); // dynamic access available

    if(jsonobject.HasValue("user"))
    {
        string username = Convert.ToString(jsonobject.GetValue("user")); // Tom
    }

    // use setDynamicValue if you don't want to use the dynamic approach
    jsonobject.setDynamicValue("user", "John"); // set different value for 'user'

    jsonobject.ToString(); // outputs a formatted json
    jsonobject.ToPrettyJson(); // outputs a indented formatted json for display

# Example, json array

    JsonArray jsonarray = new JsonArray();
    jsonarray.Add("Tom"); // add to the array
    jsonarray.Add(new Json()); // add to the array
    var data = jsonarray.GetValue("1"); // get value at index 1

# Example, parsing json

    string rawjson = @"{'user':'Tom', 'userid' : 15}";
    Json user = new Json(rawjson); // automatically parsing any valid json
    string userfirstname = user.dynamic.user; // get value by key using dynamic'Tom'
    userfirstname = Convert.ToString(user.GetValue("user")); // or get value using GetValue 'Tom'