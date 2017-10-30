using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using static Xamarin.Forms.Controls.Issues.Issue2259;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class SafeAreaPageiOS : ContentPage
	{
		Label safeLimits;

		[Preserve(AllMembers = true)]
		class Person
		{
			public Person(string firstName, string lastName, string city, string state)
			{
				FirstName = firstName;
				LastName = lastName;
				City = city;
				State = state;
			}
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string City { get; set; }
			public string State { get; set; }
		}

		public SafeAreaPageiOS(ICommand restore, Command<Page> setRoot)
		{
			Title = "Safe Area";
			BackgroundColor = Color.Azure;
			On<iOS>().SetUseSafeArea(true);

			Construct(this, restore, setRoot);
		}

		void Construct(ContentPage page, ICommand restore, Command<Page> setRoot)
		{
			safeLimits = new Label { Text = "nothing" };
			var grid = new Grid
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
			};
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			var safeLimitsTop = new Label
			{
				Text = "top",
				BackgroundColor = Color.Pink,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
				InputTransparent = true
			};
			grid.Children.Add(safeLimitsTop);
			Grid.SetRow(safeLimitsTop, 0);
			var safeLimitsBottom = new Label
			{
				Text = "bottom",
				BackgroundColor = Color.Pink,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.End,
				InputTransparent = true
			};
			grid.Children.Add(safeLimitsBottom);
			Grid.SetRow(safeLimitsBottom, 2);

			var content = new ScrollView
			{
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Fill,
					HorizontalOptions = LayoutOptions.Fill,
					Children =
				{
					safeLimits,
					new Button
					{
						Text = "Set Content as root",
						Command = new Command(() =>
						{
							var pageSafe = new SafeAreaPageiOS(restore,setRoot);
							setRoot.Execute(pageSafe);
						})
					},
					new Button
					{
						Text = "Set NavigationPage as root",
						Command = new Command(() =>
						{
							var pageSafe = new SafeAreaPageiOS(restore,setRoot);
							setRoot.Execute(new NavigationPage(pageSafe));
						})
					},
					new Button
					{
						Text = "Set TabbedPage as root",
						Command = new Command(() =>
						{
							var pageSafe = new SafeAreaPageiOS(restore,setRoot);
							var pageNotSafe = new SafeAreaPageiOS(restore,setRoot);
							pageNotSafe.On<iOS>().SetUseSafeArea(false);
							pageNotSafe.Title ="Not Using Safe Area";
							var tabbedPage = new TabbedPage();
							tabbedPage.Children.Add(pageSafe);
							tabbedPage.Children.Add(pageNotSafe);
							setRoot.Execute(tabbedPage);
						})
					},
					new Button
					{
						Text = "Toggle use safe area",
						Command = new Command(() => On<iOS>().SetUseSafeArea(!On<iOS>().UseSafeArea()))
					},
					new Button
					{
						Text = "ListViewPage with safe area",
						Command = new Command(()=>{
							var pageLIST = new ListViewPage("1");
							pageLIST.On<iOS>().SetUseSafeArea(true);
							setRoot.Execute(pageLIST);
						})
					},
					new Button
					{
						Text = "TableView+TextCell with safe area",
						Command = new Command(()=>{
							var pageTable = new TableViewPage();
							pageTable.On<iOS>().SetUseSafeArea(true);
							setRoot.Execute(pageTable);
						})
					},
					new Button
					{
						Text = "Back To Gallery",
						Command = restore
					},

				}
				}
			};
			grid.Children.Add(content);
			Grid.SetRow(content, 1);

			page.Content = grid;
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == "SafeAreaInsets")
			{
				safeLimits.Text = $" Top:{On<iOS>().SafeAreaInsets().Top} - Bottom:{On<iOS>().SafeAreaInsets().Bottom} - Left:{On<iOS>().SafeAreaInsets().Left} - Right:{On<iOS>().SafeAreaInsets().Right}";
			}
			base.OnPropertyChanged(propertyName);
		}

		[Preserve(AllMembers = true)]
		class MViewCell : ViewCell
		{
			Label firstNameLabel = new Label();
			Label lastNameLabel = new Label();
			Label cityLabel = new Label();
			Label stateLabel = new Label { HorizontalOptions = LayoutOptions.End };

			public MViewCell()
			{
				View = new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					Children =
					{
						firstNameLabel,
						lastNameLabel,
						cityLabel,
						stateLabel
					}
				};
				firstNameLabel.SetBinding(Label.TextProperty, "FirstName");
				lastNameLabel.SetBinding(Label.TextProperty, "LastName");
				cityLabel.SetBinding(Label.TextProperty, "City");
				stateLabel.SetBinding(Label.TextProperty, "State");

			}
		}

		[Preserve(AllMembers = true)]
		class TableViewPage : ContentPage
		{
			public TableViewPage()
			{

				Content = new TableView
				{
					Intent = TableIntent.Form,
					Root = new TableRoot("Table Title") {
					new TableSection ("Section 1 Title") {
						new TextCell {
							Text = "TextCell Text",
							Detail = "TextCell Detail"
						},
						new EntryCell {
							Label = "EntryCell:",
							Placeholder = "default keyboard",
							Keyboard = Keyboard.Default
						}
					},
					new TableSection ("Section 2 Title") {
						new EntryCell {
							Label = "Another EntryCell:",
							Placeholder = "phone keyboard",
							Keyboard = Keyboard.Telephone
						},
						new SwitchCell {
							Text = "SwitchCell:"
						}
					}
					}
				};
			}
		}

		[Preserve(AllMembers = true)]
		class ListViewPage : ContentPage
		{
			ListView _listview;
			List<Person> _People = new List<Person>();

			public ListViewPage(string id)
			{
				Title = $"List {id}";

				for (var x = 0; x < 1000; x++)
				{
					_People.Add(new Person($"Bob {x}", $"Bobson {x}", "San Francisco", "California"));
				}

				_listview = new ListView(ListViewCachingStrategy.RecycleElementAndDataTemplate) { ItemTemplate = new DataTemplate(typeof(MViewCell)) };
				_listview.ItemsSource = _People;
				Content = _listview;
			}
		}
	}

}
