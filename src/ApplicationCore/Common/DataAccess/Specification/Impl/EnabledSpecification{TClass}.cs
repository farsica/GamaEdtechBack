﻿namespace GamaEdtech.Backend.Common.DataAccess.Specification.Impl
{
    using System;
    using System.Linq.Expressions;

    using GamaEdtech.Backend.Common.DataAccess.Entities;
    using GamaEdtech.Backend.Common.DataAccess.Specification;

    public sealed class EnabledSpecification<TClass>(bool enabled) : SpecificationBase<TClass>
        where TClass : class, IEnablable<TClass>
    {
        private readonly bool enabled = enabled;

        public override Expression<Func<TClass, bool>> Expression() => t => t.Enabled == enabled;
    }
}
