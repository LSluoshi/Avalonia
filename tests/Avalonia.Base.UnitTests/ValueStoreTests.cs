using System;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Data;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class ValueStoreTests
    {
        [Fact]
        public void Bindings_Should_Be_Subscribed_Before_BeginInit()
        {
            var target = CreateTarget();
            var observable1 = new TestObservable<string>("foo");
            var observable2 = new TestObservable<string>("bar");

            target.AddBinding(Window.TitleProperty, observable1, BindingPriority.LocalValue);
            target.AddBinding(Window.TitleProperty, observable2, BindingPriority.LocalValue);

            Assert.Equal(1, observable1.SubscribeCount);
            Assert.Equal(1, observable2.SubscribeCount);
        }

        [Fact]
        public void Non_Active_Binding_Should_Not_Be_Subscribed_Before_BeginInit()
        {
            var target = CreateTarget();
            var observable1 = new TestObservable<string>("foo");
            var observable2 = new TestObservable<string>("bar");

            target.AddBinding(Window.TitleProperty, observable1, BindingPriority.LocalValue);
            target.AddBinding(Window.TitleProperty, observable2, BindingPriority.Style);

            Assert.Equal(1, observable1.SubscribeCount);
            Assert.Equal(0, observable2.SubscribeCount);
        }

        [Fact]
        public void Bindings_Should_Not_Be_Subscribed_After_BeginInit()
        {
            var target = CreateTarget();
            var observable1 = new TestObservable<string>("foo");
            var observable2 = new TestObservable<string>("bar");
            var observable3 = new TestObservable<string>("baz");

            target.BeginInit();
            target.AddBinding(Window.TitleProperty, observable1, BindingPriority.LocalValue);
            target.AddBinding(Window.TitleProperty, observable2, BindingPriority.LocalValue);
            target.AddBinding(Window.TitleProperty, observable3, BindingPriority.Style);

            Assert.Equal(0, observable1.SubscribeCount);
            Assert.Equal(0, observable2.SubscribeCount);
            Assert.Equal(0, observable3.SubscribeCount);
        }

        [Fact]
        public void Active_Binding_Should_Be_Subscribed_After_EndInit()
        {
            var target = CreateTarget();
            var observable1 = new TestObservable<string>("foo");
            var observable2 = new TestObservable<string>("bar");
            var observable3 = new TestObservable<string>("baz");

            target.BeginInit();
            target.AddBinding(Window.TitleProperty, observable1, BindingPriority.LocalValue);
            target.AddBinding(Window.TitleProperty, observable2, BindingPriority.LocalValue);
            target.AddBinding(Window.TitleProperty, observable3, BindingPriority.Style);
            target.EndInit();

            Assert.Equal(0, observable1.SubscribeCount);
            Assert.Equal(1, observable2.SubscribeCount);
            Assert.Equal(0, observable3.SubscribeCount);
        }

        private ValueStore CreateTarget()
        {
            var o = new AvaloniaObject();
            return o.Values;
        }

        public class TestObservable<T> : ObservableBase<BindingValue<T>>
        {
            private readonly T _value;
            
            public TestObservable(T value) => _value = value;

            public int SubscribeCount { get; private set; }

            protected override IDisposable SubscribeCore(IObserver<BindingValue<T>> observer)
            {
                ++SubscribeCount;
                observer.OnNext(_value);
                return Disposable.Empty;
            }
        }
    }
}
