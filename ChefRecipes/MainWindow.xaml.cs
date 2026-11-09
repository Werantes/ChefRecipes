using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChefRecipes
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SeedDatabase();
            RefreshList();
        }

        private static void SeedDatabase()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            if (!db.Recipes.Any())
            {
                db.Recipes.Add(new Recipe
                {
                    Title = "Борщ",
                    Description = "Традиционный суп",
                    CookingTimeMinutes = 120,
                    Ingredients = new System.Collections.Generic.List<Ingredient>
                    {
                        new Ingredient { Name = "Свекла", Quantity = "2 шт" },
                        new Ingredient { Name = "Мясо", Quantity = "400г" }
                    },
                    Steps = new System.Collections.Generic.List<Step>
                    {
                        new Step { StepNumber = 1, Description = "Сварить бульон" },
                        new Step { StepNumber = 2, Description = "Добавить овощи" }
                    }
                });
                db.SaveChanges();
            }
        }

        private void RefreshList()
        {
            using var db = new AppDbContext();
            RecipesList.ItemsSource = db.Recipes.ToList();
            RecipesList.DisplayMemberPath = "Title";
        }

        private void RecipesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecipesList.SelectedItem is Recipe recipe)
            {
                ShowDetails(recipe.Id);
            }
        }

        private void ShowDetails(int recipeId)
        {
            using var db = new AppDbContext();
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId);
            if (recipe == null) return;

            db.Entry(recipe).Collection(r => r.Ingredients).Load();
            db.Entry(recipe).Collection(r => r.Steps).Load();

            TitleText.Text = recipe.Title;
            DescText.Text = recipe.Description;
            TimeText.Text = $"Время: {recipe.CookingTimeMinutes} мин";

            IngredientsList.ItemsSource = recipe.Ingredients
                .Select(i => $"{i.Name} - {i.Quantity}").ToList();

            StepsList.ItemsSource = recipe.Steps
                .OrderBy(s => s.StepNumber)
                .Select(s => $"{s.StepNumber}. {s.Description}").ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddRecipeWindow();
            if (window.ShowDialog() == true)
            {
                RefreshList();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (RecipesList.SelectedItem is Recipe selected)
            {
                int id = selected.Id;
                var window = new AddRecipeWindow(id);
                if (window.ShowDialog() == true)
                {
                    RefreshList();
                    SelectRecipeById(id);
                }
            }
            else
            {
                MessageBox.Show("Выберите рецепт для редактирования!");
            }
        }

        private void SelectRecipeById(int id)
        {
            foreach (var item in RecipesList.Items)
            {
                if (item is Recipe r && r.Id == id)
                {
                    RecipesList.SelectedItem = r;
                    return;
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (RecipesList.SelectedItem is Recipe selected)
            {
                if (MessageBox.Show("Удалить?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using var db = new AppDbContext();
                    var recipe = db.Recipes.FirstOrDefault(r => r.Id == selected.Id);
                    if (recipe != null)
                    {
                        db.Recipes.Remove(recipe);
                        db.SaveChanges();
                    }
                    RefreshList();
                    ClearDetails();
                }
            }
        }

        private void ClearDetails()
        {
            TitleText.Text = "";
            DescText.Text = "";
            TimeText.Text = "";
            IngredientsList.ItemsSource = null;
            StepsList.ItemsSource = null;
        }
    }
}