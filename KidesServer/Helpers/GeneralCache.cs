using System;
using System.Collections.Generic;

namespace KidesServer.Helpers
{
	public static class GeneralCache
	{
		private static Dictionary<string, Dictionary<string, CacheObject>> _objectCache = new Dictionary<string, Dictionary<string, CacheObject>>();
		private static readonly object _dictLock = new object();

		public static void NewCacheObject(string cache, string hash, object toCache, TimeSpan expireTime)
		{
			try
			{
				lock (_dictLock)
				{
					if (!_objectCache.ContainsKey(cache))
						_objectCache.Add(cache, new Dictionary<string, CacheObject>());

					CacheObject cacheObject = new CacheObject(toCache, expireTime);
					if (_objectCache[cache].ContainsKey(hash))
						_objectCache[cache].Remove(hash);
					_objectCache[cache].Add(hash, cacheObject);
				}
			}
			catch (Exception e)
			{
				ErrorLog.WriteLog(e.Message);
			}
		}

		public static object GetCacheObject(string cache, string hash)
		{
			try
			{
				lock (_dictLock)
				{
					if (!_objectCache.ContainsKey(cache))
						return null;
					if (_objectCache[cache].ContainsKey(hash))
					{
						var cachedObject = _objectCache[cache][hash];
						var expired = cachedObject.IsExpired();
						if (!expired)
							return _objectCache[cache][hash].CachedObject;
						else
							_objectCache[cache].Remove(hash);

					}
					return null;
				}
			}
			catch (Exception e)
			{
				ErrorLog.WriteLog(e.Message);
				return null;
			}
		}

		public static bool ContainsCacheObject(string cache, string hash)
		{
			try
			{
				lock (_dictLock)
				{
					if (!_objectCache.ContainsKey(cache))
						return false;
					if (!_objectCache[cache].ContainsKey(hash))
						return false;
					var expired = _objectCache[cache][hash].IsExpired();
					if (!expired)
						return true;
					else
						_objectCache[cache].Remove(hash);
					return false;
				}
			}
			catch (Exception e)
			{
				ErrorLog.WriteLog(e.Message);
				return false;
			}
		}
	}

	public class CacheObject
	{
		private readonly DateTime _timeCached;
		private TimeSpan _expireTime;
		public object CachedObject;

		public CacheObject(object toCache, TimeSpan expireTime)
		{
			_timeCached = DateTime.Now;
			_expireTime = expireTime;
			CachedObject = toCache;
		}

		public bool IsExpired()
		{
			if (_timeCached + _expireTime < DateTime.Now)
				return true;
			return false;
		}
	}
}