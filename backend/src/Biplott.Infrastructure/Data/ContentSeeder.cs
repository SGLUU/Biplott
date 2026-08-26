using Biplott.Core.Entities;
using Biplott.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biplott.Infrastructure.Data;

public static class ContentSeeder
{
    public static async Task SeedContentAsync(BiplottDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        // 1. Seed Traits
        if (!await context.Traits.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seeding default Traits...");
            var traits = new List<Trait>
            {
                new() { Code = "RiskTolerance", Name = "Liều lĩnh", Description = "Dám chơi lớn, không ngại rủi ro", Category = "Personality" },
                new() { Code = "ChaosEnergy", Name = "Năng lượng bất ổn", Description = "Thích nổi loạn, phá vỡ trật tự", Category = "Personality" },
                new() { Code = "Intuition", Name = "Trực giác", Description = "Tin vào linh cảm và giác quan thứ 6", Category = "Spirituality" },
                new() { Code = "Stability", Name = "Ổn định", Description = "Cẩn trọng, an toàn, kiên định", Category = "Lifestyle" },
                new() { Code = "Order", Name = "Trật tự", Description = "Ngăn nắp, logic, có kế hoạch", Category = "Mindset" },
                new() { Code = "Patience", Name = "Kiên nhẫn", Description = "Trầm tĩnh, biết chờ đợi thời cơ", Category = "Mindset" },
                new() { Code = "Independence", Name = "Độc lập", Description = "Tự do, thích tự chủ quyết định", Category = "Personality" },
                new() { Code = "Exploration", Name = "Khám phá", Description = "Tò mò, ham học hỏi và trải nghiệm mới", Category = "Lifestyle" },
                new() { Code = "Emotion", Name = "Cảm xúc", Description = "Nhạy cảm, sống theo con tim", Category = "Emotional" },
                new() { Code = "Nostalgia", Name = "Hoài niệm", Description = "Trân trọng ký ức và những giá trị xưa", Category = "Emotional" },
                new() { Code = "SpiritualVibe", Name = "Tâm linh meme", Description = "Hệ tâm linh vũ trụ gửi tín hiệu", Category = "Spirituality" },
                new() { Code = "MemeAffinity", Name = "Hài hước châm biếm", Description = "Tự trào, nhìn đời bằng lăng kính giải trí", Category = "Humor" }
            };

            await context.Traits.AddRangeAsync(traits, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        // 2. Check if questions already populated (minimum 50)
        int questionCount = await context.Questions.CountAsync(cancellationToken);
        if (questionCount >= 50)
        {
            return;
        }

        logger.LogInformation("Seeding 100+ Vietnamese Questions across 10 Themes...");

        // Ensure Themes exist
        var traitMap = await context.Traits.ToDictionaryAsync(t => t.Code, t => t.Id, cancellationToken);

        var themesData = GetThemesSeedData();

        foreach (var themeData in themesData)
        {
            var existingTheme = await context.Themes.FirstOrDefaultAsync(t => t.Code == themeData.Code, cancellationToken);
            int themeId;

            if (existingTheme == null)
            {
                var newTheme = new Theme
                {
                    Code = themeData.Code,
                    Name = themeData.Name,
                    Description = themeData.Description,
                    Icon = themeData.Icon,
                    SortOrder = themeData.SortOrder,
                    IsActive = true
                };
                await context.Themes.AddAsync(newTheme, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                themeId = newTheme.Id;
            }
            else
            {
                themeId = existingTheme.Id;
            }

            // Seed questions for this theme
            foreach (var qSeed in themeData.Questions)
            {
                bool qExists = await context.Questions.AnyAsync(q => q.ThemeId == themeId && q.Content == qSeed.Content, cancellationToken);
                if (qExists) continue;

                var question = new Question
                {
                    ThemeId = themeId,
                    QuestionType = qSeed.QuestionType,
                    Content = qSeed.Content,
                    Subtitle = qSeed.Subtitle,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                int choiceOrder = 1;
                foreach (var cSeed in qSeed.Choices)
                {
                    var choice = new QuestionChoice
                    {
                        Content = cSeed.Content,
                        SubContent = cSeed.SubContent,
                        OrderIndex = choiceOrder++,
                        IsActive = true
                    };

                    foreach (var (traitCode, weight) in cSeed.Traits)
                    {
                        if (traitMap.TryGetValue(traitCode, out int traitId))
                        {
                            choice.ChoiceTraits.Add(new ChoiceTrait
                            {
                                TraitId = traitId,
                                Weight = weight
                            });
                        }
                    }

                    question.Choices.Add(choice);
                }

                await context.Questions.AddAsync(question, cancellationToken);
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("ContentSeeder completed successfully. Total questions: {Count}", await context.Questions.CountAsync(cancellationToken));
    }

    private class ThemeSeed
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<QuestionSeed> Questions { get; set; } = new();
    }

    private class QuestionSeed
    {
        public string Content { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public QuestionType QuestionType { get; set; } = QuestionType.SingleChoice;
        public List<ChoiceSeed> Choices { get; set; } = new();
    }

    private class ChoiceSeed
    {
        public string Content { get; set; } = string.Empty;
        public string? SubContent { get; set; }
        public List<(string TraitCode, double Weight)> Traits { get; set; } = new();
    }

    private static List<ThemeSeed> GetThemesSeedData()
    {
        var list = new List<ThemeSeed>();

        // 1. THEME_PERSONALITY (Tính cách)
        list.Add(new ThemeSeed
        {
            Code = "THEME_PERSONALITY",
            Name = "Tính cách & Bản thân",
            Description = "Khám phá bản ngã, cá tính và phong cách sống độc bản",
            Icon = "🧠",
            SortOrder = 1,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Khi đối diện với một thử thách hoàn toàn mới, phản xạ đầu tiên của bạn là gì?",
                    Subtitle = "Chọn theo phản xạ tự nhiên của trực giác",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Lên kế hoạch chi tiết từng bước", Traits = new() { ("Order", 0.9), ("Stability", 0.7) } },
                        new() { Content = "Lao vào làm luôn, vừa làm vừa sửa", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.8) } },
                        new() { Content = "Hỏi ý kiến bạn bè và chuyên gia", Traits = new() { ("Emotion", 0.6), ("Stability", 0.5) } },
                        new() { Content = "Tin hoàn toàn vào giác quan thứ 6", Traits = new() { ("Intuition", 0.95), ("Independence", 0.7) } }
                    }
                },
                new()
                {
                    Content = "Bạn là người thuộc tuýp nào khi làm việc nhóm?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Thủ lĩnh gánh team quyết đoán", Traits = new() { ("Independence", 0.9), ("RiskTolerance", 0.7) } },
                        new() { Content = "Hậu phương vững chắc âm thầm hỗ trợ", Traits = new() { ("Patience", 0.9), ("Stability", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Vô tình phát hiện bạn cùng phòng dùng trộm đồ của mình nhiều lần...",
                    Subtitle = "Tình huống kịch tính thường ngày",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Nói chuyện thẳng thắn rõ ràng một lần", Traits = new() { ("Order", 0.8), ("Independence", 0.7) } },
                        new() { Content = "Tẩm thuốc xổ vào đồ ăn để tự nghiệm ra bài học", Traits = new() { ("ChaosEnergy", 0.95), ("MemeAffinity", 0.9) } },
                        new() { Content = "Im lặng và giấu hết đồ quý giá đi", Traits = new() { ("Patience", 0.7), ("Stability", 0.6) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Năng lượng chủ đạo của bạn hôm nay là gì?",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🔥 Bùng cháy nhiệt huyết", Traits = new() { ("RiskTolerance", 0.9), ("Exploration", 0.8) } },
                        new() { Content = "🧊 Điềm tĩnh như băng", Traits = new() { ("Patience", 0.9), ("Stability", 0.8) } },
                        new() { Content = "⚡ Hỗn loạn bất định", Traits = new() { ("ChaosEnergy", 0.95), ("MemeAffinity", 0.8) } },
                        new() { Content = "✨ Mộng mơ bay bổng", Traits = new() { ("Intuition", 0.9), ("Emotion", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Nếu được sở hữu một siêu năng lực duy nhất trong 24 giờ:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Biết trước tương lai và kết quả xổ số", Traits = new() { ("Intuition", 0.9), ("RiskTolerance", 0.8) } },
                        new() { Content = "Tàng hình để đi dạo khắp nơi không ai biết", Traits = new() { ("Independence", 0.9), ("Exploration", 0.7) } },
                        new() { Content = "Đọc được suy nghĩ của người khác", Traits = new() { ("Emotion", 0.9), ("Order", 0.6) } },
                        new() { Content = "Dịch chuyển tức thời đến bất kỳ đâu", Traits = new() { ("Exploration", 0.95), ("ChaosEnergy", 0.7) } }
                    }
                },
                new()
                {
                    Content = "Bạn thà sống một cuộc đời...",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Bình yên êm ả, không sóng gió", Traits = new() { ("Stability", 0.95), ("Patience", 0.8) } },
                        new() { Content = "Đầy thăng trầm nhưng rực rỡ kịch tính", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Một câu châm ngôn phản ánh đúng nhất con người bạn:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "\"Cứ đi rồi sẽ đến\"", Traits = new() { ("Exploration", 0.9), ("Patience", 0.7) } },
                        new() { Content = "\"Không thử sao biết không được\"", Traits = new() { ("RiskTolerance", 0.95), ("ChaosEnergy", 0.7) } },
                        new() { Content = "\"Cẩn tắc vô áy náy\"", Traits = new() { ("Order", 0.9), ("Stability", 0.8) } },
                        new() { Content = "\"Vạn sự tùy duyên\"", Traits = new() { ("Intuition", 0.9), ("SpiritualVibe", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Sếp giao một dự án bất khả thi với deadline ngày mai:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Uống 3 lon tăng lực, thức trắng đêm cày", Traits = new() { ("Patience", 0.8), ("Order", 0.7) } },
                        new() { Content = "Viết email từ chối khéo léo kèm lý do chính đáng", Traits = new() { ("Independence", 0.9), ("Stability", 0.8) } },
                        new() { Content = "Dùng AI gánh hết và cầu mong không bị soi", Traits = new() { ("MemeAffinity", 0.9), ("RiskTolerance", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Quyết định quan trọng nhất bạn thường dựa vào đâu?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Phân tích logic và số liệu thực tế", Traits = new() { ("Order", 0.9), ("Stability", 0.8) } },
                        new() { Content = "Cảm giác mách bảo từ con tim", Traits = new() { ("Intuition", 0.95), ("Emotion", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bấm trong 3 giây: Bạn thích màu nào hơn?",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🔴 Đỏ rực rỡ nhiệt huyết", Traits = new() { ("RiskTolerance", 0.8), ("ChaosEnergy", 0.7) } },
                        new() { Content = "🔵 Xanh dương trầm tĩnh", Traits = new() { ("Stability", 0.85), ("Order", 0.8) } },
                        new() { Content = "🟡 Vàng kim thịnh vượng", Traits = new() { ("Intuition", 0.75), ("Exploration", 0.7) } },
                        new() { Content = "🟣 Tím bí ẩn mộng mơ", Traits = new() { ("SpiritualVibe", 0.9), ("Emotion", 0.8) } }
                    }
                }
            }
        });

        // 2. THEME_LOVE (Tình cảm & Hẹn hò)
        list.Add(new ThemeSeed
        {
            Code = "THEME_LOVE",
            Name = "Tình cảm & Hẹn hò",
            Description = "Chuyện tình duyên, người yêu cũ, thính dạo và hẹn hò bất ổn",
            Icon = "❤️",
            SortOrder = 2,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Trong một mối quan hệ tình cảm, điều gì là quan trọng nhất với bạn?",
                    Subtitle = "Chọn câu trả lời thật lòng nhất",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Sự tin tưởng tuyệt đối", Traits = new() { ("Stability", 0.9), ("Order", 0.7) } },
                        new() { Content = "Sự thấu hiểu và đồng hành", Traits = new() { ("Emotion", 0.95), ("Patience", 0.8) } },
                        new() { Content = "Cảm xúc mãnh liệt lãng mạn", Traits = new() { ("RiskTolerance", 0.8), ("ChaosEnergy", 0.7) } },
                        new() { Content = "Không gian tự do riêng tư", Traits = new() { ("Independence", 0.95), ("Exploration", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bạn nhận được tin nhắn từ Người Yêu Cũ lúc 2 giờ sáng:",
                    Subtitle = "\"Em/Anh còn thức không?\"",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Seen và không rep, tiếp tục ngủ", Traits = new() { ("Independence", 0.9), ("Stability", 0.8) } },
                        new() { Content = "Rep ngay lập tức: \"Có chuyện gì thế?\"", Traits = new() { ("Emotion", 0.9), ("Nostalgia", 0.85) } },
                        new() { Content = "Chụp màn hình gửi vào nhóm bạn thân đàm đạo", Traits = new() { ("MemeAffinity", 0.95), ("ChaosEnergy", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Gu hẹn hò lý tưởng của bạn vào tối thứ Bảy:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Ăn tối lãng mạn dưới ánh nến lung linh", Traits = new() { ("Emotion", 0.9), ("Nostalgia", 0.7) } },
                        new() { Content = "Trà chanh vỉa hè chém gió xuyên đêm", Traits = new() { ("MemeAffinity", 0.85), ("Stability", 0.6) } },
                        new() { Content = "Cùng nhau chơi game co-op hoặc cày phim tại nhà", Traits = new() { ("Stability", 0.9), ("Patience", 0.8) } },
                        new() { Content = "Lên xe đi phượt ngẫu hứng đến một thị trấn lạ", Traits = new() { ("Exploration", 0.95), ("RiskTolerance", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bạn tin vào tình yêu sét đánh hay tình yêu mưa dầm thấm lâu?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "⚡ Sét đánh từ cái nhìn đầu tiên", Traits = new() { ("Intuition", 0.9), ("RiskTolerance", 0.8) } },
                        new() { Content = "🌧️ Mưa dầm thấm lâu qua năm tháng", Traits = new() { ("Patience", 0.95), ("Stability", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Trạng thái tình cảm hiện tại của bạn?",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🌸 Độc thân kiêu hãnh", Traits = new() { ("Independence", 0.95), ("ChaosEnergy", 0.6) } },
                        new() { Content = "💖 Đang đắm chìm trong tình yêu", Traits = new() { ("Emotion", 0.9), ("Stability", 0.8) } },
                        new() { Content = "🌪️ Mập mờ không tên", Traits = new() { ("ChaosEnergy", 0.9), ("RiskTolerance", 0.8) } },
                        new() { Content = "💸 Chỉ yêu tiền và Jackpot", Traits = new() { ("MemeAffinity", 0.95), ("Order", 0.7) } }
                    }
                },
                new()
                {
                    Content = "Khi cãi nhau với người yêu, bạn thường xử lý thế nào?",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Chủ động làm hòa trước để giữ hòa khí", Traits = new() { ("Patience", 0.9), ("Emotion", 0.8) } },
                        new() { Content = "Cùng ngồi lại phân tích đúng sai rõ ràng", Traits = new() { ("Order", 0.9), ("Stability", 0.7) } },
                        new() { Content = "Chiến tranh lạnh đợi đối phương xin lỗi", Traits = new() { ("Independence", 0.8), ("ChaosEnergy", 0.7) } },
                        new() { Content = "Mua trà sữa / đồ ăn ngon dỗ dành", Traits = new() { ("MemeAffinity", 0.85), ("Emotion", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Thà bị người yêu quản lý chặt chẽ hay thà bị thả tự do hoàn toàn?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Quản lý chặt để cảm nhận được quan tâm", Traits = new() { ("Emotion", 0.8), ("Stability", 0.7) } },
                        new() { Content = "Thả tự do hoàn toàn tôn trọng đời tư", Traits = new() { ("Independence", 0.95), ("Exploration", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bạn lỡ quên kỷ niệm ngày yêu nhau và đối phương đang giận tím mặt:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Thừa nhận thật thà và bù đắp bằng chuyến du lịch", Traits = new() { ("Exploration", 0.8), ("Emotion", 0.8) } },
                        new() { Content = "Nói dối rằng đang chuẩn bị một bất ngờ lớn", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.8) } },
                        new() { Content = "Tặng ngay chiếc vé Bịp lót cầu may đổi đời", Traits = new() { ("MemeAffinity", 0.95), ("Intuition", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Tình yêu hay Sự nghiệp quan trọng hơn với bạn ở giai đoạn này?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "💼 Sự nghiệp và tài chính vững vàng", Traits = new() { ("Order", 0.9), ("Stability", 0.85) } },
                        new() { Content = "❤️ Tình yêu chân thành và tổ ấm", Traits = new() { ("Emotion", 0.95), ("Patience", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Biểu tượng tình yêu trong mắt bạn:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "💍 Chiếc nhẫn trọn đời", Traits = new() { ("Stability", 0.9), ("Order", 0.8) } },
                        new() { Content = "🕊️ Cánh chim tự do bay lượn", Traits = new() { ("Independence", 0.95), ("Exploration", 0.8) } },
                        new() { Content = "🔥 Ngọn lửa đam mê", Traits = new() { ("RiskTolerance", 0.9), ("Emotion", 0.85) } },
                        new() { Content = "☕ Tách cà phê ấm áp", Traits = new() { ("Patience", 0.85), ("Nostalgia", 0.8) } }
                    }
                }
            }
        });

        // 3. THEME_MEMORY (Ký ức)
        list.Add(new ThemeSeed
        {
            Code = "THEME_MEMORY",
            Name = "Ký ức & Quá khứ",
            Description = "Những hoài niệm tuổi thơ, ký ức học trò và dấu mốc thời gian",
            Icon = "🕰️",
            SortOrder = 3,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Ký ức tuổi thơ nào khiến bạn bồi hồi nhất khi nhớ lại?",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Những buổi trưa trốn ngủ đi hái trộm trái cây", Traits = new() { ("Exploration", 0.9), ("ChaosEnergy", 0.8) } },
                        new() { Content = "Bữa cơm gia đình ấm áp bên mâm cơm chiều", Traits = new() { ("Nostalgia", 0.95), ("Emotion", 0.9) } },
                        new() { Content = "Những trò chơi dân gian cùng lũ bạn đầu ngõ", Traits = new() { ("Nostalgia", 0.85), ("Patience", 0.7) } },
                        new() { Content = "Cảm giác cầm tờ 2000 đồng đi mua que kem đá", Traits = new() { ("MemeAffinity", 0.8), ("Nostalgia", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Nếu có cỗ máy thời gian quay về quá khứ:",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Sửa lại một lỗi lầm từng khiến bạn hối tiếc", Traits = new() { ("Order", 0.85), ("Emotion", 0.9) } },
                        new() { Content = "Chỉ đứng nhìn lại bản thân ngày xưa từ xa", Traits = new() { ("Nostalgia", 0.95), ("Patience", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Vô tình tìm thấy cuốn sổ nhật ký thời cấp 3 của mình:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Đọc ngấu nghiến và cười bò vì sự ngây ngô", Traits = new() { ("MemeAffinity", 0.9), ("Nostalgia", 0.85) } },
                        new() { Content = "Đốt ngay lập tức để tiêu hủy bằng chứng đen tối", Traits = new() { ("ChaosEnergy", 0.9), ("Independence", 0.7) } },
                        new() { Content = "Cất vào ngăn kéo cẩn thận làm kỷ vật gia truyền", Traits = new() { ("Stability", 0.8), ("Nostalgia", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Âm thanh gợi nhớ tuổi thơ nhất:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = " tiếng ve kêu râm ran mùa hè", Traits = new() { ("Nostalgia", 0.9), ("Emotion", 0.8) } },
                        new() { Content = "🔔 Tiếng chuông trống tan trường", Traits = new() { ("Independence", 0.8), ("Nostalgia", 0.85) } },
                        new() { Content = "🍦 Tiếng còi bóp kem dạo", Traits = new() { ("MemeAffinity", 0.85), ("Nostalgia", 0.9) } },
                        new() { Content = "🌧️ Tiếng mưa rơi lộp độp trên mái tôn", Traits = new() { ("Patience", 0.85), ("Intuition", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bài hát cũ gắn liền với mối tình đầu của bạn bất ngờ vang lên trong quán cafe:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Mỉm cười nhẹ nhàng và cảm ơn kỷ niệm đẹp", Traits = new() { ("Nostalgia", 0.9), ("Emotion", 0.85) } },
                        new() { Content = "Tim đập nhanh và nhớ lại từng ánh mắt nụ cười", Traits = new() { ("Emotion", 0.95), ("Nostalgia", 0.9) } },
                        new() { Content = "Đổi quán khác ngay lập tức vì không muốn nhớ lại", Traits = new() { ("Independence", 0.8), ("Order", 0.7) } },
                        new() { Content = "Shazam bài hát lưu vào playlist nghe lại", Traits = new() { ("Stability", 0.7), ("Nostalgia", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bạn là người thường giữ lại kỷ vật cũ hay dọn dẹp vứt bỏ?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Giữ lại tất cả làm kỷ niệm dù chật nhà", Traits = new() { ("Nostalgia", 0.95), ("Emotion", 0.8) } },
                        new() { Content = "Dọn sạch sẽ, sống tối giản cho tương lai", Traits = new() { ("Order", 0.9), ("Independence", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Món đồ chơi đầu tiên bạn tự tích cóp tiền để mua được:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Mô hình siêu nhân / búp bê", Traits = new() { ("Nostalgia", 0.8), ("Emotion", 0.7) } },
                        new() { Content = "Máy chơi game 4 nút / xếp hình", Traits = new() { ("Exploration", 0.85), ("MemeAffinity", 0.8) } },
                        new() { Content = "Chiếc xe đạp nhỏ", Traits = new() { ("Independence", 0.9), ("Exploration", 0.8) } },
                        new() { Content = "Cuốn truyện tranh / bộ bài tây", Traits = new() { ("Intuition", 0.8), ("Nostalgia", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Họp lớp sau 10 năm ra trường:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Háo hức tham gia gặp lại thầy cô bạn cũ", Traits = new() { ("Nostalgia", 0.9), ("Emotion", 0.85) } },
                        new() { Content = "Viện cớ bận để ở nhà ngủ", Traits = new() { ("Independence", 0.9), ("Stability", 0.8) } },
                        new() { Content = "Đến ăn no nê rồi về trước lúc tính tiền", Traits = new() { ("ChaosEnergy", 0.9), ("MemeAffinity", 0.95) } }
                    }
                },
                new()
                {
                    Content = "Thời thanh xuân đẹp nhất ở điểm nào?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Sự tự do và không phải lo nghĩ tiền bạc", Traits = new() { ("Independence", 0.9), ("Exploration", 0.8) } },
                        new() { Content = "Những tình cảm trong sáng không toan tính", Traits = new() { ("Emotion", 0.95), ("Nostalgia", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Nhắc về quá khứ, bạn cảm thấy:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "✨ Tự hào về hành trình đã qua", Traits = new() { ("Stability", 0.85), ("Order", 0.8) } },
                        new() { Content = "🌅 Bồi hồi xao xuyến", Traits = new() { ("Nostalgia", 0.95), ("Emotion", 0.9) } },
                        new() { Content = "🌱 Trưởng thành và mạnh mẽ hơn", Traits = new() { ("Independence", 0.9), ("Patience", 0.8) } },
                        new() { Content = "⏩ Chỉ nhìn về tương lai phía trước", Traits = new() { ("Exploration", 0.85), ("RiskTolerance", 0.8) } }
                    }
                }
            }
        });

        // 4. THEME_TRAVEL (Du lịch)
        list.Add(new ThemeSeed
        {
            Code = "THEME_TRAVEL",
            Name = "Du lịch & Khám phá",
            Description = "Những chuyến đi bốc đồng, leo núi vượt biển và xách balo lên",
            Icon = "🌎",
            SortOrder = 4,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Nếu ngày mai được xách vali đi ngay không cần xin phép sếp, bạn sẽ chọn:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "🏖️ Nằm dài ở bãi biển nhiệt đới ngắm hoàng hôn", Traits = new() { ("Patience", 0.8), ("Stability", 0.8) } },
                        new() { Content = "🏔️ Leo đỉnh núi tuyết chinh phục giới hạn", Traits = new() { ("RiskTolerance", 0.95), ("Exploration", 0.9) } },
                        new() { Content = "🏙️ Khám phá thủ đô phồn hoa không ngủ", Traits = new() { ("ChaosEnergy", 0.8), ("Exploration", 0.85) } },
                        new() { Content = "🌲 Cắm trại trong rừng sâu không có sóng wifi", Traits = new() { ("Independence", 0.95), ("Intuition", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Phong cách du lịch của bạn là gì?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Lên lịch trình chi tiết từ vé máy bay đến quán ăn", Traits = new() { ("Order", 0.95), ("Stability", 0.85) } },
                        new() { Content = "Tùy hứng, đến nơi rồi tính sau", Traits = new() { ("ChaosEnergy", 0.9), ("Exploration", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bạn bị lạc đường ở một quốc gia hoàn toàn xa lạ vào lúc nửa đêm:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Bật Google Maps tìm đồn cảnh sát gần nhất", Traits = new() { ("Order", 0.85), ("Stability", 0.8) } },
                        new() { Content = "Tạt vào quán rượu địa phương làm một ly hỏi đường", Traits = new() { ("Exploration", 0.9), ("RiskTolerance", 0.85) } },
                        new() { Content = "Cứ đi tiếp theo trực giác, biết đâu gặp duyên lành", Traits = new() { ("Intuition", 0.95), ("SpiritualVibe", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Bạn thích phương tiện di chuyển nào nhất?",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "✈️ Máy bay bay thẳng hạng thương gia", Traits = new() { ("Stability", 0.8), ("Order", 0.75) } },
                        new() { Content = "🚆 Tàu hỏa ngắm cảnh dọc đường", Traits = new() { ("Nostalgia", 0.9), ("Patience", 0.85) } },
                        new() { Content = "🏍️ Phượt xe máy tự do tự tại", Traits = new() { ("Independence", 0.95), ("RiskTolerance", 0.9) } },
                        new() { Content = "🚢 Du thuyền sang trọng trên biển", Traits = new() { ("Emotion", 0.8), ("Patience", 0.7) } }
                    }
                },
                new()
                {
                    Content = "Khi đi du lịch, bạn ưu tiên điều gì nhất?",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Thưởng thức ẩm thực đặc sản đường phố", Traits = new() { ("Exploration", 0.85), ("Emotion", 0.75) } },
                        new() { Content = "Chụp 1000 tấm ảnh sống ảo đẹp mê hồn", Traits = new() { ("MemeAffinity", 0.9), ("Order", 0.6) } },
                        new() { Content = "Tìm hiểu văn hóa lịch sử địa phương", Traits = new() { ("Nostalgia", 0.85), ("Order", 0.8) } },
                        new() { Content = "Chỉ cần một nơi yên tĩnh để ngủ xả stress", Traits = new() { ("Stability", 0.9), ("Patience", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Đi du lịch một mình (Solo Travel) hay đi cùng nhóm bạn đông?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Đi một mình tự do khám phá bản thân", Traits = new() { ("Independence", 0.95), ("Intuition", 0.8) } },
                        new() { Content = "Đi nhóm đông vui vẻ ồn ào", Traits = new() { ("Emotion", 0.85), ("ChaosEnergy", 0.75) } }
                    }
                },
                new()
                {
                    Content = "Một chuyến đi trong mơ của bạn trị giá 1 tỷ đồng:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Khám phá Nam Cực ngắm chim cánh cụt", Traits = new() { ("Exploration", 0.95), ("RiskTolerance", 0.85) } },
                        new() { Content = "Tour quanh các cung điện cổ kính châu Âu", Traits = new() { ("Nostalgia", 0.9), ("Order", 0.75) } },
                        new() { Content = "Safari ngắm động vật hoang dã ở châu Phi", Traits = new() { ("Exploration", 0.9), ("ChaosEnergy", 0.8) } },
                        new() { Content = "Nghỉ dưỡng ở resort 7 sao Maldives cả tháng", Traits = new() { ("Stability", 0.9), ("Patience", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Hành lý bị thất lạc ở sân bay ngay ngày đầu tiên:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Bình tĩnh khiếu nại và đợi sân bay xử lý", Traits = new() { ("Patience", 0.9), ("Order", 0.85) } },
                        new() { Content = "Mua ngay đồ mới tại chợ đêm và quẩy tiếp", Traits = new() { ("ChaosEnergy", 0.9), ("RiskTolerance", 0.8) } },
                        new() { Content = "Khóc một trận rồi gọi về cho mẹ", Traits = new() { ("Emotion", 0.95), ("Nostalgia", 0.7) } }
                    }
                },
                new()
                {
                    Content = "Bạn thích ngắm bình minh trên biển hay hoàng hôn trên núi?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🌅 Bình minh trên biển tràn đầy năng lượng", Traits = new() { ("Exploration", 0.85), ("RiskTolerance", 0.75) } },
                        new() { Content = "🌄 Hoàng hôn trên núi sâu lắng bình yên", Traits = new() { ("Patience", 0.9), ("Intuition", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Điểm đến tiếp theo trong tâm tưởng bạn:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🌸 Tokyo hiện đại rực rỡ", Traits = new() { ("Order", 0.8), ("Exploration", 0.85) } },
                        new() { Content = "🏰 Paris lãng mạn cổ kính", Traits = new() { ("Emotion", 0.9), ("Nostalgia", 0.85) } },
                        new() { Content = "🏝️ Bali an yên chữa lành", Traits = new() { ("Intuition", 0.9), ("Stability", 0.8) } },
                        new() { Content = "🎲 Las Vegas thử vận may", Traits = new() { ("RiskTolerance", 0.95), ("ChaosEnergy", 0.9) } }
                    }
                }
            }
        });

        // 5. THEME_FUTURE (Tương lai & Ước vọng)
        list.Add(new ThemeSeed
        {
            Code = "THEME_FUTURE",
            Name = "Tương lai & Ước vọng",
            Description = "Giấc mơ đổi đời, trúng 100 tỷ, nghỉ hưu sớm và viễn cảnh tương lai",
            Icon = "🚀",
            SortOrder = 5,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Nếu trúng Jackpot 100 tỷ vào chiều nay, việc đầu tiên bạn làm là gì?",
                    Subtitle = "Hãy tưởng tượng viễn cảnh ngọt ngào đó",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Nộp đơn thôi việc và ném laptop vào sếp", Traits = new() { ("ChaosEnergy", 0.95), ("MemeAffinity", 0.9) } },
                        new() { Content = "Mua nhà đất, vàng và gửi tiết kiệm lấy lãi", Traits = new() { ("Order", 0.95), ("Stability", 0.9) } },
                        new() { Content = "Bao cả gia đình và bạn bè đi du lịch vòng quanh thế giới", Traits = new() { ("Emotion", 0.9), ("Exploration", 0.85) } },
                        new() { Content = "Âm thầm đầu tư khởi nghiệp dự án mơ ước", Traits = new() { ("RiskTolerance", 0.9), ("Independence", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bạn muốn nghỉ hưu sớm ở tuổi 35 hay làm việc đến già vì đam mê?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Nghỉ hưu sớm ở tuổi 35 tận hưởng cuộc sống", Traits = new() { ("Independence", 0.95), ("Stability", 0.8) } },
                        new() { Content = "Làm việc đến già vì đam mê cống hiến", Traits = new() { ("Patience", 0.9), ("Order", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Trí tuệ nhân tạo (AI) trong 10 năm tới sẽ:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Làm hết việc cho con người, tha hồ ăn chơi", Traits = new() { ("MemeAffinity", 0.9), ("Independence", 0.8) } },
                        new() { Content = "Thay thế phần lớn công việc, tạo nhiều biến động", Traits = new() { ("Stability", 0.8), ("Order", 0.7) } },
                        new() { Content = "Giúp con người khám phá vũ trụ và trường thọ", Traits = new() { ("Exploration", 0.95), ("RiskTolerance", 0.85) } },
                        new() { Content = "Tự phát triển ý thức và kiểm soát thế giới", Traits = new() { ("ChaosEnergy", 0.9), ("Intuition", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Ước vọng lớn nhất của bạn trong 5 năm tới:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "💰 Tự do tài chính không lo nghĩ", Traits = new() { ("Stability", 0.9), ("Order", 0.8) } },
                        new() { Content = "🏡 Có một mái ấm bình yên", Traits = new() { ("Emotion", 0.95), ("Patience", 0.85) } },
                        new() { Content = "🌍 Đặt chân đến 20 quốc gia", Traits = new() { ("Exploration", 0.95), ("Independence", 0.9) } },
                        new() { Content = "⭐ Trở thành người có tầm ảnh hưởng", Traits = new() { ("RiskTolerance", 0.85), ("ChaosEnergy", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bạn chuyển sang một ngành nghề hoàn toàn mới:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Học thêm bằng cấp, chuẩn bị kiến thức kỹ lưỡng", Traits = new() { ("Order", 0.9), ("Patience", 0.85) } },
                        new() { Content = "Nhảy việc ngay, áp lực tạo kim cương", Traits = new() { ("RiskTolerance", 0.95), ("ChaosEnergy", 0.85) } },
                        new() { Content = "Xin quẻ Bịp lót xem có hợp mệnh không đã", Traits = new() { ("SpiritualVibe", 0.95), ("Intuition", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Sống ở biệt thự ven biển hay penthouse giữa trung tâm thành phố?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🏖️ Biệt thự ven biển bình yên", Traits = new() { ("Patience", 0.85), ("Stability", 0.85) } },
                        new() { Content = "🏙️ Penthouse trung tâm sầm uất", Traits = new() { ("Exploration", 0.85), ("RiskTolerance", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Nếu được gửi một bức thư cho chính bạn 10 năm sau:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "\"Bạn đã giàu và hạnh phúc chưa?\"", Traits = new() { ("Emotion", 0.85), ("Order", 0.75) } },
                        new() { Content = "\"Cảm ơn vì đã không bao giờ bỏ cuộc!\"", Traits = new() { ("Patience", 0.95), ("Stability", 0.85) } },
                        new() { Content = "\"Vé số trúng thưởng ngày hôm nay là bao nhiêu?\"", Traits = new() { ("MemeAffinity", 0.95), ("RiskTolerance", 0.8) } },
                        new() { Content = "\"Hãy luôn giữ tâm hồn tự do nhé!\"", Traits = new() { ("Independence", 0.95), ("Exploration", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Khi nghĩ về tuổi 60 của mình, bạn hình dung bản thân đang:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Trồng rau, nuôi cá, đọc sách an nhàn", Traits = new() { ("Patience", 0.9), ("Stability", 0.9) } },
                        new() { Content = "Vẫn lái motor đi phượt khắp các châu lục", Traits = new() { ("Independence", 0.95), ("Exploration", 0.9) } },
                        new() { Content = "Vui vầy cùng con cháu quây quần", Traits = new() { ("Emotion", 0.95), ("Nostalgia", 0.85) } },
                        new() { Content = "Ngồi đếm tiền trúng thưởng Bịp lót mỗi ngày", Traits = new() { ("MemeAffinity", 0.95), ("ChaosEnergy", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bạn tin số phận đã an bài hay do con người tự tạo ra?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Bàn tay ta làm nên tất cả", Traits = new() { ("Independence", 0.9), ("Order", 0.85) } },
                        new() { Content = "Thiên thời địa lợi nhân hòa quyết định", Traits = new() { ("Intuition", 0.9), ("SpiritualVibe", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Một từ khóa cho tương lai của bạn:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "💎 Bứt phá ngoạn mục", Traits = new() { ("RiskTolerance", 0.95), ("ChaosEnergy", 0.85) } },
                        new() { Content = "🌊 Bình an tự tại", Traits = new() { ("Stability", 0.95), ("Patience", 0.9) } },
                        new() { Content = "🌟 Tỏa sáng rực rỡ", Traits = new() { ("Exploration", 0.9), ("Emotion", 0.85) } },
                        new() { Content = "🔮 Bí ẩn kỳ diệu", Traits = new() { ("Intuition", 0.95), ("SpiritualVibe", 0.9) } }
                    }
                }
            }
        });

        // 6. THEME_INSTINCT (Trực giác)
        list.Add(new ThemeSeed
        {
            Code = "THEME_INSTINCT",
            Name = "Trực giác & Linh cảm",
            Description = "Bản năng nhạy bén, giác quan thứ sáu và phản xạ chớp nhoáng",
            Icon = "⚡",
            SortOrder = 6,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Chọn một cánh cửa bí mật mở ra vận may của bạn:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "🚪 Cánh cửa gỗ sồi cổ kính tỏa mùi trầm hương", Traits = new() { ("Nostalgia", 0.9), ("Patience", 0.8) } },
                        new() { Content = "🚪 Cánh cửa kim loại sáng lóa phong cách tương lai", Traits = new() { ("Exploration", 0.9), ("Order", 0.8) } },
                        new() { Content = "🚪 Cánh cửa dát vàng lấp lánh chạm khắc rồng phượng", Traits = new() { ("RiskTolerance", 0.9), ("Intuition", 0.85) } },
                        new() { Content = "🚪 Cánh cửa ẩn sau bức rèm nhung tím huyền bí", Traits = new() { ("SpiritualVibe", 0.95), ("Intuition", 0.95) } }
                    }
                },
                new()
                {
                    Content = "Bấm không cần suy nghĩ: Chẵn hay Lẻ?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🔵 Số Chẵn tròn trịa", Traits = new() { ("Stability", 0.9), ("Order", 0.85) } },
                        new() { Content = "🔴 Số Lẻ phá cách", Traits = new() { ("ChaosEnergy", 0.9), ("RiskTolerance", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bạn bước vào sảnh một khách sạn sang trọng và thấy 3 thang máy:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Thang máy bên trái vừa mở cửa", Traits = new() { ("Intuition", 0.85), ("Order", 0.75) } },
                        new() { Content = "Thang máy ở giữa đông người nhất", Traits = new() { ("Stability", 0.8), ("Emotion", 0.7) } },
                        new() { Content = "Thang máy bên phải vắng tanh", Traits = new() { ("Independence", 0.9), ("Exploration", 0.8) } },
                        new() { Content = "Đi thang bộ rèn luyện sức khỏe", Traits = new() { ("Patience", 0.9), ("Independence", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bấm trong 2 giây: Con số nào lóe lên đầu tiên trong đầu bạn?",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "Số 7 may mắn", Traits = new() { ("Intuition", 0.9), ("SpiritualVibe", 0.9) } },
                        new() { Content = "Số 8 phát tài", Traits = new() { ("Stability", 0.85), ("Order", 0.8) } },
                        new() { Content = "Số 9 trường cửu", Traits = new() { ("Patience", 0.9), ("Nostalgia", 0.8) } },
                        new() { Content = "Số 3 tài năng", Traits = new() { ("Exploration", 0.85), ("ChaosEnergy", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Khi có một cảm giác bất an không rõ lý do:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Lập tức dừng lại kiểm tra mọi thứ", Traits = new() { ("Intuition", 0.95), ("Order", 0.85) } },
                        new() { Content = "Kệ, nghĩ nhiều quá thôi, đi tiếp", Traits = new() { ("RiskTolerance", 0.85), ("ChaosEnergy", 0.8) } },
                        new() { Content = "Gọi điện tâm sự với người thân", Traits = new() { ("Emotion", 0.9), ("Stability", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bạn tin vào giác quan thứ sáu của mình đến mức nào?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Rất tin, linh cảm của tôi thường cực kỳ chuẩn", Traits = new() { ("Intuition", 0.95), ("SpiritualVibe", 0.9) } },
                        new() { Content = "Không tin lắm, mọi thứ cần chứng cứ khoa học", Traits = new() { ("Order", 0.95), ("Stability", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Chọn một biểu tượng năng lượng bạn cảm nhận mạnh nhất:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "⚡ Sấm sét uy lực dũng mãnh", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.85) } },
                        new() { Content = "🌊 Dòng nước uyển chuyển bao dung", Traits = new() { ("Patience", 0.9), ("Emotion", 0.85) } },
                        new() { Content = "🌱 Mầm cây vươn mình sinh sôi", Traits = new() { ("Stability", 0.9), ("Exploration", 0.8) } },
                        new() { Content = "🌌 Dải ngân hà vô tận huyền bí", Traits = new() { ("SpiritualVibe", 0.95), ("Intuition", 0.95) } }
                    }
                },
                new()
                {
                    Content = "Tung một đồng xu lên trời để quyết định một việc khó khăn:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Làm đúng theo mặt đồng xu rơi xuống", Traits = new() { ("Intuition", 0.85), ("RiskTolerance", 0.8) } },
                        new() { Content = "Trong lúc đồng xu đang bay, bạn đã biết lòng mình muốn mặt nào", Traits = new() { ("Intuition", 0.95), ("Emotion", 0.9) } },
                        new() { Content = "Tung lại lần nữa vì không thích kết quả", Traits = new() { ("ChaosEnergy", 0.9), ("MemeAffinity", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Linh cảm mách bảo bạn rẽ trái hay rẽ phải?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "⬅️ Rẽ Trái - Lối đi bí ẩn", Traits = new() { ("Intuition", 0.9), ("Exploration", 0.85) } },
                        new() { Content = "➡️ Rẽ Phải - Con đường quen thuộc", Traits = new() { ("Stability", 0.9), ("Order", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Nhịp đập trực giác hiện tại:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "💓 Hồi hộp đón chờ", Traits = new() { ("Emotion", 0.85), ("RiskTolerance", 0.8) } },
                        new() { Content = "🧘 Bình thản như không", Traits = new() { ("Patience", 0.95), ("Stability", 0.9) } },
                        new() { Content = "🎯 Tập trung cao độ", Traits = new() { ("Order", 0.9), ("Intuition", 0.85) } },
                        new() { Content = "🎲 Sẵn sàng nát cùng số phận", Traits = new() { ("MemeAffinity", 0.95), ("ChaosEnergy", 0.9) } }
                    }
                }
            }
        });

        // 7. THEME_ENTERTAINMENT (Giải trí)
        list.Add(new ThemeSeed
        {
            Code = "THEME_ENTERTAINMENT",
            Name = "Giải trí & Thư giãn",
            Description = "Phim ảnh, âm nhạc, cày phim thâu đêm và thú vui tiêu khiển",
            Icon = "🎬",
            SortOrder = 7,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Thể loại phim bạn yêu thích nhất vào một tối cuối tuần:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "💣 Hành động mãn nhãn, cháy nổ ngập tràn", Traits = new() { ("RiskTolerance", 0.85), ("ChaosEnergy", 0.8) } },
                        new() { Content = "🧩 Trinh thám hại não, plot twist giật gân", Traits = new() { ("Order", 0.9), ("Intuition", 0.85) } },
                        new() { Content = "😂 Hài hước bựa cười xả stress", Traits = new() { ("MemeAffinity", 0.95), ("ChaosEnergy", 0.75) } },
                        new() { Content = "🍿 Tình cảm lãng mạn nhẹ nhàng", Traits = new() { ("Emotion", 0.9), ("Nostalgia", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Cày phim bộ liên tục hay xem phim lẻ 2 tiếng là xong?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Cày liên tục 16 tập không ngủ", Traits = new() { ("ChaosEnergy", 0.9), ("Patience", 0.75) } },
                        new() { Content = "Xem phim lẻ 2 tiếng dứt điểm gọn gàng", Traits = new() { ("Order", 0.85), ("Stability", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Khi nghe nhạc, bạn là người:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Nghe đi nghe lại đúng 1 bài hát quen thuộc cả tuần", Traits = new() { ("Nostalgia", 0.95), ("Stability", 0.85) } },
                        new() { Content = "Bật Discover Weekly khám phá nhạc mới liên tục", Traits = new() { ("Exploration", 0.95), ("Independence", 0.8) } },
                        new() { Content = "Nghe theo bảng xếp hạng thịnh hành Top trending", Traits = new() { ("Emotion", 0.75), ("Order", 0.7) } },
                        new() { Content = "Chỉ nghe nhạc không lời Lo-fi để tập trung", Traits = new() { ("Patience", 0.9), ("Order", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Bạn thích chơi trò chơi nào hơn?",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🎮 Game sinh tồn bắn súng", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.85) } },
                        new() { Content = "♟️ Cờ vua đấu trí", Traits = new() { ("Order", 0.95), ("Patience", 0.9) } },
                        new() { Content = "🃏 Ma sói / Uno cùng bạn bè", Traits = new() { ("MemeAffinity", 0.9), ("Emotion", 0.8) } },
                        new() { Content = "🎰 Bịp lót quay số đổi đời", Traits = new() { ("Intuition", 0.9), ("RiskTolerance", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Đi xem concert của thần tượng nhưng hết vé VIP:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Mua vé chợ đen chấp nhận giá cắt cổ", Traits = new() { ("RiskTolerance", 0.95), ("Emotion", 0.9) } },
                        new() { Content = "Mua vé khán đài xa xôi vẫn cổ vũ hết mình", Traits = new() { ("Patience", 0.85), ("Emotion", 0.8) } },
                        new() { Content = "Ở nhà xem livestream và tiết kiệm tiền", Traits = new() { ("Order", 0.9), ("Stability", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Đi karaoke: Bạn là ca sĩ chính gánh mic hay khán giả vỗ tay ăn hoa quả?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🎤 Ca sĩ chính hát từ đầu đến cuối", Traits = new() { ("ChaosEnergy", 0.85), ("RiskTolerance", 0.8) } },
                        new() { Content = "🍉 Khán giả ăn hoa quả và bấm bài", Traits = new() { ("Patience", 0.9), ("Stability", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Một bộ truyện tranh / phim hoạt hình gắn liền với tuổi thơ bạn:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Doraemon chú mèo máy thần kỳ", Traits = new() { ("Nostalgia", 0.95), ("Emotion", 0.85) } },
                        new() { Content = "Conan thám tử lừng danh", Traits = new() { ("Order", 0.9), ("Intuition", 0.85) } },
                        new() { Content = "One Piece hành trình vua hải tặc", Traits = new() { ("Exploration", 0.95), ("RiskTolerance", 0.9) } },
                        new() { Content = "Dragon Ball 7 viên ngọc rồng", Traits = new() { ("ChaosEnergy", 0.9), ("RiskTolerance", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bạn bị dính spoiler cái kết của bộ phim yêu thích trước khi kịp xem:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Block người spoil ngay lập tức không nói nhiều", Traits = new() { ("Independence", 0.9), ("Order", 0.8) } },
                        new() { Content = "Kệ, xem phim là thưởng thức hành trình", Traits = new() { ("Patience", 0.9), ("Stability", 0.85) } },
                        new() { Content = "Đi spoil tiếp cho người khác cùng chung nỗi đau", Traits = new() { ("ChaosEnergy", 0.95), ("MemeAffinity", 0.95) } }
                    }
                },
                new()
                {
                    Content = "Đọc sách giấy truyền thống hay đọc E-book trên Kindle?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "📖 Sách giấy thơm mùi mực in", Traits = new() { ("Nostalgia", 0.9), ("Patience", 0.85) } },
                        new() { Content = "📱 E-book tiện lợi chứa ngàn cuốn", Traits = new() { ("Order", 0.85), ("Exploration", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Nhạc cụ bạn muốn biết chơi nhất:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🎹 Piano cổ điển sâu lắng", Traits = new() { ("Patience", 0.9), ("Order", 0.85) } },
                        new() { Content = "🎸 Guitar mộc mạc phong trần", Traits = new() { ("Independence", 0.9), ("Emotion", 0.85) } },
                        new() { Content = "🥁 Dàn trống sôi động cuồng nhiệt", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.9) } },
                        new() { Content = "🎻 Violin réo rắt ma mị", Traits = new() { ("Intuition", 0.9), ("SpiritualVibe", 0.85) } }
                    }
                }
            }
        });

        // 8. THEME_LIFESTYLE (Cuộc sống & Đời thường)
        list.Add(new ThemeSeed
        {
            Code = "THEME_LIFESTYLE",
            Name = "Cuộc sống & Đời thường",
            Description = "Drama công sở, deadline dí, ăn vặt và những thói quen thường nhật",
            Icon = "☕",
            SortOrder = 8,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Thói quen mỗi sáng mở mắt ra của bạn là gì?",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Cầm điện thoại lướt mạng xã hội 30 phút", Traits = new() { ("MemeAffinity", 0.85), ("ChaosEnergy", 0.7) } },
                        new() { Content = "Uống một cốc nước ấm và tập thể dục nhẹ", Traits = new() { ("Stability", 0.95), ("Order", 0.9) } },
                        new() { Content = "Pha một ly cà phê đậm đặc để khởi động não", Traits = new() { ("Order", 0.8), ("Patience", 0.75) } },
                        new() { Content = "Tắt báo thức ngủ tiếp thêm 5 phút", Traits = new() { ("Independence", 0.85), ("ChaosEnergy", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bạn là Cú Đêm hay Chim Sớm?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🦉 Cú đêm sáng tạo về khuya", Traits = new() { ("ChaosEnergy", 0.85), ("Independence", 0.8) } },
                        new() { Content = "🐦 Chim sớm dậy đón bình minh", Traits = new() { ("Order", 0.9), ("Stability", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bạn vô tình gửi nhầm tin nhắn nói xấu Sếp vào nhóm chat có Sếp:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Thu hồi ngay và giả vờ bị hack tài khoản", Traits = new() { ("ChaosEnergy", 0.9), ("MemeAffinity", 0.9) } },
                        new() { Content = "Soạn sẵn đơn xin thôi việc trong danh dự", Traits = new() { ("Independence", 0.9), ("Order", 0.8) } },
                        new() { Content = "Gửi tiếp \"Haha em đùa đấy sếp ơi\" rồi nín thở", Traits = new() { ("RiskTolerance", 0.95), ("Emotion", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Món đồ uống sinh tồn của bạn:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "☕ Cà phê sữa đá sảng khoái", Traits = new() { ("Order", 0.8), ("Stability", 0.8) } },
                        new() { Content = "🧋 Trà sữa full topping ngọt ngào", Traits = new() { ("Emotion", 0.9), ("MemeAffinity", 0.85) } },
                        new() { Content = "🍵 Trà thanh nhiệt thanh lọc cơ thể", Traits = new() { ("Patience", 0.9), ("Stability", 0.9) } },
                        new() { Content = "🥤 Nước tăng lực cày deadline", Traits = new() { ("RiskTolerance", 0.85), ("ChaosEnergy", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Cuối tháng tài khoản còn đúng 50.000 đồng:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Mua 5 gói mì tôm ăn cầm cự qua ngày", Traits = new() { ("Stability", 0.9), ("Patience", 0.85) } },
                        new() { Content = "Mua ngay 5 vé Bịp lót all-in đổi đời", Traits = new() { ("RiskTolerance", 0.99), ("ChaosEnergy", 0.95) } },
                        new() { Content = "Gọi điện về nhà cầu cứu phụ huynh", Traits = new() { ("Emotion", 0.85), ("Nostalgia", 0.75) } },
                        new() { Content = "Đi ăn ké đồng nghiệp và hứa tháng sau trả", Traits = new() { ("MemeAffinity", 0.9), ("Independence", 0.6) } }
                    }
                },
                new()
                {
                    Content = "Nấu ăn ở nhà hay Đặt đồ ăn ngoài App?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🍳 Tự nấu ăn ngon lành sạch sẽ", Traits = new() { ("Patience", 0.9), ("Order", 0.85) } },
                        new() { Content = "🛵 Đặt app nhanh gọn tiện lợi", Traits = new() { ("Independence", 0.85), ("MemeAffinity", 0.75) } }
                    }
                },
                new()
                {
                    Content = "Bạn phản ứng thế nào khi bị hủy hẹn vào phút chót?",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Mừng thầm vì được ở nhà nằm lướt điện thoại", Traits = new() { ("Independence", 0.95), ("Stability", 0.8) } },
                        new() { Content = "Bực mình vì đã mất công lên đồ trang điểm", Traits = new() { ("Emotion", 0.9), ("Order", 0.75) } },
                        new() { Content = "Đi chơi một mình hoặc rủ người khác ngay", Traits = new() { ("Exploration", 0.85), ("ChaosEnergy", 0.8) } },
                        new() { Content = "Cảm thông vì chắc bạn có việc đột xuất", Traits = new() { ("Patience", 0.95), ("Emotion", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Phòng ngủ của bạn lúc này đang:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Gọn gàng ngăn nắp chuẩn phong cách tối giản", Traits = new() { ("Order", 0.95), ("Stability", 0.9) } },
                        new() { Content = "Bừa bộn có tổ chức, chỉ mình tôi hiểu", Traits = new() { ("ChaosEnergy", 0.85), ("MemeAffinity", 0.85) } },
                        new() { Content = "Chiếc ghế chứa chồng quần áo chưa gấp cao như núi", Traits = new() { ("MemeAffinity", 0.95), ("Patience", 0.7) } }
                    }
                },
                new()
                {
                    Content = "Mua sắm theo nhu cầu hay Mua sắm bốc đồng theo cảm xúc?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Chỉ mua thứ thực sự cần và có kế hoạch", Traits = new() { ("Order", 0.95), ("Stability", 0.9) } },
                        new() { Content = "Thích là mua, tiền kiếm được để chiều chuộng bản thân", Traits = new() { ("Emotion", 0.9), ("RiskTolerance", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Nỗi sợ lớn nhất ngày thứ Hai:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "⏰ Tiếng chuông báo thức 6h30", Traits = new() { ("MemeAffinity", 0.9), ("ChaosEnergy", 0.75) } },
                        new() { Content = "🚗 Tắc đường ngập khói bụi", Traits = new() { ("Patience", 0.85), ("Stability", 0.8) } },
                        new() { Content = "📊 Họp giao ban đầu tuần", Traits = new() { ("Order", 0.85), ("Independence", 0.75) } },
                        new() { Content = "📈 KPI chưa đạt một nửa", Traits = new() { ("RiskTolerance", 0.8), ("Emotion", 0.8) } }
                    }
                }
            }
        });

        // 9. THEME_ADVENTURE (Phiêu lưu & Thử thách)
        list.Add(new ThemeSeed
        {
            Code = "THEME_ADVENTURE",
            Name = "Phiêu lưu & Thử thách",
            Description = "Những quyết định liều lĩnh, vượt vùng an toàn và thử thách bất ngờ",
            Icon = "🧭",
            SortOrder = 9,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "Bạn được mời tham gia một trò chơi sinh tồn trúng 10 tỷ trên hoang đảo:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Đăng ký tham gia ngay, liều ăn nhiều", Traits = new() { ("RiskTolerance", 0.99), ("ChaosEnergy", 0.9) } },
                        new() { Content = "Từ chối thẳng thừng, tính mạng là trên hết", Traits = new() { ("Stability", 0.95), ("Order", 0.9) } },
                        new() { Content = "Rủ bạn thân cùng tham gia lập liên minh", Traits = new() { ("Emotion", 0.85), ("Exploration", 0.85) } },
                        new() { Content = "Nghiên cứu kỹ luật chơi trước khi quyết định", Traits = new() { ("Order", 0.9), ("Patience", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Nhảy dù từ độ cao 4000m hay Lặn ngắm cá mập dưới đáy biển?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🪂 Nhảy dù trên không trung bao la", Traits = new() { ("RiskTolerance", 0.95), ("Independence", 0.9) } },
                        new() { Content = "🦈 Lặn ngắm cá mập đáy đại dương", Traits = new() { ("Exploration", 0.95), ("ChaosEnergy", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Bạn bị lạc trong một khu rừng rậm khi trời bắt đầu tối dần:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Tìm nguồn nước và dựng chỗ trú ẩn ngay lập tức", Traits = new() { ("Order", 0.9), ("Patience", 0.9) } },
                        new() { Content = "Đốt lửa lớn phát tín hiệu cầu cứu", Traits = new() { ("RiskTolerance", 0.85), ("Stability", 0.8) } },
                        new() { Content = "Dựa vào sao Bắc Đẩu để tiếp tục đi tìm lối ra", Traits = new() { ("Intuition", 0.95), ("Exploration", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Bạn chọn đối đầu với nỗi sợ nào?",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🕷️ Rắn rết côn trùng độc", Traits = new() { ("Patience", 0.8), ("Stability", 0.75) } },
                        new() { Content = "👻 Bóng tối và sự cô độc", Traits = new() { ("Independence", 0.9), ("Intuition", 0.85) } },
                        new() { Content = "⚡ Bão giông sấm sét dữ dội", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.85) } },
                        new() { Content = "📉 Hết tiền và không có chỗ ngủ", Traits = new() { ("MemeAffinity", 0.9), ("Order", 0.7) } }
                    }
                },
                new()
                {
                    Content = "Thử một món ăn đặc sản cực kỳ kỳ dị ở nước ngoài:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Ăn thử một miếng ngay không ngần ngại", Traits = new() { ("Exploration", 0.95), ("RiskTolerance", 0.9) } },
                        new() { Content = "Ngửi mùi trước, nếu ổn mới dám nếm", Traits = new() { ("Order", 0.8), ("Stability", 0.8) } },
                        new() { Content = "Nhất quyết không ăn, chỉ ăn món quen thuộc", Traits = new() { ("Stability", 0.95), ("Patience", 0.8) } },
                        new() { Content = "Thách đố bạn cùng bàn ăn trước", Traits = new() { ("MemeAffinity", 0.9), ("ChaosEnergy", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Lựa chọn con đường bằng phẳng an toàn hay lối mòn gập ghềnh hiểm trở?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "Đường bằng an toàn vững chãi", Traits = new() { ("Stability", 0.95), ("Order", 0.9) } },
                        new() { Content = "Lối mòn gập ghềnh cảnh sắc tuyệt mỹ", Traits = new() { ("Exploration", 0.95), ("RiskTolerance", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bạn nhặt được một chiếc rương báu cổ có ổ khóa mã số bí ẩn:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Dùng búa đập khóa luôn không cần giải mã", Traits = new() { ("ChaosEnergy", 0.95), ("RiskTolerance", 0.9) } },
                        new() { Content = "Kiên nhẫn thử từng mã số logic", Traits = new() { ("Order", 0.95), ("Patience", 0.95) } },
                        new() { Content = "Bấm mã theo ngày sinh của người yêu cũ", Traits = new() { ("Nostalgia", 0.9), ("Intuition", 0.85) } }
                    }
                },
                new()
                {
                    Content = "Vượt qua giới hạn bản thân mang lại cho bạn cảm giác gì?",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Sự tự do và kiêu hãnh tuyệt đối", Traits = new() { ("Independence", 0.95), ("RiskTolerance", 0.85) } },
                        new() { Content = "Cảm giác bình yên sau cơn bão", Traits = new() { ("Patience", 0.9), ("Stability", 0.85) } },
                        new() { Content = "Khát khao tiếp tục chinh phục đỉnh cao mới", Traits = new() { ("Exploration", 0.95), ("ChaosEnergy", 0.85) } },
                        new() { Content = "Hạnh phúc vì đã không từ bỏ giữa chừng", Traits = new() { ("Emotion", 0.9), ("Patience", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Một chuyến thám hiểm Bắc Cực hay Khám phá sa mạc Sahara?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "❄️ Băng giá Bắc Cực kiên cường", Traits = new() { ("Patience", 0.9), ("Stability", 0.85) } },
                        new() { Content = "🏜️ Nắng cháy Sahara rực lửa", Traits = new() { ("RiskTolerance", 0.9), ("Exploration", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh: Tinh thần phiêu lưu trong bạn hiện tại:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "🦁 Dũng mãnh như sư tử săn mồi", Traits = new() { ("RiskTolerance", 0.95), ("Independence", 0.9) } },
                        new() { Content = "🦅 Tự do như đại bàng tung cánh", Traits = new() { ("Exploration", 0.95), ("Independence", 0.95) } },
                        new() { Content = "🐢 Bền bỉ như rùa vượt ngàn dặm", Traits = new() { ("Patience", 0.95), ("Stability", 0.9) } },
                        new() { Content = "🐒 Tinh nghịch như khỉ chuyền cành", Traits = new() { ("MemeAffinity", 0.9), ("ChaosEnergy", 0.9) } }
                    }
                }
            }
        });

        // 10. THEME_DESTINY (Định mệnh & Tâm linh meme - Climax Theme)
        list.Add(new ThemeSeed
        {
            Code = "THEME_DESTINY",
            Name = "Định mệnh & Tâm linh",
            Description = "Quẻ bói meme, thông điệp vũ trụ, linh vật phong thủy và điềm báo may mắn",
            Icon = "🔮",
            SortOrder = 10,
            Questions = new List<QuestionSeed>
            {
                new()
                {
                    Content = "🔮 Thông điệp vũ trụ gửi đến bạn hôm nay là gì?",
                    Subtitle = "Khoảnh khắc linh thiêng quyết định con số vận mệnh",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "🌟 Cứ nát hết mình, vũ trụ sẽ tự chữa lành", Traits = new() { ("MemeAffinity", 0.95), ("SpiritualVibe", 0.9) } },
                        new() { Content = "🔥 May mắn đang gõ cửa, hãy mở toang sổ tiết kiệm", Traits = new() { ("RiskTolerance", 0.9), ("Intuition", 0.9) } },
                        new() { Content = "🌊 Bình tĩnh sống, Jackpot rồi cũng sẽ tới lượt", Traits = new() { ("Patience", 0.95), ("Stability", 0.9) } },
                        new() { Content = "⚡ Quyết định bất ngờ sẽ tạo nên kỳ tích", Traits = new() { ("ChaosEnergy", 0.95), ("Intuition", 0.95) } }
                    }
                },
                new()
                {
                    Content = "Chọn một linh vật tâm linh meme hộ mệnh cho bạn:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "🐱 Mèo Thần Tài vẫy tay mỏi nhừ cầu tiền về", Traits = new() { ("Stability", 0.85), ("SpiritualVibe", 0.9) } },
                        new() { Content = "🐸 Cóc Ngậm Tiền ngậm luôn cả vận may", Traits = new() { ("Intuition", 0.9), ("Order", 0.8) } },
                        new() { Content = "🦆 Vịt Vàng bối rối nhưng luôn gặp hên", Traits = new() { ("MemeAffinity", 0.95), ("ChaosEnergy", 0.85) } },
                        new() { Content = "🐲 Rồng Vàng uy nghi phun ra Jackpot", Traits = new() { ("RiskTolerance", 0.95), ("SpiritualVibe", 0.95) } }
                    }
                },
                new()
                {
                    Content = "Bạn nằm mơ thấy một giấc mơ kỳ lạ đêm qua:",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Giải mã giấc mơ theo sổ mơ dân gian đánh số", Traits = new() { ("SpiritualVibe", 0.95), ("Nostalgia", 0.85) } },
                        new() { Content = "Tin rằng đó là điềm báo của một khởi đầu rực rỡ", Traits = new() { ("Intuition", 0.95), ("Emotion", 0.85) } },
                        new() { Content = "Quên sạch sẽ sau khi uống cốc nước", Traits = new() { ("Independence", 0.85), ("Stability", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Bấm ngay lập tức: Con số tâm linh huyền bí của bạn:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "✨ 777 - Phép màu vũ trụ", Traits = new() { ("SpiritualVibe", 0.99), ("Intuition", 0.95) } },
                        new() { Content = "💰 888 - Tiền tài vô như nước", Traits = new() { ("Stability", 0.9), ("Order", 0.85) } },
                        new() { Content = "👑 999 - Quyền lực trường cửu", Traits = new() { ("Patience", 0.95), ("Independence", 0.9) } },
                        new() { Content = "🃏 000 - Khởi đầu vô tận", Traits = new() { ("ChaosEnergy", 0.95), ("Exploration", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Lá bài Tarot bí ẩn nào đang vẫy gọi bạn?",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "🃏 The Fool - Khởi đầu tự do không sợ hãi", Traits = new() { ("Exploration", 0.95), ("RiskTolerance", 0.9) } },
                        new() { Content = "🎡 Wheel of Fortune - Vòng quay định mệnh xoay chuyển", Traits = new() { ("SpiritualVibe", 0.95), ("Intuition", 0.95) } },
                        new() { Content = "⭐ The Star - Hy vọng và niềm tin tỏa sáng", Traits = new() { ("Emotion", 0.9), ("Patience", 0.85) } },
                        new() { Content = "🏰 The Tower - Phá vỡ trật tự cũ để tái sinh", Traits = new() { ("ChaosEnergy", 0.99), ("RiskTolerance", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bạn tin vào Thần Tài hay tin vào Bản Thân?",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🙏 Có thờ có thiêng, có kiêng có lành", Traits = new() { ("SpiritualVibe", 0.95), ("Intuition", 0.9) } },
                        new() { Content = "💪 Tự lực cánh sinh, số phận trong tay mình", Traits = new() { ("Independence", 0.95), ("Order", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Một điềm báo may mắn bất ngờ xuất hiện:",
                    QuestionType = QuestionType.SingleChoice,
                    Choices = new()
                    {
                        new() { Content = "Phân chim rơi trúng đầu lúc đi làm", Traits = new() { ("MemeAffinity", 0.95), ("SpiritualVibe", 0.85) } },
                        new() { Content = "Nhặt được tờ tiền rơi trên đường", Traits = new() { ("Intuition", 0.85), ("Stability", 0.8) } },
                        new() { Content = "Bướm bay lượn quanh người 3 vòng", Traits = new() { ("SpiritualVibe", 0.9), ("Emotion", 0.85) } },
                        new() { Content = "Đồng hồ chỉ đúng 11:11 lúc vô tình nhìn vào", Traits = new() { ("Intuition", 0.95), ("Order", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Thầy bói phán: \"Năm nay bạn sẽ có một bước ngoặt tài chính cực lớn\":",
                    QuestionType = QuestionType.Scenario,
                    Choices = new()
                    {
                        new() { Content = "Mua ngay vé Bịp lót hàng ngày để đón đầu cơ hội", Traits = new() { ("RiskTolerance", 0.95), ("SpiritualVibe", 0.9) } },
                        new() { Content = "Cười trừ và tiếp tục chăm chỉ làm việc", Traits = new() { ("Patience", 0.9), ("Order", 0.9) } },
                        new() { Content = "Xin thêm quẻ xem nên lấy chồng/vợ năm nào", Traits = new() { ("Emotion", 0.9), ("Nostalgia", 0.8) } }
                    }
                },
                new()
                {
                    Content = "Hành cung mệnh của bạn nghiêng về:",
                    QuestionType = QuestionType.ThisOrThat,
                    Choices = new()
                    {
                        new() { Content = "🔥 Kim & Hỏa - Quyết liệt, bùng nổ, đổi đời", Traits = new() { ("RiskTolerance", 0.9), ("ChaosEnergy", 0.85) } },
                        new() { Content = "💧 Mộc & Thủy - Mềm mại, bền bỉ, trường tồn", Traits = new() { ("Patience", 0.95), ("Stability", 0.9) } }
                    }
                },
                new()
                {
                    Content = "Bấm nhanh câu thần chú chốt số:",
                    QuestionType = QuestionType.QuickInstinct,
                    Choices = new()
                    {
                        new() { Content = "✨ \"Bịp lót xin số, nát cũng cam lòng!\"", Traits = new() { ("MemeAffinity", 0.99), ("ChaosEnergy", 0.95) } },
                        new() { Content = "🎯 \"Vạn sự tùy duyên, số này ắt trúng!\"", Traits = new() { ("Intuition", 0.95), ("SpiritualVibe", 0.95) } },
                        new() { Content = "🍀 \"Tâm thành ắt ứng nghiệm!\"", Traits = new() { ("Patience", 0.9), ("SpiritualVibe", 0.9) } },
                        new() { Content = "🚀 \"All-in đổi đời ngay hôm nay!\"", Traits = new() { ("RiskTolerance", 0.99), ("Independence", 0.85) } }
                    }
                }
            }
        });

        return list;
    }
}
