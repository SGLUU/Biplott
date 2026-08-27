using System.Text;
using Biplott.Application.DTOs;
using Biplott.Core.Entities;
using Biplott.Infrastructure.Data;
using Biplott.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Biplott.Tests;

public class ContentImportServiceTests
{
    private static (BiplottDbContext db, ContentImportService service, Theme theme, Trait trait) CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<BiplottDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new BiplottDbContext(options);
        var theme = new Theme { Code = "THEME_CAREER", Name = "Sự nghiệp", IsActive = true };
        var trait = new Trait { Code = "Independence", Name = "Độc lập", IsActive = true };
        db.Themes.Add(theme);
        db.Traits.Add(trait);
        db.SaveChanges();

        var service = new ContentImportService(db, NullLogger<ContentImportService>.Instance);
        return (db, service, theme, trait);
    }

    [Fact]
    public async Task ValidateImport_ValidCsv_ShouldReturnValidResultWithPreview()
    {
        var (_, service, _, _) = CreateTestContext();
        var csvContent = "ThemeCode,QuestionType,QuestionText,Subtitle,Choice1Text,Choice1Traits,Choice2Text,Choice2Traits\n" +
                         "THEME_CAREER,SingleChoice,Chọn cách đi làm?,Hàng ngày,Đi xe buýt,Independence:0.8,Đi bộ,Independence:0.5\n";

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var result = await service.ValidateImportFileAsync(ms, "questions.csv");

        Assert.True(result.IsValid);
        Assert.Equal(1, result.TotalRows);
        Assert.Equal(1, result.ValidCount);
        Assert.Empty(result.Errors);
        Assert.Single(result.PreviewItems);
        Assert.Equal("Chọn cách đi làm?", result.PreviewItems[0].Content);
        Assert.Equal(2, result.PreviewItems[0].Choices.Count);
    }

    [Fact]
    public async Task ValidateImport_InvalidThemeAndInvalidWeight_ShouldReportErrors()
    {
        var (_, service, _, _) = CreateTestContext();
        var csvContent = "ThemeCode,QuestionType,QuestionText,Subtitle,Choice1Text,Choice1Traits,Choice2Text,Choice2Traits\n" +
                         "THEME_NONEXISTENT,SingleChoice,Nội dung hợp lệ,,Lựa chọn 1,Independence:1.5,Lựa chọn 2,Independence:0.5\n";

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var result = await service.ValidateImportFileAsync(ms, "questions.csv");

        Assert.False(result.IsValid);
        Assert.Equal(1, result.InvalidCount);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Field == "ThemeCode");
        Assert.Contains(result.Errors, e => e.Field.Contains("Weight"));
    }

    [Fact]
    public async Task ValidateImport_FormulaInjection_ShouldBeSanitized()
    {
        var (db, service, theme, _) = CreateTestContext();
        var jsonContent = @"[
            {
                ""themeCode"": ""THEME_CAREER"",
                ""questionType"": ""SingleChoice"",
                ""content"": ""=SUM(1,2)"",
                ""choices"": [
                    { ""content"": ""+CMD('calc')"" },
                    { ""content"": ""@malicious"" }
                ]
            }
        ]";

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
        var validation = await service.ValidateImportFileAsync(ms, "test.json");
        Assert.True(validation.IsValid);

        // Confirm import
        var confirmReq = new ImportConfirmRequest { Items = validation.PreviewItems };
        var confirmResult = await service.ConfirmImportAsync(confirmReq);

        Assert.True(confirmResult.Success);
        Assert.Equal(1, confirmResult.ImportedQuestionsCount);

        var savedQ = await db.Questions.Include(q => q.Choices).FirstAsync(q => q.ThemeId == theme.Id);
        Assert.StartsWith("'", savedQ.Content); // Sanitized leading '='
        Assert.StartsWith("'", savedQ.Choices[0].Content); // Sanitized leading '+'
        Assert.StartsWith("'", savedQ.Choices[1].Content); // Sanitized leading '@'
    }

    [Fact]
    public async Task GenerateTemplate_CsvAndJson_ShouldReturnValidPayloads()
    {
        var (_, service, _, _) = CreateTestContext();

        var (csvBytes, csvType, csvName) = await service.GenerateTemplateAsync("csv");
        Assert.NotEmpty(csvBytes);
        Assert.Equal("text/csv; charset=utf-8", csvType);
        Assert.Equal("biplott_questions_template.csv", csvName);

        var (jsonBytes, jsonType, jsonName) = await service.GenerateTemplateAsync("json");
        Assert.NotEmpty(jsonBytes);
        Assert.Equal("application/json", jsonType);
        Assert.Equal("biplott_questions_template.json", jsonName);
    }
}