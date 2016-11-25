﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class AddReturnStatementReturningDefaultValue
    {
        public static string Foo()
        {
        }

        public static string FooWithStatement()
        {
            string s = null;
        }

        public static int FooInt32()
        {
        }

        public static void FooVoid()
        {
        }

        public static IEnumerable FooEnumerable()
        {
        }

        public static IEnumerable<object> FooEnumerableOfT()
        {
        }

        public static xxx FooErrorType()
        {
        }

        public static int FooExpressionBody() => ;
    }
}
