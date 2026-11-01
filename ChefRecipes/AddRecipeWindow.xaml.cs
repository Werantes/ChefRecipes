using System;
using System.Linq;
using System.Windows;

namespace ChefRecipes
{
    public partial class AddRecipeWindow : Window
    {
        private AppDbContext db;

        public AddRecipeWindow(AppDbContext db)
        {
            InitializeComponent();
            this.db = db;
            this.Owner = Application.Current.MainWindow;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            int minutes = 0;
            int.TryParse(TimeBox.Text, out minutes);

            var recipe = new Recipe
            {
                Title = TitleBox.Text.Trim(),
                Description = DescBox.Text.Trim(),
                CookingTimeMinutes = minutes,
                Ingredients = new System.Collections.Generic.List<Ingredient>(),
                Steps = new System.Collections.Generic.List<Step>()
            };

            var ingredients = IngBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ing in ingredients)
            {
                var parts = ing.Split('-');
                recipe.Ingredients.Add(new Ingredient
                {
                    Name = parts[0].Trim(),
                    Quantity = parts.Length > 1 ? parts[1].Trim() : ""
                });
            }

            var steps = StepsBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < steps.Length; i++)
            {
                recipe.Steps.Add(new Step
                {
                    StepNumber = i + 1,
                    Description = steps[i].Trim()
                });
            }

            db.Recipes.Add(recipe);
            db.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}