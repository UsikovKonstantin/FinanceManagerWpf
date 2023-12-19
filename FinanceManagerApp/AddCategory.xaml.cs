using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace Finance_Manager;

/// <summary>
/// Форма для добавления/удаления категорий.
/// </summary>
public partial class AddCategory : UiWindow
{
	/// <summary>
	/// Родительское окно.
	/// </summary>
	private readonly MainWindow ParentWindow;

	/// <summary>
	/// Кисть элементов управления по умолчанию.
	/// </summary>
	private readonly Brush StandartBrush;

	public AddCategory(MainWindow parent)
    {
		ParentWindow = parent;
		InitializeComponent();
        RefreshStackPanelCategories();
		StandartBrush = textBoxNewCategoryName.Background;
    }

	/// <summary>
	/// Клик по кнопке "Добавить категорию".
	/// </summary>
	private void ButtonAddCategoryClick(object sender, RoutedEventArgs e)
    {
		// Проверяем, что ввели не пустую строку
		if (textBoxNewCategoryName.Text.Trim() == "")
		{
			textBoxNewCategoryName.Background = Brushes.Red;
			textBoxNewCategoryName.ToolTip = "Нужно указать название категории.";
			return;
		}
		textBoxNewCategoryName.Background = StandartBrush;
		textBoxNewCategoryName.ToolTip = null;

		// Добавить категорию
		try
        {
            ParentWindow.Controller.AddCategory(textBoxNewCategoryName.Text.Trim());
        }
        catch (Exception exception)
        {
			MessageBox messageBoxError = new MessageBox
			{
				Title = "Ошибка",
				Content = exception.Message,
				ShowFooter = false
			};
			messageBoxError.ShowDialog();
		}

		ParentWindow.RefreshData();
		RefreshStackPanelCategories();
    }

	/// <summary>
	/// Обновить содержимое stackPanel с категориями.
	/// </summary>
	private void RefreshStackPanelCategories()
    {
        stackPanelCategories.Children.Clear();
        DataTable dataTableCategories = ParentWindow.Controller.Categories;
        for (var i = 2; i < dataTableCategories.Rows.Count; i++)
        {
            DataRow row = dataTableCategories.Rows[i];
            string? categoryName = row[1].ToString();
            if (categoryName != null)
				CreateControlsForCategory(categoryName);
        }
        stackPanelCategories.UpdateLayout();
    }

    /// <summary>
    /// Создание элемента управления для категории.
    /// </summary>
    private void CreateControlsForCategory(string categoryName)
    {
        // Основа для элементов управления
		Grid grid = new Grid { Margin = new Thickness(5) };

	    // Добавление колонок
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

		// TextBlock для вывода названия категории
		TextBlock textBlockCategoryName = new TextBlock
        {
            Text = categoryName,
            Margin = new Thickness(5)
		};

        // Кнопка для удаления категории
		Button buttonRemoveCategory = new Button
		{
			Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Remove.png")) },
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
		};
        buttonRemoveCategory.Click += ButtonRemoveCategoryClick;

		// Добавление элементов на grid
		Grid.SetColumn(textBlockCategoryName, 0);
        Grid.SetColumn(buttonRemoveCategory, 1);
        grid.Children.Add(textBlockCategoryName);
        grid.Children.Add(buttonRemoveCategory);

		// Добавление созданного grid на stackPanel
		stackPanelCategories.Children.Add(grid);
    }

	/// <summary>
	/// Клик по кнопке "Удалить категорию".
	/// </summary>
	private void ButtonRemoveCategoryClick(object sender, RoutedEventArgs e)
	{
		Button? btnRemove = sender as Button;
		Grid? grid = VisualTreeHelper.GetParent(btnRemove) as Grid;
		TextBlock? textBlockCategory = grid?.Children[0] as TextBlock;
		string? categoryName = textBlockCategory?.Text;
		if (categoryName != null)
			ParentWindow.Controller.RemoveCategory(categoryName);

		ParentWindow.RefreshData();
		RefreshStackPanelCategories();
	}
}
