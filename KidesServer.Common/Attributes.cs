using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace KidesServer.Common
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public sealed class ReturnsAttribute : Attribute
	{
		public ReturnsAttribute(Type T)
		{
			ResultType = T;
		}

		public Type ResultType { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
	{
		public void OnResourceExecuting(ResourceExecutingContext context)
		{
			var factories = context.ValueProviderFactories;
			factories.RemoveType<FormValueProviderFactory>();
			factories.RemoveType<JQueryFormValueProviderFactory>();
		}

		public void OnResourceExecuted(ResourceExecutedContext context)
		{
		}
	}
}
