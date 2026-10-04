using System;
using System.Collections.Generic;

namespace DI
{
    public class Container
    {
        private readonly Dictionary<Type, Func<object>> _bindings = new();
        private readonly HashSet<object> _initialized = new();

        public void BindAsSingle<T>(Func<T> factory) where T : class
        {
            T instance = null;
            _bindings[typeof(T)] = () => instance ??= factory();
        }

        public void BindAsSingleNonLazy<T>(Func<T> factory) where T : class
        {
            BindAsSingle(factory);
            _bindings[typeof(T)].Invoke(); // Force creation now (non-lazy)
        }

        public void BindAsTransient<T>(Func<T> factory) where T : class
        {
            _bindings[typeof(T)] = () => factory();
        }

        public T Resolve<T>() where T : class
        {
            return Resolve(typeof(T)) as T;
        }

        public object Resolve(Type type)
        {
            if (_bindings.TryGetValue(type, out var factory))
            {
                var instance = factory();
                InitializeIfPreInitializable(instance);
                return instance;
            }

            throw new Exception($"Type {type.Name} not bound in container.");
        }

        private void InitializeIfPreInitializable(object instance)
        {
            if (instance is IPreInitializable preInit && _initialized.Add(instance))
            {
                preInit.PreInitialize();
            }
        }
    }
}