using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CarouselPageNavigation
{
	public partial class MainPage : CarouselPage
	{
		TaskFactory _factory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
		IList<Message> _items;

		public MainPage()
		{
			InitializeComponent();

			Message initialSelectItem;

			_items = new ObservableCollection<Message>();
			_items.Add(NewMessage());
			_items.Add(initialSelectItem = NewMessage());
			_items.Add(NewMessage());

			ItemsSource = _items;

			// 初期値として3つ、2つ目を選択した状態で開く
			SelectedItem = initialSelectItem;

			CurrentPageChanged += (sender, e) =>
			{
				var currentItem = SelectedItem as Message;
				var currentIndex = _items.IndexOf(currentItem);

				// 次のアイテムを動的に追加してみる
				if (currentIndex == _items.Count() - 1)
				{
					// 同じコンテキストで呼び出すと若干カクつくので、StartNewで別コンテキストで追加する
					_factory.StartNew(() => _items.Add(NewMessage()));
				}
				// 前のアイテムを動的に追加してみる
				else if (currentIndex == 0)
				{
					_factory.StartNew(async () =>
					{
						// 単にStartNewで別コンテキストにするだけだと、若干のカクつきがあり、少し遅延させると良さげ
						await Task.Delay(100);
						_items.Insert(0, NewMessage());
					});
				}
			};

		}

		Message NewMessage()
		{
			return new Message { TapCommand = new Command<int>(OnItemClick) };
		}


		public void OnItemClick(int messageId)
		{
			var item = _items.FirstOrDefault((msg) => msg.Id == messageId);
			if (item != null)
			{
				var idx = _items.IndexOf(item);

				if (messageId % 2 == 0)
				{
					// 追加
					var newItem = NewMessage();
					var newIdx = idx;
					_items.Insert(newIdx, newItem);
					System.Diagnostics.Debug.WriteLine($"added {newItem.Id} to {newIdx}");
				}
				else
				{
					// 削除
					var removeIdx = idx;
					_items.RemoveAt(removeIdx);
					System.Diagnostics.Debug.WriteLine($"removed at {removeIdx}");

					// 表示中のものを削除した場合は、
					if (removeIdx == idx)
					{
						if (_items.Any())
						{
							// 1つ前か（先頭の場合は前がないので同じ位置）を選択する
							var next = _items[Math.Max(idx - 1, 0)];
							SelectedItem = next;
						}
						else
						{
							// TODO: 前の画面に戻る
						}
					}
				}
			}
		}
	}

	class Message : IDisposable
	{
		static int _index = 0;
		static System.Random _random = new System.Random();
		public int Id { get; set; }
		public string Name => Id.ToString();
		public Color Color { get; set; }
		public Command TapCommand { get; set; }

		internal Message()
		{
			Id = _index++;
			Color = Color.FromRgb(_random.Next(255), _random.Next(255), _random.Next(255));
		}

		public void Dispose()
		{
			// TODO: ItemsSource程度なら、別にリストを減らさなくても大丈夫かな？
		}
	}
}
