using ExpenseTracker.Application.DTOs.Categories;

namespace ExpenseTracker.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();

    Task<CategoryDto?> GetByIdAsync(int id);

    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);

    Task<bool> DeleteAsync(int id);

    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);
}