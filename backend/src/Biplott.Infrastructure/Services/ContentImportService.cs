using System.Globalization;
using System.Text;
using System.Text.Json;
using Biplott.Application.DTOs;
using Biplott.Application.Services;
using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Biplott.Infrastructure.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;

namespace Biplott.Infrastructure.Services;

public class ContentImportService : IContentImportService
{
    private readonly BiplottDbContext _dbContext;
    private readonly ILogger<ContentImportService> _logger;

    private static readonly Dictionary<string, List<ImportQuestionPreviewDto>> SessionCache = new();
    private static readonly object SessionLock = new();

    public ContentImportService(BiplottDbContext dbContext, ILogger<ContentImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ImportValidationResultDto> ValidateImportFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        List<ImportQuestionPreviewDto> parsedQuestions;

        try
        {
            parsedQuestions = ext switch
            {
                ".csv" => ParseCsv(fileStream),
                ".json" => await ParseJsonAsync(fileStream, cancellationToken),
                ".xlsx" or ".xls" => ParseExcel(fileStream),
                _ => throw new ArgumentException($"Định dạng file '{ext}' không được hỗ trợ. Vui lòng sử dụng .csv, .xlsx hoặc .json.")
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            return new ImportValidationResultDto
            {
                IsValid = false,
                TotalRows = 0,
                ValidCount = 0,
                InvalidCount = 0,
                Errors = new List<ImportRowErrorDto>
                {
                    new() { RowIndex = 0, Field = "File", Message = $"Lỗi đọc file: {ex.Message}" }
                }
            };
        }

        var themes = await _dbContext.Themes.AsNoTracking().ToDictionaryAsync(t => t.Code.ToUpperInvariant(), cancellationToken);
        var traits = await _dbContext.Traits.AsNoTracking().ToDictionaryAsync(t => t.Code.ToLowerInvariant(), cancellationToken);

        var errors = new List<ImportRowErrorDto>();
        int validCount = 0;
        int invalidCount = 0;

        for (int i = 0; i < parsedQuestions.Count; i++)
        {
            var q = parsedQuestions[i];
            var qErrors = new List<string>();

            // 1. Theme check
            if (string.IsNullOrWhiteSpace(q.ThemeCode))
            {
                var err = "Mã chủ đề (ThemeCode) không được để trống.";
                qErrors.Add(err);
                errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = "ThemeCode", Message = err });
            }
            else if (!themes.ContainsKey(q.ThemeCode.ToUpperInvariant()))
            {
                var err = $"Chủ đề '{q.ThemeCode}' không tồn tại trong hệ thống.";
                qErrors.Add(err);
                errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = "ThemeCode", Message = err });
            }

            // 2. Question type check
            if (!Enum.TryParse<QuestionType>(q.QuestionType, true, out _))
            {
                var err = $"Loại câu hỏi '{q.QuestionType}' không hợp lệ. Các loại hỗ trợ: SingleChoice, ThisOrThat, Scenario, QuickInstinct.";
                qErrors.Add(err);
                errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = "QuestionType", Message = err });
            }

            // 3. Question content check
            if (string.IsNullOrWhiteSpace(q.Content))
            {
                var err = "Nội dung câu hỏi không được để trống.";
                qErrors.Add(err);
                errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = "Content", Message = err });
            }

            // 4. Choices check
            if (q.Choices == null || q.Choices.Count < 2)
            {
                var err = $"Câu hỏi phải có ít nhất 2 lựa chọn (hiện có: {q.Choices?.Count ?? 0}).";
                qErrors.Add(err);
                errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = "Choices", Message = err });
            }
            else
            {
                for (int cIdx = 0; cIdx < q.Choices.Count; cIdx++)
                {
                    var choice = q.Choices[cIdx];
                    if (string.IsNullOrWhiteSpace(choice.Content))
                    {
                        var err = $"Lựa chọn #{cIdx + 1} có nội dung rỗng.";
                        qErrors.Add(err);
                        errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = $"Choice{cIdx + 1}", Message = err });
                    }

                    // Trait check
                    foreach (var ct in choice.Traits)
                    {
                        if (string.IsNullOrWhiteSpace(ct.TraitCode)) continue;

                        if (!traits.ContainsKey(ct.TraitCode.ToLowerInvariant()))
                        {
                            var err = $"Lựa chọn #{cIdx + 1}: Thuộc tính '{ct.TraitCode}' không tồn tại trong hệ thống.";
                            qErrors.Add(err);
                            errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = $"Choice{cIdx + 1}.Traits", Message = err });
                        }

                        if (ct.Weight < 0.0 || ct.Weight > 1.0)
                        {
                            var err = $"Lựa chọn #{cIdx + 1}: Trọng số thuộc tính '{ct.TraitCode}' ({ct.Weight}) phải nằm trong khoảng từ 0.0 đến 1.0.";
                            qErrors.Add(err);
                            errors.Add(new ImportRowErrorDto { RowIndex = q.RowIndex, Field = $"Choice{cIdx + 1}.Weight", Message = err });
                        }
                    }
                }
            }

            if (qErrors.Count > 0)
            {
                q.IsValid = false;
                q.Errors = qErrors;
                invalidCount++;
            }
            else
            {
                q.IsValid = true;
                validCount++;
            }
        }

        var sessionId = Guid.NewGuid().ToString("N");
        lock (SessionLock)
        {
            SessionCache[sessionId] = parsedQuestions;
        }

        return new ImportValidationResultDto
        {
            IsValid = invalidCount == 0 && validCount > 0,
            TotalRows = parsedQuestions.Count,
            ValidCount = validCount,
            InvalidCount = invalidCount,
            Errors = errors,
            PreviewItems = parsedQuestions,
            ImportSessionId = sessionId
        };
    }

    public async Task<ImportConfirmResponseDto> ConfirmImportAsync(ImportConfirmRequest request, CancellationToken cancellationToken = default)
    {
        List<ImportQuestionPreviewDto>? items = request.Items;

        if (items == null && !string.IsNullOrWhiteSpace(request.ImportSessionId))
        {
            lock (SessionLock)
            {
                if (SessionCache.TryGetValue(request.ImportSessionId, out var cached))
                {
                    items = cached;
                }
            }
        }

        if (items == null || items.Count == 0)
        {
            throw new ArgumentException("Không có dữ liệu câu hỏi nào để nhập.");
        }

        var validItems = items.Where(i => i.IsValid).ToList();
        if (validItems.Count == 0)
        {
            throw new ArgumentException("Tất cả các dòng dữ liệu đều không hợp lệ. Vui lòng kiểm tra lại lỗi và thử lại.");
        }

        var themes = await _dbContext.Themes.ToDictionaryAsync(t => t.Code.ToUpperInvariant(), cancellationToken);
        var traits = await _dbContext.Traits.ToDictionaryAsync(t => t.Code.ToLowerInvariant(), cancellationToken);

        using var transaction = _dbContext.Database.IsRelational() ? await _dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        try
        {
            var now = DateTime.UtcNow;
            int importedQuestions = 0;
            int importedChoices = 0;

            foreach (var item in validItems)
            {
                if (!themes.TryGetValue(item.ThemeCode.ToUpperInvariant(), out var theme))
                    continue;

                Enum.TryParse<QuestionType>(item.QuestionType, true, out var qType);

                var question = new Question
                {
                    ThemeId = theme.Id,
                    QuestionType = qType == 0 ? QuestionType.SingleChoice : qType,
                    Content = SanitizeFormulaInjection(item.Content),
                    Subtitle = !string.IsNullOrWhiteSpace(item.Subtitle) ? SanitizeFormulaInjection(item.Subtitle) : null,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                int orderIdx = 0;
                foreach (var c in item.Choices)
                {
                    var choice = new QuestionChoice
                    {
                        Content = SanitizeFormulaInjection(c.Content),
                        SubContent = !string.IsNullOrWhiteSpace(c.SubContent) ? SanitizeFormulaInjection(c.SubContent) : null,
                        OrderIndex = orderIdx++,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    foreach (var t in c.Traits)
                    {
                        if (traits.TryGetValue(t.TraitCode.ToLowerInvariant(), out var traitEntity))
                        {
                            choice.ChoiceTraits.Add(new ChoiceTrait
                            {
                                TraitId = traitEntity.Id,
                                Weight = Math.Clamp(t.Weight, 0.0, 1.0)
                            });
                        }
                    }

                    question.Choices.Add(choice);
                    importedChoices++;
                }

                await _dbContext.Questions.AddAsync(question, cancellationToken);
                importedQuestions++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            _logger.LogInformation("Successfully imported {QuestionsCount} questions and {ChoicesCount} choices.", importedQuestions, importedChoices);

            return new ImportConfirmResponseDto
            {
                Success = true,
                ImportedQuestionsCount = importedQuestions,
                ImportedChoicesCount = importedChoices,
                Message = $"Đã nhập thành công {importedQuestions} câu hỏi và {importedChoices} lựa chọn vào hệ thống."
            };
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            _logger.LogError(ex, "Transaction rolled back during content import.");
            throw new InvalidOperationException($"Lỗi khi lưu dữ liệu vào cơ sở dữ liệu: {ex.Message}");
        }
    }

    public async Task<(byte[] FileBytes, string ContentType, string FileName)> GenerateTemplateAsync(string format, CancellationToken cancellationToken = default)
    {
        var fmt = (format ?? "csv").ToLowerInvariant();

        var sampleQuestions = new List<ImportQuestionPreviewDto>
        {
            new()
            {
                RowIndex = 1,
                ThemeCode = "THEME_CAREER",
                QuestionType = "SingleChoice",
                Content = "Sếp giao deadline gấp lúc 17h30 chiều thứ Sáu, bạn sẽ:",
                Subtitle = "Tình huống công sở nan giải",
                Choices = new List<ImportChoicePreviewDto>
                {
                    new()
                    {
                        Content = "Âm thầm tắt máy, vờ như không thấy tin nhắn",
                        SubContent = "Bình yên là trên hết",
                        Traits = new List<ImportTraitPreviewDto>
                        {
                            new() { TraitCode = "Independence", Weight = 0.9 },
                            new() { TraitCode = "Intuition", Weight = 0.5 }
                        }
                    },
                    new()
                    {
                        Content = "Ngồi lại cày xuyên đêm kèm gửi sếp hóa đơn tăng ca",
                        SubContent = "Tất cả vì tiền thưởng",
                        Traits = new List<ImportTraitPreviewDto>
                        {
                            new() { TraitCode = "RiskTolerance", Weight = 0.8 },
                            new() { TraitCode = "Stability", Weight = 0.4 }
                        }
                    },
                    new()
                    {
                        Content = "Rủ sếp đi nhậu tâm sự mỏng",
                        SubContent = "Lấy lòng là chiến thuật",
                        Traits = new List<ImportTraitPreviewDto>
                        {
                            new() { TraitCode = "Emotion", Weight = 0.85 },
                            new() { TraitCode = "Exploration", Weight = 0.6 }
                        }
                    }
                }
            },
            new()
            {
                RowIndex = 2,
                ThemeCode = "THEME_LOVE",
                QuestionType = "ThisOrThat",
                Content = "Người yêu cũ mời đi đám cưới, phong bì bao nhiêu?",
                Subtitle = "Chuyện khó xử của tình yêu",
                Choices = new List<ImportChoicePreviewDto>
                {
                    new()
                    {
                        Content = "Đi 500k và ăn cỗ nhiệt tình lấy lại vốn",
                        Traits = new List<ImportTraitPreviewDto>
                        {
                            new() { TraitCode = "Stability", Weight = 0.8 }
                        }
                    },
                    new()
                    {
                        Content = "Gửi phong bì rỗng kèm lời chúc chân thành",
                        Traits = new List<ImportTraitPreviewDto>
                        {
                            new() { TraitCode = "Independence", Weight = 0.95 }
                        }
                    }
                }
            }
        };

        if (fmt == "json")
        {
            var json = JsonSerializer.Serialize(sampleQuestions, new JsonSerializerOptions { WriteIndented = true });
            return (Encoding.UTF8.GetBytes(json), "application/json", "biplott_questions_template.json");
        }

        if (fmt == "xlsx")
        {
            var excelRows = MapToTemplateRows(sampleQuestions);
            using var ms = new MemoryStream();
            await MiniExcel.SaveAsAsync(ms, excelRows, cancellationToken: cancellationToken);
            return (ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "biplott_questions_template.xlsx");
        }

        // Default: CSV
        {
            var csvRows = MapToTemplateRows(sampleQuestions);
            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, new UTF8Encoding(true), leaveOpen: true))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                await csv.WriteRecordsAsync(csvRows, cancellationToken);
                await writer.FlushAsync(cancellationToken);
            }
            return (ms.ToArray(), "text/csv; charset=utf-8", "biplott_questions_template.csv");
        }
    }

    private static List<ImportQuestionPreviewDto> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
        });

        var records = csv.GetRecords<dynamic>().ToList();
        var list = new List<ImportQuestionPreviewDto>();
        int rowIdx = 1;

        foreach (IDictionary<string, object> row in records)
        {
            rowIdx++;
            var q = ParseFlatRow(row, rowIdx);
            if (q != null) list.Add(q);
        }

        return list;
    }

    private static async Task<List<ImportQuestionPreviewDto>> ParseJsonAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var items = JsonSerializer.Deserialize<List<ImportQuestionPreviewDto>>(json, options);

        if (items == null) return new List<ImportQuestionPreviewDto>();

        for (int i = 0; i < items.Count; i++)
        {
            items[i].RowIndex = i + 1;
        }

        return items;
    }

    private static List<ImportQuestionPreviewDto> ParseExcel(Stream stream)
    {
        var rows = stream.Query(useHeaderRow: true).ToList();
        var list = new List<ImportQuestionPreviewDto>();
        int rowIdx = 1;

        foreach (IDictionary<string, object> row in rows)
        {
            rowIdx++;
            var q = ParseFlatRow(row, rowIdx);
            if (q != null) list.Add(q);
        }

        return list;
    }

    private static ImportQuestionPreviewDto? ParseFlatRow(IDictionary<string, object> row, int rowIdx)
    {
        string GetVal(string key)
        {
            foreach (var k in row.Keys)
            {
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                {
                    return row[k]?.ToString()?.Trim() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        var themeCode = GetVal("ThemeCode");
        var qType = GetVal("QuestionType");
        var content = GetVal("QuestionText");
        if (string.IsNullOrWhiteSpace(content)) content = GetVal("Content");
        var subtitle = GetVal("Subtitle");

        if (string.IsNullOrWhiteSpace(themeCode) && string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var choices = new List<ImportChoicePreviewDto>();

        for (int i = 1; i <= 6; i++)
        {
            var cText = GetVal($"Choice{i}Text");
            if (string.IsNullOrWhiteSpace(cText)) cText = GetVal($"Choice{i}");
            if (string.IsNullOrWhiteSpace(cText)) continue;

            var cSub = GetVal($"Choice{i}Sub");
            var cTraitsStr = GetVal($"Choice{i}Traits");

            var traits = ParseTraitsString(cTraitsStr);
            choices.Add(new ImportChoicePreviewDto
            {
                Content = cText,
                SubContent = !string.IsNullOrWhiteSpace(cSub) ? cSub : null,
                Traits = traits
            });
        }

        return new ImportQuestionPreviewDto
        {
            RowIndex = rowIdx,
            ThemeCode = themeCode,
            QuestionType = string.IsNullOrWhiteSpace(qType) ? "SingleChoice" : qType,
            Content = content,
            Subtitle = !string.IsNullOrWhiteSpace(subtitle) ? subtitle : null,
            Choices = choices
        };
    }

    private static List<ImportTraitPreviewDto> ParseTraitsString(string traitsStr)
    {
        var result = new List<ImportTraitPreviewDto>();
        if (string.IsNullOrWhiteSpace(traitsStr)) return result;

        var pairs = traitsStr.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split(new[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var traitCode = parts[0].Trim();
                if (double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var weight))
                {
                    result.Add(new ImportTraitPreviewDto { TraitCode = traitCode, Weight = weight });
                }
            }
        }
        return result;
    }

    public class QuestionTemplateRow
    {
        public string ThemeCode { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Choice1Text { get; set; } = string.Empty;
        public string Choice1Sub { get; set; } = string.Empty;
        public string Choice1Traits { get; set; } = string.Empty;
        public string Choice2Text { get; set; } = string.Empty;
        public string Choice2Sub { get; set; } = string.Empty;
        public string Choice2Traits { get; set; } = string.Empty;
        public string Choice3Text { get; set; } = string.Empty;
        public string Choice3Sub { get; set; } = string.Empty;
        public string Choice3Traits { get; set; } = string.Empty;
        public string Choice4Text { get; set; } = string.Empty;
        public string Choice4Sub { get; set; } = string.Empty;
        public string Choice4Traits { get; set; } = string.Empty;
    }

    private static List<QuestionTemplateRow> MapToTemplateRows(List<ImportQuestionPreviewDto> questions)
    {
        var rows = new List<QuestionTemplateRow>();
        foreach (var q in questions)
        {
            var row = new QuestionTemplateRow
            {
                ThemeCode = q.ThemeCode,
                QuestionType = q.QuestionType,
                QuestionText = q.Content,
                Subtitle = q.Subtitle ?? string.Empty
            };

            for (int i = 0; i < q.Choices.Count && i < 4; i++)
            {
                var c = q.Choices[i];
                var traitsStr = string.Join(";", c.Traits.Select(t => $"{t.TraitCode}:{t.Weight.ToString("0.##", CultureInfo.InvariantCulture)}"));

                switch (i)
                {
                    case 0:
                        row.Choice1Text = c.Content;
                        row.Choice1Sub = c.SubContent ?? string.Empty;
                        row.Choice1Traits = traitsStr;
                        break;
                    case 1:
                        row.Choice2Text = c.Content;
                        row.Choice2Sub = c.SubContent ?? string.Empty;
                        row.Choice2Traits = traitsStr;
                        break;
                    case 2:
                        row.Choice3Text = c.Content;
                        row.Choice3Sub = c.SubContent ?? string.Empty;
                        row.Choice3Traits = traitsStr;
                        break;
                    case 3:
                        row.Choice4Text = c.Content;
                        row.Choice4Sub = c.SubContent ?? string.Empty;
                        row.Choice4Traits = traitsStr;
                        break;
                }
            }

            rows.Add(row);
        }
        return rows;
    }

    private static string SanitizeFormulaInjection(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var trimmed = input.Trim();
        if (trimmed.StartsWith("=") || trimmed.StartsWith("+") || trimmed.StartsWith("-") || trimmed.StartsWith("@"))
        {
            return "'" + trimmed;
        }
        return trimmed;
    }
}