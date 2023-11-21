using System;
using Microsoft.Extensions.DependencyInjection;

namespace CorPool.BackEnd.Helpers {
    /**
     * This small helper class helps the DI container construct
     * instances of Lazy<T>. Lazy normally requires a construction
     * function for type T, but DI does not know how to provide one.
     * This extension class instead asks for the DI container in its
     * constructor, which the DI container _can_ give, and then this
     * class will use the right function to construct an instance of
     * T.
     */
    public class LazyDep<T> : Lazy<T> where T : class {
        public LazyDep(IServiceProvider provider) : base(provider.GetService<T>) { }
    }
}
