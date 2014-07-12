using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;

namespace Scripl.Core
{
    public static class Register
    {
        public static ContainerBuilder RegisterScriplCore(this ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(Register).Assembly).AsSelf().AsImplementedInterfaces();

            return builder;
        }
    }
}
