﻿using FubuMVC.Core.Http.Cookies;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuMVC.Tests.Http.Cookies
{
    [TestFixture]
    public class CookieStateTester
    {
        [Test]
        public void parse_with_one_value()
        {
            var state = CookieState.Parse("key", "1");
            state.Name.ShouldEqual("key");
            state.Value.ShouldEqual("1");
        }

        [Test]
        public void parse_with_multiple_values()
        {
            var state = CookieState.Parse("key", "a=1");

            state.Name.ShouldEqual("key");
            state.Value.ShouldBeNull();

            state["a"].ShouldEqual("1");
        }

        [Test]
        public void parse_with_multiple_values_2()
        {
            var state = CookieState.Parse("key", "a=1&b=2&c=3");

            state.Name.ShouldEqual("key");
            state.Value.ShouldBeNull();

            state["a"].ShouldEqual("1");
            state["b"].ShouldEqual("2");
            state["c"].ShouldEqual("3");
        }

        [Test]
        public void silly_with_works()
        {
            var state = new CookieState("key").With("a", "1").With("b", "2");

            state["a"].ShouldEqual("1");
            state["b"].ShouldEqual("2");
        }

        private void roundtrips(string name, string text)
        {
            var state = CookieState.Parse(name, text);
            state.ToString().ShouldEqual(name + "=" + text);
        }

        [Test]
        public void writing()
        {
            roundtrips("a", "1");
            roundtrips("a", "a1=1&a2=2&a3=3");

        }
    }
}