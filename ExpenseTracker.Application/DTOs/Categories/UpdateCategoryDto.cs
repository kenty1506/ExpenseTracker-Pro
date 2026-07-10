namespace ExpenseTracker.Application.DTOs.Categories;

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = "#6366F1";

    public string Icon { get; set; } = "category";
}