using AutoFixture;
using Grpc.Core;
using Grpc.Net.Client;
using AuthServices;
using UserServices;
using GradeServices;
using static AuthServices.AuthService;
using static UserServices.UserService;
using static GradeServices.GradeService;

namespace UniverSystemGun;

class Program
{
    const string GATEWAY_SERVICE_URI = "http://localhost:5000";
    const string AUTH_SERVICE_URI = "http://localhost:5001";
    const string USER_SERVICE_URI = "http://localhost:5003";
    const string GRADE_SERVICE_URI = "http://localhost:5007";

    const int DOP = 3; // Degree of parallelism
    const int MIN_DELAY_MS = 100;
    const int MAX_DELAY_MS = 500;
    const int MAX_GRADE_VALUE = 5;
    const int MIN_GRADE_VALUE = 1;

    static readonly IReadOnlyList<string> CourseIds =
    [
        "55555555-5555-5555-5555-555555555555", // Программирование
        "66666666-6666-6666-6666-666666666666", // Базы данных
        "77777777-7777-7777-7777-777777777777", // Алгоритмы и структуры данных
        "88888888-8888-8888-8888-888888888888", // Веб-разработка
        "99999999-9999-9999-9999-999999999999", // Математический анализ
        "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", // Операционные системы
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"  // Компьютерные сети
    ];

    static readonly IReadOnlyList<string> StudentIds =
    [
        "33333333-3333-3333-3333-333333333333", // Евстигней Абрикосов
        "66666666-6666-6666-6666-666666666666", // Анна Петрова
        "77777777-7777-7777-7777-777777777777", // Сергей Сидоров
        "88888888-8888-8888-8888-888888888888"  // Мария Козлова
    ];

    static readonly IReadOnlyList<string> TeacherIds =
    [
        "44444444-4444-4444-4444-444444444444", // Фома Киняев
        "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", // Мария Иванова
        "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"  // Алексей Смирнов
    ];

    static readonly IReadOnlyList<(string Username, string Password)> TestUsers =
    [
        ("student1", "student1"),
        ("student2", "student2"),
        ("student3", "student3"),
        ("student4", "student4"),
        ("teacher1", "teacher1"),
        ("teacher2", "teacher2"),
        ("teacher3", "teacher3")
    ];

    static async Task Main(string[] args)
    {
        var authChannel = GrpcChannel.ForAddress(AUTH_SERVICE_URI);
        var userChannel = GrpcChannel.ForAddress(USER_SERVICE_URI);
        var gradeChannel = GrpcChannel.ForAddress(GRADE_SERVICE_URI);

        var authClient = new AuthServiceClient(authChannel);
        var userClient = new UserServiceClient(userChannel);
        var gradeClient = new GradeServiceClient(gradeChannel);

        var rnd = new Random();
        var fx = new Fixture();
        var cts = new CancellationTokenSource();

        // Собираем токены для аутентифицированных пользователей
        var userTokens = new Dictionary<string, string>();

        // Сначала выполняем логин для всех тестовых пользователей
        var loginTasks = TestUsers.Select(async user =>
        {
            try
            {
                var loginRequest = new AuthServices.LoginRequest
                {
                    Username = user.Username,
                    Password = user.Password
                };

                var response = await authClient.LoginAsync(loginRequest);
                userTokens[user.Username] = response.Token;
                Console.WriteLine($"Login successful for {user.Username}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Login failed for {user.Username}: {ex.Status.Detail}");
            }
        });

        await Task.WhenAll(loginTasks);
        Console.WriteLine($"Collected {userTokens.Count} valid tokens");

        var authTasks = Enumerable
            .Range(0, DOP)
            .Select(_ => RunAuthTests(authClient, fx, rnd, cts.Token));

        var userTasks = Enumerable
            .Range(0, DOP)
            .Select(_ => RunUserTests(userClient, authClient, userTokens, rnd, cts.Token));

        var gradeTasks = Enumerable
            .Range(0, DOP)
            .Select(_ => RunGradeTests(gradeClient, authClient, userTokens, fx, rnd, cts.Token));

        await Task.WhenAll([
            .. authTasks,
            .. userTasks,
            .. gradeTasks
        ]);

        Console.WriteLine("Load testing completed");

        await authChannel.ShutdownAsync();
        await userChannel.ShutdownAsync();
        await gradeChannel.ShutdownAsync();
    }

    static async Task RunAuthTests(
        AuthServiceClient client,
        Fixture fixture,
        Random random,
        CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var testUser = TestUsers[random.Next(TestUsers.Count)];
                var request = new AuthServices.LoginRequest
                {
                    Username = testUser.Username,
                    Password = testUser.Password
                };

                var response = await client.LoginAsync(request, cancellationToken: token);
                Console.WriteLine($"{nameof(RunAuthTests)} Login successful for {testUser.Username}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"{nameof(RunAuthTests)} Failed with `{ex.Status.StatusCode}`: {ex.Status.Detail}");
            }

            await RandomDelay(random, token);
        }
    }

    static async Task RunUserTests(
        UserServiceClient userClient,
        AuthServiceClient authClient,
        Dictionary<string, string> userTokens,
        Random random,
        CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // Выбираем случайный токен для аутентификации
                var tokenEntry = userTokens.ElementAt(random.Next(userTokens.Count));
                var authToken = tokenEntry.Value;

                // Выбираем случайного пользователя для запроса профиля
                var targetUserId = GetRandomUserId(random);

                var headers = new Metadata
                {
                    { "Authorization", $"{authToken}" }
                };

                var request = new GetUserProfileRequest { UserId = targetUserId };
                var response = await userClient.GetUserProfileAsync(request, headers, cancellationToken: token);

                Console.WriteLine($"{nameof(RunUserTests)} Profile retrieved for user {targetUserId}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"{nameof(RunUserTests)} Failed with `{ex.Status.StatusCode}`: {ex.Status.Detail}");
            }

            await RandomDelay(random, token);
        }
    }

    static async Task RunGradeTests(
        GradeServiceClient gradeClient,
        AuthServiceClient authClient,
        Dictionary<string, string> userTokens,
        Fixture fixture,
        Random random,
        CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // Для операций с оценками нужен преподаватель
                var teacherTokens = userTokens.Where(kv => kv.Key.StartsWith("teacher") || kv.Key == "admin1")
                                             .ToDictionary(kv => kv.Key, kv => kv.Value);

                if (teacherTokens.Any())
                {
                    var teacherToken = teacherTokens.ElementAt(random.Next(teacherTokens.Count)).Value;
                    var headers = new Metadata { { "Authorization", $"{teacherToken}" } };

                    // Чередуем операции добавления оценки и получения оценок студента
                    if (random.Next(2) == 0)
                    {
                        // Добавление оценки
                        var addGradeRequest = new GradeServices.AddGradeRequest
                        {
                            StudentId = StudentIds[random.Next(StudentIds.Count)],
                            CourseId = CourseIds[random.Next(CourseIds.Count)],
                            GradeValue = random.Next(MIN_GRADE_VALUE, MAX_GRADE_VALUE + 1),
                            TeacherId = TeacherIds[random.Next(TeacherIds.Count)]
                        };

                        var addResponse = await gradeClient.AddGradeAsync(addGradeRequest, headers, cancellationToken: token);
                        Console.WriteLine($"{nameof(RunGradeTests)} Grade added: {addResponse.GradeId}");
                    }
                    else
                    {
                        // Получение оценок студента
                        var studentId = StudentIds[random.Next(StudentIds.Count)];
                        var getGradesRequest = new GetStudentGradesRequest { StudentId = studentId };

                        var getResponse = await gradeClient.GetStudentGradesAsync(getGradesRequest, headers, cancellationToken: token);
                        Console.WriteLine($"{nameof(RunGradeTests)} Retrieved {getResponse.Grades.Count} grades for student {studentId}");
                    }
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"{nameof(RunGradeTests)} Failed with `{ex.Status.StatusCode}`: {ex.Status.Detail}");
            }

            await RandomDelay(random, token);
        }
    }

    static string GetRandomUserId(Random random)
    {
        var allUserIds = new[]
        {
            "11111111-1111-1111-1111-111111111111", // student1
            "22222222-2222-2222-2222-222222222222", // teacher1
            "33333333-3333-3333-3333-333333333333", // student2
            "44444444-4444-4444-4444-444444444444", // student3
            "55555555-5555-5555-5555-555555555555", // student4
            "66666666-6666-6666-6666-666666666666", // teacher2
            "77777777-7777-7777-7777-777777777777", // teacher3
        };

        return allUserIds[random.Next(allUserIds.Length)];
    }

    static Task RandomDelay(Random random, CancellationToken cancellationToken)
    {
        int delay = MIN_DELAY_MS + random.Next(MAX_DELAY_MS - MIN_DELAY_MS);
        return Task.Delay(delay, cancellationToken);
    }
}