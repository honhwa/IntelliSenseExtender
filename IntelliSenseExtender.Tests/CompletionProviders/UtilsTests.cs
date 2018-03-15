﻿using System.Linq;
using System.Threading;
using IntelliSenseExtender.Editor;
using NUnit.Framework;

namespace IntelliSenseExtender.Tests.CompletionProviders
{
    [TestFixture]
    public class UtilsTests : AbstractCompletionProviderTest
    {
        [Test]
        public void NamespaceResolver_ShouldAddUsingAtSortedPlace()
        {
            const string source = @"
                using System;
                using System.Threading;

                namespace ns.something
                {
                    public class Test {}
                }";

            var document = GetTestDocument(source);
            var newDoc = new NamespaceResolver().AddNamespaceImportAsync("System.Collections", document, CancellationToken.None).Result;
            var newDocText = newDoc.GetTextAsync().Result.ToString();

            var usingIndexes = new[] {
                    "using System;",
                    "using System.Collections;",
                    "using System.Threading;" }
                .Select(u => newDocText.IndexOf(u))
                .ToArray();

            Assert.That(usingIndexes, Does.Not.Contain(-1), "Missing namespace!");
            Assert.That(usingIndexes, Is.Ordered);
        }

        [Test]
        public void NamespaceResolver_ShouldAddUsingInsideNamespaceIfUsingsArePresent()
        {
            const string source = @"
                namespace ns.something
                {
                    using System;

                    public class Test {}
                }";

            var document = GetTestDocument(source);
            var newDoc = new NamespaceResolver().AddNamespaceImportAsync("System.Collections", document, CancellationToken.None).Result;
            var newDocText = newDoc.GetTextAsync().Result.ToString();

            Assert.That(newDocText, Does.Contain("using System.Collections;"));
            Assert.That(newDocText.IndexOf("using System.Collections;"),
                Is.GreaterThan(newDocText.IndexOf("namespace ns.something")));
        }
    }
}
