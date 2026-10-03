using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFitMeal.Data.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SavedAt",
                table: "FavoriteMeals",
                newName: "DateSaved");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "FavoriteMeals",
                newName: "MealType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MealType",
                table: "FavoriteMeals",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "DateSaved",
                table: "FavoriteMeals",
                newName: "SavedAt");
        }
    }
}
