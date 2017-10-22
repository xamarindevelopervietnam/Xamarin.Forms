using System;
using System.Linq;

namespace Xamarin.Forms.Controls
{
	public class FlowDirectionGallery : ContentPage
	{
		FlowDirection DeviceDirection => Device.Info.CurrentFlowDirection;

		public FlowDirectionGallery()
		{
			SetContent(DeviceDirection);
		}

		void SetContent(FlowDirection direction)
		{
			var hOptions = LayoutOptions.Start;

			var imageCell = new DataTemplate(typeof(ImageCell));
			imageCell.SetBinding(ImageCell.ImageSourceProperty, ".");
			imageCell.SetBinding(ImageCell.TextProperty, ".");

			var textCell = new DataTemplate(typeof(TextCell));
			textCell.SetBinding(TextCell.DetailProperty, ".");

			var entryCell = new DataTemplate(typeof(EntryCell));
			entryCell.SetBinding(EntryCell.TextProperty, ".");
			//entryCell.SetValue(EntryCell.HorizontalTextAlignmentProperty, TextAlignment.Center);

			var switchCell = new DataTemplate(typeof(SwitchCell));
			switchCell.SetBinding(SwitchCell.OnProperty, ".");
			switchCell.SetValue(SwitchCell.TextProperty, "Switch Cell!");

			var viewCell = new DataTemplate(() => new ViewCell
			{
				View = new StackLayout
				{
					Children = { new Label { HorizontalOptions = hOptions, Text = "View Cell!" } }
				}
			});

			var flipButton = new Button
			{
				Text = direction == FlowDirection.RightToLeft ? "Switch to Left To Right" : "Switch to Right To Left"
			};

			flipButton.Clicked += (s, e) =>
			{
				if (Content.FlowDirection == FlowDirection.LeftToRight || Content.FlowDirection == FlowDirection.MatchParent)
					SetContent(FlowDirection.RightToLeft);
				else
					SetContent(FlowDirection.LeftToRight);
			};

			var grid = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
					new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
				}
			};

			int col = 0;
			int row = 0;

			var ai = AddView<ActivityIndicator>(grid, ref col, ref row);
			ai.IsRunning = true;

			var box = AddView<BoxView>(grid, ref col, ref row);
			box.WidthRequest = box.HeightRequest = 20;
			box.BackgroundColor = Color.Purple;

			var btn = AddView<Button>(grid, ref col, ref row);
			btn.Text = "Some text";

			var date = AddView<DatePicker>(grid, ref col, ref row, 2);

			var edit = AddView<Editor>(grid, ref col, ref row);
			edit.WidthRequest = 100;
			edit.HeightRequest = 100;
			edit.Text = "Some longer text for wrapping";

			var entry = AddView<Entry>(grid, ref col, ref row);
			entry.WidthRequest = 100;
			entry.Text = "Some text";

			var image = AddView<Image>(grid, ref col, ref row);
			image.Source = "oasis.jpg";

			var lbl1 = AddView<Label>(grid, ref col, ref row);
			lbl1.WidthRequest = 100;
			lbl1.HorizontalTextAlignment = TextAlignment.Start;
			lbl1.Text = "Start text";

			var lblLong = AddView<Label>(grid, ref col, ref row);
			lblLong.WidthRequest = 100;
			lblLong.HorizontalTextAlignment = TextAlignment.Start;
			lblLong.Text = "Start text that should wrap and wrap and wrap";

			var lbl2 = AddView<Label>(grid, ref col, ref row);
			lbl2.WidthRequest = 100;
			lbl2.HorizontalTextAlignment = TextAlignment.End;
			lbl2.Text = "End text";

			var lbl3 = AddView<Label>(grid, ref col, ref row);
			lbl3.WidthRequest = 100;
			lbl3.HorizontalTextAlignment = TextAlignment.Center;
			lbl3.Text = "Center text";

			//var ogv = AddView<OpenGLView>(grid, ref col, ref row, hOptions, vOptions, margin);

			var pkr = AddView<Picker>(grid, ref col, ref row);
			pkr.ItemsSource = Enumerable.Range(0, 10).ToList();

			var sld = AddView<Slider>(grid, ref col, ref row);
			sld.WidthRequest = 100;
			sld.Maximum = 10;
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				sld.Value += 1;
				if (sld.Value == 10d)
					sld.Value = 0;
				return true;
			});

			var stp = AddView<Stepper>(grid, ref col, ref row);

			var swt = AddView<Switch>(grid, ref col, ref row);

			var time = AddView<TimePicker>(grid, ref col, ref row, 2);

			var prog = AddView<ProgressBar>(grid, ref col, ref row, 2);
			prog.WidthRequest = 200;
			prog.BackgroundColor = Color.DarkGray;
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				prog.Progress += .1;
				if (prog.Progress == 1d)
					prog.Progress = 0;
				return true;
			});

			var srch = AddView<SearchBar>(grid, ref col, ref row, 2);
			srch.WidthRequest = 200;
			srch.Text = "Some text";

			TableView tbl = new TableView
			{
				Intent = TableIntent.Menu,
				Root = new TableRoot
					{
						new TableSection("TableView")
						{
							new TextCell
							{
								Text = "A",
							},

							new TextCell
							{
								Text = "B",
							},

							new TextCell
							{
								Text = "C",
							},

							new TextCell
							{
								Text = "D",
							},
						}
					}
			};

			var stack = new StackLayout
			{
				Children = {        new Label { Text = $"Device Direction: {DeviceDirection}" },
									flipButton,
									grid,
									new Label { Text = "TableView", FontSize = 10, TextColor = Color.DarkGray },
									tbl,
									new Label { Text = "ListView w/ TextCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => "Text Cell!"), ItemTemplate = textCell },
									new Label { Text = "ListView w/ SwitchCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => true), ItemTemplate = switchCell },
									new Label { Text = "ListView w/ EntryCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => "Entry Cell!"), ItemTemplate = entryCell },
									new Label { Text = "ListView w/ ImageCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3).Select(c => "coffee.png"), ItemTemplate = imageCell },
									new Label { Text = "ListView w/ ViewCell", FontSize = 10, TextColor = Color.DarkGray },
									new ListView { HorizontalOptions = hOptions, ItemsSource = Enumerable.Range(0, 3), ItemTemplate = viewCell },
								 },

				HorizontalOptions = hOptions
			};

			Content = new ScrollView
			{
				Content = stack,
				FlowDirection = direction
			};
		}

		T AddView<T>(Grid grid, ref int col, ref int row, int colSpan = 1) where T : View
		{
			var hOptions = LayoutOptions.Start;
			var vOptions = LayoutOptions.End;
			var margin = new Thickness(0, 10);
			var bgColor = Color.LightGray;

			T view = (T)Activator.CreateInstance(typeof(T));

			view.VerticalOptions = vOptions;
			view.HorizontalOptions = hOptions;
			view.Margin = margin;
			view.BackgroundColor = bgColor;

			var label = new Label { Text = $"({col},{row}) {typeof(T).ToString()}", FontSize = 10, TextColor = Color.DarkGray };

			if (colSpan > 1 && col > 0)
				NextCell(ref col, ref row, colSpan);

			grid.Children.Add(label, col, col + colSpan, row, row + 1);
			grid.Children.Add(view, col, col + colSpan, row, row + 1);

			NextCell(ref col, ref row, colSpan);

			return (T)view;
		}

		void NextCell(ref int col, ref int row, int colspan)
		{
			if (col == 0 && colspan == 1)
			{
				col = 1;
			}
			else
			{
				col = 0;
				row++;
			}
		}
	}
}