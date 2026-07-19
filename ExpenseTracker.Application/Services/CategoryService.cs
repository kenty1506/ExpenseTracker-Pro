using ExpenseTracker.Application.DTOs.Categories;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories =
            await _categoryRepository.GetAllAsync(
                _currentUserService.UserId);

        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category =
            await _categoryRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

        return category == null
            ? null
            : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto dto)
    {
        var existingCategory =
            await _categoryRepository.GetByNameAsync(
                dto.Name.Trim(),
                _currentUserService.UserId);

        if (existingCategory != null)
        {
            throw new ArgumentException(
                $"A category named '{dto.Name}' already exists.");
        }

        var category = new Category
        {
            UserId = _currentUserService.UserId,
            Name = dto.Name.Trim(),
            Color = dto.Color,
            Icon = dto.Icon
        };

        var created =
            await _categoryRepository.CreateAsync(category);

        return MapToDto(created);
    }

    public async Task<CategoryDto?> UpdateAsync(
        int id,
        UpdateCategoryDto dto)
    {
        var category =
            await _categoryRepository.GetByIdAsync(
                id,
                _currentUserService.UserId);

        if (category == null)
            return null;

        var duplicate =
            await _categoryRepository.GetByNameAsync(
                dto.Name.Trim(),
                _currentUserService.UserId);

        if (duplicate != null &&
            duplicate.Id != id)
        {
            throw new ArgumentException(
                $"A category named '{dto.Name}' already exists.");
        }

        category.Name = dto.Name.Trim();
        category.Color = dto.Color;
        category.Icon = dto.Icon;
        category.UpdatedAt = DateTime.UtcNow;

        var updated =
            await _categoryRepository.UpdateAsync(category);

        return updated
            ? MapToDto(category)
            : null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _categoryRepository.DeleteAsync(
            id,
            _currentUserService.UserId);
    }

    private static CategoryDto MapToDto(
        Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Color = category.Color,
            Icon = category.Icon
        };
    }
}