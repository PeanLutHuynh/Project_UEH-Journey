using System;
using System.Text;
using System.Threading;
using System.Collections.Generic; 
using System.IO;
using System.Threading.Tasks;


namespace Role_Playing_Game;

public partial class Program
{
    static string tempPath = "Tempdata.txt";
    static Character? _character;
    static char[][]? _map;
    static DateTime previoiusRender = DateTime.Now;
    static int movesSinceLastBattle;
    static bool gameRunning = true;
    const double randomBattleChance = 1d / 10d;
    const int movesBeforeRandomBattle = 4;
    static DateTime StartTime;
    static TimeSpan savedTime; 

    static Character Character
    {
        get => _character!;
        set => _character = value;
    }

    static char[][] Map
    {
        get => _map!;
        set => _map = value;
    }

    private static readonly string[] maptext =
    [
        "Di chuyển: Các phím mũi tên hoặc (w, a, s, d)",
        "Kiểm tra trạng thái: [enter]",
        "Thoát trò chơi: [escape]",
    ];

    private static readonly string[] defaultCombatText =
    [
        "1) Chiến đấu",
        "2) Bỏ chạy",
        "3) Kiểm tra trạng thái",
    ];

    private static string[]? _combatText;

    static string[] CombatText
    {
        get => _combatText!;
        set => _combatText = value;
    }


    public static void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        Exception? exception = null;
        try
        {

            Console.CursorVisible = false;
            Initialize();
            OpeningScreen();
            StartTime = DateTime.Now;
            savedTime = LoadSavedTime();
            while (gameRunning)
            {
                UpdateCharacter();
                HandleMapUserInput();
                if (gameRunning)
                {
                    RenderWorldMapView();
                    SleepAfterRender();
                }
            }
        }
        catch (Exception e)
        {
            exception = e;
            throw;
        }
        finally
        {
            Console.Clear();
            Console.WriteLine(exception?.ToString() ?? "Trò chơi kết thúc.");
            Console.CursorVisible = true;
        }
    }

    static void Initialize()
    {
        Map = Maps.Town;
        Character = new();
        {
            var (i, j) = FindTileInMap(Map, 'X')!.Value;
            Character.I = i * 7;
            Character.J = j * 4;
        }
        Character.MapAnimation = Sprites.IdleRight;
    }

    static void IntroScreen(string[] lines)
    {
        int screenWidth = Console.WindowWidth;
        int screenHeight = Console.WindowHeight;

        int startRow = (screenHeight / 2) - (lines.Length / 2);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            int startColumn = (screenWidth - line.Length) / 2;

            Console.SetCursorPosition(startColumn, startRow + i);

            Console.WriteLine(line);
        }
    }

    static void OpeningScreen()
    {
        Console.Clear();
        string[] line1 =
{
@"                   _________       _______     ________________     ________         _______                    ",
@"                  /         |     /       |   /                |   /        |       /       |                   ",
@"                  |$$$$$$$  |    $$$$$$$  |   |$$$$$$$$$$$$$$  |   |$$$$$$$ |      $$$$$$$  |                   ",
@"                  |$$$$$$$  |    $$$$$$$  |   |$$$$$$$$$$$$$$__|   |$$$$$$$ |      $$$$$$$  |                   ",
@"                  |$$$$$$$  |    $$$$$$$  |   |$$$$$      |____    |$$$$$$$ \______$$$$$$$  |                   ",
@"                  |$$$$$$$  |    $$$$$$$  |   |$$$$$$$$$$$$$$  |   |$$$$$$$$$$$$$$$$$$$$$$  |                   ",
@"                  |$$$$$$$  |___ $$$$$$$  |   |$$$$$$$$$$$$$$__|   |$$$$$$$$$$$$$$$$$$$$$$  |                   ",
@"                  |$$$$$$$       $$$$$$$  |   |$$$$$      |____    |$$$$$$$ /------$$$$$$$  |                   ",
@"                  |$$$$$$$$$$$$$$$$$$$$$ /    |$$$$$$$$$$$$$$  /   |$$$$$$$ |      $$$$$$$  |                   ",
@"                   \$$$$$$$$$$$$$$$$$$$ /     |$$$$$$$$$$$$$$ /    |$$$$$$$ |      $$$$$$$ /                    ",
@"                                     _                                                           __             ",
@"                                    /_\                             $$\                         /$/             ",
@"             ______   __   __  __  _____  __   __      __      __   _/$  __   __      __ __  __  __             ",
@"            /$$$$$ \ /$$| |$$| $$| $$$$/ |$$|  $$|    |$$     |$$|  $$| |$$|  $$|    |$$/$/  \$$/$$/            ",
@"            |$$$_$_/ |$$\-/$$| $$| $$--\ |$$|  $$|    |$$     |$$|  $$| |$$|  $$|    |$$$|    \$$$/             ",
@"            |$$|     |$$| |$$| $$| $$$$\ \$$$$$$$/    \$$$$$$ \$$$$$$$/ \$$$$$$$/    |$$\$\   /$$/              ",
@"                                                                                                                ",
@"                                                                                                                ",
@"                                       Nhấn [Enter] để bắt đầu trò chơi                                         ",
@"                                                                                                                ",
@"                                                                                                                "
    };

        Console.ForegroundColor = ConsoleColor.Cyan;
        IntroScreen(line1);
        Console.WriteLine();
        Console.ResetColor();
        PressEnterToContinue();
        string[] line2 = {
            "Bạn sắp bước vào cuộc hành trình của 1 sinh viên UEH.",
            "",
            "Chúc bạn ra trường thành công",
            "",
            "Nhấn [enter] để bắt đầu..."
        };

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow; 
        IntroScreen(line2);
        PressEnterToContinue();


        Console.WriteLine();
        Console.Write(" Nhập họ tên của bạn: ");
        Character.Name = Console.ReadLine();

        Console.Write(" Nhập nơi sinh: ");
        Character.PlaceOfBirth = Console.ReadLine();

        Console.Write(" Nhập năm sinh: ");
        int year;
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out year) && year <= 2024 && year >=1900)
            {
                Character.Year = year;
                break;
            }
            else
            {
                Console.Write(" Không hợp lệ. Nhập lại số nguyên phù hợp và bé hơn năm hiện tại: ");
            }
        }

        Console.Write(" Nhập MSSV: ");
        long studentId;
        while (true)
        {
            if (long.TryParse(Console.ReadLine(), out studentId) && studentId > 30000000000)
            {
                Character.StudentID = studentId;
                break;
            }
            else
            {
                Console.Write(" Không hợp lệ. Nhập lại MSSV đúng định dạng: ");
            }
        }

        Console.Write(" Nhập ngành học: ");
        Character.Major = Console.ReadLine();
        PrintData(studentId);
    }

    static void UpdateCharacter()
    {
        if (Character.MapAnimation == Sprites.RunUp && Character.MapAnimationFrame is 2 or 4 or 6) Character.J--;
        if (Character.MapAnimation == Sprites.RunDown && Character.MapAnimationFrame is 2 or 4 or 6) Character.J++;
        if (Character.MapAnimation == Sprites.RunLeft) Character.I--;
        if (Character.MapAnimation == Sprites.RunRight) Character.I++;
        Character.MapAnimationFrame++;

        if (Character.Moved)
        {
            HandleCharacterMoved();
            Character.Moved = false;
        }
    }

    static void HandleCharacterMoved()
    {
        movesSinceLastBattle++;
        switch (Map[Character.TileJ][Character.TileI])
        {
            case 'B': CSB(); break;
            case 'N': CSN(); break;
            case 'A': CSA(); break;
            case 'i': SleepAtInn(); break;
            case 's': ShopAtStore(); break;
            case 'c': OpenChest(); break;
            case '0': TransitionMapToTown(); break;
            case '1': TransitionMapToField(); break;
            case '2': TransitionMapToCastle(); break;
            case 'g': FightGuardBoss(); break;
            case ' ': ChanceForRandomBattle(); break;
            case 'k': FightKing(); break;
            case 'h': HiddenWaterFountain(); break;
        }
    }

    static void SaveData()
    {
        string tempPath = "Tempdata.txt";
        TimeSpan totalTimePlayed = LoadSavedTime() + (DateTime.Now - StartTime);
        Character.Time = FormatPlayTime(totalTimePlayed);
        using (StreamWriter writer = new StreamWriter(tempPath, false)) 
        {
            writer.WriteLine(Character.StudentID);
            writer.WriteLine(Character.Name);
            writer.WriteLine(Character.Level);
            writer.WriteLine(Character.Experience);
            writer.WriteLine(Character.ExperienceToNextLevel);
            writer.WriteLine(Character.Health);
            writer.WriteLine(Character.Gold);
            writer.WriteLine(Character.Damage);
            writer.WriteLine(Character.HighScore);
            if (Character.ScoreB == 5 || Character.ScoreB == 6) writer.WriteLine(Character.ScoreB); else writer.WriteLine("0");
            if (Character.ScoreN == 5 || Character.ScoreN == 6) writer.WriteLine(Character.ScoreN); else writer.WriteLine("0");
            if (Character.ScoreA == 5 || Character.ScoreA == 6) writer.WriteLine(Character.ScoreA); else writer.WriteLine("0");
            writer.WriteLine(Character.Time);

        }
    }

    static void PrintData(long studentId)
    {
        string tempPath = "Tempdata.txt";
        if (File.Exists(tempPath))
        {
            FileInfo fileInfo = new FileInfo(tempPath);
            string? firstLine;
            using (StreamReader reader = new StreamReader(tempPath))
            {
                firstLine = reader.ReadLine();
            }
            if (fileInfo.Length != 0 && studentId.ToString() == firstLine)
            {
                for (int k = 0; k < 40; k++) Console.Write("-");
                Console.WriteLine();
                Console.WriteLine(" Hệ thống tìm thấy dữ liệu của bạn trước đó,");
                Console.WriteLine(" Bạn có muốn khôi phục dữ liệu và tiếp tục trò chơi không ?");
                Console.WriteLine(" (Nếu bạn chọn không thì dữ liệu sẽ bị xóa toàn bộ),");
                Console.WriteLine();
            inputagain:
                Console.Write(" Lựa chọn của bạn là (Nhấn Yes (Y) hoặc No (N)): ");
                char playerAnswer = Char.ToUpper(Console.ReadKey(false).KeyChar);
                Console.WriteLine();
                if (playerAnswer == 'Y')
                {
                    string[] lines = new string[13];
                    lines = File.ReadAllLines(tempPath);

                    Character.Level = int.Parse(lines[2]);
                    Character.Experience = int.Parse(lines[3]);
                    Character.ExperienceToNextLevel = int.Parse(lines[4]);
                    Character.Health = int.Parse(lines[5]);
                    Character.Gold = int.Parse(lines[6]);
                    Character.Damage = int.Parse(lines[7]);
                    Character.HighScore = int.Parse(lines[8]);
                    Character.ScoreB = int.Parse(lines[9]);
                    Character.ScoreN = int.Parse(lines[10]);
                    Character.ScoreA = int.Parse(lines[11]);
                    Character.Time = lines[12];
                    Console.WriteLine(" Dữ liệu đã được khôi phục thành công.");
                    Console.WriteLine();
                    Console.Write(" Nhấn [enter] để tiếp tục...");
                    PressEnterToContinue();
                }
                else if (playerAnswer == 'N')
                {
                    File.WriteAllText(tempPath, string.Empty);
                    Console.WriteLine();
                    Console.WriteLine(" Dữ liệu lưu trước đó đã được xóa.");
                    Console.WriteLine();
                    Console.Write(" Nhấn [enter] để tiếp tục...");
                    PressEnterToContinue();
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(" Lệnh không khả dụng, hãy nhập lại!");
                    goto inputagain;
                }
            }
        }
        else
        {
            Console.WriteLine(" File không tồn tại.");
        }
    }

    static string FormatPlayTime(TimeSpan timePlayed)
    {
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
            timePlayed.Hours,
            timePlayed.Minutes,
            timePlayed.Seconds);
        return formattedTime;
    }

    static TimeSpan LoadSavedTime()
    {
        string tempPath = "Tempdata.txt";
        if (File.Exists(tempPath))
        {
            string[] lines = File.ReadAllLines(tempPath);
            if (lines.Length >= 13)
            {
                Character.Time = lines[12]; 
                string[] timeParts = Character.Time.Split(':');
                if (timeParts.Length == 3 &&
                    int.TryParse(timeParts[0], out int hours) &&
                    int.TryParse(timeParts[1], out int minutes) &&
                    int.TryParse(timeParts[2], out int seconds))
                {
                    return new TimeSpan(hours, minutes, seconds); 
                }
            }
        }
        return TimeSpan.Zero; 
    }

    static void RecordTime()
    {
        TimeSpan savedTime = LoadSavedTime(); 
        TimeSpan currentSessionTime = DateTime.Now - StartTime; 
        TimeSpan totalTimePlayed = savedTime + currentSessionTime; 
        Character.Time = FormatPlayTime(totalTimePlayed); 
    }

   static async Task RenderStatusString()
    {
        Console.Clear(); 

        
        _ = DisplayPlayTime();

        while (gameRunning)
        {
            Console.SetCursorPosition(0, 0); 
            Console.WriteLine();
            Console.WriteLine(" Bảng trạng thái");
            Console.WriteLine();
            Console.WriteLine($" Cấp độ:      {Character.Level}");
            Console.WriteLine($" Kinh nghiệm: {Character.Experience}/{Character.ExperienceToNextLevel}");
            Console.WriteLine($" Thể lực:     {Character.Health}/{Character.MaxHealth}");
            Console.WriteLine($" Xu:          {Character.Gold}");
            Console.WriteLine($" Sức mạnh:    {Character.Damage}");
            Console.WriteLine($" Điểm:        {Character.HighScore}");
            Console.WriteLine($" Thời gian chơi: {Character.Time}"); 
            Console.WriteLine();
            Console.Write(" Nhấn [enter] để tiếp tục...");
            
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
            {
                return;
            }
            await Task.Delay(100);
        }
    }


    static async Task DisplayPlayTime()
    {
        while (gameRunning)
        {
            RecordTime(); 
            await Task.Delay(1000); 
        }
    }

    static void RenderDeathScreen()
    {
        Console.Clear();
        Console.WriteLine();
        RecordTime();
        Console.WriteLine(" Có lẽ thử thách lần này quá khó nhằn với bạn...");
        Console.WriteLine();
        Console.WriteLine(" Bạn thất bại trong việc chinh phục đời sinh viên");
        Console.WriteLine();
        Console.WriteLine(" Trò chơi kết thúc.");
        Console.WriteLine();
        Console.WriteLine(" Thời gian bạn chơi là: " + Character.Time + " với số điểm là: " + Character.HighScore);
        Console.Write(" Bạn có muốn chơi lại không? (Nhấn Yes (Y) hoặc No (N)): ");
        char input = Char.ToUpper(Console.ReadKey(false).KeyChar);
        if (input == 'Y')
        {
            Console.WriteLine("");
            Console.WriteLine(" Đang khởi động lại trò chơi");
            Thread.Sleep(3000);
            Initialize();
            while (gameRunning)
            {
                UpdateCharacter();
                HandleMapUserInput();

                StartTime = DateTime.Now;
                while (gameRunning)
                {
                    UpdateCharacter();
                    HandleMapUserInput();
                    if (gameRunning)
                    {
                        RenderWorldMapView();
                        SleepAfterRender();
                    }
                }
            }
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine(" Hẹn gặp bạn lần sau");
            Console.WriteLine();
            Console.Write(" Nhấn [enter] để tiếp tục...");
            PressEnterToContinue();
        }
    }

    static void PressEnterToContinue()
    {
    GetInput:
        ConsoleKey key = Console.ReadKey(true).Key;
        switch (key)
        {
            case ConsoleKey.Enter:
                return;
            case ConsoleKey.Escape:
                Console.Clear();
            decideagain:
                Console.WriteLine(" Bạn có chắc muốn thoát trò chơi không (tiến trình của bạn sẽ được lưu lại)?");
                Console.WriteLine(" Nhấn Yes (Y) hoặc No (N):");
                char input = Char.ToUpper(Console.ReadKey(true).KeyChar);
                if (input == 'Y')
                {
                    Console.WriteLine();
                    RecordTime();
                    SaveData();
                    Console.WriteLine(" Hẹn gặp bạn lần sau");
                    Thread.Sleep(1000);
                    gameRunning = false;
                    return;
                }
                else if (input == 'N')
                {
                    return;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine(" Lệnh không khả dụng, hãy nhập lại!");
                    goto decideagain;
                }
            default:
                Console.WriteLine(" Lệnh không hợp lệ, hãy nhập lại!");
                goto GetInput;
        }
    }

    static void OpenChest()
    {
        Character.Gold++;
        Map[Character.TileJ][Character.TileI] = 'e';
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine(" Bạn nhặt được 1 đồng xu trong chiếc hộp.");
        Console.WriteLine();
        Console.WriteLine($" Số xu hiện đang sở hữu: {Character.Gold}");
        Console.WriteLine();
        Console.Write(" Nhấn [enter] để tiếp tục...");
        PressEnterToContinue();
    }

    static void SleepAtInn()
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine(" Bạn vào KTX để nghỉ ngơi...");
        Console.WriteLine();
        Console.WriteLine(" ZzzZzzZzz...");
        Console.WriteLine();
        Console.WriteLine(" Thể lục của bạn đã hồi phục.");
        Console.WriteLine();
        Console.Write(" Nhấn phím [enter] để tiếp tục...");
        Character.Health = Character.MaxHealth;
        PressEnterToContinue();
    }

    static void HiddenWaterFountain()
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine(" Bạn đi lạc vào 1 phòng ẩn");
        Console.WriteLine(" Tiếng nhạc trong phòng vang lên tiếp thêm cho bạn sức mạnh");
        Console.WriteLine(" Bạn hồi tưởng về những gì mình đã trải qua trong cuộc hành trình");
        Console.WriteLine();
        Console.WriteLine(" Thể lực của bạn đã hồi phục.");
        Console.WriteLine();
        Console.Write(" Nhấn [enter] để tiếp tục...");
        Character.Health = Character.MaxHealth;
        PressEnterToContinue();
    }

    static void TransitionMapToTown()
    {
        Map = Maps.Town;
        var (i, j) = FindTileInMap(Map, '1')!.Value;
        Character.I = i * 7;
        Character.J = j * 4;
    }

    static void TransitionMapToField()
    {
        char c = Map == Maps.Town ? '0' : '2';
        Map = Maps.Field;
        var (i, j) = FindTileInMap(Map, c)!.Value;
        Character.I = i * 7;
        Character.J = j * 4;
    }

    static void TransitionMapToCastle()
    {
        Map = Maps.Castle;
        var (i, j) = FindTileInMap(Map, '1')!.Value;
        Character.I = i * 7;
        Character.J = j * 4;
    }

    static void ReadyForBattle()
    {
        if (Character.ScoreB == 5 && Character.ScoreN == 5 && Character.ScoreA == 5)
        {
            Console.Clear();
            string[] introLines = new string[]
    {
        " Đã đến lúc bạn đối diện với thử thách cuối cùng rồi",
        "",
        " Hãy tiến đến tòa lâu đài nằm ở bìa rừng để đặt dấu chấm hết cho cuộc hành trình này!",
        "",
        " Nhấn phím [Enter] để tiếp tục..."
    };
            IntroScreen(introLines);
            PressEnterToContinue();
        }
    }

    static void CSB()
    {
        Console.Clear();
        if (Character.ScoreB == 5)
        {
            Console.WriteLine("Bạn đã hoàn thành thử thách của cơ sở B rồi");
            Console.WriteLine();
            Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
            PressEnterToContinue();
        }
        else if (Character.ScoreB == 6)
        {
            Console.WriteLine("Bạn không còn quyền thực hiện thử thách của cơ sở B nữa");
            Console.WriteLine();
            Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
            PressEnterToContinue();
        }
        else
        {
            Console.WriteLine("THỬ THÁCH CỦA CƠ SỞ B UEH:");
            Console.WriteLine();
            Console.WriteLine("Thực hiện bài test Sinh Hoạt Công Dân sau:");
            string[] questions =
        {
        "UEH được thành lập vào năm nào?",
        "Khoa nào tại UEH chuyên về đào tạo Quản trị Kinh doanh?",
        "UEH có trụ sở chính tại quận nào của TP.HCM?",
        "Màu chính thức của UEH là màu gì?",
        "Phương châm của UEH là gì?",
        "Động lực chính nào để thúc đẩy đổi mới sáng tạo và phát triển bền vững?",
        "Tư duy thiết kế và đổi mới sáng tạo bền vững được áp dụng cho đối tượng nào?",
        "Công ty nào đã thành công trong việc chuyển đổi từ một nhà bán lẻ truyền thống sang một nền tảng thương mại điện tử hàng đầu?",
        "Việc chuyển đổi số trong doanh nghiệp nhỏ và vừa (SME) cần chú ý điều gì?",
        "Để chuẩn bị cho kỷ nguyên số, bạn cần rèn luyện điều gì?"
        };
            string[,] options =
            {
        { "A. 1976", "B. 1980", "C. 1985", "D. 1990" },
        { "A. Khoa Tài chính", "B. Khoa Marketing", "C. Khoa Quản trị", "D. Khoa Kinh tế" },
        { "A. Quận 1", "B. Quận 3", "C. Quận 10", "D. Quận Bình Thạnh" },
        { "A. Đỏ", "B. Vàng", "C. Xanh lá cây", "D. Xanh dương" },
        { "A. Kiến tạo tương lai", "B. Tiên phong đổi mới", "C. Vì cộng đồng phát triển", "D. Tri thức vì cuộc sống" },
        { "A. Áp lực từ đối thủ cạnh tranh", "B. Đổi mới Tăng trưởng kinh tế nhanh chóng mà không cân nhắc hậu quả", "C. Hành vi con người, phát triển công nghệ tư duy đổi mới sáng tạo", "D. Sự phổ biến của công nghệ cao" },
        { "A. Dành riêng cho các công ty lớn", "B. Chỉ dành cho những nhà khoa học", "C. Dành cho tất cả mọi người", "D. Chỉ dành cho những người có kiến thức chuyên môn cao" },
        { "A. Barnes & Noble", "B. Blockbuster", "C. Walmart", "D. Amazon" },
        { "A. Tận dụng công nghệ số để tối ưu hóa hoạt động mở rộng thị trường", "B. Chỉ sử dụng công nghệ khi bắt buộc", "C. Giảm đầu tư vào công nghệ", "D. Tập trung vào thị trường nội địa" },
        { "A. Kỹ năng giao tiếp qua thư tay", "B. Mindset, skillset, toolset", "C. Kỹ năng làm việc với giấy tờ", "D. Sử dụng máy đánh chữ" },
        };
            int falseAns = 5;
            Random random = new Random();
            List<int> selectedIndexes = new List<int>();

            
            while (selectedIndexes.Count < 5)
            {
                int randomIndex = random.Next(questions.Length);
                if (!selectedIndexes.Contains(randomIndex))
                {
                    selectedIndexes.Add(randomIndex);
                }
            }
            char[] correctAnswers = { 'A', 'C', 'B', 'D', 'D', 'C', 'C', 'C', 'A', 'B' };
            Console.WriteLine("Bấm YES (Y) để bắt đầu hoặc NO (N) để kết thúc");
    
        inputagain:
            if (falseAns == 0)
            {
                Character.ScoreB = 6;
                Console.WriteLine("Bạn đã hết số lần tham gia trò chơi");
                Console.WriteLine();
                Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                PressEnterToContinue();
            }
            else
            {
                Console.Write("Lựa chọn của bạn là: ");
                char playerAnswer = Char.ToUpper(Console.ReadKey(false).KeyChar);
                Console.WriteLine();
                if (playerAnswer == 'Y')
                {
                    bool allCorrect = false;

                    while (!allCorrect && falseAns > 0)
                    {
                        allCorrect = true; 

                        for (int i = 0; i < selectedIndexes.Count; i++)
                        {
                            int questionIndex = selectedIndexes[i];
                            for (int k = 0; k < 40; k++) Console.Write("-");
                            Console.WriteLine();
                            Console.WriteLine(" Câu hỏi " + (i + 1) + ": " + questions[questionIndex]);

                            for (int j = 0; j < 4; j++)
                            {
                                Console.WriteLine(options[questionIndex, j]);
                            }

                            Console.Write("Nhập đáp án (A, B, C, D): ");
                            playerAnswer = Char.ToUpper(Console.ReadKey(false).KeyChar);
                            Console.WriteLine();

                            if (playerAnswer == correctAnswers[questionIndex])
                            {
                                Console.WriteLine();
                                Console.WriteLine("Chính xác!");
                                Character.ScoreB++;
                            }
                            else
                            {
                                Console.WriteLine();
                                Character.ScoreB = 0;
                                falseAns--;
                                Console.WriteLine("Rất tiếc! Bạn đã trả lời sai");
                                if (falseAns == 0)
                                {
                                    goto inputagain;
                                }
                                else
                                {
                                    Console.WriteLine("Bạn còn " + falseAns + " lần chơi lại! Bạn có muốn chơi lại không ?");
                                    Console.WriteLine();
                                    goto inputagain;
                                }
                            }
                        }

                        for (int k = 0; k < 40; k++) Console.Write("-");
                        Console.WriteLine();
                        Console.WriteLine("Chúc mừng bạn đã hoàn thành thử thách và nhận được 1000 điểm");
                        Console.WriteLine();
                        Console.WriteLine("Bạn đã lên cấp");
                        Character.Level++;
                        Character.Experience = 0;
                        Character.ExperienceToNextLevel *= 2;
                        Character.Damage++;
                        Character.Health = Character.MaxHealth;
                        Character.HighScore += 1000;
                        Console.WriteLine();
                        Console.WriteLine($"Cấp độ hiện tại của bạn là: {Character.Level}.");
                        Console.WriteLine();
                        Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                        PressEnterToContinue();
                        ReadyForBattle();
                    }
                }
                else if (playerAnswer == 'N')
                {
                    for (int k = 0; k < 40; k++) Console.Write("-");
                    Console.WriteLine("");
                    Console.WriteLine("Hẹn gặp lại bạn lần sau!");
                    Console.WriteLine();
                    Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                    PressEnterToContinue();
                }
                else
                {
                    Console.WriteLine("Lệnh không khả dụng. Vui lòng chọn lại.");
                    goto inputagain;
                }
            }
        }
    }

    static void CSN()
    {
        Console.Clear();
        if (Character.ScoreB != 5)
        {
            Console.WriteLine("Bạn phải hoàn thành thử thách của cơ sở B trước đã!");
            Console.WriteLine();
            Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
            PressEnterToContinue();
        }
        else
        {
            if (Character.ScoreN == 5)
            {
                Console.WriteLine("Bạn đã hoàn thành thử thách của cơ sở N rồi");
                Console.WriteLine();
                Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                PressEnterToContinue();
            }
            else if (Character.ScoreN == 6)
            {
                Console.WriteLine("Bạn không còn quyền thực hiện thử thách của cơ sở N nữa");
                Console.WriteLine();
                Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                PressEnterToContinue();
            }
            else
            {
                Console.WriteLine("THỬ THÁCH CỦA CƠ SỞ N UEH:");
                Console.WriteLine();
                Console.WriteLine("Thực hiện bài test cơ sở ngành sau:");
                string[] questions =
                {
            "Phương thức nào được sử dụng để chuyển đổi một chuỗi thành chữ hoa trong C#?",
            "Khi nào vòng lặp foreach được sử dụng trong C#?",
            "Kết quả của biểu thức 10 % 3 là gì?",
            "Nếu biến x có giá trị là 5, kết quả của biểu thức ++x * 2 là gì?",
            "Để khai báo một hàm không trả về giá trị trong C#, ta sử dụng từ khóa nào?",
            "Kết quả mà khoa học dữ liệu hướng đến là:",
            "Lựa chọn nào sau đây không phải là một bước trong quy trình khai thác dữ liệu:",
            "Tiền xử lý dữ liệu không bao gồm các bước nào sau đây:",
            "Phân lớp dữ liệu là thuộc phương pháp:",
            "Thuật toán phân lớp nào sau đây cho phép xử lý trên nhiều kiểu/loại dữ liệu khác nhau:",
            };
                string[,] options =
                {
            { "A. String.ToUpperCase()", "B. String.ToUpper()", "C. String.ConvertToUpper()", "D. String.Uppercase()" },
            { "A. Để lặp qua các phần tử của một mảng hoặc danh sách", "B. Để lặp qua một số nguyên nhất định", "C. Để lặp vô hạn", "D. Để thực hiện lặp lồng"},
            { "A. 3", "B. 1", "C. 10", "D. 0" },
            { "A. 10", "B. 12", "C. 8", "D. 6" },
            { "A. void", "B. int", "C. return", "D. funtion" },
            { "A. Dữ liệu", "B. Thông tin", "C. Tri thức", "D. Tất cả đều đúng" },
            { "A. Data Understading", "B. Data preparatiion", "C. Data mining", "D. Evaluation" },
            { "A. Làm sạch dữ liệu", "B. Chuyển đổi dữ liệu", "C. Thu thập dữ liệu", "D. Rút gọn dữ liệu" },
            { "A. Không giám sát", "B. Bán giám sát", "C. Có giám sát", "D. Phương pháp lai" },
            { "A. SVM", "B. Cây quyết định", "C. Logistic Regression", "D. Mạng nơ ron" }
            };
                int falseAns = 5;
                Random random = new Random();
                List<int> selectedIndexes = new List<int>();

                while (selectedIndexes.Count < 5)
                {
                    int randomIndex = random.Next(questions.Length);
                    if (!selectedIndexes.Contains(randomIndex))
                    {
                        selectedIndexes.Add(randomIndex);
                    }
                }
                char[] correctAnswers = { 'B', 'A', 'B', 'B', 'A', 'C', 'C', 'C', 'C', 'B' };
                Console.WriteLine("Bấm YES (Y) để bắt đầu hoặc NO (N) để kết thúc");
            inputagain:
                if (falseAns == 0)
                {
                    Character.ScoreN = 6;
                    Console.WriteLine("Bạn đã hết số lần tham gia trò chơi");
                    Console.WriteLine();
                    Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                    PressEnterToContinue();
                }
                else
                {
                    Console.Write("Lựa chọn của bạn là: ");
                    char playerAnswer = Char.ToUpper(Console.ReadKey(false).KeyChar);
                    Console.WriteLine();


                    if (playerAnswer == 'Y')
                    {


                        bool allCorrect = false;

                        while (!allCorrect && falseAns > 0)
                        {
                            allCorrect = true; 

                            for (int i = 0; i < selectedIndexes.Count; i++)
                            {
                                int questionIndex = selectedIndexes[i];
                                for (int k = 0; k < 40; k++) Console.Write("-");
                                Console.WriteLine();
                                Console.WriteLine(" Câu hỏi " + (i + 1) + ": " + questions[questionIndex]);

                                for (int j = 0; j < 4; j++)
                                {
                                    Console.WriteLine(options[questionIndex, j]);
                                }

                                Console.Write("Nhập đáp án (A, B, C, D): ");
                                playerAnswer = Char.ToUpper(Console.ReadKey(false).KeyChar);
                                Console.WriteLine();

                                if (playerAnswer == correctAnswers[questionIndex])
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Chính xác!");
                                    Character.ScoreN++;
                                }
                                else
                                {
                                    falseAns--;
                                    Character.ScoreN = 0;
                                    Console.WriteLine();
                                    Console.WriteLine("Rất tiếc! Bạn đã trả lời sai");
                                    if (falseAns == 0)
                                    {
                                        goto inputagain;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Bạn còn " + falseAns + " lần chơi lại! Bạn có muốn chơi lại không ?");
                                        Console.WriteLine();
                                        goto inputagain;
                                    }
                                }
                            }

                            for (int k = 0; k < 40; k++) Console.Write("-");
                            Console.WriteLine();
                            Console.WriteLine("Chúc mừng bạn đã hoàn thành thử thách và nhận được 1000 điểm");
                            Console.WriteLine();
                            Console.WriteLine("Bạn đã lên cấp");
                            Character.Level++;
                            Character.Experience = 0;
                            Character.ExperienceToNextLevel *= 2;
                            Character.Damage++;
                            Character.Health = Character.MaxHealth;
                            Character.HighScore += 1000;
                            Console.WriteLine();
                            Console.WriteLine($"Cấp độ hiện tại của bạn là: {Character.Level}.");
                            Console.WriteLine();
                            Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                            PressEnterToContinue();
                            ReadyForBattle();
                        }
                    }
                    else if (playerAnswer == 'N')
                    {
                        for (int k = 0; k < 40; k++) Console.Write("-");
                        Console.WriteLine("");
                        Console.WriteLine("Hẹn gặp lại bạn lần sau!");
                        Console.WriteLine();
                        Console.WriteLine("Nhấn phím [Enter] để tiếp tục");
                        PressEnterToContinue();
                    }
                    else
                    {
                        Console.WriteLine("Lệnh không khả dụng. Vui lòng chọn lại.");
                        goto inputagain;
                    }
                }
            }
        }
    }


    static void CSA()
    {
        Console.Clear();
        if (Character.ScoreN != 5)
        {
            Console.WriteLine("Bạn phải hoàn thành thử thách của cơ sở N trước đã!");
            Console.WriteLine();
            Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
            PressEnterToContinue();
        }
        else
        {
            if (Character.ScoreA == 5)
            {
                Console.WriteLine("Bạn đã hoàn thành thử thách của cơ sở A rồi");
                Console.WriteLine();
                Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                PressEnterToContinue();
            }
            else if (Character.ScoreA == 6)
            {
                Console.WriteLine("Bạn không còn quyền thực hiện thử thách của cơ sở A nữa");
                Console.WriteLine();
                Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                PressEnterToContinue();
            }
            else
            {
                Console.WriteLine("THỬ THÁCH CỦA CƠ SỞ A UEH:");
                Console.WriteLine();
                Console.WriteLine("Thực hiện bài test về các tập đoàn công nghệ sau:");
                string[] questions =
                {
            "Cty/doanh nghiệp công nghệ hợp tác với ĐH UEH là",
            "Cty công nghệ có doanh thu cao nhất VN năm 2023",
            "Microsoft thành lập vào năm nào?",
            "Cty phần mềm lớn nhất Việt Nam",
            "Nhà sáng lập của Apple?",
            "Thung lũng Silicon nằm ở đâu tại Hoa Kỳ?",
            "“Big Blue” là biệt danh của tập đoàn nào?",
            "Viettel đứng thứ mấy thế giới về lĩnh vực viễn thông?",
            "Elon Musk mua lại twitter vào năm nào?",
            "Tập đoàn viễn thông Quân Đội VN?"
            };
                string[,] options =
                {
            { "A. HSC", "B. VRG", "C. FAST", "D. ACB" },
            { "A. FPT", "B. VIETTEL", "C. SEVT", "D. NashTech" },
            { "A. 1975", "B. 1976", "C. 1977", "D. 1974" },
            { "A. FPT", "B. Rikkeisoft", "C. CMC Corporationy", "D. Harvey Nash" },
            { "A. Steve Jobs", "B. Tim Cook", "C. Mark Zuckerberg", "D. Chris Hughes" },
            { "A. New York", "B. Washington", "C. San Francisco", "D. Florida" },
            { "A. Nvidia", "B. IBM", "C. Hewlett-Packard", "D. Oracle" },
            { "A. 1", "B. 2", "C. 3", "D. 4" },
            { "A. 2020", "B. 2021", "C. 2022", "D. 2023" },
            { "A. Viettel", "B. Nvidia", "C. FPT", "D. VNPT" }
            };
                int falseAns = 5;
                Random random = new Random();
                List<int> selectedIndexes = new List<int>();

                while (selectedIndexes.Count < 5)
                {
                    int randomIndex = random.Next(questions.Length);
                    if (!selectedIndexes.Contains(randomIndex))
                    {
                        selectedIndexes.Add(randomIndex);
                    }
                }
                char[] correctAnswers = { 'C', 'C', 'A', 'A', 'A', 'C', 'B', 'B', 'C', 'A' };
                Console.WriteLine("Bấm YES (Y) để bắt đầu hoặc NO (N) để kết thúc.");
            
            inputagain:
                if (falseAns == 0)
                {
                    Character.ScoreA = 6;
                    Console.WriteLine("Bạn đã hết số lần tham gia trò chơi");
                    Console.WriteLine();
                    Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                    PressEnterToContinue();
                }
                else
                {
                    Console.Write("Lựa chọn của bạn là: ");
                    char playerAnswer = Char.ToUpper(Console.ReadKey(false).KeyChar);
                    Console.WriteLine();


                    if (playerAnswer == 'Y')
                    {


                        bool allCorrect = false;

                        while (!allCorrect && falseAns > 0)
                        {
                            allCorrect = true; 

                            for (int i = 0; i < selectedIndexes.Count; i++)
                            {
                                int questionIndex = selectedIndexes[i];
                                for (int k = 0; k < 40; k++) Console.Write("-");
                                Console.WriteLine();
                                Console.WriteLine(" Câu hỏi " + (i + 1) + ": " + questions[questionIndex]);

                                for (int j = 0; j < 4; j++)
                                {
                                    Console.WriteLine(options[questionIndex, j]);
                                }

                                Console.Write("Nhập đáp án (A, B, C, D): ");
                                playerAnswer = Char.ToUpper(Console.ReadKey(false).KeyChar);
                                Console.WriteLine();

                                if (playerAnswer == correctAnswers[questionIndex])
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Chính xác!");
                                    Character.ScoreA++;
                                }
                                else
                                {
                                    falseAns--;
                                    Character.ScoreA = 0;
                                    Console.WriteLine();
                                    Console.WriteLine("Rất tiếc! Bạn đã trả lời sai");
                                    if (falseAns == 0)
                                    {
                                        goto inputagain;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Bạn còn " + falseAns + " lần chơi lại! Bạn có muốn chơi lại không ?");
                                        Console.WriteLine();
                                        goto inputagain;
                                    }
                                }
                            }

                            for (int k = 0; k < 40; k++) Console.Write("-");
                            Console.WriteLine();
                            Console.WriteLine("Chúc mừng bạn đã hoàn thành thử thác và nhận được 1000 điểm");
                            Console.WriteLine();
                            Console.WriteLine("Bạn đã lên cấp");
                            Character.Level++;
                            Character.Experience = 0;
                            Character.ExperienceToNextLevel *= 2;
                            Character.Damage++;
                            Character.Health = Character.MaxHealth;
                            Character.HighScore += 1000;
                            Console.WriteLine();
                            Console.WriteLine($"Cấp độ hiện tại của bạn là: {Character.Level}.");
                            Console.WriteLine();
                            Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                            PressEnterToContinue();
                            ReadyForBattle();
                        }
                    }
                    else if (playerAnswer == 'N')
                    {
                        for (int k = 0; k < 40; k++) Console.Write("-");
                        Console.WriteLine("");
                        Console.WriteLine("Hẹn gặp lại bạn lần sau!");
                        Console.WriteLine();
                        Console.WriteLine("Nhấn phím [Enter] để tiếp tục...");
                        PressEnterToContinue();
                    }
                    else
                    {
                        Console.WriteLine("Lệnh không khả dụng. Vui lòng chọn lại.");
                        goto inputagain;
                    }
                }
            }
        }
    }

    static void ShopAtStore()
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine(" Bạn vào phòng gym");
        Console.WriteLine();
        if (Character.Gold >= 6)
        {
            Character.Gold -= 6;
            Character.Damage++;
            Console.WriteLine($" Bạn dùng 6 xu để tập gym.");
            Console.WriteLine();
            Console.WriteLine($" Bạn tăng 1 điểm sức mạnh.");
        }
        else if (Character.Damage >= 5)
        {
            Console.WriteLine(" Bạn đã mạnh lắm rồi, không cần tập gym nữa đâu.");
        }
        else
        {
            Console.WriteLine(" Chủ phòng Gym: " + "'Muốn tập gym thì đem 6 xu tới đây!'");
        }
        Console.WriteLine();
        Console.Write(" Nhấn [enter] để tiếp tục...");
        PressEnterToContinue();
    }

    static void ChanceForRandomBattle()
    {
        if (Map == Maps.Town)
        {
            return;
        }
        if (movesSinceLastBattle > movesBeforeRandomBattle && Random.Shared.NextDouble() < randomBattleChance)
        {
            Battle(Map == Maps.Castle ? EnemyType.Guard : EnemyType.Boar, out _);
            if (!gameRunning)
            {
                return;
            }
        }
    }

    static void FightGuardBoss()
    {
        Battle(EnemyType.GuardBoss, out bool ranAway);
        if (!gameRunning)
        {
            return;
        }
        else if (ranAway)
        {
            Character.J++;
            Character.MapAnimation = Sprites.RunDown;
        }
        else
        {
            Map[Character.TileJ][Character.TileI] = ' ';
        }
    }

    static string TotNghiep()
    {
        if (Character.HighScore >= 4000)
        {
            return Character.GraduationRanking[3];
        }
        else if (Character.HighScore >= 3000)
        {
            return Character.GraduationRanking[2];
        }
        else if (Character.HighScore >= 2000)
        {
            return Character.GraduationRanking[1];
        }
        else
            return Character.GraduationRanking[0];

    }
    static void FightKing()
    {
        string grade = TotNghiep();
        string[] line3 = {
     $"╔══════════════════════════════════════════════════════════════════╗",
     $"║ ┌──────────────────────────────────────────────────────────────┐ ║",
     $"║ │                       BẰNG TỐT NGHIỆP                        │ ║",
     $"║ │                                                              │ ║",
     $"║ │           UNIVERSITY OF ECONOMICS HO CHI MINH CITY           │ ║",
     $"║ │                                                              │ ║",
     $"║ │    HỌ VÀ TÊN:                                                | ║",
     $"║ │                                                              │ ║",
     $"║ │    NĂM SINH:                                                 │ ║",
     $"║ │                                                              │ ║",
     $"║ │    NƠI SINH:                                                 │ ║",
     $"║ │                                                              │ ║",
     $"║ │    LOẠI TỐT NGHIỆP:                                          │ ║",
     $"║ │                                                              │ ║",
     $"║ │    NGÀNH HỌC:                                                │ ║",
     $"║ │                                                              │ ║",
     $"║ │                                     KT. HIỆU TRƯỞNG          │ ║",
     $"║ │                                                              │ ║",
     $"║ │                                   GS.TS SỬ ĐÌNH THÀNH        │ ║",
     $"║ │                                                              │ ║",
     $"║ └──────────────────────────────────────────────────────────────┘ ║",
     $"╚══════════════════════════════════════════════════════════════════╝",

        };
        Battle(EnemyType.FinalBoss, out bool ranAway);
        if (!gameRunning)
        {
            return;
        }
        else if (ranAway)
        {
            Character.J++;
            Character.MapAnimation = Sprites.RunDown;
        }
        else
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine(" Bạn đã ra trường thành công");
            Console.WriteLine();
            Console.ResetColor(); 
            Console.ForegroundColor = ConsoleColor.Cyan;
            IntroScreen(line3);
            DrawPlayerInfo(line3);
            Console.WriteLine();
            Console.Write(" Nhấn [enter] để kết thúc");
            PressEnterToContinue();
            gameRunning = false;
            return;
        }

        static void DrawPlayerInfo(string[] a)
        {
            string grade = TotNghiep();
            int screenHeight = Console.WindowHeight;
            int screenWidth = Console.WindowWidth;
            int startRow = (screenHeight / 2) - (a.Length / 2) + 6;
            int startCol = screenWidth / 2 - 2;

            Console.SetCursorPosition(startCol, startRow);
            Console.WriteLine(Character.Name);

            Console.SetCursorPosition(startCol, startRow + 2);
            Console.WriteLine(Character.Year);

            Console.SetCursorPosition(startCol, startRow + 4);
            Console.WriteLine(Character.PlaceOfBirth);

            Console.SetCursorPosition(startCol, startRow + 6);
            Console.WriteLine(grade);

            Console.SetCursorPosition(startCol, startRow + 8);
            Console.WriteLine(Character.Major);
        }

    }

    static void HandleMapUserInput()
    {
        while (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case
                    ConsoleKey.UpArrow or ConsoleKey.W or
                    ConsoleKey.DownArrow or ConsoleKey.S or
                    ConsoleKey.LeftArrow or ConsoleKey.A or
                    ConsoleKey.RightArrow or ConsoleKey.D:
                    if (Character.IsIdle)
                    {
                        var (tileI, tileJ) = key switch
                        {
                            ConsoleKey.UpArrow or ConsoleKey.W => (Character.TileI, Character.TileJ - 1),
                            ConsoleKey.DownArrow or ConsoleKey.S => (Character.TileI, Character.TileJ + 1),
                            ConsoleKey.LeftArrow or ConsoleKey.A => (Character.TileI - 1, Character.TileJ),
                            ConsoleKey.RightArrow or ConsoleKey.D => (Character.TileI + 1, Character.TileJ),
                            _ => throw new Exception("bug"),
                        };
                        if (Maps.IsValidCharacterMapTile(Map, tileI, tileJ))
                        {
                            switch (key)
                            {
                                case ConsoleKey.UpArrow or ConsoleKey.W:
                                    Character.J--;
                                    Character.MapAnimation = Sprites.RunUp;
                                    break;
                                case ConsoleKey.DownArrow or ConsoleKey.S:
                                    Character.J++;
                                    Character.MapAnimation = Sprites.RunDown;
                                    break;
                                case ConsoleKey.LeftArrow or ConsoleKey.A:
                                    Character.MapAnimation = Sprites.RunLeft;
                                    break;
                                case ConsoleKey.RightArrow or ConsoleKey.D:
                                    Character.MapAnimation = Sprites.RunRight;
                                    break;
                            }
                        }
                    }
                    break;
                case ConsoleKey.Enter:
                    RenderStatusString().Wait();
                    break;
                case ConsoleKey.Escape:
                    Console.Clear();
                decideagain:
                    Console.WriteLine(" Bạn có chắc muốn thoát trò chơi không (tiến trình của bạn sẽ được lưu lại)?");
                    Console.WriteLine(" Nhấn Yes (Y) hoặc No (N):");
                    char input = Char.ToUpper(Console.ReadKey(true).KeyChar);
                    if (input == 'Y')
                    {
                        Console.WriteLine();
                        RecordTime();
                        SaveData();
                        Console.WriteLine(" Hẹn gặp bạn lần sau");
                        Thread.Sleep(1000);
                        gameRunning = false;
                        return;
                    }
                    else if (input == 'N')
                    {
                        return;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine(" Lệnh không khả dụng, hãy nhập lại!");
                        goto decideagain;
                    }
            }
        }
    }

    static void Battle(EnemyType enemyType, out bool ranAway)
    {
        movesSinceLastBattle = 0;
        ranAway = false;

        int enemyHealth = enemyType switch
        {
            EnemyType.Boar => 03,
            EnemyType.GuardBoss => 20,
            EnemyType.Guard => 10,
            EnemyType.FinalBoss => 50,
            _ => 1,
        };

        switch (enemyType)
        {
            case EnemyType.Boar:
                CombatText =
                [
                    "Bạn gặp phải thử thách trên hành trình!",
                    "1) Chiến đấu",
                    "2) Bỏ chạy",
                    "3) Kiểm tra trạng thái",
                ];
                break;
            case EnemyType.GuardBoss:
                if (Character.Level < 2)
                {
                    CombatText =
                    [
                        "Bạn đã gặp người bảo vệ lâu đài",
                        "Anh ta khá mạnh, có lẽ bạn nên",
                        "chạy khỏi đây và trở lại khi mạnh hơn",
                        "1) Chiến đấu",
                        "2) Bỏ chạy ",
                        "3) Kiểm tra trạng thái",
                    ];
                }
                else
                {
                    CombatText =
                    [
                        "Bạn đã gặp người bảo vệ lâu đài",
                        "1) Chiến đấu",
                        "2) Bỏ chạy",
                        "3) Kiểm tra trạng thái",
                    ];
                }
                break;
            case EnemyType.Guard:
                CombatText =
                [
                    "Bạn đã bị tấn công bởi lính gác trong lâu đài",
                    "1) Chiến đấu",
                    "2) Bỏ chạy",
                    "3) Kiểm tra trạng thái",
                ];
                break;
            case EnemyType.FinalBoss:
                if (Character.Level < 3)
                {
                    CombatText =
                    [
                        "Bạn đã gặp thử thách cuối cùng của đời sinh viên!",
                        "Thử thách này rất khó khăn, bạn sẽ",
                        "bỏ cuộc hay chiến đấu đến cùng",
                        "1) Chiến đấu",
                        "2) Bỏ chạy",
                        "3) Kiểm tra trạng thái",
                    ];
                }
                else
                {
                    CombatText =
                    [
                        "Bạn đã gặp thử thách cuối cùng của đời sinh viên!",
                        "1) Chiến đấu",
                        "2) Bỏ chạy",
                        "3) Kiểm tra trạng thái",
                    ];
                }
                break;
        }

        int frameLeft = 0;
        int frameRight = 0;

        string[] animationLeft = Sprites.IdleRight;
        string[] animationRight = enemyType switch
        {
            EnemyType.Boar => Sprites.IdleBoar,
            EnemyType.Guard => Sprites.IdleLeft,
            EnemyType.GuardBoss => Sprites.IdleLeft,
            EnemyType.FinalBoss => Sprites.IdleLeft,
            _ => [Sprites.Error],
        };

        bool pendingConfirmation = false;

        while (true)
        {
            if (animationLeft == Sprites.GetUpAnimationRight && frameLeft == animationLeft.Length - 1)
            {
                frameLeft = 0;
                animationLeft = Sprites.IdleRight;
            }
            else if (animationLeft == Sprites.IdleRight || frameLeft < animationLeft.Length - 1)
            {
                frameLeft++;
            }
            if (frameLeft >= animationLeft.Length) frameLeft = 0;
            frameRight++;
            if (frameRight >= animationRight.Length) frameRight = 0;
            while (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.D1 or ConsoleKey.NumPad1:
                        if (!pendingConfirmation)
                        {
                            switch (Random.Shared.Next(2))
                            {
                                case 0:
                                    frameLeft = 0;
                                    animationLeft = Sprites.PunchRight;
                                    CombatText =
                                    [
                                        " Bạn đã gây sát thương cho kẻ thù!",
                                        "",
                                        " Nhấn [enter] để tiếp tục...",
                                    ];
                                    enemyHealth -= Character.Damage;
                                    break;
                                case 1:
                                    frameLeft = 0;
                                    animationLeft = Sprites.FallLeft;
                                    CombatText =
                                    [
                                        " Kẻ dịch quá nhanh",
                                        " Bạn đã bị tấn công!",
                                        "",
                                        " Nhấn [enter] để tiếp tục...",
                                    ];
                                    Character.Health--;
                                    break;
                            }
                            pendingConfirmation = true;
                        }
                        break;
                    case ConsoleKey.D2 or ConsoleKey.NumPad2:
                        if (!pendingConfirmation)
                        {
                            bool success = enemyType switch
                            {
                                EnemyType.Boar => Random.Shared.Next(10) < 9,
                                EnemyType.Guard => Random.Shared.Next(10) < 7,
                                _ => true,
                            };
                            if (success)
                            {
                                Console.Clear();
                                Console.WriteLine();
                                Console.WriteLine(" Bạn đã bỏ chạy");
                                Console.WriteLine();
                                Console.Write(" Nhấn [enter] để tiếp tục...");
                                PressEnterToContinue();
                                ranAway = true;
                                return;
                            }
                            else
                            {
                                frameLeft = 0;
                                animationLeft = Sprites.FallLeft;
                                CombatText =
                                [
                                    " Bạn đã cố gắng chạy nhưng địch",
                                    " đã đánh lén bạn từ phía sau và bạn",
                                    " đã bị thương",
                                    "",
                                    " Nhấn [enter] để tiếp tục...",
                                ];
                                Character.Health--;
                                pendingConfirmation = true;
                            }
                        }
                        break;
                    case ConsoleKey.D3 or ConsoleKey.NumPad3:
                        if (!pendingConfirmation)
                        {
                            RenderStatusString().Wait();
                            if (!gameRunning)
                            {
                                return;
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (pendingConfirmation)
                        {
                            pendingConfirmation = false;
                            if (animationLeft == Sprites.FallLeft && frameLeft == animationLeft.Length - 1)
                            {
                                frameLeft = 0;
                                animationLeft = Sprites.GetUpAnimationRight;
                            }
                            else
                            {
                                frameLeft = 0;
                                animationLeft = Sprites.IdleRight;
                            }
                            CombatText = defaultCombatText;
                            if (Character.Health <= 0)
                            {
                                RenderDeathScreen();
                                gameRunning = false;
                                return;
                            }
                            if (enemyHealth <= 0)
                            {
                                if (enemyType is EnemyType.FinalBoss)
                                {
                                    return;
                                }
                                int scoreGain = enemyType switch
                                {
                                    EnemyType.Boar => 50,
                                    EnemyType.GuardBoss => 300,
                                    EnemyType.Guard => 100,
                                    EnemyType.FinalBoss => 700,
                                    _ => 0,
                                };
                                int experienceGain = enemyType switch
                                {
                                    EnemyType.Boar => 1,
                                    EnemyType.GuardBoss => 20,
                                    EnemyType.Guard => 10,
                                    EnemyType.FinalBoss => 9001, 
                                    _ => 0,
                                };
                                Console.Clear();
                                Console.WriteLine();
                                Console.WriteLine(" Bạn đã vượt qua thử thách!");
                                Console.WriteLine();
                                Console.WriteLine($" Bạn nhận được {experienceGain} điểm kinh nghiệm và {scoreGain} điểm.");
                                Console.WriteLine();
                                Character.HighScore += scoreGain;
                                Character.Experience += experienceGain;
                                if (Character.Experience >= Character.ExperienceToNextLevel)
                                {
                                    Character.Level++;
                                    Character.Experience = 0;
                                    Character.ExperienceToNextLevel *= 2;
                                    Character.Damage++;
                                    Character.Health = Character.MaxHealth;
                                    Console.WriteLine($" Bạn đã lên cấp {Character.Level}.");
                                    Console.WriteLine();
                                }
                                Console.WriteLine();
                                Console.Write(" Nhấn [enter] để tiếp tục...");
                                PressEnterToContinue();
                                return;
                            }
                        }
                        break;
                    case ConsoleKey.Escape:
                        Console.Clear();
                    decideagain:
                        Console.WriteLine(" Bạn có chắc muốn thoát trò chơi không (tiến trình của bạn sẽ được lưu lại)?");
                        Console.WriteLine(" Nhấn Yes (Y) hoặc No (N):");
                        char input = Char.ToUpper(Console.ReadKey(true).KeyChar);
                        if (input == 'Y')
                        {
                            Console.WriteLine();
                            RecordTime();
                            SaveData();
                            Console.WriteLine(" Hẹn gặp bạn lần sau");
                            Thread.Sleep(1000);
                            gameRunning = false;
                            return;
                        }
                        else if (input == 'N')
                        {
                            return;
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine(" Lệnh không khả dụng, hãy nhập lại!");
                            goto decideagain;
                        }

                }
            }
            RenderBattleView(animationLeft[frameLeft], animationRight[frameRight]);
            SleepAfterRender();
        }
    }

    static void SleepAfterRender()
    {
        DateTime now = DateTime.Now;
        TimeSpan sleep = TimeSpan.FromMilliseconds(33) - (now - previoiusRender);
        if (sleep > TimeSpan.Zero)
        {
            Thread.Sleep(sleep);
        }
        previoiusRender = DateTime.Now;
    }

    static (int I, int J)? FindTileInMap(char[][] map, char c)
    {
        for (int j = 0; j < map.Length; j++)
        {
            for (int i = 0; i < map[j].Length; i++)
            {
                if (map[j][i] == c)
                {
                    return (i, j);
                }
            }
        }
        return null;
    }

    static void RenderWorldMapView()
    {
        Console.CursorVisible = false;

        var (width, height) = GetWidthAndHeight();
        int heightCutOff = (int)(height * .80);
        int midWidth = width / 2;
        int midHeight = heightCutOff / 2;
        StringBuilder sb = new(width * height);
        for (int j = 0; j < height; j++)
        {
            if (OperatingSystem.IsWindows() && j == height - 1)
            {
                break;
            }

            for (int i = 0; i < width; i++)
            {
                if (j >= heightCutOff)
                {
                    int line = j - heightCutOff - 1;
                    int character = i - 1;
                    if (i < width - 1 && character >= 0 && line >= 0 && line < maptext.Length && character < maptext[line].Length)
                    {
                        char ch = maptext[line][character];
                        sb.Append(char.IsWhiteSpace(ch) ? ' ' : ch);
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                    continue;
                }

                if (i is 0 && j is 0)
                {
                    sb.Append('╔');
                    continue;
                }
                if (i is 0 && j == heightCutOff - 1)
                {
                    sb.Append('╚');
                    continue;
                }
                if (i == width - 1 && j is 0)
                {
                    sb.Append('╗');
                    continue;
                }
                if (i == width - 1 && j == heightCutOff - 1)
                {
                    sb.Append('╝');
                    continue;
                }
                if (i is 0 || i == width - 1)
                {
                    sb.Append('║');
                    continue;
                }
                if (j is 0 || j == heightCutOff - 1)
                {
                    sb.Append('═');
                    continue;
                }

                // character
                if (i > midWidth - 4 && i < midWidth + 4 && j > midHeight - 2 && j < midHeight + 3)
                {
                    int ci = i - (midWidth - 3);
                    int cj = j - (midHeight - 1);
                    string characterMapRender = Character.Render;
                    sb.Append(characterMapRender[cj * 8 + ci]);
                    continue;
                }

                int mapI = i - midWidth + Character.I + 3;
                int mapJ = j - midHeight + Character.J + 1;

                int tileI = mapI < 0 ? (mapI - 6) / 7 : mapI / 7;
                int tileJ = mapJ < 0 ? (mapJ - 3) / 4 : mapJ / 4;

                int pixelI = mapI < 0 ? 6 + ((mapI + 1) % 7) : (mapI % 7);
                int pixelJ = mapJ < 0 ? 3 + ((mapJ + 1) % 4) : (mapJ % 4);

                string tileRender = Maps.GetMapTileRender(Map, tileI, tileJ);
                char c = tileRender[pixelJ * 8 + pixelI];
                sb.Append(char.IsWhiteSpace(c) ? ' ' : c);
            }
            if (!OperatingSystem.IsWindows() && j < height - 1)
            {
                sb.AppendLine();
            }
        }
        Console.SetCursorPosition(0, 0);
        Console.Write(sb);
    }

    static void RenderBattleView(string spriteLeft, string spriteRight)
    {
        Console.CursorVisible = false;

        var (width, height) = GetWidthAndHeight();
        int midWidth = width / 2;
        int thirdHeight = height / 3;
        int textStartJ = thirdHeight + 7;

        StringBuilder sb = new(width * height);
        for (int j = 0; j < height; j++)
        {
            if (OperatingSystem.IsWindows() && j == height - 1)
            {
                break;
            }

            for (int i = 0; i < width; i++)
            {
                if (j >= textStartJ)
                {
                    int line = j - textStartJ - 1;
                    int character = i - 1;
                    if (i < width - 1 && character >= 0 && line >= 0 && line < CombatText.Length && character < CombatText[line].Length)
                    {
                        char ch = CombatText[line][character];
                        sb.Append(char.IsWhiteSpace(ch) ? ' ' : ch);
                        continue;
                    }
                }

                if (i > midWidth - 4 - 10 && i < midWidth + 4 - 10 && j > thirdHeight - 2 && j < thirdHeight + 3)
                {
                    int ci = i - (midWidth - 3) + 10;
                    int cj = j - (thirdHeight - 1);
                    string characterMapRender = spriteLeft;
                    sb.Append(characterMapRender[cj * 8 + ci]);
                    continue;
                }

                if (i > midWidth - 4 + 10 && i < midWidth + 4 + 10 && j > thirdHeight - 2 && j < thirdHeight + 3)
                {
                    int ci = i - (midWidth - 3) - 10;
                    int cj = j - (thirdHeight - 1);
                    string characterMapRender = spriteRight;
                    sb.Append(characterMapRender[cj * 8 + ci]);
                    continue;
                }

                sb.Append(' ');
            }
            if (!OperatingSystem.IsWindows() && j < height - 1)
            {
                sb.AppendLine();
            }
        }
        Console.SetCursorPosition(0, 0);
        Console.Write(sb);
    }

    static (int Width, int Height) GetWidthAndHeight()
    {
    RestartRender:
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        if (OperatingSystem.IsWindows())
        {
            try
            {
                if (Console.BufferHeight != height) Console.BufferHeight = height;
                if (Console.BufferWidth != width) Console.BufferWidth = width;
            }
            catch (Exception)
            {
                Console.Clear();
                goto RestartRender;
            }
        }
        return (width, height);
    }
}

public enum EnemyType
{
    Boar,
    Guard,
    GuardBoss,
    FinalBoss,
}
