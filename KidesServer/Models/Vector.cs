namespace KidesServer.Models
{
	public class Vector2<T>
	{
		private T _x;
		private T _y;
		public T X { get => _x; set => _x = value; }
		public T Y { get => _y; set => _y = value; }

		public Vector2()
		{
			_x = default(T);
			_y = default(T);
		}

		public Vector2(Vector2<T> i)
		{
			_x = i.X;
			_y = i.Y;
		}

		public Vector2(T x, T y)
		{
			_x = x;
			_y = y;
		}

		//These will throw expections if type T does not have the operator itself.
		public static Vector2<T> operator +(Vector2<T> v, Vector2<T> v2)
		{
			return new Vector2<T>((dynamic)v.X + (dynamic)v2.X, (dynamic)v2.Y + (dynamic)v2.Y);
		}

		public static Vector2<T> operator -(Vector2<T> v, Vector2<T> v2)
		{
			return new Vector2<T>((dynamic)v.X - (dynamic)v2.X, (dynamic)v2.Y - (dynamic)v2.Y);
		}

		public static Vector2<T> operator -(Vector2<T> v)
		{
			return new Vector2<T>(-(dynamic)v.X, -(dynamic)v.Y);
		}

		public static bool operator ==(Vector2<T> lhs, Vector2<T> rhs)
		{
			return (lhs.X.Equals(rhs.X) && lhs.Y.Equals(rhs.Y));
		}

		public static bool operator !=(Vector2<T> lhs, Vector2<T> rhs)
		{
			return (!lhs.X.Equals(rhs.X) || !lhs.Y.Equals(rhs.Y));
		}

		public override bool Equals(object obj)
		{
			if(obj is Vector2<T> x)
				return x == this;
			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override string ToString()
		{
			return ($"{_x.ToString()}, {_y.ToString()}");
		}

		public string ToString(string format)
		{
			var first = format.Replace("%X", _x.ToString());
			return first.Replace("%Y", _y.ToString());
		}
	}
}
