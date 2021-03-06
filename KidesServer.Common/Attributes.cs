﻿using System;

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
}
