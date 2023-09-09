using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Tradables
{
	public interface IHardResource : IResource
	{

	}
	public interface ICash : IResource
	{

	}

	public interface IResource
	{

	}
	public enum TypeOfResourceType
	{
		HardResource,
		Cash
	}
	public class ResourceAttribute : System.Attribute
	{
		public TypeOfResourceType TypeOfResourceType { get; private set; }
		public string Name { get; private set; }
		public Type TheType { get; private set; } //I do not have any idea how to name it normaly
		public ResourceAttribute(string name, TypeOfResourceType typeOfResourceType, Type type)
		{
			Name = name;
			TypeOfResourceType = typeOfResourceType;
			TheType = type;
		}
	}
	public static class Resources
	{
		public static IDictionary<Type, ResourceAttribute> ResourcesList = new Dictionary<Type, ResourceAttribute>();
		static Resources()
		{
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				var resourceAttribute = type.GetCustomAttribute<ResourceAttribute>();
				if (resourceAttribute != null)
				{
					ResourcesList.Add(type, resourceAttribute);
				}
			}
		}

		public static ResourceAttribute GetResourceAttributeByType(Type type)
		{
			return ResourcesList[type];
		}

	}

}
