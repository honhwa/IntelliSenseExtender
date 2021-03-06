﻿using System.Linq;
using IntelliSenseExtender.IntelliSense.Providers;
using Microsoft.CodeAnalysis.Completion;
using NUnit.Framework;

namespace IntelliSenseExtender.Tests.CompletionProviders
{
    public class ExtensionMethods : AbstractCompletionProviderTest
    {
        private readonly CompletionProvider Provider = new AggregateTypeCompletionProvider(
            Options_Default,
            new ExtensionMethodsCompletionProvider());

        [Test]
        public void ProvideReferencesCompletions_Linq()
        {
            const string source = @"
                using System.Collections.Generic;
                public class Test {
                    public void Method() {
                        var list = new List<string>();
                        list.
                    }
                }";

            var completions = GetCompletions(Provider, source, "list.");
            var completionsNames = completions.Select(completion => completion.DisplayText);
            Assert.That(completionsNames, Does.Contain("Select<>  (System.Linq)"));
        }

        [Test]
        public void ProvideUserCodeCompletions()
        {
            const string mainSource = @"
                public class Test {
                    public void Method() {
                        object obj = null;
                        obj.
                    }
                }";
            const string extensionsFile = @"
                namespace NM
                {
                    public static class ObjectExtensions
                    {
                        public static void Do(this object obj)
                        { }
                    }
                }";

            var completions = GetCompletions(Provider, mainSource, extensionsFile, "obj.");
            var completionsNames = completions.Select(completion => completion.DisplayText);
            Assert.That(completionsNames, Does.Contain("Do  (NM)"));
        }

        [Test]
        public void ProvideCompletionsForLiterals()
        {
            const string mainSource = @"
                public class Test {
                    public void Method() {
                        111.
                    }
                }";
            const string extensionsFile = @"
                namespace NM
                {
                    public static class ObjectExtensions
                    {
                        public static void Do(this object obj)
                        { }
                    }
                }";

            var completions = GetCompletions(Provider, mainSource, extensionsFile, "111.");
            var completionsNames = completions.Select(completion => completion.DisplayText);
            Assert.That(completionsNames, Does.Contain("Do  (NM)"));
        }

        [Test]
        public void DoNotProvideCompletionsIfMemberIsNotAccessed()
        {
            const string source = @"
                using System;
                namespace A{
                    class CA
                    {
                        [System.Obsolete]
                        public void MA(int par)
                        {
                            var a = 0;
                        }
                    }
                }
                namespace B{
                    static class B{
                        public static void ExtIntM(this int par)
                        { }
                    }
                }";
            const string extensionsFile = @"
                namespace NM
                {
                    public static class ObjectExtensions
                    {
                        public static void Do(this object obj)
                        { }
                    }
                }";

            var document = GetTestDocument(source, extensionsFile);

            for (int i = 0; i < source.Length; i++)
            {
                var context = GetContext(document, Provider, i);
                Provider.ProvideCompletionsAsync(context).Wait();
                var completions = GetCompletions(context);

                Assert.That(completions, Is.Empty);
            }
        }

        [Test]
        public void DoNotProvideCompletionsWhenTypeIsAccessed()
        {
            const string mainSource = @"
                public class Test {
                    public void Method() {
                        object.
                    }
                }";
            const string extensionsFile = @"
                namespace NM
                {
                    public static class ObjectExtensions
                    {
                        public static void Do(this object obj)
                        { }
                    }
                }";

            var completions = GetCompletions(Provider, mainSource, extensionsFile, "object.");
            Assert.That(completions, Is.Empty);
        }

        [Test]
        public void DoNotProvideObsolete()
        {
            const string mainSource = @"
                public class Test {
                    public void Method() {
                        object obj = null;
                        obj.
                    }
                }";
            const string extensionsFile = @"
                namespace NM
                {
                    [System.Obsolete]
                    public static class ObjectExtensions1
                    {
                        public static void Do1(this object obj)
                        { }
                    }

                    public static class ObjectExtensions2
                    {
                        [System.Obsolete]
                        public static void Do2(this object obj)
                        { }
                    }
                }";

            var completions = GetCompletions(Provider, mainSource, extensionsFile, "obj.")
                .Select(c => c.DisplayText)
                .ToList();

            Assert.That(completions, Does.Not.Contain("Do1  (NM)"));
            Assert.That(completions, Does.Not.Contain("Do2  (NM)"));
        }

        [Test]
        public void SuggestMethodsIfInvokedWithPresentText()
        {
            const string mainSource = @"
                public class Test {
                    public void Method() {
                        object obj = null;
                        obj.Some
                    }
                }";
            const string extensionsFile = @"
                namespace NM
                {
                    public static class ObjectExtensions1
                    {
                        public static void SomeExtension(this object obj)
                        { }
                    }
                }";

            var completions = GetCompletions(Provider, mainSource, extensionsFile, "obj.Some")
                .Select(c => c.DisplayText);

            Assert.That(completions, Does.Contain("SomeExtension  (NM)"));
        }
    }
}
