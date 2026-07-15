using Asp.Versioning;
using ExpenseTracker.Application.DTOs.Categories;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;
/// <summary>
/// Manages transaction categories for the authenticated user.
/// </summary>
/// <remarks>
/// Provides endpoints to create, retrieve, update, and delete income and expense categories.
/// </remarks>
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Tags("Categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Categories retrieved successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a category by its identifier.
    /// </summary>
    /// <param name="id">
    /// The category identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Category found.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Category not found.
    /// </response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    /// <summary>
    /// Creates a new transaction category.
    /// </summary>
    /// <param name="dto">
    /// The category information to create.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="201">
    /// Category created successfully.
    /// </response>
    /// <response code="400">
    /// Invalid category data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="409">
    /// A conflicting category already exists.
    /// </response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            category);
    }

    /// <summary>
    /// Deletes a transaction category.
    /// </summary>
    /// <param name="id">
    /// The category identifier.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="204">
    /// Category deleted successfully.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Category not found.
    /// </response>
    /// <response code="409">
    /// The category is referenced by related data.
    /// </response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Updates an existing transaction category.
    /// </summary>
    /// <param name="id">
    /// The category identifier.
    /// </param>
    /// <param name="dto">
    /// The updated category information.
    /// </param>
    /// <returns>
    /// The operation result.
    /// </returns>
    /// <response code="200">
    /// Category updated successfully.
    /// </response>
    /// <response code="400">
    /// Invalid category data.
    /// </response>
    /// <response code="401">
    /// Authentication is required.
    /// </response>
    /// <response code="404">
    /// Category not found.
    /// </response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
    {
        var category =await _categoryService.UpdateAsync(id, dto);
        if (category == null)
            return NotFound();
        return Ok(category);
    }
}
