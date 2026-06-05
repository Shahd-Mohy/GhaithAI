namespace GhaithAI.API.Constants
{
    public static class MoodTypes
    {
            public const int VeryLow = 1;
            public const int Low = 2;
            public const int Okay = 3;
            public const int Good = 4;
            public const int Great = 5;

            public static string GetLabel(int score) => score switch
            {
                1 => "Very Low",
                2 => "Low",
                3 => "Okay",
                4 => "Good",
                5 => "Great",
                _ => "Unknown"
            };

            public static string GetBadge(int score) => score switch
            {
                1 => "VL",
                2 => "L",
                3 => "O",
                4 => "G",
                5 => "Gr",
                _ => " "
            };
            public static bool IsValid(int score) => score >= 1 && score <= 5;
        }
    }