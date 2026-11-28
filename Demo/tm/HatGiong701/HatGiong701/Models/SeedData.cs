using HatGiong701.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HatGiong701.Models
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                await context.Database.MigrateAsync();
                // 1. TẠO QUYỀN (ROLES) VÀ TÀI KHOẢN ADMIN MẶC ĐỊNH
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

                string[] roleNames = { "Admin", "Customer" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Tài khoản Admin: admin@hatgiong.com / Admin@123
                var adminEmail = "admin@hatgiong.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    var newAdmin = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Quản Trị Viên",
                        Address = "An Giang, Việt Nam",
                        EmailConfirmed = true
                    };
                    var createAdmin = await userManager.CreateAsync(newAdmin, "Admin@123");
                    if (createAdmin.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdmin, "Admin");
                    }
                }

                // 2. TẠO DỮ LIỆU 8 DANH MỤC THẬT
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Hạt giống rau ăn lá", Description = "Các loại rau xanh ngắn ngày, dễ trồng tại nhà" },
                        new Category { Name = "Hạt giống củ quả", Description = "Cà chua, dưa leo, ớt, bầu bí cho sân vườn" },
                        new Category { Name = "Hạt giống hoa", Description = "Các loại hoa trang trí ban công, sân thượng" },
                        new Category { Name = "Hạt giống rau gia vị", Description = "Húng quế, ngò rí, hành lá, bạc hà" },
                        new Category { Name = "Hạt giống rau mầm", Description = "Cải mầm, mầm hướng dương, mầm củ cải đỏ (rất được ưa chuộng vì thu hoạch chỉ sau 5–7 ngày)." },
                        new Category { Name = "Hạt giống cây ăn trái", Description = "Dâu tây, dưa lưới, dưa hấu mini, chanh dây..." },
                        new Category { Name = "Hạt giống cây dược liệu", Description = "Bạc hà, sả chanh, cúc la mã, cỏ ngọt." },
                        new Category { Name = "Hạt giống cây cảnh mini", Description = "Sen đá, xương rồng mix, cỏ may mắn để bàn làm việc." }
                    );
                    await context.SaveChangesAsync();
                }

                // 3. TẠO DỮ LIỆU 17 HẠT GIỐNG THẬT
                if (!context.Seeds.Any())
                {
                    var catRau = context.Categories.First(c => c.Name.Contains("rau ăn lá")).CategoryId;
                    var catQua = context.Categories.First(c => c.Name.Contains("củ quả")).CategoryId;
                    var catHoa = context.Categories.First(c => c.Name.Contains("hoa")).CategoryId;
                    var catGiaVi = context.Categories.First(c => c.Name.Contains("gia vị")).CategoryId;
                    var catRauMam = context.Categories.First(c => c.Name.Contains("rau mầm")).CategoryId;
                    var catAnTrai = context.Categories.First(c => c.Name.Contains("cây ăn trái")).CategoryId;
                    var catDuocLieu = context.Categories.First(c => c.Name.Contains("cây dược liệu")).CategoryId;
                    var catCanhMini = context.Categories.First(c => c.Name.Contains("cây cảnh mini")).CategoryId;

                    context.Seeds.AddRange(
                        new Seed
                        {
                            Name = "Hạt giống Cải ngọt mỡ F1",
                            Price = 15000m,
                            StockQuantity = 60,
                            ImageUrl = "/images/seeds/f3d699df-b6a1-4740-8bde-05ae2b017eb3.jpg",
                            Description = "Giống cải ngọt chịu nhiệt tốt, thân mập, lá xanh non mềm ngọt, ít sâu bệnh, thích hợp trồng thùng xốp tại nhà.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Ban công",
                            CareLevel = "Dễ",
                            GerminationRate = 92,
                            HarvestDays = 28,
                            PlantingGuide = "Gieo hạt trực tiếp lên bề mặt đất ẩm, phủ lớp đất mỏng 0.5cm, tưới phun sương giữ ẩm 2 lần/ngày.",
                            CategoryId = catRau
                        },
                        new Seed
                        {
                            Name = "Hạt giống Xà lách xoăn Mỹ",
                            Price = 18000m,
                            StockQuantity = 45,
                            ImageUrl = "/images/seeds/e7ef1494-ab79-4eb8-bdf8-784b67bda91d.jpg",
                            Description = "Lá xanh giòn ngọt, mép lá xoăn đẹp mắt, không đắng, có thể tỉa lá ăn dần nhiều lần.",
                            PlantingSeason = "Thu",
                            SuitableSpace = "Trong nhà",
                            CareLevel = "Dễ",
                            GerminationRate = 88,
                            HarvestDays = 35,
                            PlantingGuide = "Không cần ngâm ủ, rải đều hạt lên giá thể tơi xốp, đặt nơi thoáng mát có ánh sáng nhẹ.",
                            CategoryId = catRau
                        },
                        new Seed
                        {
                            Name = "Hạt giống Cà chua bi đỏ lùn",
                            Price = 25000m,
                            StockQuantity = 35,
                            ImageUrl = "/images/seeds/7559c700-ccae-458b-a58c-d1975a9a7c2d.jpg",
                            Description = "Cây thấp lùn cao khoảng 40–50cm thích hợp trồng chậu, sai trĩu quả đỏ mọng, vị ngọt thanh.",
                            PlantingSeason = "Xuân",
                            SuitableSpace = "Ban công",
                            CareLevel = "Trung bình",
                            GerminationRate = 85,
                            HarvestDays = 65,
                            PlantingGuide = "Ngâm hạt nước ấm 2 sôi 3 lạnh trong 4 giờ trước khi gieo. Đặt chậu nơi có nắng tối thiểu 6 tiếng/ngày.",
                            CategoryId = catQua
                        },
                        new Seed
                        {
                            Name = "Hạt giống Dưa leo baby",
                            Price = 50000m,
                            StockQuantity = 30,
                            ImageUrl = "/images/seeds/84f79993-b674-4549-8234-6da80b0b4eb1.jpg",
                            Description = "Trái nhỏ vừa ăn, vỏ mỏng xanh bóng, đặc ruột giòn ngọt, không cần thụ phấn tay khi trồng ban công.",
                            PlantingSeason = "Hè",
                            SuitableSpace = "Sân vườn",
                            CareLevel = "Trung bình",
                            GerminationRate = 90,
                            HarvestDays = 45,
                            PlantingGuide = "Ngâm hạt nước ấm 3 giờ, ủ khăn giấy ẩm đến khi nứt nanh rồi gieo vào chậu lớn, làm giàn leo khi cây cao 25cm.",
                            CategoryId = catQua
                        },
                        new Seed
                        {
                            Name = "Hạt giống Hoa Hướng Dương",
                            Price = 45000m,
                            StockQuantity = 50,
                            ImageUrl = "/images/seeds/15e55467-aa51-4300-bd79-519cf3a95ed6.jpg",
                            Description = "Cây lùn cao khoảng 35–45cm, bông to vàng rực rỡ, rất bền hoa, thích hợp trồng chậu trang trí bàn hoặc ban công.",
                            PlantingSeason = "Hè",
                            SuitableSpace = "Ban công",
                            CareLevel = "Dễ",
                            GerminationRate = 90,
                            HarvestDays = 55,
                            PlantingGuide = "Gieo hạt sâu 1cm vào đất tơi xốp, tưới giữ ẩm đều trong 5 ngày đầu và đặt nơi nhiều nắng.",
                            CategoryId = catHoa
                        },
                        new Seed
                        {
                            Name = "Hạt giống Hoa Mười Giờ",
                            Price = 20000m,
                            StockQuantity = 70,
                            ImageUrl = "/images/seeds/6e50d2f4-11ec-4ff4-8940-d47f535baf9d.jpg",
                            Description = "Hoa cánh kép rực rỡ đủ màu sắc (đỏ, vàng, hồng, cam), chịu nắng hạn cực tốt, nở rộ mỗi sáng.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Ban công",
                            CareLevel = "Dễ",
                            GerminationRate = 85,
                            HarvestDays = 40,
                            PlantingGuide = "Trộn hạt nhỏ với ít cát khô rồi rải đều lên mặt đất ẩm, không lấp đất dày, phun sương nhẹ mỗi sáng.",
                            CategoryId = catHoa
                        },
                        new Seed
                        {
                            Name = "Hạt giống Húng quế",
                            Price = 20000m,
                            StockQuantity = 8,
                            ImageUrl = "/images/seeds/2017cdf2-4c38-416e-8327-d2acd80e79aa.jpg",
                            Description = "Mùi thơm nồng đặc trưng, dễ chăm sóc, càng bấm ngọn cây càng đẻ nhiều nhánh thu hoạch quanh năm.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Trong nhà",
                            CareLevel = "Dễ",
                            GerminationRate = 82,
                            HarvestDays = 40,
                            PlantingGuide = "Rải hạt lên bề mặt đất ẩm, phủ lớp trấu hun mỏng, đặt nơi có ánh sáng tán xạ đến khi cây lên 2 lá thật.",
                            CategoryId = catGiaVi
                        },
                        new Seed
                        {
                            Name = "Hạt giống Ngò rí",
                            Price = 15000m,
                            StockQuantity = 55,
                            ImageUrl = "/images/seeds/dfde6b05-af12-4fa4-a02b-f1cca38b846a.jpg",
                            Description = "Thân xanh non, lá thơm đậm, sinh trưởng đồng đều, dùng làm rau sống hoặc gia vị nêm nếm hằng ngày.",
                            PlantingSeason = "Đông",
                            SuitableSpace = "Ban công",
                            CareLevel = "Dễ",
                            GerminationRate = 85,
                            HarvestDays = 35,
                            PlantingGuide = "Chà nhẹ cho hạt tách đôi rồi ngâm nước ấm 6 giờ trước khi gieo giúp hạt nảy mầm nhanh gấp đôi.",
                            CategoryId = catGiaVi
                        },
                        new Seed
                        {
                            Name = "Hạt giống rau mầm Củ cải đỏ",
                            Price = 22000m,
                            StockQuantity = 80,
                            ImageUrl = "/images/seeds/3a2b1c64-07a2-496d-8967-9bfe407cf643.jpg",
                            Description = "Thân mầm màu tím hồng đẹp mắt, vị cay nhẹ giòn ngon, giàu vitamin, thu hoạch siêu nhanh chỉ sau 5–7 ngày.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Trong nhà",
                            CareLevel = "Dễ",
                            GerminationRate = 95,
                            HarvestDays = 6,
                            PlantingGuide = "Ngâm hạt nước ấm 4 giờ, rải dày đều lên khay giá thể xơ dừa ẩm, đậy kín 2 ngày đầu rồi đưa ra nơi sáng nhẹ.",
                            CategoryId = catRauMam
                        },
                        new Seed
                        {
                            Name = "Hạt giống Dâu tây",
                            Price = 60000m,
                            StockQuantity = 25,
                            ImageUrl = "/images/seeds/9de2cd8c-f050-4240-b740-2c20c4b95576.jpg",
                            Description = "Giống dâu tây thích nghi tốt khí hậu Việt Nam, trái đỏ tươi, mùi thơm ngọt dịu, trồng chậu treo rất đẹp.",
                            PlantingSeason = "Xuân",
                            SuitableSpace = "Ban công",
                            CareLevel = "Khó",
                            GerminationRate = 75,
                            HarvestDays = 90,
                            PlantingGuide = "Ngâm hạt nước ấm 3 giờ, gieo trên bề mặt viên nén xơ dừa hoặc đất mịn ẩm, che mát giữ ẩm liên tục 10–14 ngày.",
                            CategoryId = catAnTrai
                        },
                        new Seed
                        {
                            Name = "Hạt giống Dưa hấu",
                            Price = 50000m,
                            StockQuantity = 6,
                            ImageUrl = "/images/seeds/6a8bda5b-f09b-4504-8fbe-6cade97aa46a.jpg",
                            Description = "Trái tròn nhỏ nặng khoảng 1.5–2kg, vỏ rất mỏng, ruột đỏ tươi ít hạt, vị ngọt sắc mát lạnh.",
                            PlantingSeason = "Hè",
                            SuitableSpace = "Sân vườn",
                            CareLevel = "Trung bình",
                            GerminationRate = 88,
                            HarvestDays = 65,
                            PlantingGuide = "Bấm nhẹ đầu nhọn hạt, ngâm nước ấm 4 giờ, ủ ẩm nứt nanh rồi trồng vào thùng xốp lớn nơi nhiều nắng.",
                            CategoryId = catAnTrai
                        },
                        new Seed
                        {
                            Name = "Hạt giống Bạc hà Âu",
                            Price = 30000m,
                            StockQuantity = 40,
                            ImageUrl = "/images/seeds/2650b72d-3848-4378-a134-5128bbb61269.jpg",
                            Description = "Lá xanh đậm chứa hàm lượng tinh dầu cao, vị the mát sảng khoái, dùng pha trà, làm đồ uống hoặc xua đuổi muỗi.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Trong nhà",
                            CareLevel = "Trung bình",
                            GerminationRate = 80,
                            HarvestDays = 50,
                            PlantingGuide = "Rải hạt trực tiếp lên mặt giá thể ẩm, không lấp đất, dùng bình xịt phun sương giữ ẩm hằng ngày.",
                            CategoryId = catDuocLieu
                        },
                        new Seed
                        {
                            Name = "Hạt giống Cúc la mã",
                            Price = 55000m,
                            StockQuantity = 32,
                            ImageUrl = "/images/seeds/33b64158-ca60-4ad1-bb0b-45fb8e0a75a9.jpg",
                            Description = "Hoa nhỏ xinh cánh trắng nhụy vàng thơm dịu như táo chín, thu hái hoa phơi khô pha trà giúp thư giãn ngủ ngon.",
                            PlantingSeason = "Thu",
                            SuitableSpace = "Ban công",
                            CareLevel = "Trung bình",
                            GerminationRate = 82,
                            HarvestDays = 60,
                            PlantingGuide = "Rải hạt trên đất mịn ẩm, ấn nhẹ cho hạt bám đất, đặt chậu nơi thoáng mát có nắng nhẹ buổi sáng.",
                            CategoryId = catDuocLieu
                        },
                        new Seed
                        {
                            Name = "Hạt giống Sen đá mix nhiều loại",
                            Price = 25000m,
                            StockQuantity = 20,
                            ImageUrl = "/images/seeds/cad5b56d-efe2-43a5-8ae0-14cd1a6194cc.jpg",
                            Description = "Bộ hạt giống tổng hợp nhiều dòng sen đá cánh hoa nhỏ nhắn, màu sắc biến đổi đẹp mắt khi phơi nắng sáng.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Trong nhà",
                            CareLevel = "Khó",
                            GerminationRate = 78,
                            HarvestDays = 75,
                            PlantingGuide = "Dùng giá thể thoát nước cực tốt (perlite, pumice, mụn dừa), rải hạt trên mặt và đậy màng bọc thực phẩm giữ ẩm 2 tuần đầu.",
                            CategoryId = catCanhMini
                        },
                        new Seed
                        {
                            Name = "Hạt giống Cỏ may mắn",
                            Price = 55000m,
                            StockQuantity = 59,
                            ImageUrl = "/images/seeds/55547d06-d765-48b9-8b96-3dcaf165a7fa.jpg",
                            Description = "Thảm cỏ xanh mướt mọc dày đều trong chậu sứ mini, mang ý nghĩa tài lộc và tạo mảng xanh thư giãn cho bàn học, bàn làm việc.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Trong nhà",
                            CareLevel = "Dễ",
                            GerminationRate = 94,
                            HarvestDays = 10,
                            PlantingGuide = "Ngâm hạt 3 giờ cho nở lớp gel bên ngoài, rải kín bề mặt đất ẩm trong chậu nhỏ và phun sương 2 lần/ngày.",
                            CategoryId = catCanhMini
                        },
                        new Seed
                        {
                            Name = "Hạt giống Xà lách Thủy tinh",
                            Price = 24000m,
                            StockQuantity = 55,
                            ImageUrl = "/images/seeds/7d83d1a4-44f5-42f7-83cb-b98b5be81241.jpg",
                            Description = "Giống xà lách chuyên dụng cho giàn thủy canh hồi lưu và thùng thủy canh tĩnh, lá xanh bóng giòn tan, chịu nhiệt tốt, không bị đắng đầu lá.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Thủy canh",
                            CareLevel = "Dễ",
                            GerminationRate = 92,
                            HarvestDays = 35,
                            PlantingGuide = "Đặt 1-2 hạt vào viên mút xốp đã ngậm no nước sạch, giữ ẩm trong bóng râm 48 giờ cho hạt nảy mầm rồi đưa lên rọ thủy canh.",
                            CategoryId = catRau
                        },
                        new Seed
                        {
                            Name = "Hạt giống Cải thìa",
                            Price = 20000m,
                            StockQuantity = 22,
                            ImageUrl = "/images/seeds/877596f9-f7fb-4f9d-bdc5-28af879de518.jpg",
                            Description = "Cây phát triển bẹ to mập, lá xanh đậm mềm ngọt, bộ rễ trắng khỏe hút dinh dưỡng cực tốt trong môi trường nước.",
                            PlantingSeason = "Quanh năm",
                            SuitableSpace = "Thủy canh",
                            CareLevel = "Dễ",
                            GerminationRate = 94,
                            HarvestDays = 30,
                            PlantingGuide = "Gieo hạt vào mút xốp ẩm, khi cây ra 2 lá thật thì chuyển lên giàn thủy canh với nồng độ dinh dưỡng PPM khoảng 600-800.",
                            CategoryId = catRau
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // 4. TẠO DỮ LIỆU 5 BÀI VIẾT CẨM NANG THẬT
                if (!context.BlogPosts.Any())
                {
                    context.BlogPosts.AddRange(
                        new BlogPost
                        {
                            Title = "Kỹ thuật ngâm ủ hạt giống theo quy tắc 2 sôi 3 lạnh giúp nảy mầm trên 90%",
                            Category = "Kỹ thuật gieo trồng",
                            Summary = "Hướng dẫn chi tiết cách pha nước ấm chuẩn nhiệt độ và ủ khăn giấy ẩm đúng kỹ thuật, khắc phục triệt để tình trạng hạt bị thối hoặc nảy mầm không đều.",
                            Content = "Nhiều người mới bắt đầu trồng rau tại nhà thường gặp tình trạng gieo hạt cả tuần nhưng cây không lên mầm. Nguyên nhân phổ biến nhất là do lớp vỏ hạt còn cứng chưa được đánh thức đúng cách. Quy tắc \"2 sôi 3 lạnh\" chính là bí quyết đơn giản giúp tăng tỷ lệ nảy mầm lên đến 90%.\n\n1. Cách pha nước ấm 2 sôi 3 lạnh chuẩn nhất:\n- Lấy 2 phần nước sôi (100°C) pha cùng 3 phần nước lạnh ở nhiệt độ phòng.\n- Nhiệt độ nước thu được sẽ rơi vào khoảng 40°C - 45°C (sờ tay thấy ấm dễ chịu, không bị rát).\n- Lưu ý: Không ngâm các loại hạt quá nhỏ như hoa mười giờ, bạc hà, xà lách vì hạt nhỏ dễ bị vón cục và trôi mất.\n\n2. Thời gian ngâm tùy theo độ dày vỏ hạt:\n- Hạt vỏ mỏng (cải ngọt, cải thìa, dưa leo): Ngâm từ 2 - 3 tiếng.\n- Hạt vỏ trung bình (cà chua, ớt, rau muống): Ngâm từ 4 - 6 tiếng.\n- Hạt vỏ cứng dày (mướp đắng, bầu bí, rau mầm hướng dương): Ngâm từ 6 - 8 tiếng.\n\n3. Cách ủ hạt nứt nanh bằng khăn giấy ẩm:\n- Sau khi ngâm đủ giờ, vớt hạt ra rửa sạch lớp nhớt bên ngoài bằng nước sạch.\n- Trải 2 lớp khăn giấy ăn lên hộp nhựa có nắp, xịt nước cho khăn ẩm đều (không đọng nước).\n- Rải hạt lên khăn giấy, đậy thêm 1 lớp khăn ẩm lên trên và đậy kín nắp hộp, đặt nơi thoáng mát tránh nắng trực tiếp.\n- Kiểm tra mỗi ngày 1 lần: khi thấy hạt vừa nứt nanh trắng (nhú mầm khoảng 1-2mm) là có thể đem gieo ngay vào giá thể.",
                            ImageUrl = "/images/blogs/f5be56d2ef5d425ca41e72d9e953d057.jpg",
                            CreatedDate = DateTime.Now.AddDays(-4)
                        },
                        new BlogPost
                        {
                            Title = "Hướng dẫn trồng rau thủy canh tại nhà cho người mới bắt đầu từ A đến Z",
                            Category = "Kỹ thuật gieo trồng",
                            Summary = "Không cần đất, không lo sâu bệnh từ bùn đất và tiết kiệm tối đa thời gian chăm sóc — tìm hiểu quy trình 4 bước tự trồng rau thủy canh sạch ngay tại ban công.",
                            Content = "Trồng rau thủy canh đang trở thành xu hướng được nhiều gia đình ở thành phố lựa chọn vì sự sạch sẽ, gọn gàng và năng suất cao. Chỉ với một thùng xốp hoặc bộ rọ thủy canh nhỏ gọn, bạn đã có thể tự trồng các loại xà lách, cải thìa, cải Kale ngay tại ban công.\n\n1. Chuẩn bị dụng cụ cơ bản:\n- Thùng thủy canh tĩnh (hoặc giàn ống hồi lưu), rọ nhựa thủy canh.\n- Giá thể ươm hạt: Mút xốp chuyên dụng hoặc viên nén xơ dừa.\n- Dinh dưỡng thủy canh (dung dịch A và B) và bút đo nồng độ PPM.\n- Hạt giống chuyên dụng (xà lách thủy tinh, cải thìa, cải bó xôi, húng lủi).\n\n2. Bước ươm hạt trên mút xốp:\n- Nhúng mút xốp vào nước sạch và bóp nhẹ cho mút ngậm no nước.\n- Đặt 1-2 hạt giống vào khe giữa của miếng mút xốp.\n- Đặt khay mút vào chỗ râm mát 2 ngày đầu, phun sương giữ ẩm cho đến khi hạt nảy mầm.\n\n3. Đưa cây lên giàn và pha dinh dưỡng:\n- Khi cây con mọc được 2 lá thật và rễ trắng đâm xuyên qua đáy mút xốp, bạn đặt từng miếng mút vào rọ nhựa rồi xếp lên giàn.\n- Pha dung dịch dinh dưỡng vào nước theo đúng tỷ lệ hướng dẫn (mức PPM lý tưởng cho rau ăn lá giai đoạn đầu là 600 - 800 PPM, giai đoạn phát triển mạnh là 800 - 1100 PPM).\n\n4. Lưu ý chăm sóc hằng ngày:\n- Đảm bảo vị trí đặt thùng thủy canh nhận được ít nhất 4 - 5 tiếng nắng nhẹ mỗi ngày.\n- Định kỳ mỗi tuần kiểm tra mực nước trong thùng, châm thêm nước sạch khi thấy mực nước hụt đi để rễ cây luôn tiếp xúc được với dinh dưỡng.",
                            ImageUrl = "/images/blogs/90671b288ae74cc6a131c6242479f33c.jpg",
                            CreatedDate = DateTime.Now.AddDays(-3)
                        },
                        new BlogPost
                        {
                            Title = "Công thức trộn đất trồng rau trong thùng xốp tơi xốp, thoát nước tốt và giàu dinh dưỡng",
                            Category = "Chăm sóc & Bón phân",
                            Summary = "Bí quyết phối trộn giá thể từ đất thịt, mụn dừa, trấu hun và phân trùn quế giúp rễ cây phát triển mạnh, hạn chế úng rễ vào mùa mưa.",
                            Content = "Chất lượng đất trồng quyết định đến 70% sự thành công của vườn rau tại nhà. Nếu chỉ dùng đất thịt thông thường, sau vài lần tưới đất sẽ bị nén chặt, bí rễ và rất dễ gây vàng lá, thối gốc.\n\n1. Công thức trộn đất chuẩn tỷ lệ 4 : 3 : 2 : 1:\n- 4 phần Đất sạch nền (đất thịt nhẹ hoặc đất phù sa đã xử lý mầm bệnh).\n- 3 phần Giá thể tạo độ tơi xốp (mụn dừa đã xả chát kết hợp với vỏ trấu hun).\n- 2 phần Phân hữu cơ giàu dinh dưỡng (phân trùn quế hoặc phân bò hoai mục).\n- 1 phần Khoáng chất và vi sinh (đá Perlite giúp thoát nước và nấm đối kháng Trichoderma phòng ngừa nấm rễ).\n\n2. Cách xử lý mụn dừa trước khi trộn:\n- Mụn dừa thô thường chứa nhiều chất chát (tanin) làm nghẹt rễ cây con. Bạn nên ngâm mụn dừa trong nước sạch từ 1-2 ngày rồi vắt ráo trước khi đem trộn đất, hoặc mua loại mụn dừa đã qua xử lý sẵn.\n\n3. Mẹo cải tạo đất sau mỗi vụ thu hoạch:\n- Sau khi thu hoạch xong một lứa rau, không cần đổ bỏ đất cũ. Hãy nhổ sạch rễ già, rắc thêm một ít vôi bột nông nghiệp, phơi nắng 2-3 ngày để diệt nấm bệnh.\n- Sau đó bổ sung thêm 20% phân trùn quế mới và trấu hun là có thể tiếp tục gieo lứa hạt tiếp theo.",
                            ImageUrl = "/images/blogs/e20618b8ef8440b38f05473a76b95427.jpg",
                            CreatedDate = DateTime.Now.AddDays(-2)
                        },
                        new BlogPost
                        {
                            Title = "3 công thức pha dung dịch tỏi ớt gừng xua đuổi sâu rầy an toàn cho vườn rau sạch",
                            Category = "Phòng trừ sâu bệnh",
                            Summary = "Tự làm thuốc trừ sâu sinh học ngay tại bếp từ những nguyên liệu sẵn có, bảo vệ rau xanh khỏi rệp sáp, sâu ăn lá mà vẫn đảm bảo an toàn tuyệt đối cho sức khỏe gia đình.",
                            Content = "Khi trồng rau sạch tại nhà, chắc chắn bạn không muốn sử dụng thuốc bảo vệ thực vật hóa học. Hỗn hợp tỏi, ớt và gừng chứa các hoạt chất cay nồng tự nhiên có khả năng xua đuổi và làm tê liệt hệ thần kinh của rệp sáp, sâu xanh, bọ trĩ rất hiệu quả.\n\n1. Nguyên liệu cần chuẩn bị:\n- 100g tỏi ta (tỏi càng già tinh dầu càng mạnh).\n- 100g ớt hiểm cay.\n- 100g gừng tươi.\n- 1 lít rượu trắng (khoảng 35 - 40 độ) và 1 hũ thủy tinh sạch có nắp đậy.\n\n2. Cách ngâm ủ dung dịch cốt:\n- Rửa sạch tỏi, ớt, gừng, để thật ráo nước rồi đem xay nhuyễn hoặc giã nhỏ.\n- Cho toàn bộ hỗn hợp vào hũ thủy tinh, đổ ngập 1 lít rượu trắng vào và đậy kín nắp.\n- Đặt hũ nơi thoáng mát, tránh ánh nắng trực tiếp trong khoảng 10 - 15 ngày là có thể chắt lấy nước cốt dùng dần.\n\n3. Cách pha loãng và phun cho cây:\n- Pha 20ml - 30ml nước cốt tỏi ớt gừng với 1 lít nước sạch (có thể nhỏ thêm 2 giọt nước rửa chén sinh học để tăng độ bám dính trên mặt lá).\n- Phun đều lên cả mặt trên và mặt dưới lá vào lúc chiều mát (sau 16h30).\n- Lưu ý: Nên phun phòng định kỳ 7 - 10 ngày/lần. Trước khi thu hoạch rau khoảng 3 ngày thì ngưng phun để rau không bị ám mùi cay nồng.",
                            ImageUrl = "/images/blogs/f81172393f804bfeb30647d785278886.jpg",
                            CreatedDate = DateTime.Now.AddDays(-1)
                        },
                        new BlogPost
                        {
                            Title = "Cẩm nang cây cảnh ban công đẹp chuẩn resort mini",
                            Category = "Vườn ban công",
                            Summary = "Cây cảnh không chỉ mang lại giá trị thẩm mỹ, mà còn góp phần thanh lọc không khí, giảm tiếng ồn, tạo cảm giác gần gũi với thiên nhiên. Đặc biệt, với diện tích ban công nhỏ, việc lựa chọn đúng loại cây và bố trí hợp lý sẽ biến góc nhỏ ấy trở thành chốn nghỉ dưỡng chuẩn resort ngay trong ngôi nhà của bạn.",
                            Content = "Chăm sóc cây cảnh đúng cách là yếu tố quan trọng giúp ban công luôn xanh đẹp và bền vững. Theo cẩm nang cây cảnh ban công, bạn nên chú ý đến chế độ tưới nước, thời điểm thích hợp nhất là vào buổi sáng sớm hoặc chiều mát để cây hấp thụ tốt và tránh bị sốc nhiệt. Bên cạnh đó, việc bón phân hữu cơ định kỳ mỗi 1–2 tháng sẽ giúp cây luôn khỏe mạnh, lá xanh bóng và ra hoa đúng mùa.\n\nĐối với ban công nhỏ, cắt tỉa thường xuyên sẽ giữ cho cây gọn gàng, không chiếm diện tích và tạo tính thẩm mỹ cao. Ngoài ra, cần chọn chậu có lỗ thoát nước để hạn chế tình trạng úng rễ, đồng thời vệ sinh ban công thường xuyên để tránh ẩm mốc. Những mẹo nhỏ này sẽ giúp không gian ban công của bạn luôn tươi mới, mang lại cảm giác thư giãn đúng chuẩn resort mini.\n\nNgoài việc áp dụng cẩm nang cây cảnh ban công, bạn có thể thêm những chi tiết decor:\n- Sàn lát gỗ nhựa hoặc thảm cỏ nhân tạo.\n- Bàn ghế mây tre nhỏ gọn.\n- Đèn vàng ấm áp, nến thơm hoặc chuông gió.\n- Tiểu cảnh nước mini, sỏi trắng rải lối.",
                            ImageUrl = "/images/blogs/b90ba86352424b30adafaf383479362b.jpg",
                            CreatedDate = DateTime.Now
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}