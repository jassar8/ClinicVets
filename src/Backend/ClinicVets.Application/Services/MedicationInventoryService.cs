using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Validation;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

public sealed class MedicationInventoryService
{
    private readonly IMedicationRepository _repository;

    public MedicationInventoryService(IMedicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Medication>> SearchAsync(string? query, string filterLabel)
    {
        var trimmed = query?.Trim();
        var all = await _repository.GetAllAsync();
        return all
            .Where(m => MedicationSearchFilter.Matches(m, trimmed, filterLabel))
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<(bool IsSuccess, string Message, Medication? Item)> AddAsync(
        string name,
        int stockQuantity,
        double unitPrice,
        DateTime expirationDate,
        string notes)
    {
        var validation = ValidateFields(name, stockQuantity, unitPrice);
        if (!validation.IsSuccess)
            return (false, validation.Message, null);

        var normalized = name.Trim();
        var existing = await _repository.GetByNameAsync(normalized);
        if (existing is not null)
            return (false, "A medicine with this name already exists.", null);

        var item = new Medication
        {
            Name = normalized,
            StockQuantity = stockQuantity,
            UnitPrice = unitPrice,
            ExpirationDate = expirationDate.Date,
            Notes = notes.Trim()
        };

        await _repository.AddAsync(item);
        return (true, "Medicine added successfully.", item);
    }

    public async Task<(bool IsSuccess, string Message)> UpdateAsync(
        int id,
        int stockQuantity,
        double unitPrice,
        DateTime expirationDate,
        string notes)
    {
        if (!MedicationInputValidation.IsValidStockQuantity(stockQuantity))
            return (false, "Stock quantity must be zero or greater.");

        if (!MedicationInputValidation.IsValidUnitPrice(unitPrice))
            return (false, "Unit price must be zero or greater.");

        var item = await _repository.GetByIdAsync(id);
        if (item is null)
            return (false, "Medicine not found.");

        item.StockQuantity = stockQuantity;
        item.UnitPrice = unitPrice;
        item.ExpirationDate = expirationDate.Date;
        item.Notes = notes.Trim();
        await _repository.UpdateAsync(item);
        return (true, "Medicine updated successfully.");
    }

    public async Task<(bool IsSuccess, string Message)> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted
            ? (true, "Medicine removed successfully.")
            : (false, "Medicine not found.");
    }

    private static (bool IsSuccess, string Message) ValidateFields(string name, int stockQuantity, double unitPrice)
    {
        if (!MedicationInputValidation.IsRequiredName(name))
            return (false, "Medicine name is required.");

        if (!MedicationInputValidation.IsValidStockQuantity(stockQuantity))
            return (false, "Stock quantity must be zero or greater.");

        if (!MedicationInputValidation.IsValidUnitPrice(unitPrice))
            return (false, "Unit price must be zero or greater.");

        return (true, string.Empty);
    }
}
