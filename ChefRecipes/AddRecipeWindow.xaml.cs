using System;
using System.Linq;
using System.Windows;

namespace ChefRecipes
{
    public partial class AddRecipeWindow : Window
    {
        private readonly int? editingRecipeId;

        public AddRecipeWindow(int? recipeId = null)
        {
            InitializeComponent();
            editingRecipeId = recipeId;
            Owner = Application.Current.MainWindow;

            if (editingRecipeId != null)
            {
                Title = "Редактировать рецепт";
                LoadRecipe(editingRecipeId.Value);
            }
            else
            {
                Title = "Добавить рецепт";
            }
        }

        private void LoadRecipe(int recipeId)
        {
            using var db = new AppDbContext();
            var recipe = db.Recipes.FirstOrDefault(r => r.Id == recipeId);
            if (recipe == null) return;

            db.Entry(recipe).Collection(r => r.Ingredients).Load();
            db.Entry(recipe).Collection(r => r.Steps).Load();

            TitleBox.Text = recipe.Title;
            DescBox.Text = recipe.Description;
            TimeBox.Text = recipe.CookingTimeMinutes.ToString();
            IngBox.Text = string.Join(Environment.NewLine,
                recipe.Ingredients.Select(i => $"{i.Name} - {i.Quantity}"));
            StepsBox.Text = string.Join(Environment.NewLine,
                recipe.Steps.OrderBy(s => s.StepNumber).Select(s => s.Description));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            if (!int.TryParse(TimeBox.Text, out int minutes) || minutes < 0)
            {
                MessageBox.Show("Введите корректное время в минутах!");
                return;
            }

            try
            {
                using var db = new AppDbContext();

                Recipe recipe;
                if (editingRecipeId == null)
                {
                    recipe = new Recipe();
                    db.Recipes.Add(recipe);
                }
                else
                {
                    recipe = db.Recipes.FirstOrDefault(r => r.Id == editingRecipeId.Value)!;
                    if (recipe == null)
                    {
                        MessageBox.Show("Рецепт не найден (возможно, был удалён).");
                        return;
                    }

                    db.Entry(recipe).Collection(r => r.Ingredients).Load();
                    db.Entry(recipe).Collection(r => r.Steps).Load();

                    db.Ingredients.RemoveRange(recipe.Ingredients.ToList());
                    db.Steps.RemoveRange(recipe.Steps.ToList());
                    recipe.Ingredients.Clear();
                    recipe.Steps.Clear();
                }

                recipe.Title = TitleBox.Text.Trim();
                recipe.Description = DescBox.Text.Trim();
                recipe.CookingTimeMinutes = minutes;

                foreach (var line in IngBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Split('-');
                    recipe.Ingredients.Add(new Ingredient
                    {
                        Name = parts[0].Trim(),
                        Quantity = parts.Length > 1 ? parts[1].Trim() : ""
                    });
                }

                var stepLines = StepsBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < stepLines.Length; i++)
                {
                    recipe.Steps.Add(new Step
                    {
                        StepNumber = i + 1,
                        Description = stepLines[i].Trim()
                    });
                }

                db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}