using KidesServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Helpers
{
	public static class MathHelpers
	{
		public static double VectorDistance(Vector2<int> rhs, Vector2<int> lhs)
		{
			return Math.Sqrt(Math.Pow(rhs.X - lhs.X, 2) - Math.Pow(rhs.Y - lhs.Y, 2));
		}
	}
}
