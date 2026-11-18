using System.Globalization;

class SavingsGoal
{
    // This class keeps the main savings information together so the program can update and display one goal easily.
    public string GoalName { get; set; }
    public double GoalAmount { get; set; }
    public double CurrentAmount { get; set; }

    public SavingsGoal(string goalName, double goalAmount)
    {
        GoalName = goalName;
        GoalAmount = goalAmount;
        CurrentAmount = 0;
    }
}

class Deposit
{
    // Each deposit needs its own date and amount so the user can see how their savings grew over time.
    public DateTime Date { get; set; }
    public double Amount { get; set; }

    public Deposit(DateTime date, double amount)
    {
        Date = date;
        Amount = amount;
    }
}

class Program
{
    // Keeping the file name in one place makes saving and loading use the same file every time.
    private const string SaveFileName = "corvette_savings.txt";
    private static SavingsGoal goal = new("Corvette", 0);
    private static List<Deposit> deposits = new();

    static void Main()
    {
        // Loading before showing the menu lets the user continue their progress from a previous session.
        LoadData();
        Console.WriteLine("Corvette Savings Tracker");

        if (goal.GoalAmount == 0)
        {
            Console.WriteLine("No savings goal has been set.");
            SetGoal();
        }

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n1. View Goal Progress");
            Console.WriteLine("2. Add Deposit");
            Console.WriteLine("3. View Deposit History");
            Console.WriteLine("4. Save Data");
            Console.WriteLine("5. Exit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1": ViewProgress(); break;
                case "2": AddDeposit(); break;
                case "3": ViewDepositHistory(); break;
                case "4": SaveData(); break;
                case "5":
                    SaveData();
                    Console.WriteLine("Your progress was saved.");
                    running = false;
                    break;
                default: Console.WriteLine("Please choose a number from 1 to 5."); break;
            }
        }
    }

    static void SetGoal()
    {
        Console.Write("Enter the target Corvette price: $");
        goal.GoalAmount = ReadPositiveNumber();
        Console.WriteLine($"Your goal is set to {goal.GoalAmount:C2}.");
    }

    static void AddDeposit()
    {
        Console.Write("Enter deposit amount: $");
        double amount = ReadPositiveNumber();
        deposits.Add(new Deposit(DateTime.Now, amount));
        goal.CurrentAmount += amount;
        Console.WriteLine($"Deposit added. Total saved: {goal.CurrentAmount:C2}");
    }

    static void ViewProgress()
    {
        if (goal.GoalAmount <= 0)
        {
            Console.WriteLine("Set a goal before viewing progress.");
            return;
        }

        double percentage = Math.Min(goal.CurrentAmount / goal.GoalAmount * 100, 100);
        double remaining = Math.Max(goal.GoalAmount - goal.CurrentAmount, 0);
        Console.WriteLine($"\nGoal: {goal.GoalName}");
        Console.WriteLine($"Target price: {goal.GoalAmount:C2}");
        Console.WriteLine($"Current savings: {goal.CurrentAmount:C2}");
        Console.WriteLine($"Progress: {percentage:F1}%");
        Console.WriteLine($"Amount remaining: {remaining:C2}");

        if (remaining == 0)
        {
            Console.WriteLine("Congratulations! You reached your Corvette goal!");
        }
        else
        {
            Console.WriteLine($"Estimated time remaining: {EstimateTimeRemaining(remaining)}");
        }
    }

    static string EstimateTimeRemaining(double amountRemaining)
    {
        if (deposits.Count == 0)
            return "Add deposits to calculate an estimate.";

        // Using the user's actual deposit history makes the estimate more personal than assuming a fixed monthly amount.
        double totalMonths = Math.Max((DateTime.Now - deposits[0].Date).TotalDays / 30.44, 1);
        double averageMonthlyDeposit = goal.CurrentAmount / totalMonths;
        if (averageMonthlyDeposit <= 0)
            return "Not enough information yet.";

        double months = amountRemaining / averageMonthlyDeposit;
        int wholeMonths = (int)Math.Ceiling(months);
        return wholeMonths == 1 ? "about 1 month" : $"about {wholeMonths} months";
    }

    static void ViewDepositHistory()
    {
        if (deposits.Count == 0)
        {
            Console.WriteLine("No deposits have been recorded yet.");
            return;
        }

        Console.WriteLine("\nDeposit History");
        Console.WriteLine("-------------------------------");
        for (int i = 0; i < deposits.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {deposits[i].Date:g} - {deposits[i].Amount:C2}");
        }
    }

    static void SaveData()
    {
        try
        {
            // A simple text format keeps the project easy to understand while still keeping the progress.
            using StreamWriter writer = new(SaveFileName);
            writer.WriteLine($"GOAL|{goal.GoalName}|{goal.GoalAmount.ToString(CultureInfo.InvariantCulture)}|{goal.CurrentAmount.ToString(CultureInfo.InvariantCulture)}");
            foreach (Deposit deposit in deposits)
            {
                writer.WriteLine($"DEPOSIT|{deposit.Date.Ticks}|{deposit.Amount.ToString(CultureInfo.InvariantCulture)}");
            }
            Console.WriteLine($"Data saved to {SaveFileName}.");
        }
        catch (IOException exception)
        {
            Console.WriteLine($"Could not save data: {exception.Message}");
        }
    }

    static void LoadData()
    {
        if (!File.Exists(SaveFileName)) return;

        try
        {
            // Reading one record at a time lets the program rebuild the goal and deposit history when it starts.
            foreach (string line in File.ReadAllLines(SaveFileName))
            {
                string[] parts = line.Split('|');
                if (parts.Length < 4 && parts.Length < 3) continue;

                if (parts[0] == "GOAL" && parts.Length >= 4 &&
                    double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double target) &&
                    double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double current))
                {
                    goal = new SavingsGoal(parts[1], target) { CurrentAmount = current };
                }
                else if (parts[0] == "DEPOSIT" && parts.Length >= 3 &&
                         long.TryParse(parts[1], out long ticks) &&
                         double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double amount))
                {
                    deposits.Add(new Deposit(new DateTime(ticks), amount));
                }
            }
        }
        catch (Exception exception) when (exception is IOException or FormatException or ArgumentOutOfRangeException)
        {
            Console.WriteLine($"Saved data could not be loaded: {exception.Message}");
            goal = new SavingsGoal("Corvette", 0);
            deposits.Clear();
        }
    }

    static double ReadPositiveNumber()
    {
        while (true)
        {
            string? input = Console.ReadLine();
            if (double.TryParse(input, out double number) && number > 0)
                return number;

            Console.Write("Please enter a positive number: $");
        }
    }
}
