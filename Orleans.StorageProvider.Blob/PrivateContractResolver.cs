namespace Orleans.StorageProvider.Blob
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Orleans;

    internal class PrivateContractResolver : DefaultContractResolver
    {
        private static readonly string ObserverSubscriptionManagerFullName = typeof (ObserverSubscriptionManager<>).FullName;
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            if (objectType.FullName.Contains(ObserverSubscriptionManagerFullName))
            {
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                MemberInfo[] fields = objectType.GetFields(flags);
                return
                    fields.Concat(objectType.GetProperties(flags).Where(propertyInfo => propertyInfo.CanWrite)).ToList();
            }

            return base.GetSerializableMembers(objectType);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (type.FullName.Contains(ObserverSubscriptionManagerFullName))
            {
                return base.CreateProperties(type, MemberSerialization.Fields);
            }

            return base.CreateProperties(type, memberSerialization);
        }
    }

}
