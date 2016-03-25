using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class NavigationUnitTest : BaseTestFixture
	{
		[Test]
		public async Task TestNavigationImplPush ()
		{
			NavigationPage nav = new NavigationPage ();

			Assert.IsNull (nav.CurrentPage);

			Label child = new Label {Text = "Label"};
			Page childRoot = new ContentPage {Content = child};

			await nav.Navigation.PushAsync (childRoot);

			Assert.AreSame (childRoot, nav.CurrentPage);
		}

		[Test]
		public async Task TestNavigationImplPop ()
		{
			NavigationPage nav = new NavigationPage ();
			
			Label child = new Label ();
			Page childRoot = new ContentPage {Content = child};

			Label child2 = new Label ();
			Page childRoot2 = new ContentPage {Content = child2};
			
			await nav.Navigation.PushAsync (childRoot);
			await nav.Navigation.PushAsync (childRoot2);

			bool fired = false;
			nav.Popped += (sender, e) => fired = true;
			var popped = await nav.Navigation.PopAsync ();

			Assert.True (fired);
			Assert.AreSame (childRoot, nav.CurrentPage);
			Assert.AreEqual (childRoot2, popped);

			await nav.PopAsync ();
			var last = await nav.Navigation.PopAsync ();

			Assert.IsNull (last);
		}

		[Test]
		public async Task TestPushRoot ()
		{
			NavigationPage nav = new NavigationPage ();

			Assert.IsNull (nav.CurrentPage);

			Label child = new Label {Text = "Label"};
			Page childRoot = new ContentPage {Content = child};

			await nav.PushAsync (childRoot);

			Assert.AreSame (childRoot, nav.CurrentPage);
		}

		[Test]
		public async Task TestPushEvent ()
		{
			NavigationPage nav = new NavigationPage ();
			
			Label child = new Label ();
			Page childRoot = new ContentPage {Content = child};

			bool fired = false;
			nav.Pushed += (sender, e) => fired = true;

			await nav.PushAsync (childRoot);
			
			Assert.True (fired);
		}

		[Test]
		public async Task TestDoublePush ()
		{
			NavigationPage nav = new NavigationPage ();
			
			Label child = new Label ();
			Page childRoot = new ContentPage {Content = child};
			
			await nav.PushAsync (childRoot);

			bool fired = false;
			nav.Pushed += (sender, e) => fired = true;

			await nav.PushAsync (childRoot);
			
			Assert.False (fired);
			Assert.AreEqual (childRoot, nav.CurrentPage);
		}

		[Test]
		public async Task TestPop ()
		{
			NavigationPage nav = new NavigationPage ();
			
			Label child = new Label ();
			Page childRoot = new ContentPage {Content = child};

			Label child2 = new Label ();
			Page childRoot2 = new ContentPage {Content = child2};
			
			await nav.PushAsync (childRoot);
			await nav.PushAsync (childRoot2);

			bool fired = false;
			nav.Popped += (sender, e) => fired = true;
			var popped = await nav.PopAsync ();

			Assert.True (fired);
			Assert.AreSame (childRoot, nav.CurrentPage);
			Assert.AreEqual (childRoot2, popped);

			await nav.PopAsync ();
			var last = await nav.PopAsync ();

			Assert.IsNull (last);
		}

		[Test]
		public void TestTint ()
		{
			var nav = new NavigationPage ();

			Assert.AreEqual (Color.Default, nav.Tint);

			bool signaled = false;
			nav.PropertyChanged += (sender, args) => {
				if (args.PropertyName == "Tint")
					signaled = true;
			};

			nav.Tint = new Color (1, 0, 0);

			Assert.AreEqual (new Color (1, 0, 0), nav.Tint);
			Assert.True (signaled);
		}

		[Test]
		public void TestTintDoubleSet ()
		{
			var nav = new NavigationPage ();

			bool signaled = false;
			nav.PropertyChanged += (sender, args) => {
				if (args.PropertyName == "Tint")
					signaled = true;
			};

			nav.Tint = nav.Tint;

			Assert.False (signaled);
		}

		[Test]
		public async Task TestPopToRoot ()
		{
			var nav = new NavigationPage ();

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			var root = new ContentPage {Content = new View ()};
			var child1 = new ContentPage {Content = new View ()};
			var child2 = new ContentPage {Content = new View ()};

			await nav.PushAsync (root);
			await nav.PushAsync (child1);
			await nav.PushAsync (child2);

			nav.PopToRootAsync ();

			Assert.True (signaled);
			Assert.AreEqual (root, nav.CurrentPage);
		}

		[Test]
		public async Task TestStackCopy ()
		{
			var nav = new NavigationPage ();

			bool signaled = false;
			nav.PoppedToRoot += (sender, args) => signaled = true;

			var root = new ContentPage {Content = new View ()};
			var child1 = new ContentPage {Content = new View ()};
			var child2 = new ContentPage {Content = new View ()};

			await nav.PushAsync (root);
			await nav.PushAsync (child1);
			await nav.PushAsync (child2);

			var copy = nav.StackCopy;

			Assert.AreEqual (child2, copy.Pop ());
			Assert.AreEqual (child1, copy.Pop ());
			Assert.AreEqual (root, copy.Pop ());
		}

		[Test]
		public void ConstructWithRoot ()
		{
			var root = new ContentPage ();
			var nav = new NavigationPage (root);

			Assert.AreEqual (root, nav.LogicalChildren[0]);
			Assert.AreEqual (1, nav.StackDepth);
		}

		[Test]
		public async Task NavigationChangedEventArgs ()
		{
			var rootPage = new ContentPage { Title = "Root" };
			var navPage = new NavigationPage (rootPage);

			var rootArg = new Page ();

			navPage.Pushed += (s, e) => {
				rootArg = e.Page;
			};

			var pushPage = new ContentPage {
				Title = "Page 2"
			};

			await navPage.PushAsync (pushPage);

			Assert.AreEqual (rootArg, pushPage);

			var secondPushPage = new ContentPage {
				Title = "Page 3"
			};

			await navPage.PushAsync (secondPushPage);

			Assert.AreEqual (rootArg, secondPushPage);
		}

		[Test]
		public async Task CurrentPageChanged()
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new NavigationPage (root);

			bool changing = false;
			navPage.PropertyChanging += (object sender, PropertyChangingEventArgs e) => {
				if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName) {
					Assert.That (navPage.CurrentPage, Is.SameAs (root));
					changing = true;
				}
			};

			var next = new ContentPage { Title = "Next" };

			bool changed = false;
			navPage.PropertyChanged += (sender, e) => {
				if (e.PropertyName == NavigationPage.CurrentPageProperty.PropertyName) {
					Assert.That (navPage.CurrentPage, Is.SameAs (next));
					changed = true;
				}
			};

			await navPage.PushAsync (next);

			Assert.That (changing, Is.True, "PropertyChanging was not raised for 'CurrentPage'");
			Assert.That (changed, Is.True, "PropertyChanged was not raised for 'CurrentPage'");
		}

		[Test]
		public async void HandlesPopToRoot ()
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new NavigationPage (root);

			await navPage.PushAsync (new ContentPage ());
			await navPage.PushAsync (new ContentPage ());

			bool popped = false;
			navPage.PoppedToRoot += (sender, args) => {
				popped = true;
			};

			await navPage.Navigation.PopToRootAsync ();

			Assert.True (popped);
		}

		[Test]
		public void SendsBackButtonEventToCurrentPage ()
		{
			var current = new BackButtonPage ();
			var navPage = new NavigationPage (current);

			var emitted = false;
			current.BackPressed += (sender, args) => emitted = true;

			navPage.SendBackButtonPressed ();

			Assert.True (emitted);
		}

		[Test]
		public void DoesNotSendBackEventToNonCurrentPage ()
		{
			var current = new BackButtonPage ();
			var navPage = new NavigationPage (current);
			navPage.PushAsync (new ContentPage ());

			var emitted = false;
			current.BackPressed += (sender, args) => emitted = true;

			navPage.SendBackButtonPressed ();

			Assert.False (emitted);
		}

		[Test]
		public async void NavigatesBackWhenBackButtonPressed ()
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new NavigationPage (root);

			await navPage.PushAsync (new ContentPage ());

			var result = navPage.SendBackButtonPressed ();

			Assert.AreEqual (root, navPage.CurrentPage);
			Assert.True (result);
		}

		[Test]
		public async void DoesNotNavigatesBackWhenBackButtonPressedIfHandled ()
		{
			var root = new BackButtonPage { Title = "Root" };
			var second = new BackButtonPage () {Handle = true};
			var navPage = new NavigationPage (root);

			await navPage.PushAsync (second);

			navPage.SendBackButtonPressed ();

			Assert.AreEqual (second, navPage.CurrentPage);
		}

		[Test]
		public void DoesNotHandleBackButtonWhenNoNavStack ()
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new NavigationPage (root);

			var result = navPage.SendBackButtonPressed ();
			Assert.False (result);
		}

		[Test (Description = "CurrentPage should not be set to null when you attempt to pop the last page")]
		[Property ("Bugzilla", 28335)]
		public async Task CurrentPageNotNullPoppingRoot()
		{
			var root = new ContentPage { Title = "Root" };
			var navPage = new NavigationPage (root);
			var popped = await navPage.PopAsync ();
			Assert.That (popped, Is.Null);
			Assert.That (navPage.CurrentPage, Is.SameAs (root));
		}
	}

	internal class BackButtonPage : ContentPage
	{
		public event EventHandler BackPressed;

		public bool Handle = false;

		protected override bool OnBackButtonPressed ()
		{
			if (BackPressed != null)
				BackPressed (this, EventArgs.Empty);
			return Handle;
		}
	}
}