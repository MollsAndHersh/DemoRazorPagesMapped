using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace DemoRazorPages
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public static IServiceCollection Clone(this IServiceCollection services)
        {
            var newServices = new ServiceCollection();
            var collection = (ICollection<ServiceDescriptor>)newServices;
            foreach (ServiceDescriptor descriptor in services)
            {
                collection.Add(descriptor);
            }
            return newServices;
        }

    }
}
