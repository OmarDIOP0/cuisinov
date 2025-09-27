using Newtonsoft.Json;

namespace CantineFront.Helpers
{
    public static class SessionHelper
    {
        public static void SetObjectInSession(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetCustomObjectFromSession<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static int SetAddToListInSession<T>(this ISession session, string key, T newItem)
        {
            var value = session.GetString(key);
            List<T> oldValues = value == null ? default(List<T>) : JsonConvert.DeserializeObject<List<T>>(value);
            if(oldValues != null) {
                oldValues.Add(newItem);
            }
            else
            {
                oldValues=new List<T>() { newItem };
            }
            session.SetString(key, JsonConvert.SerializeObject(oldValues));
            return oldValues.Count; 
        }
        public static int SetListInSession<T>(this ISession session, string key, List<T> listItem)
        {

            session.SetString(key, JsonConvert.SerializeObject(listItem));
            return listItem?.Count??0;
        }
        public static List<T> GetListObjectFromSession<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(List<T>) : JsonConvert.DeserializeObject<List<T>>(value);
        }
    }
}
