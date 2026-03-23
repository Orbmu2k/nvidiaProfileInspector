// TinyIoC - An easy to use, lightweight dependency injection container
// 
// Designed for use in C# / .NET Applications
//
// Copyright (c) 2024 Steven鸽鸽
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace nvidiaProfileInspector.TinyIoc
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public sealed class TinyIoCContainer : IDisposable
    {
        private readonly ConcurrentDictionary<Type, ResolveScope> _scopes = new ConcurrentDictionary<Type, ResolveScope>();
        private readonly ConcurrentDictionary<Type, Func<ResolveScope, object>> _registeredTypes = new ConcurrentDictionary<Type, Func<ResolveScope, object>>();
        private readonly ConcurrentDictionary<Type, Func<ResolveScope, object>> _singletons = new ConcurrentDictionary<Type, Func<ResolveScope, object>>();
        private readonly ConcurrentDictionary<Type, List<Func<ResolveScope, object>>> _multiples = new ConcurrentDictionary<Type, List<Func<ResolveScope, object>>>();
        private bool _disposed;

        public TinyIoCContainer()
        {
            RegisterInstance(this);
        }

        public void Register<TRegister, TConcrete>() where TConcrete : TRegister, new()
        {
            ThrowIfDisposed();
            _registeredTypes[typeof(TRegister)] = scope => new TConcrete();
        }

        public void Register<TRegister>(Func<TRegister> factory) where TRegister : class
        {
            ThrowIfDisposed();
            _registeredTypes[typeof(TRegister)] = scope => factory();
        }

        public void Register<TRegister>(Func<TinyIoCContainer, TRegister> factory) where TRegister : class
        {
            ThrowIfDisposed();
            _registeredTypes[typeof(TRegister)] = scope => factory(this);
        }

        public void Register<TConcrete>() where TConcrete : class, new()
        {
            ThrowIfDisposed();
            _registeredTypes[typeof(TConcrete)] = scope => new TConcrete();
        }

        public void Register<TConcrete>(bool autoResolve) where TConcrete : class
        {
            ThrowIfDisposed();
            _registeredTypes[typeof(TConcrete)] = scope => CreateInstance(typeof(TConcrete));
        }

        public void RegisterSingleton<TRegister, TConcrete>() where TConcrete : TRegister, new()
        {
            ThrowIfDisposed();
            _singletons[typeof(TRegister)] = scope => new TConcrete();
        }

        public void RegisterSingleton<TRegister>(Func<TRegister> factory) where TRegister : class
        {
            ThrowIfDisposed();
            _singletons[typeof(TRegister)] = scope => factory();
        }

        public void RegisterSingleton<TRegister>(Func<TinyIoCContainer, TRegister> factory) where TRegister : class
        {
            ThrowIfDisposed();
            _singletons[typeof(TRegister)] = scope => factory(this);
        }

        public void RegisterSingleton<TConcrete>() where TConcrete : class, new()
        {
            ThrowIfDisposed();
            _singletons[typeof(TConcrete)] = scope => new TConcrete();
        }

        public void RegisterSingleton<TConcrete>(bool autoResolve) where TConcrete : class
        {
            ThrowIfDisposed();
            _singletons[typeof(TConcrete)] = scope => CreateInstance(typeof(TConcrete));
        }

        public void RegisterInstance<TRegister>(TRegister instance) where TRegister : class
        {
            ThrowIfDisposed();
            _singletons[typeof(TRegister)] = scope => instance;
        }

        public void RegisterMultiple<TRegister>(params Func<TRegister>[] factories) where TRegister : class
        {
            ThrowIfDisposed();
            var list = _multiples.GetOrAdd(typeof(TRegister), _ => new List<Func<ResolveScope, object>>());
            lock (list)
            {
                list.AddRange(factories.Select(f => (Func<ResolveScope, object>)(scope => f())));
            }
        }

        public TResolve Resolve<TResolve>() where TResolve : class
        {
            return (TResolve)Resolve(typeof(TResolve));
        }

        public object Resolve(Type resolveType)
        {
            ThrowIfDisposed();

            if (_singletons.TryGetValue(resolveType, out var singletonFactory))
            {
                return GetOrCreateScope(resolveType).GetOrCreate(singletonFactory);
            }

            if (_registeredTypes.TryGetValue(resolveType, out var factory))
            {
                return factory(GetOrCreateScope(resolveType));
            }

            if (resolveType.IsClass)
            {
                return CreateInstance(resolveType);
            }

            throw new TinyIoCResolutionException(resolveType);
        }

        public TResolve TryResolve<TResolve>() where TResolve : class
        {
            return TryResolve(typeof(TResolve)) as TResolve;
        }

        public object TryResolve(Type resolveType)
        {
            ThrowIfDisposed();

            if (_singletons.TryGetValue(resolveType, out var singletonFactory))
            {
                return GetOrCreateScope(resolveType).GetOrCreate(singletonFactory);
            }

            if (_registeredTypes.TryGetValue(resolveType, out var factory))
            {
                return factory(GetOrCreateScope(resolveType));
            }

            return null;
        }

        public IEnumerable<TResolve> ResolveAll<TResolve>() where TResolve : class
        {
            return ResolveAll(typeof(TResolve)).Cast<TResolve>();
        }

        public IEnumerable<object> ResolveAll(Type resolveType)
        {
            ThrowIfDisposed();
            var results = new List<object>();

            if (_multiples.TryGetValue(resolveType, out var factories))
            {
                lock (factories)
                {
                    results.AddRange(factories.Select(f => f(GetOrCreateScope(resolveType))));
                }
            }

            var single = TryResolve(resolveType);
            if (single != null && !results.Contains(single))
            {
                results.Add(single);
            }

            return results;
        }

        private ResolveScope GetOrCreateScope(Type type)
        {
            return _scopes.GetOrAdd(type, _ => new ResolveScope());
        }

        private object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .ToArray();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var resolvedParams = new object[parameters.Length];
                var canResolve = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    resolvedParams[i] = TryResolve(paramType);

                    if (resolvedParams[i] == null && !parameters[i].HasDefaultValue)
                    {
                        canResolve = false;
                        break;
                    }

                    if (resolvedParams[i] == null && parameters[i].HasDefaultValue)
                    {
                        resolvedParams[i] = parameters[i].DefaultValue;
                    }
                }

                if (canResolve)
                {
                    return constructor.Invoke(resolvedParams);
                }
            }

            throw new TinyIoCResolutionException(type);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TinyIoCContainer));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _scopes.Clear();
                _registeredTypes.Clear();
                _singletons.Clear();
                _multiples.Clear();
            }
        }

        private class ResolveScope
        {
            private object _instance;
            private readonly object _lock = new object();
            private bool _created;

            public object GetOrCreate(Func<ResolveScope, object> factory)
            {
                if (_created)
                    return _instance;

                lock (_lock)
                {
                    if (!_created)
                    {
                        _instance = factory(this);
                        _created = true;
                    }
                    return _instance;
                }
            }
        }
    }

    public class TinyIoCResolutionException : Exception
    {
        public Type ResolveType { get; }

        public TinyIoCResolutionException(Type resolveType)
            : base($"Could not resolve type: {resolveType.FullName}")
        {
            ResolveType = resolveType;
        }
    }

    public static class TinyIoC
    {
        private static TinyIoCContainer _container;

        public static TinyIoCContainer Container
        {
            get
            {
                if (_container == null)
                {
                    Interlocked.CompareExchange(ref _container, new TinyIoCContainer(), null);
                }
                return _container;
            }
        }

        public static void Setup(TinyIoCContainer container)
        {
            _container = container;
        }

        public static T Resolve<T>() where T : class => Container.Resolve<T>();
        public static T TryResolve<T>() where T : class => Container.TryResolve<T>();
        public static IEnumerable<T> ResolveAll<T>() where T : class => Container.ResolveAll<T>();
        public static object Resolve(Type type) => Container.Resolve(type);
        public static object TryResolve(Type type) => Container.TryResolve(type);
        public static IEnumerable<object> ResolveAll(Type type) => Container.ResolveAll(type);
    }
}
