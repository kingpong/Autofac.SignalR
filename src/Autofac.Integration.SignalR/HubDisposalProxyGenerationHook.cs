// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Reflection;

using Castle.DynamicProxy;

namespace Autofac.Integration.SignalR
{
    internal class HubDisposalProxyGenerationHook : IProxyGenerationHook
    {
        /// <summary>
        /// Enables the proxy class created in <see cref="RegistrationExtensions.RegisterHubWithLifetimeScope{T}"/>
        /// to intercept <b>only</b> the virtual Dispose(bool) method.
        /// </summary>
        // ReSharper disable once EmptyConstructor
        public HubDisposalProxyGenerationHook() {}

        public void MethodsInspected() {}

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo) {}

        public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            // only intercept "protected void Dispose(bool)"
            return methodInfo.IsFamily
                   && methodInfo.ReturnType == typeof (void)
                   && methodInfo.Name == "Dispose"
                   && methodInfo.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] {typeof (bool)});
        }

        /// <summary>
        /// Returns true if the other object is of the same class.
        /// This allows Castle DynamicProxy to reuse the same generated class.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            // This class contains no data so every instance is equivalent to every other instance.
            return obj.GetType() == GetType();
        }

        /// <summary>
        /// Returns the same value for every instance of this class.
        /// Implemented for good measure to match the behavior of <see cref="Equals"/>
        /// </summary>
        public override int GetHashCode()
        {
            // This class contains no data so every instance is equivalent to every other instance.
            return GetType().GetHashCode();
        }
    }
}