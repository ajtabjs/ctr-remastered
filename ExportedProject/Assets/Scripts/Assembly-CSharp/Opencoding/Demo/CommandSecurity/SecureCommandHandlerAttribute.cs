using System;

namespace Opencoding.Demo.CommandSecurity
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
	public class SecureCommandHandlerAttribute : Attribute
	{
	}
}
